using UnityEngine;

public class BlackjackTableInteractable : Interactable
{
    [Header("Blackjack Baðlantýlarý")]
    public BlackjackManager blackjackManager;

    [Header("Oyuncu Kontrolü")]
    public MonoBehaviour playerMovementScript;
    public PlayerInteraction playerInteractionScript;

    private void Start()
    {
        promptMessage = "Blackjack Oyna [E]";
    }

    protected override void Interact()
    {
        LockPlayer(true);
        // Masa açýlýrken desteyi sýfýrlamak üzere Manager'a bildiriyoruz
        blackjackManager.OpenTable(this);
    }

    public void EndInteraction()
    {
        LockPlayer(false);
    }

    private void LockPlayer(bool isLocked)
    {
        if (isLocked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (playerInteractionScript != null)
            playerInteractionScript.enabled = !isLocked;

        if (playerMovementScript != null)
            playerMovementScript.enabled = !isLocked;
    }
}