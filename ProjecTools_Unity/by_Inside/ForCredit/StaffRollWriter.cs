using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StaffRollWriter : MonoBehaviour
{
    /// <summary>
    /// スタッフロールの入力を、Excelデータを参照して行う
    /// </summary>

    [SerializeField] private StaffList staffList;
    [SerializeField, Range(0, 10)] private byte lineBreakNum = 5;//改行数
    [SerializeField, Range(20, 30)] private byte positionTextSize = 25;//役職のテキストの大きさ
    [SerializeField, Range(0, 10)]private float spLineBreakMag = 1.5f;//多めの改行時に、標準改行数に掛ける数

    /// <summary>
    /// Excelデータを参照してスタッフロールTextを作成する
    /// </summary>
    /// <param name="textMesh">出力先のTMP</param>
    public void WriteStaffRoll(TextMeshProUGUI textMesh)
    {
        string temp = "";

        foreach (StaffListVariable value in staffList.list)
        {
            //役職
            temp += $"<size={positionTextSize}>- {value.Position} -</size>\n";

            //名前
            string[] names = value.Names.Split(',');
            foreach (string name in names)
            {
                temp += $"{name}";

                //その役職における最後の担当者ではない場合、改行する
                if(name != names.Last()){
                    temp += "\n";
                }
            }

            //最後の役職でないか
            if(value != staffList.list.Last())
            {
                //役職間の改行
                int breakNum = lineBreakNum;
                //少し多めに改行するか？
                if (value.SPLineBreak)
                {
                    //「基本改行数×任意の数」を切り上げた数
                    float f = lineBreakNum * spLineBreakMag;
                    breakNum = (int)Mathf.Ceil(f);
                }
                temp += MakeLineBreak(breakNum);
            }
        }
        //最終的に出来上がった文章を代入しておわり
        textMesh.text = temp;
        devlog.log("スタッフロールの入力が完了しました。");
    }

    /// <summary>
    /// 指定回数改行する
    /// </summary>
    /// <param name="num">改行する回数</param>
    /// <returns>指定回数改行したText</returns>
    private string MakeLineBreak(int num)
    {
        string temp = "";
        for (int i = 0; i < num; i++)
        {
            temp += "\n";
        }
        return temp;
    }
}
