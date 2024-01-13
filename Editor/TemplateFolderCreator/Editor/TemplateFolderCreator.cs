using System.IO;
using UnityEngine;
using UnityEditor;

namespace LightGive
{
	public class TemplateFolderCreator
	{
		[MenuItem("Tools/LightGive/Folder Creator/Create Folder")]
		static void CreateTemplateFolder()
		{
			CreateFolder("GameData");
			CreateFolder("Scripts");
			CreateFolder("Prefabs");
			CreateFolder("Sprites");
			CreateFolder("Materials");
			CreateFolder("PhysicsMaterials");
			CreateFolder("Textures");
			CreateFolder("Fonts");
			CreateFolder("Scenes");
		}

		/// <summary>
		/// フォルダを作成する
		/// </summary>
		/// <returns><c></c>, if folder was created, <c>false</c> otherwise.</returns>
		/// <param name="_folderName">作成するフォルダ名</param>
		static bool CreateFolder(string _folderName)
		{
			if (Directory.Exists("Assets/" + _folderName))
			{
				// フォルダが存在する.
				Debug.Log("フォルダが存在するため作りませんでした:" + _folderName);
				return false;
			}

			string guid = AssetDatabase.CreateFolder("Assets", _folderName);
			string newFolderPath = AssetDatabase.GUIDToAssetPath(guid);
			return true;
		}
	}
}