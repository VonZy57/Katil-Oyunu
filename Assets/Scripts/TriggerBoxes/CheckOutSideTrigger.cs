using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class CheckOutSideTrigger : Interactable
{
    [Header("Geçiş Ayarları")]
    public Image fadeImage;
    public float fadeDuration = 2f;

    private bool isTransitioning = false;

    void Start()
    {
        promptMessage = "E - Exit";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            StartTransition();
    }

    protected override void Interact()
    {
        StartTransition();
    }

    void StartTransition()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        GetComponent<MissionObjective>()?.OnInteracted();

        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            Color color = fadeImage.color;
            color.a = 0f;
            fadeImage.color = color;

            fadeImage.DOFade(1f, fadeDuration).OnComplete(() =>
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1));
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
