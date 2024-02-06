using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchievementItem : MonoBehaviour
{
    [SerializeField]private Image iconImg;
    private AchievementSceneManager manager;
    private int itemID;
    
    // Start is called before the first frame update
    void Start()
    {
        manager = AchievementSceneManager.instance;
        itemID = transform.GetSiblingIndex();
    }

    /// <summary>
    /// アイコンを設定・このObj生成時にmanagerから呼び出される
    /// </summary>
    /// <param name="icon">設定するアイコン</param>
    public void SetItem(Sprite icon)
    {
        iconImg.sprite = icon;
    }

    /// <summary>
    /// マウスオーバーした時の処理
    /// </summary>
    public void PointerEnter()
    {
        manager.MoveCursor(itemID);
    }
}
