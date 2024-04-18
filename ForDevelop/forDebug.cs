#if DEBUG
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.Profiling;
using TMPro;
using UnityEngine.UI;
using System;
using System.IO;

    ///<summary>
    ///FPSとか各種情報を表示する。
    ///</summary>
public class forDebug : MonoBehaviour
{
    int frameCount;
    float prevTime;

    //各種情報を表示/非表示させるキー
    [SerializeField, Header("for InfoOpening")] private KeyCode key_OpenTopInfo = KeyCode.Alpha1;//画面上部の情報を表示/非表示させるキー
    [SerializeField] private KeyCode key_OpenSceneList = KeyCode.Alpha2 ;//シーン一覧を表示/非表示させるキー
    [SerializeField] private KeyCode key_OpenLogWindow = KeyCode.Alpha3;//LogWindowを表示/非表示させるキー

    //画面上部の各種情報について
    [SerializeField, Header("for TopDebugInfo")] private GameObject topTextObj;
    private TextMeshProUGUI text;
    [SerializeField] private AudioMixer _mixer;
    [SerializeField] private float _calcTiming = 0.5f;

    //SceneListについて
    [SerializeField, Header("for SceneList")] private GameObject sceneListObj;//シーン一覧を表示するObj
    [SerializeField] private Scrollbar sceneListScroll;

    //LogWindowについて
    [SerializeField, Header("for LogWindow")] private bool openLogWhenWarning = true;//LogWarningの時もLogWindowを自動的に開くか
    [SerializeField] private GameObject logWindowObj;//Logを表示するObj
    [SerializeField] private GameObject logTextObj;//LogWindowのTextを持つObj
    private TextMeshProUGUI logText;//LogWindowのText
    private ContentSizeFitter logTextSizeFit;
    [SerializeField] private Scrollbar logWindowScroll;

    [SerializeField, Header("for Settings")] private GameObject settingsObj;

    private SaveDataManager _saveDataManager;//セーブデータを管理するスクリプト
    private AudioManager _audio;//AudioManager取得用
    private float _startupTime;//起動時間
    private string infoText;

    void Start()
    {
        Manager_CommonGroup commonM = Manager_CommonGroup.instance;
        _saveDataManager = commonM.saveM;//設定内容一覧をセーブデータから取得
        _audio = commonM.audioM;

        Debug.developerConsoleVisible = false;//ビルド版のDevelopmentConsoleを表示させない

        text = topTextObj.GetComponent<TextMeshProUGUI>();

        //LogTextにまつわるアレコレを取得
        logText = logTextObj.GetComponent<TextMeshProUGUI>();
        logTextSizeFit = logTextObj.GetComponent<ContentSizeFitter>();

        //devlog.logを取得
        //https://www.urablog.xyz/entry/2017/04/25/195351#ApplicationlogMessageReceived%E3%82%92%E4%BD%BF%E3%81%A3%E3%81%A6%E3%83%AD%E3%82%B0%E3%82%92%E3%82%AD%E3%83%A3%E3%83%83%E3%83%81 より
        Application.logMessageReceived += OnLogMessage;//Log出力されたら OnLogMessage()を実行

        devlog.log("起動時のシーン : " + SceneManager.GetActiveScene().name);
    }

