using UnityEngine;

public class BlinkingEffect : MonoBehaviour
{
    public Renderer targetRenderer;
    public Color emissionColor = Color.white;
    public float maxIntensity = 3.1f;
    [Range(0f, 1f)] public float flickerChance = 0.05f;

    private Material mat;
    private float flickerTimer;

    void Start()
    {
        targetRenderer ??= GetComponent<Renderer>();

        mat = targetRenderer.material;
        mat.EnableKeyword("_EMISSION");
    }

    void Update()
    {
        float intensity = maxIntensity;

        flickerTimer -= Time.deltaTime;
        if (flickerTimer <= 0f)
        {
            if (Random.value < flickerChance)
                intensity = 0f;

            flickerTimer = Random.Range(0.05f, 0.15f);
        }

        mat.SetColor("_EmissionColor", emissionColor * intensity);
    }
}
