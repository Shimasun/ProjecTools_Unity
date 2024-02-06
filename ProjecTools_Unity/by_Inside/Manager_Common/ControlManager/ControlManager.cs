using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlManager : MonoBehaviour
{
    /// <summary>
    /// コントロール入力を受け付ける
    /// </summary>
    
    public static ControlManager instance;

    public bool[] done{get; private set;} = new bool[2];
    public bool[] cancel{get; private set;} = new bool[2];
    public bool[] mouseMiddle{get; private set;} = new bool[2];

    [SerializeField]private MouseSideOfDone mouseSideOfDone = MouseSideOfDone.Left;//左右どちらを決定ボタンとするか

    public enum Operate
    {
        OnFrame = 0,//1フレームだけ取得
        Continue = 1//押している間true・長押し
    }

    private enum MouseSideOfDone
    {
        Left = 0,
        Right = 1
    }

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetInput_Mouse();
    }

    //マウスの左右中ボタンの押下状況の取得
    private void GetInput_Mouse()
    {
        //決定ボタンの入力状況
        done[(int)Operate.OnFrame] = Input.GetMouseButtonDown((int)mouseSideOfDone);
        done[(int)Operate.Continue] = Input.GetMouseButton((int)mouseSideOfDone);

        //キャンセルボタンの入力状況
        int cancelSide = 1 - (int)mouseSideOfDone;
        cancel[(int)Operate.OnFrame] = Input.GetMouseButtonDown(cancelSide);
        cancel[(int)Operate.Continue] = Input.GetMouseButton(cancelSide);

        //（マウス）中ボタンの入力状況
        mouseMiddle[(int)Operate.OnFrame] = Input.GetMouseButtonDown(2);
        mouseMiddle[(int)Operate.Continue] = Input.GetMouseButton(2) || Input.GetKey(KeyCode.Space);

    }
}
