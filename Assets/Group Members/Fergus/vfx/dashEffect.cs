using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class dashEffect : MonoBehaviour
{

    public float activeTime = 2f;
    public float spriteRefreshRateBright = 0.1f;
    public Transform positionToSpawn;
    public Vector3 positionOffset;
    public Material mat1;
    public Material mat2;
    public float spriteDestroyDelay = 0.5f;
    public Sprite mainSprite;
    
    private bool isTrailActive;
    private SpriteRenderer mySpriteRenderer;
    private Vector3 offsetPosition;
    private Material originalMat;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void StartTrail()
    {
        if (!isTrailActive)
        {
            isTrailActive = true;
            StartCoroutine(ActivateTrail(activeTime));
        }
        
    }

    public void EndTrail()
    {
        StopCoroutine(ActivateTrail(0f));
    }

     IEnumerator ActivateTrail(float timeActive)
     { 
         while (timeActive > 0)
         {

             timeActive -= spriteRefreshRateBright;
             
             if (mySpriteRenderer == null)
             {
                 mySpriteRenderer = GetComponent<SpriteRenderer>();
             }

             originalMat = mySpriteRenderer.material;
             //mySpriteRenderer.material = mat1;

             GameObject gObj1 = new GameObject();
            
             gObj1.transform.SetPositionAndRotation(positionToSpawn.position, positionToSpawn.rotation);
             gObj1.transform.localScale = positionToSpawn.transform.localScale;
             SpriteRenderer sr1 = gObj1.AddComponent<SpriteRenderer>();
             

             GameObject gObj2 = new GameObject();
             
             gObj2.transform.localScale = positionToSpawn.transform.localScale;
             if (positionToSpawn.transform.localScale.x > 0)
             {
                 offsetPosition = positionToSpawn.position + positionOffset;
             }
             else
             {
                 offsetPosition = positionToSpawn.position +
                                  new Vector3(Mathf.Abs(positionOffset.x), positionOffset.y, positionOffset.z);
             }
             gObj2.transform.SetPositionAndRotation(offsetPosition, positionToSpawn.rotation);
             SpriteRenderer sr2 = gObj2.AddComponent<SpriteRenderer>();

             //Sprite sprite = mySpriteRenderer.sprite;
        
             sr1.sprite = mainSprite;
             sr2.sprite = mainSprite;
             sr1.material = mat1;
             sr2.material = mat2;
            
             Destroy(gObj1, spriteDestroyDelay);
             Destroy(gObj2, spriteDestroyDelay);
             yield return new WaitForSeconds(spriteRefreshRateBright);
             mySpriteRenderer.material = originalMat;
         }

         isTrailActive = false;
     }

    // Update is called once per frame
    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.LeftShift) && !isTrailActive)
        // {
        //     isTrailActive = true;
        //     StartCoroutine(ActivateTrail(activeTime));
        // }
    }
}
