using UnityEngine;
using System; // Event (Action) kullanabilmek için ekledik

public class RoomKeyInteraction : Interactable
{
    public bool canInteractable = true;
    public bool isKeyCollected = false;

    public event Action OnKeyCollected; // Anahtar alındığında habercilik yapacak Olay (Event)

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        promptMessage = "E - Take";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void Interact()
    {
        if (canInteractable)
        {
            MeshRenderer mr = GetComponent<MeshRenderer>();
            mr.enabled = false;
            canInteractable = false;
            isKeyCollected = true;
            promptMessage = ""; // null yerine boş string atamak hata riskini azaltır

            // Olayı (Event) tetikle ve dinleyenlere haber ver
            OnKeyCollected?.Invoke();
        }
        
    }
}
