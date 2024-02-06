using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManagement : MonoBehaviour
{   
    private ParticleSystem PS;
    private Transform Player_Transform;
    private float TimeCount;
    [SerializeField]private bool IsJetDashEffect = false;
    //[SerializeField]private JetTimerCtrl jetTimerCtrl;

    // Start is called before the first frame update
    void Start()
    {
        PS = GetComponent<ParticleSystem>();
        GameObject PlayerObj = GameObject.FindWithTag("Player");
        Player_Transform = PlayerObj.GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {   
        if(!PS.isPlaying){
            Destroy(this.gameObject);
        }
        else if(IsJetDashEffect){
            TimeCount += Time.deltaTime;
            if (TimeCount > 2.0f){//jetTimerCtrl.jetSince
                Destroy(this.gameObject);
            }
            else{
                this.transform.position = new Vector3(0, 5.2f, Player_Transform.position.z);
            }
        }
    }
}