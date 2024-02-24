using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

///<summary>
/// 操作の受付を行う・1フレームだけの受付をしたい
///</summary>
public class ControlManager : MonoBehaviour
{
    public static ControlManager instance;

    public Device controlDevice { get; private set; } // 入力を受け付けているデバイスの種類
    [SerializeField] private Arrow _doneButton = Arrow.Down; // 決定ボタンの位置
    [SerializeField] private Arrow _cancelButton = Arrow.Right; // キャンセルボタンの位置
    [SerializeField] private bool _doCancelByQKey = true; // Qキーでキャンセルするか
    [SerializeField] private float _inputBorderTime = 0.15f; // 連続入力受付の間隔
    public float inputBorderTime => _inputBorderTime;

    // スティック系
    public float dead { get; private set; } = 0.8f; // スティック操作の閾値
    public bool[] arrow { get; private set; } = new bool[4]; // 上下左右・0,1,2,3 = Up,Right,Down,Left
    public Vector2 stickVecL { get; private set; } // 左スティック・十字キー・WASD・キーボード十字キーのベクトル
    public Vector2 stickVecR { get; private set; } // 右スティックのベクトル

    // ボタン系
    public bool[] done { get; private set; } = new bool[2]; // 決定ボタン・エンターキー
    public bool[] cancel { get; private set; } = new bool[2]; // キャンセルボタン・エスケープキー
    public bool start { get; private set; } // Switchプロコン「+」・デュアルショック4「Options」
    public bool[,] padButtons { get; private set; } = new bool[4, 2]; // ABXYボタン
    public bool[] stickButtonL { get; private set; } = new bool[2];
    public bool[] stickButtonR { get; private set; } = new bool[2];
    public bool[] mouseRightButton { get; private set; } = new bool[2];
    public bool keySpace { get; private set; } // キーボードスペースキー

    // マウス独自系
    public Vector2 mouseWheelVec { get; private set; }
    public Vector2 mouseMoveDelta { get; private set; }

    // トリガー系・[Trigger, Operate]
    public bool[,] lT { get; private set; } = new bool[2, 2]; // 左トリガー
    public bool[,] rT { get; private set; } = new bool[2, 2]; // 右トリガー

    // 入力状況系
    private Gamepad _pad; // 接続されているゲームパッドの入力状態を格納する
    private Keyboard _key; // キーボードの入力状態を格納する
    private Mouse _mouse; // マウスの入力状態を格納する

    public enum Arrow
    {
        Up = 0,
        Right = 1,
        Down = 2,
        Left = 3
    }

    public enum Trigger
    {
        Button = 0, // LR
        Trigger = 1 // ZLR
    }

    public enum Device
    {
        GamePad = 0,
        KeyMouse = 1
    }

    public enum Operate
    {
        OnFrame = 0, // 1フレームだけ
        Continue = 1 // 長押し
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Update()
    {
        // ゲームパッドが接続されていたらパッド入力のみ受け付ける
        _pad = Gamepad.current;
        if (_pad != null)
        {
            controlDevice = Device.GamePad;
            CheckButtons();
            CheckStart();
            CheckTrigger();
            CheckArrow();
        }
        else
        {
            controlDevice = Device.KeyMouse;
            _key = Keyboard.current;
            _mouse = Mouse.current;
            CheckDoneKeyboard();
            CheckCancelKeyboard();
            CheckArrowKeyboard();
            CheckOthers();
        }
    }

    //------------------------------------------------
    //------------------ゲームパッド系-------------------
    //------------------------------------------------

