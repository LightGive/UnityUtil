using UnityEngine;

namespace LightGive.UnityUtil.Runtime
{
	/// <summary>
	/// Unity共通で使うシングルトン
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
	{
		static T instance;
		public static T Instance
		{
			get
			{
				if (instance == null)
				{
					instance = (T)FindFirstObjectByType(typeof(T));
					if (instance == null)
					{
						Debug.LogError(typeof(T) + "is nothing");
					}
				}
				return instance;
			}
		}

		[SerializeField] bool _isDontDestroy;

		protected virtual void Awake()
		{
			if (CheckInstance() && _isDontDestroy)
			{
				DontDestroyOnLoad(this.gameObject);
			}
		}

		protected bool CheckInstance()
		{
			if (this == Instance)
			{
				return true;
			}
			Destroy(this);
			Destroy(gameObject);
			return false;
		}
	}
}