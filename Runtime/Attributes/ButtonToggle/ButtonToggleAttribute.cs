using UnityEngine;

namespace LightGive.UnityUtil
{
    /// <summary>
    /// boolのフィールドをラベル＋トグルボタンのような表示にする
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field)]
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