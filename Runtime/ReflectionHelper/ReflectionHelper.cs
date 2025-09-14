using System;
using System.Linq;

namespace LightGive.UnityUtil.Runtime
{
	public static class ReflectionHelper
    {
        /// <summary>
        /// 指定したインターフェースを実装している値型（構造体、列挙型）を取得する
        /// </summary>
        /// <param name="interfaceType">対象のインターフェースType</param>
        /// <returns>インターフェースを実装しているクラスのType配列</returns>
        public static Type[] GetValueTypeImplementingInterface(Type interfaceType)
        {
            if (!interfaceType.IsInterface)
            {
                throw new ArgumentException($"{interfaceType.FullName} はインターフェースではありません。");
            }

            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => t.IsValueType && interfaceType.IsAssignableFrom(t))
                .ToArray();
            return types;
        }

        /// <summary>
        /// 指定したインターフェースを実装しているクラスを取得する
        /// </summary>
        /// <param name="interfaceType">対象のインターフェースType</param>
        /// <returns>インターフェースを実装しているクラスのType配列</returns>
        public static Type[] GetClassesImplementingInterface(Type interfaceType)
        {
            if (!interfaceType.IsInterface)
            {
                throw new ArgumentException($"{interfaceType.FullName} はインターフェースではありません。");
            }

            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract && interfaceType.IsAssignableFrom(t))
                .ToArray();
            return types;
        }

        /// <summary>
        /// 指定したクラスを継承しているクラスのTypeを取得する
        /// </summary>
        /// <param name="baseType">基底クラスのType</param>
        /// <returns>基底クラスを継承しているクラスのType配列</returns>
        public static Type[] GetDerivedClasses(Type baseType)
        {
            if (!baseType.IsClass)
            {
                throw new ArgumentException($"{baseType.FullName} はクラスではありません。");
            }

            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(baseType))
                .ToArray();

            return types;
        }
    }
}