using UnityEngine;

public class SettingsMenuController : MonoBehaviour
{
    public GameObject settingsPanel;
    public FirstPersonController firstPersonController;
    public EndlessRunner endlessRunner;
    public DialogSystem dialogSystem;

    public bool isLocked = false;
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
        if (isLocked) return;
        if (!isOpen && dialogSystem != null && dialogSystem.dialogPanel.activeSelf) return;

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
            AudioListener.pause = true;
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (firstPersonController != null)
                firstPersonController.enabled = fpcWasEnabled;
            AudioListener.pause = false;
        }
    }

    public void CloseSettings()
    {
        if (!isOpen) return;
        Toggle();
    }
}
