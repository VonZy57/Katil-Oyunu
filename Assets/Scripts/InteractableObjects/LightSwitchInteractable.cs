using UnityEngine;

public class LightSwitchInteractable : Interactable
{
    [SerializeField] private Transform switchButton;
    [SerializeField] private BedInteraction bedInteraction;

    [Header("Ses Ayarları")]
    public AudioClip lightSwitchSound; // Işık kapama sesi

    private AudioSource audioSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Rotasyon ba�lang��ta s�f�r olacak //A�a�� konum
        switchButton.localRotation = Quaternion.identity;

        // AudioSource ekle
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    protected override void Interact()
    {
        //Yukar� konum
        switchButton.localRotation = Quaternion.Euler(0f, 90f, 0f);
        bedInteraction.isLightsOff = true;
        promptMessage = "";

        // Işık kapanınca ses çal
        if (audioSource != null && lightSwitchSound != null)
        {
            audioSource.PlayOneShot(lightSwitchSound);
        }
    }
}
