using UnityEngine;

public class ButtonSmashDemoObject : Interactable
{
    public ButtonSmashGame miniGameScript; // Oyun yöneticisini buraya sürükleyeceðiz

    private void Start()
    {
        promptMessage = "E - Start mini-game";
    }

    protected override void Interact()
    {
        if (miniGameScript != null)
            miniGameScript.StartMiniGame();
    }
}
