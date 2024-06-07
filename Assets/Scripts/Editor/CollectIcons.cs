using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;

public class CollectIcons {

	[MenuItem("Demo/Collect Icons")]
	static void DoCollectIcons() {
		string folder = "Assets/Resources";
		string[] guids = AssetDatabase.FindAssets("t:Texture2D", new string[] { folder });
		FolderData icons = new FolderData();
		foreach (string guid in guids) {
			string path = AssetDatabase.GUIDToAssetPath(guid);
			string subPath = path.Substring(folder.Length + 1);
			path = path.Substring(path.IndexOf("/Resources/") + 11);
			path = path.Substring(0, path.LastIndexOf('.'));
			int index = 0;
			FolderData current = icons;
			while (true) {
				int ni = subPath.IndexOf('/', index);
				if (ni < 0) {
					current.files.Add(path);
					break;
				}
				string dir = subPath.Substring(index, ni - index);
				FolderData sub;
				if (!current.subs.TryGetValue(dir, out sub)) {
					sub = new FolderData();
					current.subs.Add(dir, sub);
				}
				current = sub;
				index = ni + 1;
			}
		}
		StringBuilder code = new StringBuilder();
		Action<string, FolderData, int> func = null;
		func = (string name, FolderData current, int indent) => {
			string tabs = "";
			for (int i = 0; i < indent; i++) { tabs += "\t"; }
			code.Append(tabs);
			code.AppendLine($"public static class {name} {{");
			code.AppendLine();
			foreach (KeyValuePair<string, FolderData> kv in current.subs) {
				func(kv.Key, kv.Value, indent + 1);
				code.AppendLine();
			}
			int files = current.files.Count;
			if (files > 0) {
				code.Append(tabs);
				code.AppendLine("\tpublic static string[] paths = new string[] {");
				current.files.Sort();
				for (int i = 0; i < files; i++) {
					string file = current.files[i];
					code.Append(tabs);
					string comma = i + 1 < files ? "," : "";
					code.AppendLine($"\t\t\"{file}\"{comma}");
				}
				code.Append(tabs);
				code.AppendLine("\t};");
				code.AppendLine();
			}
			code.AppendLine($"{tabs}}}");
		};
		func("TestAtlasIconRes", icons, 0);
		File.WriteAllText("Assets/Scripts/TestAtlasIconRes.cs", code.ToString(), Encoding.UTF8);
	}

	private class FolderData {
		public readonly SortedList<string, FolderData> subs = new SortedList<string, FolderData>();
		public readonly List<string> files = new List<string>();
	}

}
