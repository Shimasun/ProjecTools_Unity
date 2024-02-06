using System.Diagnostics;

public static class devlog
{   
    /// <summary>
    /// Debug.Logの代わり・コンパイラがメソッドの呼び出しをスキップするようにビルドしてくれる
    /// https://qiita.com/toRisouP/items/d856d65dcc44916c487d
    /// </summary>
    /// <param name="something">Logに表示させる何か</param>
    [Conditional("DEBUG")]
    public static void log(object something)
    {
        UnityEngine.Debug.Log(something);
    }

    /// <summary>
    /// Debug.LogWarningの代わり・コンパイラがメソッドの呼び出しをスキップするようにビルドしてくれる
    /// </summary>
    /// /// <param name="something">Logに表示させる何か</param>
    [Conditional("DEBUG")]
    public static void logWarning(object something)
    {
        UnityEngine.Debug.LogWarning(something);
    }

    /// <summary>
    /// Debug.LogErrorの代わり・コンパイラがメソッドの呼び出しをスキップするようにビルドしてくれる
    /// </summary>
    /// /// <param name="something">Logに表示させる何か</param>
    [Conditional("DEBUG")]
    public static void logError(object something)
    {
        UnityEngine.Debug.LogError(something);
    }

    [Conditional("DEBUG")]
    public static void logException(System.Exception something)
    {
        UnityEngine.Debug.LogException(something);
    }
}
