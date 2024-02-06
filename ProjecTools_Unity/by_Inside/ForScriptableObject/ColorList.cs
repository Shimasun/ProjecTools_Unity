using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ColorList : ScriptableObject
{   
    ///<summary>
    ///UIのカラールールに基づいたリスト
    ///0:メイン　1:サブ...のように優位度が下がっていく
    ///</summary>
    public List<Color> list;
}
