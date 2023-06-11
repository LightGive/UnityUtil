using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LightGive.UnityUtil
{
    public static class GameObjectExtentions
    {
        /// <summary>
        /// 親オブジェクトを再帰的にコンポーネントの参照を見つける
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static T GetComponentInParentRecursive<T>(this GameObject gameObject) where T : Component
        {
            if (gameObject == null) { return null; }
            T component = gameObject.GetComponent<T>();
            if (component != null)
			{
                return component;
            }

            Transform parent = gameObject.transform.parent;
            while (parent != null)
            {
                component = parent.GetComponent<T>();
                if (component != null)
                {
                    return component;
                }
                parent = parent.parent;
            }
            return null;
        }
    }
}