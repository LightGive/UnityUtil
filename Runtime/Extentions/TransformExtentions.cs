using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LightGive.UnityUtil
{
    public static class TransformExtentions
    {
        /// <summary>
        /// Z座標はそのままでX,Y座標を設定します
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void SetPosition2D(this Transform transform, float x, float y)
        {
            transform.position = new Vector3(x, y, transform.position.z);
        }

        /// <summary>
        /// Z座標はそのままでX,Y座標を設定します
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="position"></param>
        public static void SetPosition2D(this Transform transform,Vector2 position)
		{
            transform.position = new Vector3(position.x, position.y, transform.position.z);
		}
    }
}