    /// <summary>
    /// ゲームパッド・上下左右ボタン・スティックの監視
    /// </summary>
    private void CheckArrow()
    {
        Vector2 dVec = _pad.dpad.ReadValue();
        stickVecL = _pad.leftStick.ReadValue();
        stickVecR = _pad.rightStick.ReadValue();
        stickButtonL[(int)Operate.OnFrame] = _pad.leftStickButton.wasPressedThisFrame;
        stickButtonR[(int)Operate.OnFrame] = _pad.rightStickButton.wasPressedThisFrame;
        stickButtonL[(int)Operate.Continue] = _pad.leftStickButton.isPressed;
        stickButtonR[(int)Operate.Continue] = _pad.leftStickButton.isPressed;

        // 十字キーでの入力があるか
        if (dVec != Vector2.zero)
        {
            stickVecL = dVec;
        }
        if (stickVecL.x >= dead)
        {
            arrow[(int)Arrow.Right] = true;
        }
        else if (stickVecL.x <= -dead)
        {
            arrow[(int)Arrow.Left] = true;
        }

        if (stickVecL.y >= dead)
        {
            arrow[(int)Arrow.Up] = true;
        }
        else if (stickVecL.y <= -dead)
        {
            arrow[(int)Arrow.Down] = true;
        }

        // 入力が無ければ
        if (stickVecL == Vector2.zero)
        {
            // リセット
            for (int i = 0; i < arrow.Length; i++)
            {
                arrow[i] = false;
            }
        }
    }

    /// <summary>
    /// ゲームパッド・トリガ系の監視
    /// </summary>
    private void CheckTrigger()
    {
        // 毎フレーム取得
        int temp = (int)Operate.Continue;
        if (_pad.leftShoulder.ReadValue() >= dead)
        {
            lT[(int)Trigger.Button, temp] = true;
        }
        else
        {
            lT[(int)Trigger.Button, temp] = false;
        }
        if (_pad.leftTrigger.ReadValue() >= dead)
        {
            lT[(int)Trigger.Trigger, temp] = true;
        }
        else
        {
            lT[(int)Trigger.Trigger, temp] = false;
        }

        if (_pad.rightShoulder.ReadValue() >= dead)
        {
            rT[(int)Trigger.Button, temp] = true;
        }
        else
        {
            rT[(int)Trigger.Button, temp] = false;
        }
        if (_pad.rightTrigger.ReadValue() >= dead)
        {
            rT[(int)Trigger.Trigger, temp] = true;
        }
        else
        {
            rT[(int)Trigger.Trigger, temp] = false;
        }

        // 1フレームだけ取得
        temp = (int)Operate.OnFrame;
        lT[(int)Trigger.Button, temp] = _pad.leftShoulder.wasPressedThisFrame;
        lT[(int)Trigger.Trigger, temp] = _pad.leftTrigger.wasPressedThisFrame;
        rT[(int)Trigger.Button, temp] = _pad.rightShoulder.wasPressedThisFrame;
        rT[(int)Trigger.Trigger, temp] = _pad.rightTrigger.wasPressedThisFrame;
    }

    /// <summary>
    /// ゲームパッド・ABXYボタンの監視
    /// </summary>
    private void CheckButtons()
    {
        // 1フレーム分の入力について
        int temp = (int)Operate.OnFrame;
        padButtons[(int)Arrow.Up, temp] = _pad.buttonNorth.wasPressedThisFrame;
        padButtons[(int)Arrow.Right, temp] = _pad.buttonEast.wasPressedThisFrame;
        padButtons[(int)Arrow.Down, temp] = _pad.buttonSouth.wasPressedThisFrame;
        padButtons[(int)Arrow.Left, temp] = _pad.buttonWest.wasPressedThisFrame;
        done[(int)Operate.OnFrame] = padButtons[(int)_doneButton, temp];
        cancel[(int)Operate.OnFrame] = padButtons[(int)_cancelButton, temp];

        // 長押し入力について
        temp = (int)Operate.Continue;
        padButtons[(int)Arrow.Up, temp] = _pad.buttonNorth.isPressed;
        padButtons[(int)Arrow.Right, temp] = _pad.buttonEast.isPressed;
        padButtons[(int)Arrow.Down, temp] = _pad.buttonSouth.isPressed;
        padButtons[(int)Arrow.Left, temp] = _pad.buttonWest.isPressed;
        done[(int)Operate.Continue] = padButtons[(int)_doneButton, temp];
        cancel[(int)Operate.Continue] = padButtons[(int)_cancelButton, temp];
    }

