using System.Collections;
using UnityEngine;

public class FallingObject : MonoBehaviour
{
    [SerializeField] private float targetAngle = 20f;
    [SerializeField] private float fallDuration = 0.8f;

    private Quaternion initialRotation;

    private void Awake()
    {
        initialRotation = transform.rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StopAllCoroutines();
            StartCoroutine(RotateFall(targetAngle));
        }
    }

    public void ResetRotation()
    {
        StopAllCoroutines();
        transform.rotation = initialRotation;
    }

    private IEnumerator RotateFall(float targetX)
    {
        Quaternion startRot = transform.rotation;
        Quaternion endRot = Quaternion.Euler(targetX, transform.eulerAngles.y, transform.eulerAngles.z);
        float elapsed = 0f;

        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / fallDuration);
            transform.rotation = Quaternion.Lerp(startRot, endRot, t);
            yield return null;
        }

        transform.rotation = endRot;
    }
}
