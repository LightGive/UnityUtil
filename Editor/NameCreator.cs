using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace LightGive.UnityUtil.Editor
{
	[InitializeOnLoad]
	public class NameCreator
	{
		[MenuItem("Tools/LightGive/Name Creator/Create Name", true)]
		static bool Validate()
		{
			return (EditorApplication.isPlaying || Application.isPlaying) == false;
		}

		[MenuItem("Tools/LightGive/Name Creator/Create Name")]
		static void Build()
		{
			var layerNames = new List<string>();
			var objs = Resources.FindObjectsOfTypeAll<UnityEngine.Object>();
			var sortingLayers = new List<string>();
			var navMeshLayers = new List<string>();

			foreach (var obj in objs)
			{
				switch (obj.name)
				{
					case "TagManager":
						{
							var sortinglayersProperty = new SerializedObject(obj).FindProperty("m_SortingLayers");

							for (var j = 0; j < sortinglayersProperty.arraySize; j++)
							{
								sortingLayers.Add(sortinglayersProperty.GetArrayElementAtIndex(j).FindPropertyRelative("name").stringValue);
							}
						}
						break;

					case "NavMeshLayers":
						{

							var navMeshlayersObject = new SerializedObject(obj);

							for (var j = 0; j < 3; j++)
							{
								navMeshLayers.Add(navMeshlayersObject.FindProperty("Built-in Layer " + j).FindPropertyRelative("name").stringValue);
							}

							for (var j = 0; j < 28; j++)
							{
								navMeshLayers.Add(navMeshlayersObject.FindProperty("User Layer " + j).FindPropertyRelative("name").stringValue);
							}
						}
						break;
				}
			}

			for (var i = 0; i < 32; i++)
			{
				layerNames.Add(LayerMask.LayerToName(i));
			}

			AssetDatabase.StartAssetEditing();
			{
				Build("Tag", InternalEditorUtility.tags);
				Build("Layer", layerNames.ToArray());
				Build("SortingLayer", sortingLayers.ToArray());
				Build("NavMeshLayer", navMeshLayers.ToArray());
				Build("Scene", EditorBuildSettings.scenes.Where(scene => scene.enabled).Select<EditorBuildSettingsScene, string>(scene => Path.GetFileNameWithoutExtension(scene.path)).ToArray());
			}
			AssetDatabase.StopAssetEditing();
			EditorUtility.UnloadUnusedAssetsImmediate();
			AssetDatabase.Refresh(ImportAssetOptions.ImportRecursive);
		}
		static string SaveFolderPath => Path.Combine("Assets", "LightGive", "NameDatas");

		static NameCreator()
		{
			if (EditorApplication.timeSinceStartup < 10)
			{
				Build();
			}
		}

		/// <summary>
		/// 生成する
		/// </summary>
		/// <param name="className">クラス名</param>
		/// <param name="names"></param>
		static void Build(string className, string[] names)
		{
			var builder = new StringBuilder();
			builder = AppendClassText(builder, className, names);
			var text = builder.ToString().Replace(",}", "}");

			//改行コードを統一
			var eol = Environment.NewLine;
			text = text.Replace("\r\n", eol).Replace("\r", eol).Replace("\n", eol);

			//ディレクトリがあるか
			if (!System.IO.Directory.Exists(SaveFolderPath))
			{
				//ディレクトリ生成
				Directory.CreateDirectory(SaveFolderPath);
			}

			var filePath = @$"{SaveFolderPath}/{className}Name.cs";

			//ファイル生成
			File.WriteAllText(filePath, text);
		}

		static StringBuilder AppendClassText(StringBuilder builder, string className, string[] names)
		{
			builder.AppendLine("public class " + className + "Name");
			builder.AppendLine("{");
			{
				AppendPropertyText(builder, names);
				AppendArrayText(builder, names);
			}
			builder.AppendLine("}");
			return builder;
		}

		static void AppendPropertyText(StringBuilder builder, IEnumerable<string> names)
		{
			var _names = names.Distinct().ToArray();
			foreach (var name in _names)
			{
				if (string.IsNullOrEmpty(name))
					continue;

				builder.AppendFormat(@"
					/// <summary>
					/// return ""{0}""
 					/// </summary>
					public const string @{1} = ""{0}"";", name, Replace(name)).AppendLine();
			}
		}

		static void AppendArrayText(StringBuilder builder, IList<string> names)
		{
			builder.Append("\n\t").AppendLine("/// <summary>");

			for (var i = 0; i < names.Count; i++)
			{
				builder.Append("\t").AppendFormat("/// <para>{0}. \"{1}\"</para>", i, names[i]).AppendLine();
			}

			builder.Append("\t").AppendLine("/// </summary>");
			builder.Append("\t").Append("public static readonly string[] names = new string[]{");

			foreach (var name in names)
			{
				builder.AppendFormat(@"""{0}"",", name);
			}

			builder.AppendLine("};");
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