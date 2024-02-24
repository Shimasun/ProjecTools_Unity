using System;

//参考
//https://kurokumasoft.com/2022/01/03/unity-savesystem-using-json/
//https://kazupon.org/unity-jsonutility/#JSON-2

///<summary>
///ユーザーデータの型となるクラス
///</summary>
[Serializable]
public class SaveData
{
    //初期状態を代入しておく

    //弄らない項目
    public uint StartupNum = 0;//起動回数
    public uint PlayTime = 0;//second・総起動時間 0秒

    //不正検知
    public ushort DY = 0;//セーブ時の「年」を任意の数で割った余り
    public byte DM = 0;//セーブ時の「月」
    public byte DD = 0;//セーブ時の「日」
    public int Check = 0;//各変数の数値に不正がないか確認する用・CalcCheckDigitで算出・(戦歴関係の値を任意の値で割った余りに任意の値を足したものの合計 - SavedDateの値)

    //オプション
    public byte Volume_BGM = 10;//BGM Lv.10
    public byte Volume_SE = 10;//SE Lv.10
    public bool Screen = true;//スクリーンのモード・true=Full, false=Window
    public byte FPS = 1;//FPS指定・ゲーム内でFPS弄らない場合は不要・0,1,2,3 = 30,60,90,120
    public byte Resolution = 1;//解像度指定・0,1,2 = 1280*720,1920*1080,3840*2160

    // 以下でセーブする変数を宣言
}