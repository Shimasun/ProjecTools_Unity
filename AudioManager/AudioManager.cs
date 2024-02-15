using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    ///<summary>
    ///オーディオの管理と再生を行う
    ///</summary>

    public static AudioManager instance;
    private AudioSource[] _source_BGM;//BGMのAudioSource
    [SerializeField] private AudioSource _source_SE;//SEのAudioSource
    [SerializeField] private SoundList _list_SE;//UI用SEのデータベース（配列）
    [SerializeField] private AudioMixer _mixer;//ミキサー
    public float FadeTime = 0.3f;  //BGMのフェードアウトにかける時間(秒)
    [SerializeField] private byte _bgmSourceNum = 3; // BGMのAudioSourceの数＝チャネル数
    public byte bgmSourceNum => _bgmSourceNum;

    private SaveDataManager _saveDataManager;//セーブデータを管理するスクリプト
    //public byte bgmSourceNum { get; private set; }

    public bool[] BGM_IsPlaying { get; private set; }//BGM再生中か

    private float time = 0;//フェードアウト用

    void Awake()
    {
        if (instance == null)
        {
            instance = this;

            // BGM用のAudioSourceの初期化
            CreateAudioSource(_bgmSourceNum);
        }
    }

    void Start()
    {
        _saveDataManager = SaveDataManager.instance;//設定内容一覧をセーブデータから取得
        while (true)
        {
            if (_saveDataManager.Loaded)
            {//セーブデータの読込待ち
                //音量の初期化
                //devlog.log("Audio初期化：セーブデータにあるBGMとSEの音量を反映させます。");
                ChangeVol(WhichAudio.BGM, _saveDataManager.saveData.Volume_BGM);
                ChangeVol(WhichAudio.SE, _saveDataManager.saveData.Volume_SE);
                break;
            }
        }
    }

    public enum WhichAudio
    {
        Master = 0,
        BGM = 1,
        SE = 2
    }

    public enum WhichSE
    {
        Done = 0,
        Cancel = 1,
        CursorMove = 2
    }

    /// <summary>
    /// 任意のAudioClipを再生する・SE用
    /// </summary>
    /// <param name="clip">再生させるSE Clip</param>
    public void SE_Play(AudioClip clip)
    {
        //devlog.log("SE：「" + clip.name + "」 を再生します。");
        ChangePitch(WhichAudio.SE, 1.0f);
        ChangeVol_Temp(WhichAudio.SE, 1.0f);
        _source_SE.PlayOneShot(clip);
    }

    /// <summary>
    /// SEのSoundListから任意のAudioClipを再生する・UIのSE用
    /// </summary>
    /// <param name="whichSE">再生させるSE種</param>
    public void SE_Play(WhichSE whichSE)
    {
        //devlog.log("UI用のSE：「" + _list_SE.SoundList[(int)whichSE].name + "」 を再生します。");
        ChangePitch(WhichAudio.SE, 1.0f);
        ChangeVol_Temp(WhichAudio.SE, 1.0f);
        _source_SE.PlayOneShot(_list_SE.list[(int)whichSE]);
    }

    /// <summary>
    /// ループするSEの再生を止める
    /// </summary>
    /// <param name="DoFadeout">停止時フェードアウトするか</param>
    public void SE_Stop(bool DoFadeout = false)
    {
        if (DoFadeout)
        {//フェードアウトを行うか
            StartCoroutine(AudioFadeout(false));
        }
        else
        {
            //devlog.log("SEを停止します。");
            _source_SE.Stop();
        }
    }

    /// <summary>
    /// BGMを再生させる
    /// </summary>
    /// <param name="clip">再生させるBGM Clip</param>
    /// <param name="StartTime">どの時点から再生開始するか(秒)</param>
    /// <param name="channel">どのチャンネルで再生させるか</param>
    public void BGM_Play(AudioClip clip, float StartTime = 0, byte channel = 0)
    {
        if (BGM_IsPlaying[channel])
        {//再生中か
            BGM_Stop(true);//フェードアウトさせる
        }
        _source_BGM[channel].clip = clip;
        _source_BGM[channel].time = StartTime;
        ChangePitch(WhichAudio.BGM, 1.0f, channel);
        ChangeVol_Temp(WhichAudio.BGM, 1.0f, channel);
        _source_BGM[channel].Play();
        BGM_IsPlaying[channel] = true;
        //devlog.log("BGM：「" + clip.name + "」 を再生します。");
    }

    /// <summary>
    /// BGMの再生を止める
    /// </summary>
    /// <param name="DoFadeout">停止時フェードアウトさせるか</param>
    /// /// <param name="channel">どのチャンネルのBGMをストップさせるか</param>
    public void BGM_Stop(bool DoFadeout = false, byte channel = 0)
    {
        if (BGM_IsPlaying[channel])
        {//再生状態か
            if (DoFadeout)
            {//フェードアウトを行うか
                StartCoroutine(AudioFadeout(true));
            }
            else
            {
                //devlog.log($"Channel{channel} BGMを停止します。");
                _source_BGM[channel].Stop();
                _source_BGM[channel].clip = null;
            }
            BGM_IsPlaying[channel] = false;
        }
    }

    /// <summary>
    /// BGMを一時停止させる
    /// </summary>
    public void BGM_Pause()
    {
        for (int i = 0; i < bgmSourceNum; i++)
        {
            if (BGM_IsPlaying[i])
            {//再生状態か
             //devlog.log("BGMを一時停止します。");
                _source_BGM[i].Pause();//一時停止して
                BGM_IsPlaying[i] = false;//停止状態にする
            }
        }
    }

    /// <summary>
    /// BGMの一時停止を解除する
    /// </summary>
    public void BGM_UnPause()
    {
        for (int i = 0; i < bgmSourceNum; i++)
        {
            if (!BGM_IsPlaying[i])
            {//停止状態か
             //devlog.log("BGMの一時停止を解除します。");
                _source_BGM[i].UnPause();
                BGM_IsPlaying[i] = true;
            }
        }
    }

    /// <summary>
    /// 「オーディオミキサで」音量の変更を行う
    /// </summary>
    /// <param name="audio">どのオーディオの音量を変えるか</param>
    /// <param name="volume">変更後の音量・0~10で指定</param>
    public void ChangeVol(WhichAudio audio, byte volume)
    {
        //設定値は10より大きい値にはならない
        if (volume > 10)
        {
            volume = 10;
        }

        //https://kingmo.jp/kumonos/unity-audiomixer-control-volume/#index_id4 より
        //音量設定は10段階で行うから、÷10して「0 ~ 1」に収める
        float fNum = (float)volume / 10.0f;

        //Mathf.Log10(value) * 20fは相対量をdBに変換する式
        //Mathf.Clampで「-80~0」の間に収まるようにしている
        float vol = Mathf.Clamp(Mathf.Log10(fNum) * 20.0f, -80.0f, 0f);

        //AudioMixerでの値を変更する
        switch (audio)
        {
            case WhichAudio.Master:
                _mixer.SetFloat("Vol_Master", vol);
                break;

            case WhichAudio.BGM:
                _mixer.SetFloat("Vol_BGM", vol);
                break;

            case WhichAudio.SE:
                _mixer.SetFloat("Vol_SE", vol);
                break;
        }
    }

    /// <summary>
    /// 「AudioSource」で音量の変更を行う
    /// 一時的に変更する用・例えばポーズ画面開く時とか
    /// </summary>
    /// <param name="audio">BGM/SE　どちらの音量を変えるか</param>
    /// <param name="volume">変更後の音量・0~1で指定</param>
    /// <param name="bgmChannel">どのチャンネルのBGMの音量を変えるか</param>
    public void ChangeVol_Temp(WhichAudio audio, float volume, byte bgmChannel = 0)
    {
        if (volume > 1)
        {//volumeが1より大きい（最大値を超える）場合は最大値(1)に設定
            volume = 1;
        }
        else if (volume < 0)
        {
            volume = 0;
        }

        switch (audio)
        {
            case WhichAudio.BGM:
                _source_BGM[bgmChannel].volume = volume;
                break;

            case WhichAudio.SE:
                _source_SE.volume = volume;
                break;
        }
    }

    /// <summary>
    /// Audioのピッチを変更する
    /// </summary>
    /// <param name="audio">BGM/SE　どちらのピッチを変えるか</param>
    /// <param name="num">変更後のピッチ</param>
    /// /// <param name="bgmChannel">どのチャンネルのBGMのピッチを変えるか</param>
    public void ChangePitch(WhichAudio audio, float num, byte bgmChannel = 0)
    {
        switch (audio)
        {
            case WhichAudio.BGM:
                _source_BGM[bgmChannel].pitch = num;
                break;

            case WhichAudio.SE:
                _source_SE.pitch = num;
                break;
        }
    }

    /// <summary>
    /// セットされているAudioClipを返す・再生中の音楽の名前表示に使えたり？
    /// </summary>
    /// <param name="which">BGM/SE　どちらにセットされているClipについて調べるか</param>
    /// /// <param name="bgmChannel">どのチャンネルのBGMのClipについてか</param>
    /// <returns>指定したAudioSourceにセットされているAudioClip</returns>
    public AudioClip GetPlayingClip(WhichAudio which, byte bgmChannel = 0)
    {
        if (which == WhichAudio.BGM)
        {
            return _source_BGM[bgmChannel].clip;
        }
        else if (which == WhichAudio.SE)
        {
            return _source_SE.clip;
        }
        return null;
    }

    /// <summary>
    /// Audioのフェードアウトを行う
    /// </summary>
    /// <param name="BGMOrSE">BGM:true　SE:false</param>
    IEnumerator AudioFadeout(bool BGMOrSE)
    {
        //https://xr-hub.com/archives/18550 より
        //FadeDeltaTime/FadeInSecondsでボリュームの比率が0に近づくようにする。
        time = 0f;
#if false
        if(BGMOrSE){
            devlog.log("BGMのフェードアウトを行います。");
        }
        else{
            devlog.log("SEのフェードアウトを行います。");
        }
#endif
        while (true)
        {
            time += Time.deltaTime;
            if (BGMOrSE)
            {
                ChangeVol_Temp(WhichAudio.BGM, (1.0f - (time / FadeTime)));
            }
            else
            {
                ChangeVol_Temp(WhichAudio.SE, (1.0f - (time / FadeTime)));
            }
            if (time >= FadeTime)
            {
                time = FadeTime;
                if (BGMOrSE)
                {
                    BGM_Stop(false);//停止
                    foreach (AudioSource source in _source_BGM)
                    {
                        source.clip = null;
                    }

                    //devlog.log("BGMのフェードアウトが完了しました。");
                }
                else
                {
                    SE_Stop(false);
                    //devlog.log("SEのフェードアウトが完了しました。");
                }
                yield break;
            }
            yield return null;
        }
    }

    /// <summary>
    /// BGM用のAudioSourceの初期化(生成)
    /// </summary>
    /// <param name="num">BGMのチャネル数</param>
    public void CreateAudioSource(byte num)
    {
        BGM_IsPlaying = new bool[num];
        _source_BGM = new AudioSource[num];
        for (byte i = 0; i < num; i++)
        {
            AudioSource src = this.gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = true;
            src.outputAudioMixerGroup = _mixer.FindMatchingGroups("BGM")[0];
            _source_BGM[i] = src;
        }
    }
}