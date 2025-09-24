namespace LightGive.UnityUtil.Runtime
{
    /// <summary>
    /// アニメーションの更新モードを定義するEnum
    /// </summary>
    public enum UpdateMode
    {
        /// <summary>
        /// 自動更新モード（Update内で自動的にアニメーションフレームを更新）
        /// </summary>
        NormalUpdate,

        /// <summary>
        /// 手動更新モード（UpdateFrameManualメソッドを呼び出してフレームを更新）
        /// </summary>
        ManualUpdate
    }
}