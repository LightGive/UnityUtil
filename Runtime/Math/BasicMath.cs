using UnityEngine;

namespace LightGive.UnityUtil.Runtime
{
	/// <summary>
	/// 基礎的な計算
	/// </summary>
	public class BasicMath
	{
        /// <summary>
        /// ひし形の内外判定
        /// 内角が180度以下である事が前提
        /// </summary>
        /// <returns></returns>
        public static bool PointInOutForSquareInteriorAngleIn180Dgree(Vector3 checkPoint, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            var ab = Vector3.Cross(b - a, b - checkPoint).z;
            var bc = Vector3.Cross(c - b, c - checkPoint).z;
            var cd = Vector3.Cross(d - c, d - checkPoint).z;
            var da = Vector3.Cross(a - d, a - checkPoint).z;
            return
                (ab > 0 && bc > 0 && cd > 0 && da > 0) ||
                (ab < 0 && bc < 0 && cd < 0 && da < 0);
        }
    }
}