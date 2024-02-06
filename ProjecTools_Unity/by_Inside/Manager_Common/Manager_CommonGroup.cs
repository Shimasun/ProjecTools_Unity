using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_CommonGroup : MonoBehaviour
{
    /// <summary>
    /// シーン共通Managerの親Obj
    /// </summary>

    public static Manager_CommonGroup instance;
    [SerializeField] private GameObject debugLogObj;//FPSとかのモニタ表示するObj


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);

#if DEBUG
            debugLogObj.SetActive(true);//FPSとかのモニタ表示をアクティブ化する
#else
            Destroy(debugLogObj);
#endif
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}
