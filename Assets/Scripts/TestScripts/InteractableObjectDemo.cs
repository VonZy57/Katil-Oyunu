using UnityEngine;

public class InteractableObjectDemo : Interactable
{
    private bool isOpen = false;
    protected override void Interact()
    {
        isOpen = !isOpen;

        Debug.Log(isOpen ? "Kapý açýldý!" : "Kapý kapandý!");
    }

    public string GetPrompt()
    {
        return isOpen ? "Kapat [E]" : "Aç [E]";
    }
}
