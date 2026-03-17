using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public string promptMessage;

    protected virtual void Interact()
    {

    }

    public void BaseInteract()
    {
        Interact();
    }

}