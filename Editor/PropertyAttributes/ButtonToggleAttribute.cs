using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LightGive.UnityUtil
{
    /// <summary>
    /// boolのフィールドをラベル＋トグルボタンのような表示にする
    /// </summary>
    public class ButtonToggleAttribute : PropertyAttribute
    {
        const string DefaultLabelTrue = "true";
        const string DefaultLabelFalse = "false";
        public readonly string Label = "";
        public readonly string LabelTrue = DefaultLabelTrue;
        public readonly string LabelFalse = DefaultLabelFalse;
        public ButtonToggleAttribute(string label)
        {
            new ButtonToggleAttribute(label, DefaultLabelTrue, DefaultLabelFalse);
        }
        public ButtonToggleAttribute(string label, string labelTrue, string labelFalse)
        {
            Label = label;
            LabelTrue = labelTrue;
            LabelFalse = labelFalse;
        }
    }
}

#if UNITY_EDITOR
namespace LightGive.UnityUtil.Editor
{
    [CustomPropertyDrawer(typeof(ButtonToggleAttribute))]
    public class ButtonToggleDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var att = (ButtonToggleAttribute)attribute;
            var labelRect = new Rect(position)
            {
                width = position.width - EditorGUIUtility.fieldWidth
            };
            GUI.Label(labelRect, att.Label);

            var buttonRect = new Rect(position)
            {
                x = position.x + EditorGUIUtility.labelWidth,
                width = position.width - EditorGUIUtility.labelWidth
            };
            EditorGUI.BeginChangeCheck();
            property.boolValue = GUI.Toggle(
                buttonRect,
                property.boolValue,
                property.boolValue ? att.LabelTrue : att.LabelFalse,
                GUI.skin.button);

            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
#endif