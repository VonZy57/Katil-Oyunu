using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;

public class EndlessRunnerPassTest : MonoBehaviour
{
    public PlayerControls controls;
    public Image blackScreenImage;

    private void Awake()
    {
        // PlayerControls sınıfından bir kontrol şeması oluşturuyoruz.
        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    void Update()
    {
        if(controls.Player.Skip.triggered)
            TransitionFade();
    }

    private void TransitionFade()
    {
        blackScreenImage.DOFade(0f, 0.1f).OnComplete(() =>
        {
            blackScreenImage.gameObject.SetActive(true);
            blackScreenImage.DOFade(1f, 0.4f).OnComplete(() =>
            {
                PassEndlessRunner();
            });
        });
    }
    public void PassEndlessRunner()
    {
        int currentBuildIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentBuildIndex + 1);
    }
}
