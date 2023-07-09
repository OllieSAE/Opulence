using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class vfxTrigger : MonoBehaviour
{
    public VisualEffect vfx;
    
    // Start is called before the first frame update
    void Start()
    {
        vfx = GetComponentInChildren<VisualEffect>();
        vfx.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator VFXTrigger()
    {
        if (!vfx.enabled)
        {
            vfx.enabled = true;
            yield return new WaitForSeconds(0.3f);
            vfx.enabled = false;
        }
    }
}
