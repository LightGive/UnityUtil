using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LightGive.UnityUtil
{
	public class SpritePreviewAttribute : PropertyAttribute
	{
		public float previewHeight;

		public SpritePreviewAttribute(float previewHeight = 32f)
		{
			this.previewHeight = previewHeight;
		}
	}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(SpritePreviewAttribute))]
	public class SpritePreviewDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			SpritePreviewAttribute preview = (SpritePreviewAttribute)attribute;
			return EditorGUIUtility.singleLineHeight + preview.previewHeight + 4f;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var preview = (SpritePreviewAttribute)attribute;

			// SpriteField
			var fieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
			EditorGUI.PropertyField(fieldRect, property, label);
			// Preview
			var previewRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2f, position.width, preview.previewHeight);
			var sprite = (Sprite)property.objectReferenceValue;
			if (sprite != null)
			{
				Texture2D texture = AssetPreview.GetAssetPreview(sprite);
				if (texture != null)
				{
					// プレビュー表示
					GUI.DrawTexture(previewRect, texture, ScaleMode.ScaleToFit);
				}
			}
			else
			{
				// SpriteFieldに何も割り当てられていない場合のメッセージ表示
				EditorGUI.HelpBox(previewRect, "No Sprite Assigned", MessageType.Info);
			}
		}
	}
#endif
}