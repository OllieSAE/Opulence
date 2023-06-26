using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class ZhiaHeadCheck : MonoBehaviour
{
    public bool playerHasObject;
    public bool hasBeenTriggered;
    public GameObject headless;
    public GameObject headAttached;

    public DialogueManager dialogueManager;
    void Start()
    {
        playerHasObject = false;
        hasBeenTriggered = false;
        headless.SetActive(true);
        headAttached.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == 6 && playerHasObject && !hasBeenTriggered)
        {
            hasBeenTriggered = true;
            
            dialogueManager.TriggerFinalDialogue();

            ReattachHead();
        }
    }

    private void ReattachHead()
    {
        headless.SetActive(false);
        headAttached.SetActive(true);
    }
}
