using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    /// <summary>
    /// 実績解放をしたり
    /// </summary>

    public static AchievementManager instance;
    private SaveDataManager save;
    private AudioManager audioM;

    [SerializeField, Header("for GetUI")] private GameObject getBarObj;//実績解放時に表示させるObj
    [SerializeField] private RectTransform canvas;//実績解放UIを表示させるCanvas
    [SerializeField, Range(0f, 10f)] private float UIdisplayTime = 3.0f;//実績解放UIを表示させてから消すまでの時間
    [SerializeField] private AudioClip getSeClip;//実績解放時のSE
    [SerializeField, Header("for DataBase")] private AchievementDB _database;
    public AchievementDB database => _database;
    [SerializeField] private SpriteList _iconList;
    public SpriteList iconList => _iconList;

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
        save = SaveDataManager.instance;
        audioM = AudioManager.instance;
    }

    /// <summary>
    /// 実績を解放する
    /// </summary>
    /// <param name="id">解放したい実績のID</param>
    /// <returns>実績を解放できたらtrue</returns>
    public bool OpenAchievement(int id)
    {
        if (id < _database.list.Count)
        {
            if (!CheckAchievementStatus(id))
            {
                //解放状況の格納
                save.saveData.Achievements[id] = true;
                save.Save(save.saveData);

                //UI表示
                audioM.SE_Play(getSeClip);
                GameObject instObj = Instantiate(getBarObj, canvas);
                instObj.GetComponent<GetAchievementUI>().SetAchievementInfo(_iconList.list[id], _database.list[id].Name, UIdisplayTime);
                devlog.log($"実績解放！ ID:{id}");
                return true;
            }
            else
            {
                devlog.logWarning($"ID {id} の実績は既に解放済です！");
            }
        }
        else
        {
            devlog.logError($"指定したID {id} は実績数の範囲外です！");
        }
        return false;
    }

    /// <summary>
    /// 指定したIDの実績が解放済かどうかをチェックする
    /// </summary>
    /// <param name="id">チェックしたい実績のID</param>
    /// <returns>解放済みであればtrue</returns>
    public bool CheckAchievementStatus(int id)
    {
        return save.saveData.Achievements[id];
    }

    /// <summary>
    /// 実績解放率を算出する
    /// </summary>
    /// <returns>{解放率, 解放数, 全体数}をfloat配列で返す</returns>
    public float[] CalcGetRate()
    {
        int trueNum = save.saveData.Achievements.Count(value => value == true);
        float allNum = (float)database.list.Count();
        float value = (float)trueNum / allNum;
        float[] result = { value, trueNum, allNum };
        return result;
    }
}
