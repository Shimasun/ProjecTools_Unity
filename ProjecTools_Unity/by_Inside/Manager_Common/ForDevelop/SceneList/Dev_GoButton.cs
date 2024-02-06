#if DEBUG
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Dev_GoButton : MonoBehaviour
{
    private Dev_SceneList DSL;

    void Start()
    {
        DSL = transform.parent.parent.gameObject.GetComponent<Dev_SceneList>();
    }
    public void Dev_ButtonPressed()
    {
        //ボタンに書かれているシーン名を使って遷移する
        TextMeshProUGUI tmp = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        DSL.Dev_GoScene(tmp.text);
    }
}
#endif