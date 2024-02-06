using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VectorGraphics;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

[ExecuteAlways]
public class Set_UIColor : MonoBehaviour
{   
    ///<summary>
    ///UIのカラールールに基づいて変色させる
    ///</summary>
    [SerializeField]private ColorList colorList; //Colorを指定したヤツ
    [SerializeField]private TextMeshProUGUI[] List_TMPro;
    [SerializeField]private SVGImage[] List_SVGImage;
    [SerializeField]private WhichColor whichColor;//どの色にするか
    [SerializeField]private WhichObj whichObj;//どんなオブジェクトか

    private enum WhichColor{
        Main = 0,
        Sub = 1,
        Other = 2
    }

    private enum WhichObj{
        TMPro = 0,
        SVGImage = 1
    }
    
    private void ChangeColor(){
        int num;    //変色対象Obj数
        int color = (int)whichColor;

        switch(whichObj){
            case WhichObj.TMPro://TextMeshProを変色
                num = List_TMPro.Length;
                if(num != 0){ //対象Objが存在する場合
                    foreach(TextMeshProUGUI text in List_TMPro){
                        text.color = colorList.list[color];
                    }
                }
                break;

            case WhichObj.SVGImage://SVGImageを変色
                num = List_SVGImage.Length;
                if(num != 0){ //対象Objが存在する場合
                    foreach(SVGImage image in List_SVGImage){
                        image.color = colorList.list[color];
                    }
                }
                break;
        }
    }

#if UNITY_EDITOR
    void Update()
    {   
        ChangeColor();
    }
#else
    void Start(){
        ChangeColor();
    }
#endif
}