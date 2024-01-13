using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LightGive.UnityUtil
{
    public class EnumListLabelAttribute : PropertyAttribute
    {
        public System.Type EnumType = null;
        public EnumListLabelAttribute(System.Type enumType) => EnumType = enumType;
    }
}

#if UNITY_EDITOR
namespace LightGive.UnityUtil.Editor
{
    [CustomPropertyDrawer(typeof(EnumListLabelAttribute))]
    public class EnumListLabelDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            int index = GetElementIndex(property);
            string enumName = GetEnumName(index);
            if (!string.IsNullOrEmpty(enumName))
            {
                label.text = enumName;
            }
            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property);
        }

        private new EnumListLabelAttribute attribute
        {
            get
            {
                return (EnumListLabelAttribute)base.attribute;
            }
        }

        private int GetElementIndex(SerializedProperty i_property)
        {
            string propertyPath = i_property.propertyPath;
            var propertys = propertyPath.Split('.');
            if (propertys.Length < 3)
            {
                return -1;
            }

            string arrayProperty = propertys[propertys.Length - 2];
            if (arrayProperty != "Array")
            {
                return -1;
            }

            var paths = propertyPath.Split('.');
            var lastPath = paths[propertys.Length - 1];
            if (!lastPath.StartsWith("data["))
            {
                return -1;
            }

            var regex = new System.Text.RegularExpressions.Regex(@"[^0-9]");
            var countText = regex.Replace(lastPath, "");
            if (!int.TryParse(countText, out var index))
            {
                return -1;
            }

            return index;
        }

        private string GetEnumName(int i_index)
        {
            if (i_index < 0)
            {
                return null;
            }

            if (System.Enum.IsDefined(attribute.EnumType, i_index))
            {
                return System.Enum.GetName(attribute.EnumType, i_index);
            }
            return null;
        }
    }
}
#endif