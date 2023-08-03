using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RescueFailsafe : MonoBehaviour
{
    public void FailsafeRescuePlayer()
    {
        LevelGenerator.Instance.RescuePlayer();
    }
}
