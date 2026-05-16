using UnityEngine;

public class MoneyInteraction : Interactable
{
    public bool isMoneyCollected = false; // Kapının kontrol edeceği değişken
    private MissionObjective missionObj;

    void Start()
    {
        missionObj = GetComponent<MissionObjective>();
        promptMessage = ""; // Başlangıçta etkileşim yazısını gizle
    }

    void Update()
    {
        if (MissionManager.Instance == null || missionObj == null) return;

        // İlgili görev aktifse ve para henüz alınmadıysa yazıyı göster
        if (!isMoneyCollected && MissionManager.Instance.CurrentMission == missionObj.requiredMission)
        {
            promptMessage = "E - Parayı Al";
        }
        else
        {
            promptMessage = ""; // Görev aktif değilse yazıyı gizle
        }
    }

    protected override void Interact()
    {
        // İlgili görevde değilsek tıklansa bile hiçbir şey yapma
        if (missionObj != null && MissionManager.Instance != null && MissionManager.Instance.CurrentMission != missionObj.requiredMission)
        {
            return;
        }

        if (!isMoneyCollected)
        {
            isMoneyCollected = true;
            promptMessage = ""; // Etkileşim yazısını temizle

            // Varsa görev sistemini tetikle
            if (missionObj != null)
            {
                missionObj.OnInteracted();
            }

            // Parayı aldıktan sonra sahneden gizle
            gameObject.SetActive(false);
        }
    }
}