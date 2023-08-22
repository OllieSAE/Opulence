using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatTracker : MonoBehaviour
{
    public int damageDealt;
    public int damageTaken;
    public int jumps;
    public int dashes;
    public int enemiesKilled;
    public int deaths;
    public float timeElapsed;
    private bool bossKilled;
    public bool expoNight;
    public TextMeshProUGUI damageDealtText;
    public TextMeshProUGUI damageTakenText;
    public TextMeshProUGUI jumpsText;
    public TextMeshProUGUI dashesText;
    public TextMeshProUGUI enemiesKilledText;
    public TextMeshProUGUI deathsText;
    public TextMeshProUGUI timeElapsedText;
    public TextMeshProUGUI bossKilledText;


    private void Start()
    {
        GameManager.Instance.bossKilledEvent += SetBossKilled;
    }

    private void OnDisable()
    {
        GameManager.Instance.bossKilledEvent -= SetBossKilled;
    }

    public void ResetAllValues()
    {
        damageDealt = 0;
        damageTaken = 0;
        jumps = 0;
        dashes = 0;
        enemiesKilled = 0;
        deaths = 0;
        timeElapsed = 0;
    }

    public void ResetOnDeathValues()
    {
        damageDealt = 0;
        damageTaken = 0;
        jumps = 0;
        dashes = 0;
        enemiesKilled = 0;
        timeElapsed = 0;
        
        deaths++;
    }

    public void UpdateTimeElapsed(float startTime, float endTime)
    {
        if (endTime == 0)
        {
            timeElapsed = startTime;
            print("start time = " + timeElapsed);
        }

        if (startTime == 0)
        {
            timeElapsed = endTime - timeElapsed;
            print("end time = " + timeElapsed);
        }
    }

    public void DisplayStats()
    {
        damageDealtText.text = damageDealt.ToString();
        damageTakenText.text = damageTaken.ToString();
        jumpsText.text = jumps.ToString();
        dashesText.text = dashes.ToString();
        enemiesKilledText.text = enemiesKilled.ToString();
        deathsText.text = deaths.ToString();
        timeElapsedText.text = timeElapsed.ToString();
        WasBossKilled();
    }

    public void SetBossKilled()
    {
        bossKilled = true;
    }

    public void WasBossKilled()
    {
        if (bossKilled && expoNight)
        {
            bossKilledText.text = "Congratulations!<br> Show your stats to the dev team!";
        }
        else
        {
            bossKilledText.text = "Thank you so much for playing!";
        }
    }
}
