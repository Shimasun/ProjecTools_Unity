using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PosRotList : ScriptableObject
{
    /// <summary>
    /// 「2つのVector3」のリスト
    /// </summary>
    public List<PosAndRotate> list;
}

[System.SerializableAttribute]
public class PosAndRotate
{
    /// <summary>
    /// 2つのVector3の定義
    /// </summary>
    public Vector3 Position;//カメラの位置
    public Vector3 Rotation;//カメラの回転
}
