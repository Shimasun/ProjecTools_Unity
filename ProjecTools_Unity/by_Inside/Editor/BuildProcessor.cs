using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    /// <summary>
    /// ビルド前後に入る処理
    /// </summary>
    
    // 実行される順番
    public int callbackOrder => 0;

    /// <summary>
    /// ビルド前処理
    /// </summary>
    /// <param name="report"></param>
    public void OnPreprocessBuild(BuildReport report)
    {
        //SceneListを作成する
        //CreateSceneList csl = new CreateSceneList();
        //CreateScriptableObject();
    }

    /// <summary>
    /// ビルド後処理
    /// </summary>
    /// <param name="report"></param>
    public void OnPostprocessBuild(BuildReport report)
    {

    }
}
