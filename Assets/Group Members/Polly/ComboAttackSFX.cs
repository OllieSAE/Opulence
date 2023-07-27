using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboAttackSFX : MonoBehaviour
{
    private FXTrigger fxTrigger;
    
    void Start()
    {
        fxTrigger = GetComponent<FXTrigger>();
    }

    public void SuccessfulComboHit(string sfxName)
    {
        if(sfxName != "null") fxTrigger.SFXTrigger(sfxName);
    }
}
