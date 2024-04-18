using UnityEngine;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System;

//参考
//https://kurokumasoft.com/2022/01/03/unity-savesystem-using-json/
//https://qiita.com/InfiniteGame/items/01da9d83853fecb95132
//https://kan-kikuchi.hatenablog.com/entry/JsonUtility

///<summary>
/// ユーザーデータのセーブとロードを行う
///</summary>
public class SaveDataManager : MonoBehaviour
{
    public static SaveDataManager instance;
    [SerializeField] private string _fileName = "Data";//セーブデータの名前・まあ変更することはないだろう
    public uint PlayTimeLimit { get; } = 3599999999;//プレイ時間・バトル時間は35億9999万9999秒=99万9999時間59分59秒までカウント
    public uint CountLimit { get; } = 999999999;//起動回数は9億9999万9999回までカウント
    public float stickDead { get; private set; } = 0.8f;

    private string _filePath;//セーブデータ保存先のパス
    public SaveData saveData { get; set; }//セーブデータ内の各データを格納・ここから各データを取得する

    public bool Loaded { get; private set; }//セーブデータをロードし終えたか

    private byte[] _list_FPS = { 30, 60, 90, 120 };

    public bool SetBGMStartTime { get; set; }//BGMの再生位置を指定するか

    [SerializeField, Header("for CheckDigit")] private int ForDate_YMod = 2000;//セーブ年のモジュロ演算に使う数
    [SerializeField] private int ForDate_By = 3;//セーブ日の合計にかける数
    [SerializeField] private int ForDigit_Mod = 10000;//チェックディジット用・変数を何で割った余りにするか
    [SerializeField] private int ForDigit_Add = 7;//チェックディジット用・各変数の計算結果に加算する数値

    //UI関連
    [SerializeField, Header("for UI")] private GameObject savingUIObj;//「セーブ中」を示すUIオブジェクト
    [SerializeField] private RectTransform uiCanvasRect;//UIオブジェクトを表示させるCanvas

#if UNITY_EDITOR
    [SerializeField,Header("for Dev")]private bool Dev_Initialize;//起動時に初期化するか
#endif

    ///////////////////////////////////////////////
    ///////////////////////////////////////////////
    ///////////////////////////////////////////////

    /// <summary>
    /// ユーザデータの「セーブ処理」を行う
    /// </summary>
    /// <param name="Save">セーブしたいSaveData変数</param>
    /// <returns>セーブ終了したらtrue</returns>
    public bool Save(SaveData Save)
    {
        devlog.log("ユーザデータの「セーブ処理」を開始します。");

        //「セーブ中」を示すUIが設定されていれば表示させる。
        GameObject instedSavingUI = null;
        if (savingUIObj != null)
        {
            instedSavingUI = Instantiate(savingUIObj, uiCanvasRect);
        }

        CalcSavedDate(Save);//セーブ日を格納
        Save.Check = CalcCheckDigit(Save);//チェックディジットを計算して格納

        string SavedData_Json = JsonUtility.ToJson(Save, true);//セーブデータの各変数をjson化する・「第2引数をtrueにすると読みやすく整形される」らしい
        StreamWriter Writer = new StreamWriter(_filePath);
        Writer.Write(SavedData_Json);
        Writer.Flush();
        Writer.Close();

        //「セーブ中」を示すUIが表示されていたら削除する。
        if (savingUIObj != null)
        {
            Destroy(instedSavingUI);
        }

        devlog.log("セーブしました。内容は以下の通りです。\n" + SavedData_Json);
        return true;
    }

    /// <summary>
    /// 取得したファイルパスにあるユーザデータの「ロード処理」を行う
    /// </summary>
    /// <param name="forced">強制的にロードさせるか</param>
    /// <returns>ロードしたSaveData変数</returns>
    public SaveData Load(bool forced = false)
    {
        //未ロードor強制ロードの場合、ロード処理をする
        if (!Loaded || forced)
        {
            devlog.log("ユーザデータの「ロード処理」を開始します。");
            if (File.Exists(_filePath))
            {//セーブデータが存在する場合
                StreamReader streamReader = new StreamReader(_filePath);
                string LoadedData_Json = streamReader.ReadToEnd();
                streamReader.Close();
                saveData = JsonUtility.FromJson<SaveData>(LoadedData_Json);

                if (!CheckCheckDigit(saveData))
                {//不正検知した場合
                    devlog.log("ユーザデータを初期化します。");
                    Save(InitializeData());//ロード用変数に初期状態のSaveDataを格納後セーブする
                    Load();
                }
                else
                {
                    devlog.log("ロードしました。内容は以下の通りです。\n" + LoadedData_Json);
                }
            }
            else
            {//セーブデータが存在しない場合
                devlog.logWarning("ユーザデータが存在しません。新しく作成します。");
                Save(InitializeData());//ロード用変数に初期状態のSaveDataを格納後セーブする
                Load();
            }
        }
        Loaded = true;
        return saveData;//最後にロードしたデータを返す
    }

