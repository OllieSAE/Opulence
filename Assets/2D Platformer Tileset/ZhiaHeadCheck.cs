using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZhiaHeadCheck : MonoBehaviour
{
    public bool playerHasObject;

    public DialogueManager dialogueManager;
    void Start()
    {
        playerHasObject = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == 6 && playerHasObject)
        {
            print("thanks for returning my head! you get a HD!");
            
            dialogueManager.TriggerFinalDialogue();
        }
    }
}
