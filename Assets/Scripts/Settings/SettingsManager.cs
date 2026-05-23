using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    const string KEY_SENSITIVITY = "Sensitivity";
    const string KEY_VOLUME      = "Volume";
    const string KEY_RESOLUTION  = "ResolutionIndex";
    const string KEY_DISPLAY     = "DisplayIndex";
    const string KEY_LANGUAGE    = "IsTurkish";

    public float Sensitivity    { get; private set; }
    public float Volume         { get; private set; }
    public int   ResolutionIndex { get; private set; }
    public int   DisplayIndex   { get; private set; }
    public bool  IsTurkish      { get; private set; }

    private Resolution[]        availableResolutions;
    private FirstPersonController playerController;
    private DialogSystem          dialogSystem;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        availableResolutions = Screen.resolutions;
    }

    void Start()
    {
        LoadAll();
        ApplyAll();
    }

    public Resolution[] GetAvailableResolutions() => availableResolutions;
    public int GetDisplayCount() => Display.displays.Length;

    // ── Setters (save + apply immediately) ──────────────────────────────────

    public void SetSensitivity(float value)
    {
        Sensitivity = value;
        PlayerPrefs.SetFloat(KEY_SENSITIVITY, value);
        ApplySensitivity();
    }

    public void SetVolume(float value)
    {
        Volume = value;
        PlayerPrefs.SetFloat(KEY_VOLUME, value);
        ApplyVolume();
    }

    public void SetResolution(int index)
    {
        ResolutionIndex = index;
        PlayerPrefs.SetInt(KEY_RESOLUTION, index);
        ApplyResolution();
    }

    public void SetDisplay(int index)
    {
        DisplayIndex = index;
        PlayerPrefs.SetInt(KEY_DISPLAY, index);
        ApplyDisplay();
    }

    public void SetLanguage(bool turkish)
    {
        IsTurkish = turkish;
        PlayerPrefs.SetInt(KEY_LANGUAGE, turkish ? 1 : 0);
        ApplyLanguage();
    }

    // ── Apply methods ────────────────────────────────────────────────────────

    void ApplySensitivity()
    {
        playerController = FindFirstObjectByType<FirstPersonController>(FindObjectsInactive.Include);
        if (playerController != null)
            playerController.mouseSensitivity = Sensitivity;
    }

    void ApplyVolume()
    {
        AudioListener.volume = Volume;
    }

    void ApplyResolution()
    {
        if (availableResolutions == null || availableResolutions.Length == 0) return;
        int idx = Mathf.Clamp(ResolutionIndex, 0, availableResolutions.Length - 1);
        Resolution res = availableResolutions[idx];
        Screen.SetResolution(res.width, res.height, Screen.fullScreenMode, res.refreshRateRatio);
    }

    void ApplyDisplay()
    {
        // Activates additional displays; moving the primary window requires a restart.
        // Unity activates up to DisplayIndex+1 displays on next launch via this preference.
        if (DisplayIndex >= 0 && DisplayIndex < Display.displays.Length)
        {
            for (int i = 1; i <= DisplayIndex; i++)
                if (!Display.displays[i].active)
                    Display.displays[i].Activate();
        }
    }

    void ApplyLanguage()
    {
        if (dialogSystem == null)
            dialogSystem = FindFirstObjectByType<DialogSystem>(FindObjectsInactive.Include);
        if (dialogSystem != null)
            dialogSystem.SetLanguage(IsTurkish);
    }

    void ApplyAll()
    {
        ApplySensitivity();
        ApplyVolume();
        ApplyResolution();
        ApplyDisplay();
        ApplyLanguage();
    }

    // ── Load ─────────────────────────────────────────────────────────────────

    void LoadAll()
    {
        Sensitivity     = PlayerPrefs.GetFloat(KEY_SENSITIVITY, 100f);
        Volume          = PlayerPrefs.GetFloat(KEY_VOLUME, 1f);

        int defaultRes  = availableResolutions != null ? availableResolutions.Length - 1 : 0;
        ResolutionIndex = PlayerPrefs.GetInt(KEY_RESOLUTION, defaultRes);

        DisplayIndex    = PlayerPrefs.GetInt(KEY_DISPLAY, 0);
        IsTurkish       = PlayerPrefs.GetInt(KEY_LANGUAGE, 0) == 1;
    }
}
