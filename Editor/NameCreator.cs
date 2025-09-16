using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace LightGive.UnityUtil.Editor
{
	public class NameCreator
	{
		private const int MaxLayerCount = 32;

		[MenuItem("Tools/LightGive/Create Name", true)]
		static bool Validate()
		{
			return !(EditorApplication.isPlaying || Application.isPlaying);
		}

		[MenuItem("Tools/LightGive/Create Name")]
		static void CreateNameClasses()
		{
			string selectedPath = EditorUtility.OpenFolderPanel("保存先フォルダを選択", "Assets", "");
			if (string.IsNullOrEmpty(selectedPath))
			{
				Debug.Log("フォルダが選択されませんでした。");
				return;
			}

			// Unityプロジェクト内のパスに変換
			string projectPath = Application.dataPath;
			selectedPath = selectedPath.Replace("\\", "/");

			if (!selectedPath.StartsWith(projectPath))
			{
				Debug.Log("Unityプロジェクト内のフォルダを選択してください。");
				return;
			}

			string relativePath = "Assets" + selectedPath.Substring(projectPath.Length);

			AssetDatabase.StartAssetEditing();
			{
				// SceneName, TagName, LayerNameのみ生成
				var enabledScenes = EditorBuildSettings.scenes
					.Where(scene => scene.enabled)
					.Select(scene => Path.GetFileNameWithoutExtension(scene.path))
					.ToArray();
				CreateClass("Scene", enabledScenes, relativePath);
				CreateClass("Tag", UnityEditorInternal.InternalEditorUtility.tags, relativePath);
				CreateClass("Layer", GetLayerNames(), relativePath);
			}
			AssetDatabase.StopAssetEditing();
			AssetDatabase.Refresh();
			AssetDatabase.Refresh(ImportAssetOptions.ImportRecursive);

			Debug.Log($"SceneName、TagName、LayerNameクラスを{relativePath}に生成しました。");
		}

		static string[] GetLayerNames()
		{
			return Enumerable.Range(0, MaxLayerCount)
				.Select(i => LayerMask.LayerToName(i))
				.Where(name => !string.IsNullOrEmpty(name))
				.ToArray();
		}

		/// <summary>
		/// 生成する
		/// </summary>
		/// <param name="className">クラス名</param>
		/// <param name="names">名前配列</param>
		/// <param name="folderPath">保存先フォルダパス</param>
		static void CreateClass(string className, string[] names, string folderPath)
		{
			var builder = new StringBuilder();
			AppendClassText(builder, className, names);
			var text = builder.ToString();

			//ディレクトリ生成
			Directory.CreateDirectory(folderPath);
			var filePath = Path.Combine(folderPath, $"{className}Name.cs");

			//ファイル生成
			try
			{
				File.WriteAllText(filePath, text);
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"ファイル生成に失敗しました: {filePath}\n{ex.Message}");
				throw;
			}
		}

		static void AppendClassText(StringBuilder builder, string className, string[] names)
		{
			var distinctNames = names.Where(name => !string.IsNullOrEmpty(name)).Distinct().ToArray();

			builder.AppendLine($"public class {className}Name");
			builder.AppendLine("{");
			{
				AppendPropertyText(builder, distinctNames);
				AppendArrayText(builder, distinctNames);
			}
			builder.AppendLine("}");
		}

		static void AppendPropertyText(StringBuilder builder, string[] distinctNames)
		{
			for (int i = 0; i < distinctNames.Length; i++)
			{
				var name = distinctNames[i];

				builder.AppendLine("    /// <summary>");
				builder.AppendLine($"    /// \"{name}\" を表す定数です。");
				builder.AppendLine("    /// </summary>");
				builder.AppendLine($"    public const string @{Replace(name)} = \"{name}\";");

				// 最後のプロパティでない場合のみ空行を追加
				if (i < distinctNames.Length - 1)
				{
					builder.AppendLine();
				}
			}
			// 配列の前に空行を追加
			builder.AppendLine();
		}

		static void AppendArrayText(StringBuilder builder, string[] distinctNames)
		{
			builder.AppendLine("    /// <summary>");

			for (var i = 0; i < distinctNames.Length; i++)
			{
				builder.AppendLine($"    /// <para>{i}. \"{distinctNames[i]}\"</para>");
			}

			builder.AppendLine("    /// </summary>");
			builder.AppendLine("    public static readonly string[] names = new string[]");
			builder.AppendLine("    {");
			var formattedNames = string.Join("," + System.Environment.NewLine, distinctNames.Select(name => $"        \"{name}\""));
			builder.AppendLine(formattedNames);
			builder.AppendLine("    };");
		}

		private static readonly HashSet<char> InvalidChars = new HashSet<char>
		{
			' ', '!', '"', '#', '$', '%', '&', '\'', '(', ')', '-', '=', '^', '~', '¥',
			'|', '[', '{', '@', '`', ']', '}', ':', '*', ';', '+', '/', '?', '.', '>', ',', '<'
		};

		static string Replace(string name)
		{
			if (string.IsNullOrEmpty(name))
				return string.Empty;

			var builder = new StringBuilder(name.Length + 1);
			foreach (char c in name)
			{
				if (!InvalidChars.Contains(c))
				{
					builder.Append(c);
				}
			}

			if (builder.Length > 0 && char.IsNumber(builder[0]))
			{
				builder.Insert(0, '_');
			}

			return builder.ToString();
		}
	}
}