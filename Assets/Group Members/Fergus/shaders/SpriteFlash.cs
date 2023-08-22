using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class SpriteFlash : MonoBehaviour
{

    [SerializeField] private Material flashMaterial;
    public VisualEffect vfxToTrigger;

    [SerializeField] private float duration;

    private SpriteRenderer spriteRenderer;

    private Material originalMaterial;

    private Coroutine flashRoutine;

    private FXTrigger fxTrigger;
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        fxTrigger = GetComponent<FXTrigger>();

        originalMaterial = spriteRenderer.material;
    }

    public void Flash()
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            spriteRenderer.material = originalMaterial;
        }

        if (vfxToTrigger != null) fxTrigger.VFXTrigger(vfxToTrigger.name);
        flashRoutine = StartCoroutine(FlashRoutine());
        
    }

    private IEnumerator FlashRoutine()
    {
        spriteRenderer.material = flashMaterial;

        yield return new WaitForSeconds(duration);

        spriteRenderer.material = originalMaterial;

        flashRoutine = null;
    }
}
