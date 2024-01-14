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
}