    void Update()
    {
        _startupTime += Time.unscaledDeltaTime;
        frameCount++;
        //FPS取得
        //https://www.sejuku.net/blog/82841 より
        float time = Time.realtimeSinceStartup - prevTime;

        if (time >= _calcTiming)
        {
            float fps = frameCount / time;

            //使用メモリ取得
            //https://baba-s.hatenablog.com/entry/2019/03/26/084000 より
            float Total = (Profiler.GetTotalReservedMemoryLong() >> 10) / 1024f;// Unity が現在および将来の割り当てのために確保している総メモリ
            float Used = (Profiler.GetTotalAllocatedMemoryLong() >> 10) / 1024f;// Unity によって割り当てられたメモリ

            float dB_BGM;
            _mixer.GetFloat("Vol_BGM", out dB_BGM);

            float dB_SE;
            _mixer.GetFloat("Vol_SE", out dB_SE);

            infoText = "FPS : " + fps.ToString("n2")
                        + "\nStartupTime : " + _startupTime.ToString("n2")
                        + "\nMemory : " + Used.ToString("n2") + " / " + Total.ToString("n2") + " MB"
                        + "\nBGM - SavedVolume : " + _saveDataManager.saveData.Volume_BGM + " (Now : " + dB_BGM.ToString("n2") + " dB)"
                        + "\nSE - SavedVolume : " + _saveDataManager.saveData.Volume_SE + " (Now : " + dB_SE.ToString("n2") + " dB)"
                        + "\nNowScene : " + SceneManager.GetActiveScene().name;

            // 再生しているBGMがあればその名前を表示
            for(byte i = 0; i < _audio.bgmSourceNum; i++)
            {
                AudioClip clip = _audio.GetPlayingClip(AudioManager.WhichAudio.BGM, i);
                if(clip != null)
                {
                    infoText += $"\nBGM[{i}] - NowPlaying : {clip.name}";
                }
            }

            text.text = infoText;

            text.text += "\n\n";
            frameCount = 0;
            prevTime = Time.realtimeSinceStartup;
        }

        //各種情報の表示/非表示
        if(Input.GetKeyDown(key_OpenTopInfo))
        {//画面上部の情報の開閉
            CloseTopInfo();
        }
        if (Input.GetKeyDown(key_OpenSceneList))
        {//シーン一覧とLogWindowの表示/非表示
            CloseSceneList();
        }
        if(Input.GetKeyDown(key_OpenLogWindow))
        {//LogWindowの表示/非表示
            CloseLogWindow();
        }
    }

    /// <summary>
    /// devlog.logが出力される時の処理・画面上部とLogWindowに表示
    /// </summary>
    private void OnLogMessage(string i_logText, string i_stackTrace, LogType i_type)
    {
        if (string.IsNullOrEmpty(i_logText))
        {
            return;
        }

        i_logText = $"{_startupTime}s : {i_logText}";

        //スタックトレース表示
        if( !string.IsNullOrEmpty( i_stackTrace ) )
        {
            switch( i_type )
            {
                case LogType.Error:
                case LogType.Assert:
                case LogType.Exception:
                    i_logText += $"\n{i_stackTrace}";
                    break;
                default:
                    break;
            }
        }

        //Logの種類で文字色変更
        switch( i_type )
        {
            case LogType.Error:
            case LogType.Assert:
            case LogType.Exception:
                i_logText = string.Format($"<color=red>{i_logText}</color>");
                logWindowObj.SetActive(true);
                break;

            case LogType.Warning:
                i_logText = string.Format($"<color=yellow>{i_logText}</color>");
                
                //Warningの時も自動的にログを開くか
                if(openLogWhenWarning)
                {
                    logWindowObj.SetActive(true);
                }
                break;

            default:
                break;
        }

        i_logText += "\n";

        //画面上部の表示について
        text.text += i_logText;//画面上部への出力

        //LogWindowについて
        logText.text += i_logText;

        //TMPの高さ情報を更新
        //valueを0にしても一番下に行かないのは、高さ情報が更新されるよりも前にvaalue設定しているかららしい
        //https://qiita.com/sonken625/items/adb6100f9f6d76dbdce4
        logTextSizeFit.SetLayoutVertical();
        logWindowScroll.value = 0f;
    }

    /// <summary>
    /// ゲーム終了時、各種データを出力
    /// </summary>
    private void OnApplicationQuit()
    {
        CopyLogMessage();
    }

