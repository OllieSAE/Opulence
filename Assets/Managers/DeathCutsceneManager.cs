using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeathCutsceneManager : MonoBehaviour
{
    public Button continueButton;

    public void EnableContinueButton()
    {
        continueButton.gameObject.SetActive(true);
    }

    public void DisableContinueButton()
    {
        continueButton.gameObject.SetActive(false);
    }
}
