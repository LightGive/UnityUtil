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

		Type[] _inheritedTypes;
		string[] _typePopupNames;
		string[] _typeFullNames;
		List<string> _typePopupNamesList;

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			if (property.propertyType != SerializedPropertyType.ManagedReference)
			{
				return new PropertyField(property);
			}

			Initialize(property);
			var currentTypeIndex = GetCurrentTypeIndex(property.managedReferenceFullTypename);

			// インデックスが範囲外の場合は0にセット
			if (currentTypeIndex < 0 || currentTypeIndex >= _typePopupNames.Length)
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
			var dropdown = new DropdownField(_typePopupNamesList, currentTypeIndex);
			dropdown.style.width = 150;
			dropdown.RegisterValueChangedCallback(evt =>
			{
				var selectedIndex = _typePopupNamesList.IndexOf(evt.newValue);
				if (selectedIndex >= 0)
				{
					UpdatePropertyToSelectedTypeIndex(property, selectedIndex);
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
					var childField = new PropertyField(copy);
					childField.Bind(property.serializedObject);
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
		/// プロパティドロワーの初期化処理
		/// 継承型の取得と表示名配列の生成を行う
		/// </summary>
		/// <param name="property">対象のシリアライズされたプロパティ</param>
		private void Initialize(SerializedProperty property)
		{
			var baseType = GetType(property);
			if (baseType == null)
			{
				// 型を取得できない場合はエラーログを出力済みなので、空の配列で初期化
				_inheritedTypes = new Type[] { null };
				GetInheritedTypeNameArrays();
				return;
			}

			GetAllInheritedTypes(baseType);
			GetInheritedTypeNameArrays();
		}

		/// <summary>
		/// 現在選択されている型のインデックスを取得
		/// </summary>
		/// <param name="typeFullName">型の完全名</param>
		/// <returns>型のインデックス</returns>
		private int GetCurrentTypeIndex(string typeFullName)
		{
			return Array.IndexOf(_typeFullNames, typeFullName);
		}

		/// <summary>
		/// 指定された基底型から継承された全ての型を取得
		/// abstractクラスとUnityEngine.Object派生クラスは自動的に除外する（キャッシュ機能付き）
		/// </summary>
		/// <param name="baseType">基底型</param>
		void GetAllInheritedTypes(Type baseType)
		{
			// キャッシュから取得を試行
			if (_typeCache.TryGetValue(baseType, out var cachedTypes))
			{
				_inheritedTypes = cachedTypes;
				return;
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
			_inheritedTypes = foundTypes;
		}

		/// <summary>
		/// 継承型の表示名配列と完全名配列を生成
		/// </summary>
		private void GetInheritedTypeNameArrays()
		{
			_typePopupNames = _inheritedTypes.Select(type => type == null ? "<null>" : type.ToString()).ToArray();
			_typeFullNames = _inheritedTypes.Select(type => type == null ? "" : $"{type.Assembly.ToString().Split(',')[0]} {type.FullName}").ToArray();

			// ToList()の結果をキャッシュして再利用
			_typePopupNamesList = new List<string>(_typePopupNames);
		}

		/// <summary>
		/// 選択された型インデックスに基づいてプロパティの値を更新
		/// </summary>
		/// <param name="property">更新対象のプロパティ</param>
		/// <param name="selectedTypeIndex">選択された型のインデックス</param>
		private void UpdatePropertyToSelectedTypeIndex(SerializedProperty property, int selectedTypeIndex)
		{
			Type selectedType = _inheritedTypes[selectedTypeIndex];
			property.managedReferenceValue = selectedType == null ? null : Activator.CreateInstance(selectedType);
			property.serializedObject.ApplyModifiedProperties();
		}

		/// <summary>
		/// SerializedProperty から対応する Type を取得
		/// 配列やリストの場合は要素の型を返す
		/// </summary>
		/// <param name="property">型を取得したいプロパティ</param>
		/// <returns>プロパティに対応する型、取得できない場合は null</returns>
		public static Type GetType(SerializedProperty property)
		{
			var fieldTypename = property.managedReferenceFieldTypename;
			if (string.IsNullOrEmpty(fieldTypename))
			{
				Debug.LogError($"managedReferenceFieldTypename が空です: {property.propertyPath}");
				return null;
			}

			var typeNameParts = fieldTypename.Split(' ');
			if (typeNameParts.Length < 2)
			{
				Debug.LogError($"managedReferenceFieldTypename の形式が無効です: {fieldTypename}");
				return null;
			}

			var assemblyName = typeNameParts[0];
			var fullTypeName = typeNameParts[1];

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