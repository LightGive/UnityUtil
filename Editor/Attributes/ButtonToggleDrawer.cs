using UnityEngine;
using UnityEditor;
using LightGive.UnityUtil;

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
