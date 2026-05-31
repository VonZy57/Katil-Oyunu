using UnityEngine;

public class SettingsMenuController : MonoBehaviour
{
    public GameObject settingsPanel;
    public FirstPersonController firstPersonController;
    public EndlessRunner endlessRunner;

    private bool isOpen = false;
    private bool fpcWasEnabled = true;
    private PlayerControls controls;

    void Awake()
    {
        controls = new PlayerControls();
        controls.Player.Pause.performed += _ => Toggle();
    }

    void OnEnable()  => controls.Player.Enable();
    void OnDisable() => controls.Player.Disable();

    void Toggle()
    {
        isOpen = !isOpen;
        settingsPanel.SetActive(isOpen);

        if (isOpen)
        {
            Time.timeScale = 0.0001f; // Tam 0 EventSystem'i bozuyor, bu pratikte dur demek
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (firstPersonController != null)
            {
                fpcWasEnabled = firstPersonController.enabled;
                firstPersonController.enabled = false;
            }
            endlessRunner?.musicAudioSource?.Pause();
            endlessRunner?.audioSource?.Pause();
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (firstPersonController != null)
                firstPersonController.enabled = fpcWasEnabled;
            endlessRunner?.musicAudioSource?.UnPause();
            endlessRunner?.audioSource?.UnPause();
        }
    }

    public void CloseSettings()
    {
        if (!isOpen) return;
        Toggle();
    }
}
