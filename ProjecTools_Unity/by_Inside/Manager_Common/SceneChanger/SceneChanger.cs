using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SceneChanger : MonoBehaviour
{
    ///<summary>
    ///シーンの読込をします。読込進捗の表示も。
    ///LoadingImageを変えてね。
    ///</summary>

    public static SceneChanger instance;
    private Image image;
    private AudioManager _audio;//AudioManager取得
    [SerializeField] private float FadeTime = 0.3f;  //フェードにかける時間(秒)
    [SerializeField] private GameObject LoadingUI;   //LoadingUIのPrefab
    private TextMeshProUGUI LoadingNum;
    private Transform Parent_Canvas;
    private GameObject InstantedObj;
    public float ProgressNum { get; private set; } = 0;
    private AsyncOperation async;//読み込んでいるシーン
    //private sbyte PreLoadedSceneID = -1;//先読みされているシーン名・先読みしていなければ-1
    private bool _alreadyLoad;//既に読み込みと遷移処理開始しているか

    /////////////////////////////////////////
    /////////////////////////////////////////
    //この二つを使ってね

    /// <summary>
    /// シーン読み込みを開始させる
    /// </summary>
    /// <param name="SceneID">移動先のシーンID</param>
    /// <param name="InstProgress">読込進捗表示するか</param>
    /// <param name="DoFade">BGMフェードアウトするか</param>
    public void SceneChange(sbyte SceneID, bool InstProgress = true, bool DoFade = true)
    {
        if (!_alreadyLoad)
        {
            devlog.log("【SceneChange実行】");
            _alreadyLoad = true;
            image.raycastTarget = true;
            StartCoroutine(SceneLoad(SceneID, InstProgress, DoFade));
        }
    }
#if false
    /// <summary>
    /// シーンの先読みを行う・現時点では非推奨・現時点では、遷移先が確定している場合のみ使用してください
    /// </summary>
    /// <param name="LoadID">読み込むシーンID</param>
    public void ScenePreLoad(sbyte LoadID)
    {
        devlog.log("(先読み命令)シーンID :「" + LoadID + "」");
        StartLoad(LoadID);//読み込み開始
        PreLoadedSceneID = LoadID;//先読みしているシーン名を書き換え
    }
#endif
    /////////////////////////////////////////
    /////////////////////////////////////////
    //以下いじるな

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }


    IEnumerator fadeout()
    {
        image.CrossFadeAlpha(0, FadeTime, true);
        yield return new WaitForSeconds(FadeTime - 0.1f);
        image.raycastTarget = false;
    }

    /// <summary>
    /// 実際のシーンロード処理
    /// </summary>
    /// <param name="LoadID">移動先のシーンID</param>
    /// <param name="InstProgress">読み込み進捗表示させるか</param>
    /// <param name="DoFade">BGMフェードアウトするか</param>
    IEnumerator SceneLoad(sbyte LoadID, bool InstProgress, bool DoFade)
    {
        bool instedProgress = false;//進捗表示UIを表示させたか
        image.CrossFadeAlpha(1, FadeTime, true);//暗転
        yield return new WaitForSeconds(FadeTime);

        if (InstProgress && LoadingUI != null)
        {//進捗表示する場合・UIが設定されていた場合
            instedProgress = true;//進捗表示行うフラグ立てる
            InstantedObj = Instantiate(LoadingUI, Parent_Canvas);//進捗表示生成
            LoadingNum = InstantedObj.GetComponent<TextMeshProUGUI>();
            yield return new WaitForSeconds(0.1f);
        }
#if false
        if (PreLoadedSceneID != -1)
        {//先読みしているか
            //読み込もうとしているシーンとは別のシーンを先読みしているか
            if (PreLoadedSceneID != LoadID)
            {
                devlog.logError("先読みしていたシーンと、移動しようとしているシーンが異なります！");
                SceneUnload();//先読みしたシーンを破棄
                StartLoad(LoadID);//読み込み開始
            }
            else
            {//先読みしているシーンへ移動する場合
                LoadID = PreLoadedSceneID;
                PreLoadedSceneID = -1;//先読み情報を初期化
            }
        }
        else
        {//先読みしていない場合
            StartLoad(LoadID);//読み込み開始
        }
#else
        StartLoad(LoadID);//読み込み開始
#endif

        while (!async.isDone)
        {
            if (instedProgress)
            {//進捗表示する場合
                ProgressNum = (async.progress / 0.9f) * 100.0f;
                LoadingNum.text = ProgressNum.ToString("00");
            }
            if (async.progress >= 0.9f)//読み込みが完了したら
            {
                if (instedProgress)
                {
                    LoadingNum.text = "100";
                }
                if (DoFade)
                {
                    for (byte i = 0; i < _audio.bgmSourceNum; i++)
                    {
                        if (_audio.BGM_IsPlaying[i])
                        {//BGM再生中でフェード指定あれば
                            _audio.BGM_Stop(true, i);//BGMのフェードアウトを行う
                            yield return new WaitForSeconds(_audio.FadeTime + 0.1f);//フェードアウト終了まで待機
                        }
                    }
                }
                devlog.log("シーンID :「" + LoadID + "」 へ移動します。");
                async.allowSceneActivation = true;//シーン遷移する
                if (InstProgress)
                {
                    Destroy(InstantedObj);
                }
                yield return new WaitForSeconds(0.01f);//完全に遷移しきってから明転
                StartCoroutine("fadeout");//明転
                _alreadyLoad = false;//遷移できるフラグ立てる
                yield break;
            }
            yield return null;
        }
    }

    void Start()
    {
        image = GetComponent<Image>();
        image.color = new Color(0, 0, 0, 1.0f);
        StartCoroutine("fadeout");
        Parent_Canvas = transform.parent.gameObject.GetComponent<Transform>();
        _audio = AudioManager.instance;
    }

    /// <summary>
    /// 非同期読み込みを開始する
    /// </summary>
    /// <param name="ID">読み込むシーンID</param>
    private void StartLoad(sbyte ID)
    {
        devlog.log("シーンID :「" + ID + "」 の読み込みを開始します。");
        async = SceneManager.LoadSceneAsync(ID);
        async.allowSceneActivation = false;
    }
#if false
    /// <summary>
    /// 先読みしたシーンを破棄する
    /// </summary>
    /// <returns>破棄した:true　破棄しない/できない:false</returns>
    private bool SceneUnload()
    {
        if (PreLoadedSceneID != -1)
        {
            devlog.log("先読みしていたシーンID :「" + PreLoadedSceneID + "」 を破棄します。（まだ破棄できないよ）");
            SceneManager.UnloadSceneAsync(PreLoadedSceneID);//先読みしたシーンを破棄
            PreLoadedSceneID = -1;//先読み情報を初期化
            return true;
        }
        else
        {
            devlog.log("先読みしているシーンが存在しません！");
            return false;
        }
    }
#endif
}