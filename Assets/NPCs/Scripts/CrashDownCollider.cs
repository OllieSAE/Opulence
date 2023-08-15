using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrashDownCollider : MonoBehaviour
{
    private int crashDownPower;
    private Combat combat;

    private void Start()
    {
        combat = GetComponentInParent<Combat>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            crashDownPower = combat.crashDownPower;
            other.GetComponentInParent<Health>().ChangeHealth(-crashDownPower,this.gameObject);
        }
    }
}
