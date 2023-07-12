using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;
using UnityEngine.VFX;

public class vfxTrigger : MonoBehaviour
{
    public VisualEffect[] vfx;
    public string[] sfx;
    public ParticleSystem[] particles;
    public Transform flipTransform;
    private bool facingLeft = false;

    [Header("Collision Module Settings")]
    public LayerMask enemyLayer;
    public LayerMask playerLayer;
    public ParticleSystem.CollisionModule colMod;
    
    //private string particleName;
    
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < particles.Length; i++)
        {
            colMod = particles[i].collision;
            //unity is definitely doing a BIT
            if (gameObject.layer == 8) colMod.collidesWith = playerLayer;
            if (gameObject.layer == 6) colMod.collidesWith = enemyLayer;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        for (int i = 0; i < particles.Length; i++)
        {
            if (particles[i].isPlaying) particles[i].transform.localScale = flipTransform.localScale; 
        }
    }

    public void VFXTrigger(string vfxName)
    {
        for (int i = 0; i < vfx.Length; i++)
        {
            if (vfxName == vfx[i].name)
            {
                vfx[i].Play();
            }
        }
    }

    public void VFXStop(string vfxName)
    {
        for (int i = 0; i < vfx.Length; i++)
        {
            if (vfxName == vfx[i].name)
            {
                vfx[i].Stop();
            }
        }
    }

    public void OnParticleCollision(GameObject other)
    {
        print("Hit enemy");
    }

    public void ParticleTrigger(string particleName)
    {
        
        for (int i = 0; i < particles.Length; i++)
        {
            if (particleName == particles[i].name)
            {
                particles[i].Play();
                print("played " + particleName);
            }
        }
    }
    
    public void SFXTrigger(string sfxName)
    {
        for (int i = 0; i < sfx.Length; i++)
        {
            if (sfxName == sfx[i])
            {
                RuntimeManager.PlayOneShot("event:/SOUND EVENTS/" + sfxName);
                print("played " +sfxName);
            }
        }
    }
}
