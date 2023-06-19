using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private bool faeSpeakingFirst;
    
    [Header("Text UI")]
    [SerializeField] private TextMeshProUGUI faeDialogueText;
    [SerializeField] private TextMeshProUGUI zhiaDialogueText;

    [Header("Buttons")]
    [SerializeField] private GameObject faeContinueButton;
    [SerializeField] private GameObject zhiaContinueButton;
    
    [Header("Sentences")]
    [SerializeField] private string[] faeDialogueSentences;
    [SerializeField] private string[] zhiaDialogueSentences;

    [Header("Animation Controllers")] 
    [SerializeField] private Animator faeSpeechBubbleAnimator;
    [SerializeField] private Animator zhiaSpeechBubbleAnimator;
    
    public int faeIndex;
    public int zhiaIndex;
    private float speechBubbleAnimationDelay = 0.6f;
    private bool dialogueStarted;
    private bool finalDialogueUnlocked;

    private void Start()
    {
        StartCoroutine(StartDialogue());
        dialogueStarted = false;
        finalDialogueUnlocked = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (zhiaContinueButton.activeInHierarchy)
            {
                TriggerContinueFaeDialogue();
            }
            else if (faeContinueButton.activeInHierarchy)
            {
                TriggerContinueZhiaDialogue();
            }
        }
    }

    private IEnumerator StartDialogue()
    {
        if (finalDialogueUnlocked)
        {
            zhiaSpeechBubbleAnimator.SetTrigger("Open");
            zhiaDialogueText.text = string.Empty;
            yield return new WaitForSeconds(speechBubbleAnimationDelay);
            StartCoroutine(TypeZhiaDialogue());
        }
        else if (faeSpeakingFirst && !finalDialogueUnlocked)
        {
            faeSpeechBubbleAnimator.SetTrigger("Open");
            
            yield return new WaitForSeconds(speechBubbleAnimationDelay);
            StartCoroutine(TypeFaeDialogue());
        }
        else
        {
            zhiaSpeechBubbleAnimator.SetTrigger("Open");
            
            yield return new WaitForSeconds(speechBubbleAnimationDelay);
            StartCoroutine(TypeZhiaDialogue());
        }
    }

    private IEnumerator TypeFaeDialogue()
    {
        foreach (char letter in faeDialogueSentences[faeIndex].ToCharArray())
        {
            faeDialogueText.text += letter;
        }
        yield return new WaitForSeconds(typingSpeed);
        
        faeContinueButton.SetActive(true);
    }
    
    private IEnumerator TypeZhiaDialogue()
    {
        //maybe breakpoint here?
        foreach (char letter in zhiaDialogueSentences[zhiaIndex].ToCharArray())
        {
            zhiaDialogueText.text += letter;
        }
        yield return new WaitForSeconds(typingSpeed);
        
        zhiaContinueButton.SetActive(true);
    }

    private IEnumerator ContinueFaeDialogue()
    {
        zhiaDialogueText.text = string.Empty;
        
        zhiaSpeechBubbleAnimator.SetTrigger("Close");
        
        yield return new WaitForSeconds(speechBubbleAnimationDelay);
        
        faeDialogueText.text = string.Empty;
        
        faeSpeechBubbleAnimator.SetTrigger("Open");
        
        yield return new WaitForSeconds(speechBubbleAnimationDelay);
        
        if (faeIndex < faeDialogueSentences.Length - 1)
        {
            if (dialogueStarted) faeIndex++;
            else dialogueStarted = true;
            
            faeDialogueText.text = string.Empty;
            StartCoroutine(TypeFaeDialogue());
        }
    }
    
    private IEnumerator ContinueZhiaDialogue()
    {
        faeDialogueText.text = string.Empty;
        
        faeSpeechBubbleAnimator.SetTrigger("Close");
        
        yield return new WaitForSeconds(speechBubbleAnimationDelay);
        
        zhiaDialogueText.text = string.Empty;
        
        zhiaSpeechBubbleAnimator.SetTrigger("Open");
        
        yield return new WaitForSeconds(speechBubbleAnimationDelay);
        
        if (zhiaIndex < zhiaDialogueSentences.Length - 1)
        {
            if (dialogueStarted) zhiaIndex++;
            else dialogueStarted = true;
            
            zhiaDialogueText.text = string.Empty;
            StartCoroutine(TypeZhiaDialogue());
        }
    }

    public void TriggerContinueFaeDialogue()
    {
        zhiaContinueButton.SetActive(false);
        if (!finalDialogueUnlocked && faeIndex >= faeDialogueSentences.Length - 2)
        {
            zhiaDialogueText.text = string.Empty;
            zhiaSpeechBubbleAnimator.SetTrigger("Close");
            GameManager.Instance.DialogueManagerEvent();
        }
        else if (finalDialogueUnlocked && faeIndex >= faeDialogueSentences.Length - 1)
        {
            
            zhiaDialogueText.text = string.Empty;
            zhiaSpeechBubbleAnimator.SetTrigger("Close");
            
        }
        else StartCoroutine(ContinueFaeDialogue());
    }

    public void TriggerContinueZhiaDialogue()
    {
        faeContinueButton.SetActive(false);
        if (!finalDialogueUnlocked && zhiaIndex >= zhiaDialogueSentences.Length - 2)
        {
            faeDialogueText.text = string.Empty;
            faeSpeechBubbleAnimator.SetTrigger("Close");
        }
        else if (finalDialogueUnlocked && zhiaIndex >= zhiaDialogueSentences.Length - 1)
        {
            faeDialogueText.text = string.Empty;
            faeSpeechBubbleAnimator.SetTrigger("Close");
            GameManager.Instance.EndTutorial();
        }
        
        else StartCoroutine(ContinueZhiaDialogue());
    }

    public void TriggerFinalDialogue()
    {
        zhiaIndex = 2;
        zhiaDialogueText.text = string.Empty;
        finalDialogueUnlocked = true;
        StartCoroutine(StartDialogue());
    }
}
