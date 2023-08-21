using System;
using System.Collections;
using System.Collections.Generic;
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

    public void Start()
    {
        ResetAllValues();
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
}
