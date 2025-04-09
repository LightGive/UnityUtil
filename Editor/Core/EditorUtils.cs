
namespace StickFiguresFighting.Animation.Editor
{
	/// <summary>
	/// エディタ拡張等で使用するユーティリティクラス
	/// </summary>
	public static class EditorUtils
	{
		/// <summary>
		/// 自動プロパティで生成されるプロパティ名をフィールド名から取得する
		/// </summary>
		/// <param name="fieldName"></param>
		/// <returns></returns>
		public static string ConvertPropFieldName(string fieldName) => $"<{fieldName}>k__BackingField";
	}
}