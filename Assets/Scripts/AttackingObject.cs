using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackingObject : MonoBehaviour {
    public AudioClip audioClip;
    public AnimationClip animationClip;
    private void OnTriggerEnter(Collider col)
    {
        AudioSource src = GetComponent<AudioSource>();
        Animation anm = GetComponent<Animation>();
        ParticleSystem system = GetComponent<ParticleSystem>();
        if (src!=null)
        {
            src.PlayOneShot(audioClip);
        }
        if(anm!=null)
        {
            anm.clip = animationClip;
            anm.Play();
        }
        if(system!=null)
        {
            system.Play();
        }
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
    }
}
