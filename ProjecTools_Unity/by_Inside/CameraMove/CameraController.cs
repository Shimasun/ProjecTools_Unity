using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    ///<summary>
    ///カメラの移動を行う
    ///</summary>

    public static CameraController instance;

    //メインカメラ
    private Camera _mainCamera;
    //メインカメラ初期位置
    private Transform _originPos;

    private Vector3 _v_Pos;
    private Vector3 _v_Rot;
    private float _t;
    private byte _targetPlayer;
    private bool _move;

    //https://kyoro-s.com/unity-9/#toc4 より
    //コルーチンを止める
    private Coroutine _coroutine;

    void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        _mainCamera = GetComponent<Camera>();
        _originPos = transform;
    }

    public void StartCameraMove(Vector3 TargetVec_Pos, Vector3 TargetVec_Rot, float MoveTime)
    {
        Initialize();
        StartCoroutine(MoveCamera(TargetVec_Pos, TargetVec_Rot, MoveTime));
    }
    public void StartChangeFoV(float targetFoV, float changeTime)
    {
        Initialize();
        StartCoroutine(ChangeFov(targetFoV, changeTime));
    }

    IEnumerator MoveCamera(Vector3 TargetVec_Pos, Vector3 TargetVec_Rot, float MoveTime)
    {//カメラ移動を行う
        _move = true;
        while (true)
        {
            if (_move)
            {
                transform.position = Vector3.SmoothDamp(transform.position, TargetVec_Pos, ref _v_Pos, MoveTime);
                transform.rotation = Quaternion.Euler(Vector3.SmoothDamp(transform.rotation.eulerAngles, TargetVec_Rot, ref _v_Rot, MoveTime));
                _t += Time.deltaTime;
                if ((_t > MoveTime * 5) && _move)
                {//指定位置へ到着したかを確認したい
                    Initialize();
                    SetGoal(TargetVec_Pos, TargetVec_Rot);
                    yield break;
                }
            }
            else
            {
                Initialize();
                SetGoal(TargetVec_Pos, TargetVec_Rot);
                yield break;
            }
            yield return null;
        }
    }
    IEnumerator ChangeFov(float target, float time)
    {//カメラの視野角変更を行う
        float initial = _mainCamera.fieldOfView;//移動前の値
        float difference = target - initial;//移動量
        float rate = difference / time;//毎秒の移動量
        _move = true;//移動許可
        while (true)
        {
            if (_move)
            {
                _mainCamera.fieldOfView += rate * Time.deltaTime;
                _t += Time.deltaTime;
                if ((_t >= time) && _move)
                {//指定位置へ到着したかを確認したい
                    Initialize();
                    SetFoVGoal(target);
                    yield break;
                }
            }
            else
            {
                Initialize();
                SetFoVGoal(target);
                yield break;
            }
            yield return null;
        }
    }

    private void Initialize()
    {
        //https://kyoro-s.com/unity-9/#toc4 より
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }
        _move = false;
        _v_Pos = Vector3.zero;
        _v_Rot = Vector3.zero;
        _t = 0;
    }

    private Vector3 SetGoal(Vector3 TargetVec_Pos, Vector3 TargetVec_Rot)
    {//カメラ移動スキップ時、ゴール位置へ瞬間移動させる
        transform.position = TargetVec_Pos;
        transform.rotation = Quaternion.Euler(TargetVec_Rot);
        Debug.Log("カメラ移動完了 Pos: " + TargetVec_Pos + " , Rot: " + TargetVec_Rot);
        return transform.position;
    }
    private float SetFoVGoal(float target)
    {
        _mainCamera.fieldOfView = target;
        Debug.Log($"視野角変更完了 FoV : {target}");
        return _mainCamera.fieldOfView;
    }
}