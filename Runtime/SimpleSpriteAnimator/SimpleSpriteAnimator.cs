using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace LightGive.UnityUtil.Runtime
{
	/// <summary>
	/// スプライトを使用したシンプルなアニメーション
	/// </summary>
	public class SimpleSpriteAnimator : MonoBehaviour
	{
		const int InvalidAnimationIndex = -1;

		[SerializeField] SpriteRenderer _spriteRenderer;
		[SerializeField] SimpleSpriteAnimationData[] _animationDatas;
		[SerializeField] UpdateMode _updateMode;
		[SerializeField] int _fps = 60;
		int _currentAnimationIndex = InvalidAnimationIndex;
		int _currentSpriteIndex;
		float _currentTime;
		float _timePerSprite;
		SimpleSpriteAnimationData _currentAnimationData;

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

		/// <summary>
		/// 指定されたインデックスのアニメーションを再生開始する
		/// </summary>
		/// <param name="index">再生するアニメーションのインデックス（デフォルト: 0）</param>
		public void Play(int index = 0)
		{
			if (!IsValidAnimationIndex(index))
			{
				Debug.LogWarning($"無効なアニメーションインデックス: {index}");
				return;
			}

			// 新しいアニメーションに変更する場合のみリセット
			if (_currentAnimationIndex != index)
			{
				SetCurrentAnimation(index);
			}

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
			if (!IsPlaying || IsPaused)
			{
				return;
			}
			State = AnimationState.Paused;
		}

		/// <summary>
		/// アニメーションを停止し、状態をリセットする
		/// </summary>
		public void Stop()
		{
			State = AnimationState.Stopped;
			_currentAnimationIndex = InvalidAnimationIndex;
			_currentSpriteIndex = 0;
			_currentTime = 0f;
			_currentAnimationData = null;
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

			var nextIndex = (_currentSpriteIndex + 1) % _currentAnimationData.Sprites.Length;
			SetCurrentSprite(nextIndex);
		}

		void UpdateFrame()
		{
			if (!CanUpdateAnimation())
			{
				return;
			}

			_currentTime += Time.deltaTime;

			int targetSpriteIndex = Mathf.FloorToInt(_currentTime / _timePerSprite) % _currentAnimationData.Sprites.Length;

			if (targetSpriteIndex != _currentSpriteIndex)
			{
				SetCurrentSprite(targetSpriteIndex);
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
			if (!IsValidAnimationIndex(index))
			{
				Debug.LogError($"SetCurrentAnimation: 無効なインデックス {index}");
				return;
			}

			_currentAnimationIndex = index;
			_currentAnimationData = _animationDatas[index];
			_currentSpriteIndex = 0;
			_currentTime = 0f;
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