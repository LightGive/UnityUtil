using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LightGive.UnityUtil.Runtime
{
	/// <summary>
	/// boolのフィールドをラベル＋トグルボタンのような表示にする
	/// </summary>
	[System.AttributeUsage(System.AttributeTargets.Field)]
	public class ButtonToggleAttribute : PropertyAttribute
	{
		const string DefaultLabelTrue = "true";
		const string DefaultLabelFalse = "false";
		[SerializeField] public readonly string Label = "";
		[SerializeField] public readonly string LabelTrue = DefaultLabelTrue;
		[SerializeField] public readonly string LabelFalse = DefaultLabelFalse;

		public ButtonToggleAttribute(string label)
		{
			Label = label;
			LabelTrue = DefaultLabelTrue;
			LabelFalse = DefaultLabelFalse;
		}

		public ButtonToggleAttribute(string labelTrue, string labelFalse)
		{
			Label = "";
			LabelTrue = labelTrue;
			LabelFalse = labelFalse;
		}

		public ButtonToggleAttribute(string label, string labelTrue, string labelFalse)
		{
			Label = label;
			LabelTrue = labelTrue;
			LabelFalse = labelFalse;
		}
	}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(ButtonToggleAttribute))]
	public class ButtonToggleDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var att = (ButtonToggleAttribute)attribute;

			if (!string.IsNullOrEmpty(att.Label))
			{
				var labelRect = new Rect(position)
				{
					width = position.width - EditorGUIUtility.fieldWidth
				};
				GUI.Label(labelRect, att.Label);

				position = new Rect(position)
				{
					x = position.x + EditorGUIUtility.labelWidth,
					width = position.width - EditorGUIUtility.labelWidth
				};
			}

			EditorGUI.BeginChangeCheck();
			property.boolValue = GUI.Toggle(
				position,
				property.boolValue,
				property.boolValue ? att.LabelTrue : att.LabelFalse,
				GUI.skin.button);

			if (EditorGUI.EndChangeCheck())
			{
				property.serializedObject.ApplyModifiedProperties();
			}
		}
	}
#endif
}