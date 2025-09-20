using System;
using UnityEngine;

#if UNITY_EDITOR
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace LightGive.UnityUtil.Runtime
{
	/// <summary>
	/// https://github.com/baba-s/Unity-SerializeReferenceExtensions
	/// 上記URLで公開されている Unity SerializeReferenceExtensionsの
	/// レイアウトをVisualElementのPropertyDrawerで作成し直して調整したもの
	/// SerializeReferenceフィールドでサブクラスを選択できるドロップダウンを表示する
	///
	/// ⚠️ 注意: UnityEngine.Object派生クラス（MonoBehaviour、ScriptableObject等）は
	/// SerializeReferenceの制限により自動的に除外されます
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class SubclassSelectorAttribute : PropertyAttribute
	{
	}

#if UNITY_EDITOR && UNITY_2019_3_OR_NEWER
	/// <summary>
	/// SubclassSelectorAttribute のカスタムプロパティドロワー
	/// インスペクターでサブクラス選択ドロップダウンを描画する
	/// </summary>
	[CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
	public class SubclassSelectorDrawer : PropertyDrawer
	{
		/// <summary>
		/// 型キャッシュ: 基底型をキーとして継承型配列を保存
		/// </summary>
		static readonly Dictionary<Type, Type[]> _typeCache = new Dictionary<Type, Type[]>();
		static readonly Type UnityObjectType = typeof(UnityEngine.Object);

		/// <summary>
		/// 型キャッシュをクリアする（開発時のアセンブリ再読み込み用）
		/// </summary>
		public static void ClearTypeCache()
		{
			_typeCache.Clear();
		}


		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			if (property.propertyType != SerializedPropertyType.ManagedReference)
			{
				return new PropertyField(property);
			}

			// 型情報をローカル変数として取得
			var baseType = GetType(property);
			if (baseType == null)
			{
				// 型を取得できない場合はエラーログを出力済みなので、デフォルトのPropertyFieldを返す
				return new PropertyField(property);
			}

			var inheritedTypes = GetAllInheritedTypes(baseType);
			var typePopupNames = inheritedTypes.Select(type => type == null ? "<null>" : type.ToString()).ToArray();
			var typeFullNames = inheritedTypes.Select(type => type == null ? "" : $"{type.Assembly.ToString().Split(',')[0]} {type.FullName}").ToArray();
			var typePopupNamesList = new List<string>(typePopupNames);

			var currentTypeIndex = Array.IndexOf(typeFullNames, property.managedReferenceFullTypename);

			// インデックスが範囲外の場合は0にセット
			if (currentTypeIndex < 0 || currentTypeIndex >= typePopupNames.Length)
			{
				currentTypeIndex = 0;
			}

			var container = new VisualElement();

			// ヘッダー行を作成
			var headerRow = new VisualElement();
			headerRow.style.flexDirection = FlexDirection.Row;
			headerRow.style.alignItems = Align.Center;
			headerRow.style.minHeight = 18;

			// 変数名のラベル
			var label = new Label(property.displayName);
			label.style.flexGrow = 1;
			label.style.marginRight = 5;
			headerRow.Add(label);

			// ドロップダウンを追加
			var dropdown = new DropdownField(typePopupNamesList, currentTypeIndex);
			dropdown.style.width = 150;
			dropdown.RegisterValueChangedCallback(evt =>
			{
				var selectedIndex = typePopupNamesList.IndexOf(evt.newValue);
				if (selectedIndex >= 0)
				{
					// 型更新ロジックを直接記述
					var selectedType = inheritedTypes[selectedIndex];
					property.managedReferenceValue = selectedType == null ? null : Activator.CreateInstance(selectedType);
					property.serializedObject.ApplyModifiedProperties();
				}
			});
			headerRow.Add(dropdown);
			container.Add(headerRow);

			// 子要素を常に表示
			var hasChildren = property.hasVisibleChildren;
			if (hasChildren)
			{
				var childrenContainer = new VisualElement();
				childrenContainer.style.marginLeft = 15;

				var copy = property.Copy();
				var endProperty = copy.GetEndProperty();
				copy.NextVisible(true);
				while (!SerializedProperty.EqualContents(copy, endProperty))
				{
					var childField = new PropertyField(copy.Copy());
					childrenContainer.Add(childField);
					if (!copy.NextVisible(false))
					{
						break;
					}
				}

				container.Add(childrenContainer);
			}

			return container;
		}

		/// <summary>
		/// 指定された基底型から継承された全ての型を取得
		/// abstractクラスとUnityEngine.Object派生クラスは自動的に除外する（キャッシュ機能付き）
		/// </summary>
		/// <param name="baseType">基底型</param>
		/// <returns>継承型の配列</returns>
		static Type[] GetAllInheritedTypes(Type baseType)
		{
			// キャッシュから取得を試行
			if (_typeCache.TryGetValue(baseType, out var cachedTypes))
			{
				return cachedTypes;
			}

			// キャッシュにない場合はアセンブリスキャンを実行
#if UNITY_2020_1_OR_NEWER
			var foundTypes = TypeCache.GetTypesDerivedFrom(baseType)
				.Where(p => p.IsClass &&
					!p.IsAbstract &&
					// UnityEngine.Object派生クラスを常に除外
					!UnityObjectType.IsAssignableFrom(p))
				.Prepend(null)
				.ToArray();
#else
			var foundTypes = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(assembly =>
				{
					try
					{
						// アセンブリの型取得で例外が発生する場合があるため try-catch で保護
						return assembly.GetTypes();
					}
					catch (ReflectionTypeLoadException ex)
					{
						// 一部の型が読み込めない場合は読み込めた型のみを返す
						return ex.Types.Where(t => t != null);
					}
					catch
					{
						// その他の例外の場合は空配列を返す
						return new Type[0];
					}
				})
				.Where(p => p != null &&
					baseType.IsAssignableFrom(p) &&
					p.IsClass &&
					!p.IsAbstract &&
					// UnityEngine.Object派生クラスを常に除外
					!UnityObjectType.IsAssignableFrom(p))
				.Prepend(null)
				.ToArray();
#endif

			// キャッシュに保存
			_typeCache[baseType] = foundTypes;
			return foundTypes;
		}

		/// <summary>
		/// SerializedProperty から対応する Type を取得
		/// 配列やリストの場合は要素の型を返す
		/// </summary>
		/// <param name="property">型を取得したいプロパティ</param>
		/// <returns>プロパティに対応する型、取得できない場合は null</returns>
		public static Type GetType(SerializedProperty property)
		{
			var managedTypename = property.managedReferenceFieldTypename;
			if (string.IsNullOrEmpty(managedTypename))
			{
				Debug.LogError($"managedReferenceFieldTypename が空です: {property.propertyPath}");
				return null;
			}

			var splitIndex = managedTypename.IndexOf(' ');
			if (splitIndex <= 0)
			{
				Debug.LogError($"managedReferenceFieldTypename の形式が無効です: {managedTypename}");
				return null;
			}

			var assemblyName = managedTypename.Substring(0, splitIndex);
			var fullTypeName = managedTypename.Substring(splitIndex + 1);

			var assembly = AppDomain.CurrentDomain.GetAssemblies()
				.FirstOrDefault(a => a.GetName().Name == assemblyName);

			if (assembly == null)
			{
				Debug.LogError($"アセンブリが見つかりません: {assemblyName}");
				return null;
			}

			var type = assembly.GetType(fullTypeName);
			if (type == null)
			{
				Debug.LogError($"型が見つかりません: {fullTypeName} (アセンブリ: {assemblyName})");
				return null;
			}

			return type;
		}
	}
#endif
}