using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageMoveControl : MonoBehaviour
{
    [SerializeField]private byte[] _speed;//1秒間の移動量
    [SerializeField]private RectTransform[] _circleRect;

    private Vector2[] _circleVec;
    private RectTransform _parentRect;
    private Vector2 _parentRange;//親オブジェクトのWidthとHeight取得用
    private Vector2[] _circlePos;//Circleの現在位置
    private float[] v;
    
    // Start is called before the first frame update
    void Start()
    {
        _parentRect = transform.parent.gameObject.GetComponent<RectTransform>();
        _parentRange = _parentRect.sizeDelta;
        _parentRange /= 2;

        //動かす画像の数に応じて宣言
        v = new float[_circleRect.Length];
        _circlePos = new Vector2[_circleRect.Length];
        _circleVec = new Vector2[_circleRect.Length];
        
        SetVec();
    }

    // Update is called once per frame
    void Update()
    {
        MoveImage();
    }

    private void MoveImage(){//画像を動かします
        for(int i = 0; i < _circleRect.Length; i++){
            //新座標算出
            float temp = v[i] * Time.deltaTime;//進行量算出
            float newX = _circlePos[i].x + (_circleVec[i].x + temp);
            float newY = _circlePos[i].y + (_circleVec[i].y + temp);
            float newVecX = _circleVec[i].x;
            float newVecY = _circleVec[i].y;
            if((newX >= _parentRange.x) || (newX <= -_parentRange.x)){//左右より外に出るか
                newVecX = -newVecX;
            }
            if((newY >= _parentRange.y) || (newY <= -_parentRange.y)){//上下より外に出るか
                newVecY = -newVecY;
            }
            _circleVec[i] = new Vector2(newVecX, newVecY);//進行方向を入力
            _circleRect[i].localPosition = new Vector2(_circleRect[i].localPosition.x + (temp * newVecX), _circleRect[i].localPosition.y + (temp * newVecY));//Circleを移動させる
            _circlePos[i] = new Vector2(_circleRect[i].localPosition.x, _circleRect[i].localPosition.y);
        }
    }

    private void SetVec(){//発射方向とスピードを算出する
        for(int i = 0; i < _circleRect.Length; i++){
            while(true){
                _circleVec[i] = new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));//発射方向を決める
                if((_circleVec[i].x != 0) && (_circleVec[i].y != 0)){
                    break;
                }
            }
            _circleVec[i].Normalize();
            _circlePos[i] = _circleRect[i].localPosition;
            v[i] = Random.Range((int)_speed[0], (int)_speed[1]);//各画像の移動スピード決定
        }
    }
}
