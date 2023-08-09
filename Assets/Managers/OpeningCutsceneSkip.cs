using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpeningCutsceneSkip : MonoBehaviour
{
    public void SkipCutscene()
    {
        GameManager.Instance.EndLevel();
    }
}
