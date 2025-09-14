using LightGive.UnityUtil.Runtime;
using UnityEngine;
using UnityEngine.Assertions;

namespace LightGive.UnityUtil.Tests
{
	public class SerializableTypeTest
	{
		[NUnit.Framework.Test]
		public void Equals_ShouldReturnTrue()
		{
			{
				var type1 = new SerializableType(typeof(int));
				var type2 = new SerializableType(typeof(int));
				var result = type1.Equals(type2);
				Assert.IsTrue(result);
			}

			{
				var type1 = new SerializableType(typeof(string));
				var type2 = new SerializableType(typeof(string));
				var result = type1.Equals(type2);
				Assert.IsTrue(result);
			}
		}

		[NUnit.Framework.Test]
		public void Equals_ShouldReturnFalse()
		{
			{
				var type1 = new SerializableType(typeof(string));
				var type2 = new SerializableType(typeof(int));
				var result = type1.Equals(type2);
				Assert.IsFalse(result);
			}

			{
				var type1 = new SerializableType(typeof(int));
				var result = type1.Equals(null);
				Assert.IsFalse(result);
			}

			{
				var type1 = new SerializableType(typeof(string));
				var result = type1.Equals(null);
				Assert.IsFalse(result);
			}
		}

		[NUnit.Framework.Test]
		public void SerializeTest()
		{
			// シリアライズ前と後で同じ値か
			{
				// int
				var t = new SerializableType(typeof(int));
				var json = JsonUtility.ToJson(t);
				var deserializedType = JsonUtility.FromJson<SerializableType>(json);
				Assert.IsTrue(t.Equals(deserializedType));
			}

			{
				// string
				var t = new SerializableType(typeof(string));
				var json = JsonUtility.ToJson(t);
				var deserializedType = JsonUtility.FromJson<SerializableType>(json);
				Assert.IsTrue(t.Equals(deserializedType));
			}
		}
	}
}
