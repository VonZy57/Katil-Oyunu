using UnityEngine;

public class MoneyInteraction : Interactable
{
    public bool isMoneyCollected = false; // Kapının kontrol edeceği değişken

    void Start()
    {
        promptMessage = "E - Parayı Al";
    }

    protected override void Interact()
    {
        if (!isMoneyCollected)
        {
            isMoneyCollected = true;
            promptMessage = ""; // Etkileşim yazısını temizle

            // Varsa görev sistemini tetikle
            MissionObjective missionObj = GetComponent<MissionObjective>();
            if (missionObj != null)
            {
                missionObj.OnInteracted();
            }

            // Parayı aldıktan sonra sahneden gizle
            gameObject.SetActive(false);
        }
    }
}