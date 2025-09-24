using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace LightGive.UnityUtil.Runtime
{
	/// <summary>
	/// スプライトを使用したシンプルなアニメーション
	/// </summary>
	public class SimpleSpriteAnimator : MonoBehaviour
	{
		[SerializeField] SpriteRenderer _spriteRenderer;
		[SerializeField] SimpleSpriteAnimationData[] _animationDatas;
		[SerializeField] UpdateMode _updateMode;
		[SerializeField] int _fps = 60;
		[SerializeField] bool _isLoop = true;
		int _currentSpriteIndex;
		float _currentTime;
		float _timePerSprite;
		SimpleSpriteAnimationData _currentAnimationData;
		bool _hasCompletedOnce;

		/// <summary>
		/// アニメーションが完了したときに呼び出されるUnityEvent
		/// </summary>
		[field: SerializeField] public UnityEvent OnAnimationComplete { get; private set; }

		/// <summary>
		/// アニメーションの現在の状態
		/// </summary>
		public AnimationState State { get; private set; } = AnimationState.Stopped;

		/// <summary>
		/// アニメーションが再生中かどうか
		/// </summary>
		public bool IsPlaying => State == AnimationState.Playing;

		/// <summary>
		/// アニメーションが一時停止中かどうか
		/// </summary>
		public bool IsPaused => State == AnimationState.Paused;

		/// <summary>
		/// アニメーションが停止中かどうか
		/// </summary>
		public bool IsStopped => State == AnimationState.Stopped;

		/// <summary>
		/// アニメーションがループ再生かどうか
		/// </summary>
		public bool IsLoop
		{
			get => _isLoop;
			set => _isLoop = value;
		}

		void Reset()
		{
			TryGetComponent(out _spriteRenderer);
		}

		void Awake()
		{
			Assert.IsNotNull(_spriteRenderer);
			Assert.IsTrue(_fps > 0);
			Assert.IsTrue(_animationDatas.Length > 0);
			Assert.IsTrue(!_animationDatas.Any(x => x == null));

			for (int i = 0; i < _animationDatas.Length; i++)
			{
				Assert.IsNotNull(_animationDatas[i].Sprites);
				Assert.IsTrue(_animationDatas[i].Sprites.Length > 0);
				Assert.IsTrue(!_animationDatas[i].Sprites.Any(x => x == null));
			}

			_timePerSprite = 1f / _fps;
		}

		void Update()
		{
			if (_updateMode == UpdateMode.NormalUpdate)
			{
				UpdateFrame();
			}
		}

		void OnDestroy()
		{
			OnAnimationComplete?.RemoveAllListeners();
		}

		/// <summary>
		/// 指定されたインデックスのアニメーションを最初から再生開始する
		/// </summary>
		/// <param name="index">再生するアニメーションのインデックス（デフォルト: 0）</param>
		/// <remarks>既に同じアニメーションが再生中でも最初からリスタートします</remarks>
		public void Play(int index = 0)
		{
			if (!IsValidAnimationIndex(index))
			{
				Debug.LogWarning($"無効なアニメーションインデックス: {index}");
				return;
			}

			// 常に最初から再生（予測可能）
			SetCurrentAnimation(index);
			State = AnimationState.Playing;
		}

		/// <summary>
		/// 一時停止中のアニメーションを再開する
		/// </summary>
		public void Resume()
		{
			if (!IsPaused)
			{
				return;
			}
			State = AnimationState.Playing;
		}

		/// <summary>
		/// 再生中のアニメーションを一時停止する
		/// </summary>
		public void Pause()
		{
			if (!IsPlaying)
			{
				return;
			}
			State = AnimationState.Paused;
		}

		/// <summary>
		/// アニメーションを停止し、状態をリセットする
		/// </summary>
		/// <remarks>スプライトは非表示にせず、最後のフレームを表示し続けます</remarks>
		public void Stop()
		{
			State = AnimationState.Stopped;
			_currentSpriteIndex = 0;
			_currentTime = 0f;
			_currentAnimationData = null;
			_hasCompletedOnce = false;
			// スプライトは非表示にしない
		}

		/// <summary>
		/// アニメーションフレームを手動で次に進める
		/// </summary>
		public void UpdateFrameManual()
		{
			if (!CanUpdateAnimation())
			{
				return;
			}

			int nextIndex = _currentSpriteIndex + 1;
			int spriteCount = _currentAnimationData.Sprites.Length;

			// 非ループ時は最後のフレームで停止
			if (!_isLoop && nextIndex >= spriteCount)
			{
				OnAnimationComplete?.Invoke();
				State = AnimationState.Stopped;
				return;
			}

			int targetIndex = nextIndex % spriteCount;
			// ループ時の完了コールバック
			if (_isLoop && targetIndex == 0)
			{
				OnAnimationComplete?.Invoke();
			}
			SetCurrentSprite(targetIndex);
		}

		void UpdateFrame()
		{
			if (!CanUpdateAnimation())
			{
				return;
			}

			_currentTime += Time.deltaTime;

			int spriteCount = _currentAnimationData.Sprites.Length;
			int rawTargetIndex = Mathf.FloorToInt(_currentTime / _timePerSprite);

			if (_isLoop)
			{
				// ループ再生
				int targetSpriteIndex = rawTargetIndex % spriteCount;
				if (targetSpriteIndex != _currentSpriteIndex)
				{
					// ループ完了タイミングでコールバック呼び出し（毎ループ）
					if (targetSpriteIndex == 0 && _currentSpriteIndex != 0)
					{
						OnAnimationComplete?.Invoke();
					}
					SetCurrentSprite(targetSpriteIndex);
				}
			}
			else
			{
				// 非ループ再生
				if (rawTargetIndex >= spriteCount)
				{
					// アニメーション完了
					if (!_hasCompletedOnce)
					{
						_hasCompletedOnce = true;
						OnAnimationComplete?.Invoke();
					}
					State = AnimationState.Stopped;
					return;
				}

				if (rawTargetIndex != _currentSpriteIndex)
				{
					SetCurrentSprite(rawTargetIndex);
				}
			}
		}

		bool CanUpdateAnimation()
		{
			return State == AnimationState.Playing && _currentAnimationData != null;
		}

		bool IsValidAnimationIndex(int index)
		{
			return index >= 0 && index < _animationDatas.Length;
		}

		bool IsValidSpriteIndex(int spriteIndex)
		{
			return _currentAnimationData?.Sprites != null &&
			       spriteIndex >= 0 &&
			       spriteIndex < _currentAnimationData.Sprites.Length;
		}

		void SetCurrentAnimation(int index)
		{
			// Play()で既にバリデーション済みのため、チェック不要
			_currentAnimationData = _animationDatas[index];
			_currentSpriteIndex = 0;
			_currentTime = 0f;
			_hasCompletedOnce = false;
			SetCurrentSprite(0);
		}

		void SetCurrentSprite(int spriteIndex)
		{
			if (!IsValidSpriteIndex(spriteIndex))
			{
				Debug.LogError($"SetCurrentSprite: 無効なスプライトインデックス {spriteIndex}");
				return;
			}

			_currentSpriteIndex = spriteIndex;
			_spriteRenderer.sprite = _currentAnimationData.Sprites[spriteIndex];
		}
	}
}