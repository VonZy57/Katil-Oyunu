using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    const string KEY_SENSITIVITY = "Sensitivity";
    const string KEY_VOLUME      = "Volume";
    const string KEY_RESOLUTION  = "ResolutionIndex";
    const string KEY_DISPLAY     = "DisplayIndex";
    const string KEY_LANGUAGE    = "IsTurkish";

    public float Sensitivity     { get; private set; }
    public float Volume          { get; private set; }
    public int   ResolutionIndex { get; private set; }
    public int   DisplayIndex    { get; private set; }
    public bool  IsTurkish       { get; private set; }

    public event System.Action OnDisplayApplied;

    private readonly List<DisplayInfo> displayLayout = new List<DisplayInfo>();
    private FirstPersonController playerController;
    private DialogSystem          dialogSystem;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        float raw = PlayerPrefs.GetFloat(KEY_SENSITIVITY, 1f);
        Sensitivity = raw > 10f
            ? Mathf.Clamp(raw / 30f, 0.1f, 10f)
            : Mathf.Clamp(raw, 0.1f, 10f);
        Volume    = PlayerPrefs.GetFloat(KEY_VOLUME, 1f);
        IsTurkish = PlayerPrefs.GetInt(KEY_LANGUAGE, 0) == 1;
        Time.timeScale = 1f;
        ApplySensitivity();
        ApplyVolume();
        ApplyLanguage();
    }

    void Start()
    {
        LoadAll();
        ApplyAll();
    }

    // ── Display layout ───────────────────────────────────────────────────────

    void RefreshDisplayLayout()
    {
        displayLayout.Clear();
        Screen.GetDisplayLayout(displayLayout);
    }

    public int GetDisplayCount()
    {
        RefreshDisplayLayout();
        return displayLayout.Count;
    }

    public List<DisplayInfo> GetDisplayLayout()
    {
        RefreshDisplayLayout();
        return displayLayout;
    }

    // ── Resolutions (filtered for current display) ───────────────────────────

    public Resolution[] GetAvailableResolutions()
    {
        RefreshDisplayLayout();
        Resolution[] all = Screen.resolutions;

        if (DisplayIndex < displayLayout.Count)
        {
            var d = displayLayout[DisplayIndex];
            var filtered = System.Array.FindAll(all,
                r => r.width <= d.width && r.height <= d.height);
            if (filtered.Length > 0) return filtered;
        }
        return all;
    }

    // ── Setters ──────────────────────────────────────────────────────────────

    public void SetSensitivity(float value)
    {
        Sensitivity = value;
        PlayerPrefs.SetFloat(KEY_SENSITIVITY, value);
        PlayerPrefs.Save();
        ApplySensitivity();
    }

    public void SetVolume(float value)
    {
        Volume = value;
        PlayerPrefs.SetFloat(KEY_VOLUME, value);
        PlayerPrefs.Save();
        ApplyVolume();
    }

    public void SetResolution(int index)
    {
        ResolutionIndex = index;
        PlayerPrefs.SetInt(KEY_RESOLUTION, index);
        PlayerPrefs.Save();
        ApplyResolution();
    }

    public void SetDisplay(int index)
    {
        DisplayIndex = index;
        PlayerPrefs.SetInt(KEY_DISPLAY, index);
        PlayerPrefs.Save();
        StartCoroutine(ApplyDisplayCoroutine());
    }

    IEnumerator ApplyDisplayCoroutine()
    {
        ApplyDisplay();
        yield return null; // Pencere taşınsın, Screen.resolutions güncellensin
        var resolutions = GetAvailableResolutions();
        ResolutionIndex = resolutions.Length - 1;
        PlayerPrefs.SetInt(KEY_RESOLUTION, ResolutionIndex);
        ApplyResolution();
        OnDisplayApplied?.Invoke();
    }

    public void SetLanguage(bool turkish)
    {
        IsTurkish = turkish;
        PlayerPrefs.SetInt(KEY_LANGUAGE, turkish ? 1 : 0);
        PlayerPrefs.Save();
        ApplyLanguage();
    }

    // ── Apply ────────────────────────────────────────────────────────────────

    void ApplySensitivity()
    {
        playerController = FindFirstObjectByType<FirstPersonController>(FindObjectsInactive.Include);
        if (playerController != null)
            playerController.mouseSensitivity = Sensitivity * 30f; // 1-10 scale → 30-300
    }

    void ApplyVolume()
    {
        AudioListener.volume = Volume;
    }

    void ApplyResolution()
    {
        var resolutions = GetAvailableResolutions();
        if (resolutions.Length == 0) return;
        int idx = Mathf.Clamp(ResolutionIndex, 0, resolutions.Length - 1);
        var res = resolutions[idx];
        Screen.SetResolution(res.width, res.height, Screen.fullScreenMode, res.refreshRateRatio);
    }

    void ApplyDisplay()
    {
        RefreshDisplayLayout();
        if (DisplayIndex < 0 || DisplayIndex >= displayLayout.Count) return;
        Screen.MoveMainWindowTo(displayLayout[DisplayIndex], Vector2Int.zero);
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
        ApplyLanguage();
        // Kaydedilmiş monitör tercihi varsa taşı + bekle; yoksa mevcut ekranda kal
        if (PlayerPrefs.HasKey(KEY_DISPLAY))
            StartCoroutine(ApplyDisplayCoroutine());
        else
            ApplyResolution();
    }

    // ── Load ─────────────────────────────────────────────────────────────────

    void LoadAll()
    {
        // Sens migration: eski format 10-300, yeni format 1-10
        float rawSens = PlayerPrefs.GetFloat(KEY_SENSITIVITY, 1f);
        Sensitivity   = rawSens > 10f
            ? Mathf.Clamp(rawSens / 30f, 0.1f, 10f)
            : Mathf.Clamp(rawSens, 0.1f, 10f);

        Volume    = PlayerPrefs.GetFloat(KEY_VOLUME, 1f);
        IsTurkish = PlayerPrefs.GetInt(KEY_LANGUAGE, 0) == 1;

        RefreshDisplayLayout();
        if (PlayerPrefs.HasKey(KEY_DISPLAY))
        {
            int savedDisplay = PlayerPrefs.GetInt(KEY_DISPLAY, 0);
            DisplayIndex = Mathf.Clamp(savedDisplay, 0, Mathf.Max(0, displayLayout.Count - 1));
        }
        else
        {
            // Tercih yok: pencerenin şu an bulunduğu ekranı kullan (Windows'un seçtiği)
            DisplayIndex = DetectCurrentDisplayIndex();
        }

        // Kaydedilen çözünürlük bu monitörde yoksa en yükseğini al
        var resolutions = GetAvailableResolutions();
        int savedRes    = PlayerPrefs.GetInt(KEY_RESOLUTION, resolutions.Length - 1);
        ResolutionIndex = Mathf.Clamp(savedRes, 0, Mathf.Max(0, resolutions.Length - 1));
    }

    int DetectCurrentDisplayIndex()
    {
        int w = Screen.currentResolution.width;
        int h = Screen.currentResolution.height;
        for (int i = 0; i < displayLayout.Count; i++)
        {
            if (displayLayout[i].width == w && displayLayout[i].height == h)
                return i;
        }
        return 0;
    }
}
