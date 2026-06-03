using System.Collections;
using UnityEngine;

public class FluorescentFlicker : MonoBehaviour
{
    public Light spotlight;
    public float flickerInterval = 0.1f;

    [Header("Material Flicker")]
    public Renderer targetRenderer;
    public Material materialOn;
    public Material materialOff;
    public int materialIndex = 3;

    void Start()
    {
        spotlight ??= GetComponent<Light>();
        StartCoroutine(FlickerRoutine());
    }

    private IEnumerator FlickerRoutine()
    {
        while (true)
        {
            bool state = !spotlight.enabled;
            spotlight.enabled = state;

            if (targetRenderer != null && materialOn != null && materialOff != null)
            {
                Material[] mats = targetRenderer.materials;
                mats[materialIndex] = state ? materialOn : materialOff;
                targetRenderer.materials = mats;
            }

            yield return new WaitForSeconds(flickerInterval);
        }
    }
}
