using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class vfxTrigger : MonoBehaviour
{
    public VisualEffect[] vfx;
    public ParticleSystem[] particles;
    public Transform flipTransform;
    private bool facingLeft = false;
    
    // Start is called before the first frame update
    void Start()
    {
        if (vfx == null) vfx[0] = GetComponentInChildren<VisualEffect>();
        if (vfx[0].enabled) vfx[0].enabled = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].transform.localScale = flipTransform.localScale; 
        }
        
    }

    public IEnumerator VFXTrigger()
    {

        for (int i = 0; i < vfx.Length; i++)
        {
            if (!vfx[i].enabled)
            {
                vfx[i].enabled = true;
                yield return new WaitForSeconds(0.3f);
                vfx[i].enabled = false;
            }
        }
    }

    public void ParticleTrigger()
    {
        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].Play();
        }
    }
}