    /// <summary>
    /// ログの内容をクリップボードにコピーする
    /// </summary>
    public void CopyLogMessage()
    {
        string temp = logText.text;

        //不要な文字列を消す
        string[] clearSt = new string[3]{"<color=red>", "<color=yellow>", "</color>"};//取り除く文字列
        foreach(string st in clearSt)
        {
            temp = temp.Replace(st, "");
        }
        string result = $"{GetDeviceInfo()}\n\n【Scene WhenLogCopy】\n{SceneManager.GetActiveScene().name}\n\n【Execution Info】\n{infoText}\n\n【Log】\n{temp}";
        GUIUtility.systemCopyBuffer = result;
        devlog.logWarning("(Dev)ログの内容をクリップボードにコピーしました");
    }

    /// <summary>
    /// 画面上部に表示させるTextの開閉
    /// </summary>
    private void CloseTopInfo()
    {
        topTextObj.SetActive(!topTextObj.activeInHierarchy);
    }

    /// <summary>
    /// 各種ウィンドウの開閉
    /// </summary>
    public void CloseSceneList()
    {
        sceneListObj.SetActive(!sceneListObj.activeInHierarchy);
        settingsObj.SetActive(!settingsObj.activeInHierarchy);
        logWindowObj.SetActive(sceneListObj.activeInHierarchy);
        sceneListScroll.value = 1f;
    }

    /// <summary>
    /// LogWindowを閉じる（正確には表示/非表示を反転させる）
    /// </summary>
    public void CloseLogWindow()
    {
        logWindowObj.SetActive(!logWindowObj.activeInHierarchy);
        logWindowScroll.value = 0f;
    }

    /// <summary>
    /// LogWindowの内容をクリアする
    /// </summary>
    public void CleanLog()
    {
        logText.text = "";
    }

    /// <summary>
    /// 実行環境情報を文字列で返す。
    /// </summary>
    /// <returns>実行環境情報(string)</returns>
    private string GetDeviceInfo()
    {
        // 参考
        // https://zenn.dev/flamers/articles/4ed4816eb8a5a7
        // https://santerabyte.com/unity-get-systeminfo/#%E3%83%87%E3%83%90%E3%82%A4%E3%82%B9%E6%A9%9F%E8%83%BD%E3%81%8C%E4%BD%BF%E7%94%A8%E3%81%A7%E3%81%8D%E3%82%8B%E3%81%8B

        string temp = "【Device Info】\n";
        temp += $"OS : {SystemInfo.operatingSystem}\n";
        temp += $"DeviceModel : {SystemInfo.deviceModel}\n";
        temp += $"DeviceType : {SystemInfo.deviceType}\n";
        temp += $"CPU : {SystemInfo.processorType}\n";
        temp += $"GPU : {SystemInfo.graphicsDeviceName}\n";
        temp += $"DeviceMemory : {(float)SystemInfo.systemMemorySize / 1024f} GB\n";
        temp += $"BatteryLevel : {SystemInfo.batteryLevel * 100f} %\n";
        temp += $"BatteryStatus : {SystemInfo.batteryStatus}";
        return temp;
    }

    /// <summary>
    /// ログをtxtファイルに書き出す
    /// </summary>
    public void LogOutput()
    {
        // 参考
        // https://nekosuko.jp/1859/
        // https://qiita.com/rohinomiya/items/000de3109abefee6c6ea
        // https://hacchi-man.hatenablog.com/entry/2020/10/01/220000

        // CopyLogの返り値を使おうと思ったら、エラーでできなかったので一旦コピーしたものをさらにコピーすることにした
        CopyLogMessage();
        string txt = GUIUtility.systemCopyBuffer;
        StreamWriter sw = new StreamWriter($"(Dev){Application.productName}_ExecuteData.txt", true);
        sw.WriteLine(txt);
        sw.Flush();// StreamWriterのバッファに書き出し残しがないか確認
        sw.Close();// ファイルを閉じる
        devlog.logWarning("(Dev)ログを実行ファイルの1つ上の階層に書き出しました");
    }
}
#endif