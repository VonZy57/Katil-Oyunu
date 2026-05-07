using UnityEngine;

public class MoneyInteraction : Interactable
{
    public bool isMoneyCollected = false; // Kapının kontrol edeceği değişken
    private MissionObjective missionObj;

    void Start()
    {
        missionObj = GetComponent<MissionObjective>();
        promptMessage = ""; // Başlangıçta etkileşim yazısını gizle

        if (MissionManager.Instance != null)
        {
            // Eğer zaten doğru görevdeysek direkt aktifleştir
            if (missionObj != null && MissionManager.Instance.CurrentMission == missionObj.requiredMission)
            {
                promptMessage = "E - Parayı Al";
            }
            else
            {
                // Değilsek, yeni görev başladığında haber vermesi için Manager'a abone ol
                MissionManager.Instance.OnMissionStart += OnMissionChanged;
            }
        }
    }

    private void OnDestroy()
    {
        // Bellek sızıntısını (Memory Leak) önlemek için obje yok olduğunda aboneliği iptal et
        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.OnMissionStart -= OnMissionChanged;
        }
    }

    private void OnMissionChanged(MissionSO newMission)
    {
        // Yeni başlayan görev bizim beklediğimiz görevse yazıyı göster
        if (missionObj != null && newMission == missionObj.requiredMission && !isMoneyCollected)
        {
            promptMessage = "E - Parayı Al";
            
            // Artık görev başladı, daha fazla dinlemeye gerek yok
            MissionManager.Instance.OnMissionStart -= OnMissionChanged;
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