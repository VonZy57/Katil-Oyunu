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

    [Header("Altyazı Dili")]
    public TMP_Dropdown languageDropdown;

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
    }

    void OnEnable()
    {
        if (SettingsManager.Instance == null) return;
        PopulateAll();
    }

    void PopulateAll()
    {
        var sm = SettingsManager.Instance;

        // Sensitivity
        if (sensitivitySlider != null)
        {
            sensitivitySlider.minValue = 10f;
            sensitivitySlider.maxValue = 300f;
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
        int displayCount = sm.GetDisplayCount();
        if (displaySection != null) displaySection.SetActive(true);

        if (displayDropdown != null)
        {
            displayDropdown.ClearOptions();
            var options = new List<string>();
            for (int i = 0; i < displayCount; i++)
            {
                var d = Display.displays[i];
                options.Add($"Monitör {i + 1}  ({d.systemWidth}x{d.systemHeight})");
            }
            displayDropdown.AddOptions(options);
            displayDropdown.SetValueWithoutNotify(sm.DisplayIndex);
            displayDropdown.RefreshShownValue();
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
    }

    void OnLanguageChanged(int index)
    {
        SettingsManager.Instance?.SetLanguage(index == 1);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    void UpdateSensitivityText(float value)
    {
        if (sensitivityValueText != null)
            sensitivityValueText.text = Mathf.RoundToInt(value).ToString();
    }

    void UpdateVolumeText(float value)
    {
        if (volumeValueText != null)
            volumeValueText.text = Mathf.RoundToInt(value * 100) + "%";
    }
}
