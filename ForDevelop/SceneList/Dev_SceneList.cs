#if DEBUG
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// （開発用）ボタン発信で任意のシーンへ移動させる
/// </summary>
public class Dev_SceneList : MonoBehaviour
{
    [SerializeField] private GameObject goButtonObj;
    [SerializeField] private RectTransform contentParentRect;
    [SerializeField] private StringList listObj;//SceneListのScriptableObj

    void Start()
    {
        foreach (string sceneName in listObj.list)
        {
            RectTransform buttonRect = Instantiate(goButtonObj, contentParentRect).GetComponent<RectTransform>();
            TextMeshProUGUI tmp = buttonRect.GetChild(0).GetComponent<TextMeshProUGUI>();
            tmp.text = sceneName;
        }
    }

    /// <summary>
    /// 即時シーン移動させる
    /// </summary>
    /// <param name="sceneName">移動先のシーン名</param>
    public void Dev_GoScene(string sceneName)
    {
        Debug.Log($"(Dev)シーン「{sceneName}」へ強制移動します");
        SceneManager.LoadScene(sceneName);
    }
}
#endif