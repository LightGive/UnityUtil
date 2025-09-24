using UnityEngine;

namespace LightGive.UnityUtil.Runtime
{
    /// <summary>
    /// スプライトアニメーションに使用するスプライト配列を格納するScriptableObject
    /// </summary>
    [CreateAssetMenu(
        fileName = "SimpleSpriteAnimationData",
        menuName = "LightGive/SimpleSpriteAnimationData")]
    public class SimpleSpriteAnimationData : ScriptableObject
    {
        /// <summary>
        /// アニメーションに使用するスプライトの配列
        /// </summary>
        [field: SerializeField] public Sprite[] Sprites { get; private set; }
    }
}