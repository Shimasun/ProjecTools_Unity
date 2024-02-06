using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class License : MonoBehaviour
{
    private SaveDataManager _save;//セーブデータを管理するスクリプト
    private AudioManager _audio;
    [SerializeField]private TextAsset textAsset;//ライセンス内容が記載されたtxt
    [SerializeField]private TextMeshProUGUI text;////ライセンス内容を代入するTMPro
    [SerializeField]private Scrollbar _scrollbar;
    [SerializeField]private float _scrollSpeed = 1.5f;//上下ボタン押したときの1秒あたりの加算値・スピード
    private bool[] _doScroll = new bool[2];//0,1 = Up,Down
    
    // Start is called before the first frame update
    void Start()
    {
        _audio = AudioManager.instance;
        _save = SaveDataManager.instance;//設定内容一覧をセーブデータから取得
        text.text = textAsset.ToString();
        _scrollbar.value = 1.0f;
    }

    private void Pressed_Cancel(){
        _audio.SE_UI_Play(AudioManager.WhichSE.Cancel);
        Destroy(this.gameObject);
    }

    void Update(){
        //右クリックされたらキャンセル処理する
        if(Input.GetMouseButtonDown(1))
        {
            Pressed_Cancel();
        }
    #if false
    //以下、キー入力への対応（過去の遺産なので削除OK）
        else if(_control.arrow[(int)ControlManager.Arrow.Up])
        {
            if(_scrollbar.value < 1){
                _scrollbar.value += _scrollSpeed * Time.deltaTime;
            }
            else{
                _scrollbar.value = 1;
            }
        }
        else if(_control.arrow[(int)ControlManager.Arrow.Down])
        {
            if(_scrollbar.value > 0){
                _scrollbar.value -= _scrollSpeed * Time.deltaTime;
            }
            else{
                _scrollbar.value = 0;
            }
        }
    #endif
    }
}
