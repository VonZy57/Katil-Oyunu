using UnityEngine;

public class RoomKeyInteraction : Interactable
{
    public bool canInteractable = true;
    public bool isKeyCollected = false;
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
            promptMessage = null;
        }
        
    }
}
