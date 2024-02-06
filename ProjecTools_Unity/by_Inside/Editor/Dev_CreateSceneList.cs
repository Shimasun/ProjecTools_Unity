using UnityEngine;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;

public class Dev_CreateSceneList
{
    /// <summary>
    /// （開発用）SceneListのScriptableObjectを作成する。
    /// 参照：https://zenn.dev/blkcatman/articles/e1a735cd88d1e6
    /// </summary>

    [MenuItem("(Dev)/CreateSceneList")]
    public static void CreateScriptableObject()
    {
        var obj = ScriptableObject.CreateInstance<StringList>();
        foreach (var scene in EditorBuildSettings.scenes)
        {
            string path = scene.path;
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            obj.list.Add(name);
        }
        string fileName = $"(DoCopy)Dev_{TypeNameToString(obj.GetType().ToString())}.asset";
        string savePath = "Assets/Editor";  //ScriptableObjの保存先
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }
        AssetDatabase.CreateAsset(obj, Path.Combine(savePath, fileName));
        AssetDatabase.Refresh();
        Debug.LogWarning("SceneListを「Assets/Editor」に作成しました。内容を「Dev_SceneNameList」にコピーしてください！");
    }
    
    private static string TypeNameToString(string type)
    {
        var typeParts = type.Split(new char[] { '.' });
        if (!typeParts.Any())
            return string.Empty;

        var words = Regex.Matches(typeParts.Last(), "(^[a-z]+|[A-Z]+(?![a-z])|[A-Z][a-z]+)")
            .OfType<Match>()
            .Select(match => match.Value)
            .ToArray();
        return string.Join(" ", words);
    }
}
