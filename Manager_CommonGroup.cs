using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// シーン共通Managerの親Obj
/// </summary>
public class Manager_CommonGroup : MonoBehaviour
{
    public static Manager_CommonGroup instance;
    [SerializeField] private GameObject debugLogObj; // FPSとかのモニタ表示するObj

    // Managerの登録
    // AudioManager
    [SerializeField, Header("Managers")] private AudioManager _audioM;
    public AudioManager audioM => _audioM;

    // SaveDataManager
    [SerializeField] private SaveDataManager _saveM;
    public SaveDataManager saveM => _saveM;

    // ControlManager
    [SerializeField] private ControlManager _controlM;
    public ControlManager controlM => _controlM;

    // AchievementManager
    [SerializeField] private AchievementManager _achieveM;
    public AchievementManager achieveM => _achieveM;

    // SceneChanger
    [SerializeField] private SceneChanger _sceneChanger;
    public SceneChanger sceneChanger => _sceneChanger;



    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);

            // BGM用のAudioSourceの初期化
            _audioM.CreateAudioSource(_audioM.bgmSourceNum);

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
