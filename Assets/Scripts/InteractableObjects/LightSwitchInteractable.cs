using System.Collections.Generic;
using UnityEngine;

public class LightSwitchInteractable : Interactable
{
    [SerializeField] private Transform switchButton;
    [SerializeField] private BedInteraction bedInteraction;
    [SerializeField] private List<GameObject> lightsBulps;

    [Header("Ses Ayarları")]
    public AudioClip lightSwitchSound; // Işık kapama sesi

    private AudioSource audioSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Rotasyon ba�lang��ta s�f�r olacak //A�a�� konum
        switchButton.localRotation = Quaternion.identity;
        promptMessage = GetPrompt();

        // AudioSource ekle
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    protected override void Interact()
    {

        bedInteraction.isLightsOff = !bedInteraction.isLightsOff; // Açıksa kapat kapalıysa aç

        //Yukar� konum
        if (bedInteraction.isLightsOff)
        {
            switchButton.localRotation = Quaternion.Euler(0f, 90f, 0f);
            lightsBulps.ForEach(bulb => bulb.SetActive(false)); // Tüm ışıkları kapat
        }       
        else
        {
            switchButton.localRotation = Quaternion.Euler(0f, 0f, 0f);
            lightsBulps.ForEach(bulb => bulb.SetActive(true)); // Tüm ışıkları aç
        }

        promptMessage = GetPrompt();

        // Işık kapanınca ses çal
        if (audioSource != null && lightSwitchSound != null)
        {
            audioSource.PlayOneShot(lightSwitchSound);
        }
    }
    
    public string GetPrompt()
    {
        return bedInteraction.isLightsOff ? "E - Switch On" : "E-Switch Off";
    }
}
