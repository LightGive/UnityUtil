using UnityEngine.UIElements;

namespace LightGive.UnityUtil.Editor
{
	/// <summary>
	/// ２分割されたVisualElementを作成する
	/// </summary>
	[UxmlElement]
	public partial class TwoPaneSplitView : UnityEngine.UIElements.TwoPaneSplitView
	{
		public TwoPaneSplitView()
		{
			RegisterCallback<AttachToPanelEvent>(OnAttached);
		}

		private void OnAttached(AttachToPanelEvent evt)
		{
			while (childCount < 2)
			{
				Add(new VisualElement());
			}
		}
	}
}