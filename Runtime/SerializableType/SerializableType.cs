using UnityEngine;
using System;
using UnityEngine.Assertions;

namespace LightGive.UnityUtil.Runtime
{
	/// <summary>
	/// Unityでシリアライズ可能な型情報を保持するクラス
	/// 型情報を文字列（AssemblyQualifiedName）として保存し、
	/// シリアライズ後に元の型情報を復元する
	/// コンストラクタを使ってインスタンスを作る必要があるのでInspectorでは使用不可
	/// </summary>
	[Serializable]
	public class SerializableType : ISerializationCallbackReceiver
	{
		[field: SerializeField] public string AssemblyQualifiedName { get; private set; }
		public Type Type { get; private set; }

		public SerializableType(Type type)
		{
			Assert.IsNotNull(type);
			Type = type;
			AssemblyQualifiedName = type != null ? type.AssemblyQualifiedName : string.Empty;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			var tmp = (SerializableType)obj;
			if (Type == null || tmp.Type == null)
			{
				return Type == null && tmp.Type == null;
			}
			return Type.Equals(tmp.Type);
		}

		public override int GetHashCode()
		{
			return Type != null ? HashCode.Combine(Type) : 0;
		}

		public void OnBeforeSerialize() { }

		public void OnAfterDeserialize()
		{
			if (!string.IsNullOrEmpty(AssemblyQualifiedName))
			{
				Type = Type.GetType(AssemblyQualifiedName);
			}
		}

		public static bool operator ==(SerializableType a, SerializableType b)
		{
			if (ReferenceEquals(a, null))
			{
				return ReferenceEquals(b, null);
			}
			return a.Equals(b);
		}

		public static bool operator !=(SerializableType a, SerializableType b)
		{
			return !(a == b);
		}
	}
}