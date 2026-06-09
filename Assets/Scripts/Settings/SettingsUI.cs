using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("Hassasiyet")]
    public Slider sensitivitySlider;
    public TextMeshProUGUI sensitivityValueText;

    [Header("Ses")]
    public Slider volumeSlider;
    public TextMeshProUGUI volumeValueText;

    [Header("Çözünürlük")]
    public TMP_Dropdown resolutionDropdown;

    [Header("Monitör")]
    public TMP_Dropdown displayDropdown;
    public GameObject displaySection;
    public float displayItemHeight = 50f;

    [Header("Altyazı Dili")]
    public TMP_Dropdown languageDropdown;

    [Header("Geri Butonu")]
    public Button backButton;

    void Awake()
    {
        // Tüm callback'leri kodda bağla — Inspector'da OnValueChanged ayarlamana gerek yok
        if (sensitivitySlider != null)
            sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);

        if (volumeSlider != null)
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);

        if (displayDropdown != null)
            displayDropdown.onValueChanged.AddListener(OnDisplayChanged);

        if (languageDropdown != null)
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);

        if (backButton != null)
            backButton.onClick.AddListener(OnBackButtonClicked);
    }

    void Start() { } // Awake'te yapıldı

    void OnDestroy()
    {
        if (sensitivitySlider != null)
            sensitivitySlider.onValueChanged.RemoveListener(OnSensitivityChanged);

        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);

        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);

        if (displayDropdown != null)
            displayDropdown.onValueChanged.RemoveListener(OnDisplayChanged);

        if (languageDropdown != null)
            languageDropdown.onValueChanged.RemoveListener(OnLanguageChanged);

        if (backButton != null)
            backButton.onClick.RemoveListener(OnBackButtonClicked);
    }

    void OnEnable()
    {
        if (SettingsManager.Instance == null) return;
        SettingsManager.Instance.OnDisplayApplied += RefreshResolutionDropdown;
        PopulateAll();
    }

    void OnDisable()
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.OnDisplayApplied -= RefreshResolutionDropdown;
    }

    void PopulateAll()
    {
        var sm = SettingsManager.Instance;

        // Sensitivity
        if (sensitivitySlider != null)
        {
            sensitivitySlider.minValue = 0.1f;
            sensitivitySlider.maxValue = 10f;
            sensitivitySlider.wholeNumbers = false;
            sensitivitySlider.SetValueWithoutNotify(sm.Sensitivity);
            UpdateSensitivityText(sm.Sensitivity);
        }

        // Volume
        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.SetValueWithoutNotify(sm.Volume);
            UpdateVolumeText(sm.Volume);
        }

        // Resolution
        if (resolutionDropdown != null)
        {
            var resolutions = sm.GetAvailableResolutions();
            resolutionDropdown.ClearOptions();
            var options = new List<string>();
            foreach (var r in resolutions)
                options.Add($"{r.width}x{r.height}  {r.refreshRateRatio.value:F0}Hz");
            resolutionDropdown.AddOptions(options);
            resolutionDropdown.SetValueWithoutNotify(sm.ResolutionIndex);
            resolutionDropdown.RefreshShownValue();
        }

        // Display
        if (displaySection != null) displaySection.SetActive(true);

        if (displayDropdown != null)
        {
            var displays = sm.GetDisplayLayout();
            displayDropdown.ClearOptions();
            var options = new List<string>();
            for (int i = 0; i < displays.Count; i++)
            {
                var d = displays[i];
                string monitorName = string.IsNullOrEmpty(d.name) ? $"Monitör {i + 1}" : d.name;
                options.Add($"{monitorName}  ({d.width}x{d.height})");
            }
            displayDropdown.AddOptions(options);
            displayDropdown.SetValueWithoutNotify(sm.DisplayIndex);
            displayDropdown.RefreshShownValue();

            // Item yüksekliğini ayarla
            var itemTemplate = displayDropdown.itemText?.transform.parent?.GetComponent<RectTransform>();
            if (itemTemplate != null)
                itemTemplate.sizeDelta = new Vector2(itemTemplate.sizeDelta.x, displayItemHeight);
        }

        // Language
        if (languageDropdown != null)
        {
            languageDropdown.ClearOptions();
            languageDropdown.AddOptions(new List<string> { "English", "Türkçe" });
            languageDropdown.SetValueWithoutNotify(sm.IsTurkish ? 1 : 0);
            languageDropdown.RefreshShownValue();
        }
    }

    // ── Callbacks ────────────────────────────────────────────────────────────

    void OnSensitivityChanged(float value)
    {
        SettingsManager.Instance?.SetSensitivity(value);
        UpdateSensitivityText(value);
    }

    void OnVolumeChanged(float value)
    {
        SettingsManager.Instance?.SetVolume(value);
        UpdateVolumeText(value);
    }

    void OnResolutionChanged(int index)
    {
        SettingsManager.Instance?.SetResolution(index);
    }

    void OnDisplayChanged(int index)
    {
        SettingsManager.Instance?.SetDisplay(index);
        // Çözünürlük listesi OnDisplayApplied event'i ile coroutine bittikten sonra yenilenir
    }

    void RefreshResolutionDropdown()
    {
        var sm = SettingsManager.Instance;
        if (sm == null || resolutionDropdown == null) return;

        var resolutions = sm.GetAvailableResolutions();
        resolutionDropdown.ClearOptions();
        var options = new List<string>();
        foreach (var r in resolutions)
            options.Add($"{r.width}x{r.height}  {r.refreshRateRatio.value:F0}Hz");
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.SetValueWithoutNotify(sm.ResolutionIndex);
        resolutionDropdown.RefreshShownValue();
    }

    void OnLanguageChanged(int index)
    {
        SettingsManager.Instance?.SetLanguage(index == 1);
    }

    void OnBackButtonClicked()
    {
        FindFirstObjectByType<SettingsMenuController>()?.CloseSettings();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    void UpdateSensitivityText(float value)
    {
        if (sensitivityValueText != null)
            sensitivityValueText.text = value.ToString("F1");
    }

    void UpdateVolumeText(float value)
    {
        if (volumeValueText != null)
            volumeValueText.text = Mathf.RoundToInt(value * 100) + "%";
    }
}
