using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HeartFlash : MonoBehaviour
{
    public Image heartImage;
    public float flashDuration = 0.1f;
    public int flashCount = 3;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void Flash()
    {
        StartCoroutine(FlashCoroutine());
    }

    IEnumerator FlashCoroutine()
    {
        for (int i = 0; i < flashCount; i++)
        {
            heartImage.enabled = false;
            yield return new WaitForSeconds(flashDuration);
            heartImage.enabled = true;
            yield return new WaitForSeconds(flashDuration);
        }
    }
}
