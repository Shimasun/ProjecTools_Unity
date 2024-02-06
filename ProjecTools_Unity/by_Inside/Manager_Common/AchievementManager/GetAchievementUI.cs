using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GetAchievementUI : MonoBehaviour
{
    /// <summary>
    /// 実績解放時に表示させるUIについて色々
    /// </summary>
    
    [SerializeField]private Image iconImg;//実績絵Image
    [SerializeField]private TextMeshProUGUI titleText;//実績名TMP
    //private CanvasGroup canvasGroup;
    private Animator animator;
    private float dispTime;//表示時間・カウント用
    private bool startCount;//カウントを開始して良いか

    // Update is called once per frame
    void Update()
    {
        if(startCount)
        {
            if(dispTime <= 0f)
            {
                //指定時間経過したらUI閉じ開始
                StartCoroutine(CloseAchieveUI());
                startCount = false;
            }
            else
            {
                dispTime -= Time.deltaTime;
            }
        }
    }

    /// <summary>
    /// 実績解放UIを表示させる・AchievementManagerから呼び出す
    /// </summary>
    /// <param name="icon">表示させる実績の絵（アイコン）</param>
    /// <param name="title">表示させる実績名</param>
    /// <param name="time">UIの表示時間</param>
    public void SetAchievementInfo(Sprite icon, string title, float time)
    {
        animator = GetComponent<Animator>();
        iconImg.sprite = icon;
        titleText.text = title;
        dispTime = time;
        //canvasGroup.CrossFadeAlpha(1f,);
        animator.SetTrigger("open");
        startCount = true;
    }

    /// <summary>
    /// 実績解放UIを閉じる
    /// </summary>
    private IEnumerator CloseAchieveUI()
    {
        //Animの逆再生
        animator.SetTrigger("close");
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        Destroy(this.gameObject);
    }
}
