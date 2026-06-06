using UnityEngine;
using System; // Event (Action) kullanabilmek için ekledik

public class RoomKeyInteraction : Interactable
{
    public bool canInteractable = true;
    public bool isKeyCollected = false;

    public AudioClip pickupSound;
    private AudioSource audioSource;

    public event Action OnKeyCollected;

    void Start()
    {
        promptMessage = "E - Take";
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update(); // Üst sınıftaki (Interactable) outline mesafe kontrolünü çalıştır
    }

    protected override void Interact()
    {
        if (canInteractable)
        {
            MeshRenderer mr = GetComponent<MeshRenderer>();
            mr.enabled = false;
            canInteractable = false;
            isKeyCollected = true;
            promptMessage = "";

            if (pickupSound != null)
                audioSource.PlayOneShot(pickupSound);

            OnKeyCollected?.Invoke();
        }
        
    }
}