    /// <summary>
    /// ゲームパッド・スタート（ポーズ）ボタンの監視
    /// </summary>
    private void CheckStart()
    {
        start = _pad.startButton.wasPressedThisFrame;
    }




    //------------------------------------------------
    //---------------キーボード・マウス系----------------
    //------------------------------------------------

    /// <summary>
    /// キーボード・マウス系・決定ボタンの監視・キーボードとマウス
    /// </summary>
    private void CheckDoneKeyboard()
    {
        done[(int)Operate.OnFrame] = (_key.enterKey.wasPressedThisFrame || _mouse.leftButton.wasPressedThisFrame);
        done[(int)Operate.Continue] = (_key.enterKey.isPressed || _mouse.leftButton.isPressed);
    }

    /// <summary>
    /// キーボード・マウス系・キャンセルボタンの監視
    /// </summary>
    private void CheckCancelKeyboard()
    {
        bool[] temp = { false, false };
        // Qキーによるキャンセルが有効か
        if (_doCancelByQKey)
        {
            temp[(int)Operate.OnFrame] = Input.GetKeyDown(KeyCode.Q);
            temp[(int)Operate.Continue] = Input.GetKey(KeyCode.Q);
        }
        start = (_key.escapeKey.wasPressedThisFrame || temp[(int)Operate.OnFrame]);
        cancel[(int)Operate.OnFrame] = (_mouse.rightButton.wasPressedThisFrame || start);
        cancel[(int)Operate.Continue] = (_key.escapeKey.isPressed || temp[(int)Operate.Continue]);
    }

    /// <summary>
    /// キーボード・マウス系・キーボードによる方向入力（WASD/十字キー）の監視
    /// </summary>
    private void CheckArrowKeyboard()
    {
        sbyte x = 0;
        sbyte y = 0;

        //WASD
        if (_key.dKey.isPressed)
        {
            arrow[(int)Arrow.Right] = true;
            x += 1;
        }
        if (_key.aKey.isPressed)
        {
            arrow[(int)Arrow.Left] = true;
            x += -1;
        }
        if (_key.wKey.isPressed)
        {
            arrow[(int)Arrow.Up] = true;
            y += 1;
        }
        if (_key.sKey.isPressed)
        {
            arrow[(int)Arrow.Down] = true;
            y += -1;
        }

        //十字キー
        if (_key.rightArrowKey.isPressed)
        {
            arrow[(int)Arrow.Right] = true;
            x += 1;
        }
        if (_key.leftArrowKey.isPressed)
        {
            arrow[(int)Arrow.Left] = true;
            x += -1;
        }
        if (_key.upArrowKey.isPressed)
        {
            arrow[(int)Arrow.Up] = true;
            y += 1;
        }
        if (_key.downArrowKey.isPressed)
        {
            arrow[(int)Arrow.Down] = true;
            y += -1;
        }

        // ベクトル代入と正規化
        stickVecL = new Vector2(x, y);
        if (stickVecL == Vector2.zero)
        {
            // 入力が無ければboolリセット
            for (int i = 0; i < arrow.Length; i++)
            {
                arrow[i] = false;
            }
        }
        else
        {
            stickVecL = stickVecL.normalized;
        }
    }

    /// <summary>
    /// キーボード・マウス系・その他の単純な入力
    /// </summary>
    private void CheckOthers()
    {
        keySpace = _key.spaceKey.wasPressedThisFrame;
        mouseWheelVec = _mouse.scroll.ReadValue(); // マウスホイールの回転量・基本はyだけ・下方向が正

        // 右クリックの状況
        mouseRightButton[(int)Operate.OnFrame] = _mouse.rightButton.wasPressedThisFrame;
        mouseRightButton[(int)Operate.Continue] = _mouse.rightButton.isPressed;

        mouseMoveDelta = _mouse.delta.ReadValue(); // マウスの移動量
    }
}