    /// <summary>
    /// 各変数の値から不正検知用のCheckDigitを算出する
    /// </summary>
    /// <param name="data">セーブしたいSaveData変数</param>
    /// <returns>不正検知用のCheckDigitの値</returns>
    private int CalcCheckDigit(SaveData data)
    {//各変数の値から不正検知用のCheckDigitを算出する
        //各変数を任意の値で割った余りに任意の値を足したものの合計
        //tempに足すものの編集をしてね
        int temp = (int)(CalcEachNumForDigit(data.StartupNum)
                        + CalcEachNumForDigit(data.PlayTime)
                        + CalcDigitForArray(data.Achievements));

        int date = (data.DY + data.DM + data.DD) * ForDate_By;//セーブ年月日を足して任意の値をかけた数
        return (temp - date);
    }
    ///////////////////////////////////////////////
    ///////////////////////////////////////////////
    ///////////////////////////////////////////////




    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            GetPath();

#if UNITY_EDITOR
            if(Dev_Initialize)
            {
                Save(InitializeData());
            }
#endif
            Load();
            StartCoroutine(CountTime());//プレイタイムカウント開始
            QualitySettings.vSyncCount = 0;//垂直同期をOffにする
            Application.targetFrameRate = _list_FPS[saveData.FPS]; //FPS設定
            //Application.targetFrameRate = _fps; //FPS設定
            CountStartup(saveData);
        }
    }

    /// <summary>
    /// セーブデータ保存先のパスを取得する
    /// </summary>
    private void GetPath()
    {
        //https://qiita.com/w_yang/items/8458cd790607d14b1b36 に保存される
        _filePath = Application.persistentDataPath + "/" + _fileName + ".json";
        //saveData = new SaveData();
    }

    /// <summary>
    /// 初期状態のユーザデータを作成する
    /// </summary>
    /// <returns>初期状態のユーザデータ</returns>
    public SaveData InitializeData()
    {
        SaveData NewSaveData = new SaveData();
        devlog.logWarning("初期状態のユーザデータを作成しました。");
        return NewSaveData;
    }

    /// <summary>
    /// 起動時間をカウントする・ポインタ渡しで色んなカウントに対応してえ
    /// </summary>
    IEnumerator CountTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            if (saveData.PlayTime != PlayTimeLimit)
            {//カウント上限じゃないかどうか
                saveData.PlayTime++;
            }
        }
    }

    /// <summary>
    /// ゲーム終了時、自動的にセーブする
    /// </summary>
    private void OnApplicationQuit()
    {
        devlog.logWarning("ゲーム終了時のオートセーブを行います。");
        Save(saveData);
    }

    /// <summary>
    /// 起動回数を加算する
    /// </summary>
    /// <param name="data">加算対象のセーブデータ</param>
    private void CountStartup(SaveData data)
    {
        if (data.StartupNum < CountLimit)
        {//上限に達していなければ
            data.StartupNum++;//起動回数を+1
        }
    }

    /// <summary>
    /// セーブした年月日を格納する
    /// </summary>
    /// <param name="data"></param>
    /// <returns>一応、計算結果に任意の数倍したものを返す</returns>
    private byte CalcSavedDate(SaveData data)
    {
        DateTime now = DateTime.Now;//日付を取得
        data.DY = (ushort)(now.Year % ForDate_YMod);//年を任意の数でモジュロ演算
        //月と日はそのまま格納
        data.DM = (byte)now.Month;
        data.DD = (byte)now.Day;
        return (byte)((data.DY + data.DM + data.DD) * ForDate_By);//一応、和の任意の数倍したものを返す
    }

    /// <summary>
    /// ユーザデータに不正がないかを確認する
    /// </summary>
    /// <param name="data">確認対象のSaveData変数</param>
    /// <returns>不正なし:true　不正あり:false</returns>
    private bool CheckCheckDigit(SaveData data)
    {
        int result = CalcCheckDigit(data);//計算結果を格納
        if (data.Check == result)
        {//チェックディジットが一致=不正ナシ
            return true;
        }
        else
        {//チェックディジットが不一致=不正アリ
            devlog.logWarning($"ユーザデータの不正を検知しました。\nセーブ内容：{data.Check}　計算結果：{result}");
            return false;
        }
    }

    //
    //セーブデータにある変数のチェックディジット計算用関数
    //
    /// <summary>
    /// 任意の値で割った余りに、任意の値を足したものを返す
    /// </summary>
    /// <param name="num">任意の値</param>
    /// <returns>任意の値で割った余りに、任意の値を足したもの</returns>
    private int CalcEachNumForDigit(uint num)
    {
        return (int)((num % ForDigit_Mod) + ForDigit_Add);
    }

    /// <summary>
    /// bool配列向け・各要素を任意の値で割った余りに任意の値を足したものを返す
    /// </summary>
    /// <param name="array">計算したいbool配列</param>
    /// <returns>計算結果</returns>
    private int CalcDigitForArray(bool[] array)
    {
        int temp = 0;
        foreach (bool b in array)
        {
            temp += CalcEachNumForDigit((uint)Convert.ToInt32(b));
        }
        return temp;
    }

    /// <summary>
    /// int配列向け・各要素を任意の値で割った余りに任意の値を足したものを返す
    /// </summary>
    /// <param name="array">計算したいint配列</param>
    /// <returns>計算結果</returns>
    private int CalcDigitForArray(int[] array)
    {
        int temp = 0;
        foreach (int i in array)
        {
            temp += CalcEachNumForDigit((uint)i);
        }
        return temp;
    }

    /// <summary>
    /// int配列向け・各要素を任意の値で割った余りに任意の値を足したものを返す
    /// </summary>
    /// <param name="array">計算したいbyte配列</param>
    /// <returns>計算結果</returns>
    private int CalcDigitForArray(byte[] array)
    {
        int temp = 0;
        foreach (byte b in array)
        {
            temp += CalcEachNumForDigit((uint)b);
        }
        return temp;
    }
    //
    //セーブデータにある配列のチェックディジット計算用関数　ここまで
    //
}
