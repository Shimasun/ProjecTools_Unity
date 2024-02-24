using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 実績確認シーンのmanager
/// </summary>
public class AchievementSceneManager : MonoBehaviour
{
    public static AchievementSceneManager instance;
    private AchievementManager achieveM;
    private ControlManager control;
    private SaveDataManager save;
    private SceneChanger SC;
    private AudioManager audioM;

    [SerializeField] private byte nextSceneID;
    [SerializeField] private GameObject itemObj;
    [SerializeField] private Sprite notGetIcon;
    [SerializeField, Header("for System")] private TextMeshProUGUI rateText;
    [SerializeField] private TextMeshProUGUI explainText;
    [SerializeField] private Scrollbar scrollbar;
    [SerializeField] private RectTransform itemParentRect;
    [SerializeField] private RectTransform cursorRect;

    private bool getOperate;
    private int selectingID;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        control = ControlManager.instance;
        save = SaveDataManager.instance;
        SC = SceneChanger.instance;
        audioM = AudioManager.instance;
        achieveM = AchievementManager.instance;

        //リスト初期化
        InitializeItems();
        MoveCursor(0, false);
        //解放率
        float[] temp = achieveM.CalcGetRate();
        temp[0] *= 100f;
        rateText.text = $"{temp[1].ToString("00")} / {temp[2].ToString("00")}\n{temp[0].ToString("00.0")} %";
        getOperate = true;
    }

    void Update()
    {
        if (getOperate)
        {
            if (control.get_cancel(ControlManager.Operate.OnFrame))
            {
                getOperate = false;
                SC.SceneChange((sbyte)nextSceneID);
            }
        }
    }

    /// <summary>
    /// 実績アイテムObjを並べる
    /// </summary>
    /// <returns>終了したらtrue</returns>
    private bool InitializeItems()
    {
        for (int i = 0; i < achieveM.database.list.Count; i++)
        {
            //Obj生成
            GameObject instObj = Instantiate(itemObj, itemParentRect);

            //アイコン仮置き
            Sprite temp = notGetIcon;
            //解放済だったら正規のアイコンに置き換え
            if (save.saveData.Achievements[i])
            {
                temp = achieveM.iconList.list[i];
            }
            //アイコン設定
            instObj.GetComponent<AchievementItem>().SetItem(temp);
        }
        scrollbar.value = 1f;
        return true;
    }

    /// <summary>
    /// カーソルを動かす
    /// </summary>
    /// <param name="id">移動先のItemのID</param>
    /// <param name="playSE">移動時のSEを鳴らすか</param>
    public void MoveCursor(int id, bool playSE = true)
    {
        if (playSE)
        {
            audioM.SE_Play(AudioManager.WhichSE.CursorMove);
        }
        cursorRect.position = itemParentRect.GetChild(id).position;
        WriteExplain(id);
    }

    /// <summary>
    /// 説明文を書き換える
    /// </summary>
    /// <param name="id">表示したい実績のID</param>
    private void WriteExplain(int id)
    {
        AchievementEntity data = achieveM.database.list[id];
        string name = "？？？？？";
        string explain = data.Hint;
        //解放済だったら正規のアイコンに置き換え
        if (save.saveData.Achievements[id])
        {
            name = data.Name;
            explain = data.Explain;
        }
        explainText.text = $"【{name}】\n{explain}";
    }
}
