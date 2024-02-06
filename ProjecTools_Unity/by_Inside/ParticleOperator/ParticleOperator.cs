using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleOperator : MonoBehaviour
{
    ///<summary>
    ///パーティクル（エフェクト）の再生と削除を行う
    ///</summary>
    
    private AudioManager _audio;
    private ParticleSystem _particle;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public bool PlayParticle(AudioClip clip = null){//パーティクルとSEの再生を行う
        //_particle = new ParticleSystem[this.gameObject.transform.childCount() + 1];
        //_particle = GetComponentsInChildren<ParticleSystem>();
        _particle = GetComponent<ParticleSystem>();
        _particle.Play();
        if(clip != null){
            _audio = AudioManager.instance;
            _audio.SE_Play(clip);
        }
        StartCoroutine(CheckPlaying());
        return true;
    }

    IEnumerator CheckPlaying(){//パーティクルの再生が終わったらDestroyする
        while(true){
            if(!_particle.isPlaying){
                Destroy(this.gameObject);
                yield break;
            }
            yield return null;
        }
    }
}
