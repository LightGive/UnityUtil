using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace LightGive.UnityUtil.Editor
{
	public class NameCreator
	{
		[MenuItem("Tools/LightGive/Create Name", true)]
		static bool Validate()
		{
			return (EditorApplication.isPlaying || Application.isPlaying) == false;
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
			string projectPath = System.IO.Path.GetFullPath("Assets").Replace("\\", "/");
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
				CreateClass("Scene", EditorBuildSettings.scenes.Where(scene => scene.enabled).Select<EditorBuildSettingsScene, string>(scene => Path.GetFileNameWithoutExtension(scene.path)).ToArray(), relativePath);
				CreateClass("Tag", UnityEditorInternal.InternalEditorUtility.tags, relativePath);
				CreateClass("Layer", GetLayerNames(), relativePath);
			}
			AssetDatabase.StopAssetEditing();
			EditorUtility.UnloadUnusedAssetsImmediate();
			AssetDatabase.Refresh(ImportAssetOptions.ImportRecursive);

			Debug.Log($"SceneName、TagName、LayerNameクラスを{relativePath}に生成しました。");
		}

		static string[] GetLayerNames()
		{
			var layerNames = new string[32];
			for (var i = 0; i < 32; i++)
			{
				layerNames[i] = LayerMask.LayerToName(i);
			}
			return layerNames.Where(name => !string.IsNullOrEmpty(name)).ToArray();
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
			builder = AppendClassText(builder, className, names);
			var text = builder.ToString().Replace(",}", "}");

			//改行コードを統一
			var eol = System.Environment.NewLine;
			text = text.Replace("\r\n", eol).Replace("\r", eol).Replace("\n", eol);

			//ディレクトリがあるか
			if (!System.IO.Directory.Exists(folderPath))
			{
				//ディレクトリ生成
				Directory.CreateDirectory(folderPath);
			}

			var filePath = Path.Combine(folderPath, $"{className}Name.cs");

			//ファイル生成
			File.WriteAllText(filePath, text);
		}

		static StringBuilder AppendClassText(StringBuilder builder, string className, string[] names)
		{
			builder.Append("public class " + className + "Name\n");
			builder.Append("{\n");
			{
				AppendPropertyText(builder, names);
				AppendArrayText(builder, names);
			}
			builder.Append("}");
			return builder;
		}

		static void AppendPropertyText(StringBuilder builder, System.Collections.Generic.IEnumerable<string> names)
		{
			var _names = names.Distinct().ToArray();
			for (int i = 0; i < _names.Length; i++)
			{
				var name = _names[i];
				if (string.IsNullOrEmpty(name))
					continue;

				builder.Append($"    /// <summary>\n");
				builder.Append($"    /// return \"{name}\"\n");
				builder.Append($"    /// </summary>\n");
				builder.Append($"    public const string @{Replace(name)} = \"{name}\";\n");

				// 最後のプロパティでない場合のみ空行を追加
				if (i < _names.Length - 1)
				{
					builder.Append("\n");
				}
			}
			// 配列の前に空行を追加
			builder.Append("\n");
		}

		static void AppendArrayText(StringBuilder builder, System.Collections.Generic.IList<string> names)
		{
			builder.Append("    /// <summary>\n");

			for (var i = 0; i < names.Count; i++)
			{
				builder.Append($"    /// <para>{i}. \"{names[i]}\"</para>\n");
			}

			builder.Append("    /// </summary>\n");
			builder.Append("    public static readonly string[] names = new string[] { ");

			for (var i = 0; i < names.Count; i++)
			{
				builder.Append($"\"{names[i]}\"");
				if (i < names.Count - 1)
					builder.Append(", ");
			}

			builder.Append(" };\n");
		}

		static string Replace(string name)
		{
			string[] invalidChars = {
			" ",
			"!",
			"\"",
			"#",
			"$",
			"%",
			"&",
			"\'",
			"(",
			")",
			"-",
			"=",
			"^",
			"~",
			"¥",
			"|",
			"[",
			"{",
			"@",
			"`",
			"]",
			"}",
			":",
			"*",
			";",
			"+",
			"/",
			"?",
			".",
			">",
			",",
			"<"
		};

			name = invalidChars.Aggregate(name, (current, t) => current.Replace(t, string.Empty));

			if (char.IsNumber(name[0]))
			{
				name = "_" + name;
			}

			return name;
		}
	}
}