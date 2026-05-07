using UnityEngine;
using TMPro;

public class CouchSitting : SittingInteraction
{
    [Header("Couch Özel Ayarları")]
    public MissionSO requiredMission; // Kalkabilmek için gereken görev
    public TextMeshProUGUI notificationText; // Ekranda göstereceğimiz uyarı metni
    public GameObject notificationObj;
    public string standUpText = "E - Stand"; // Gösterilecek metin

    private bool hasStoodUp = false;

    protected override void Start()
    {
        base.Start(); // player, playerFPS, playerController atamalarını yapar

        // Oyunu direkt oturarak başlatma işlemleri
        isSitting = true;
        isMoving = false;

        // Karakterin hareketini kısıtla (Aksi takdirde pozisyon ataması geri tepebilir)
        if (playerController)
            playerController.enabled = false;

        if (playerFPS)
        {
            playerFPS.enabled = true;
            playerFPS.SetSittingState(true); // Oturma durumuna geçir
        }

        // Oyuncuyu oturma pozisyonuna anında ışınla (Animasyon olmadan)
        if (sitReference != null && player != null)
        {
            player.transform.position = sitReference.position;
            player.transform.rotation = sitReference.rotation;
        }

        promptMessage = ""; // Başlangıçta kalkma yazısı gizli (görev gelene kadar)

        if (notificationText != null)
        {
            notificationText.gameObject.SetActive(false); // Oyun başlarken text'i gizle
        }

        // Görev yöneticisini dinle
        if (MissionManager.Instance != null)
        {
            // Eğer başlar başlamaz istenilen görevdeysek yazıyı göster
            if (MissionManager.Instance.CurrentMission == requiredMission)
            {
                ShowNotification();
            }
            else
            {
                // Değilsek, görev başladığında haber vermesi için abone ol
                MissionManager.Instance.OnMissionStart += OnMissionChanged;
            }
        }
    }

    private void OnDestroy()
    {
        // Obje yok olduğunda memory leak olmaması için abonelikten çık
        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.OnMissionStart -= OnMissionChanged;
        }
    }

    private void OnMissionChanged(MissionSO newMission)
    {
        // Beklediğimiz görev başladıysa promptu göster ve listeden çık
        if (newMission == requiredMission && !hasStoodUp)
        {
            ShowNotification();
            MissionManager.Instance.OnMissionStart -= OnMissionChanged;
        }
    }

    private void ShowNotification()
    {
        if (notificationText != null)
        {
            notificationText.text = standUpText;
            notificationText.gameObject.SetActive(true);
            notificationObj.SetActive(true);
        }
    }

    private void HideNotification()
    {
        if (notificationText != null)
        {
            notificationText.text = "";
            notificationObj.SetActive(false);
        }
    }

    protected override void Interact()
    {
        // Kalktıktan sonra bir daha oturulamasın diye burayı boş bırakıyoruz.
    }

    protected override void InteractInputForStandUp()
    {
        // Görev gelmemişse veya zaten kalktıysa işleme devam etme
        if (MissionManager.Instance != null && MissionManager.Instance.CurrentMission != requiredMission) return;
        if (hasStoodUp) return;

        if (isSitting && !isMoving)
        {
            hasStoodUp = true;
            promptMessage = ""; // Kalkınca prompt gitsin
            HideNotification(); // Kalkınca UI metnini tamamen gizle ve içini boşalt
            
            // Kalkma işlemini SittingInteraction'daki base metodundan yap
            base.StandUp();
            
            // base.StandUp() içinde promptMessage "E - Otur" olarak değişiyor, onu eziyoruz:
            promptMessage = ""; 

            // Görev tamamlama scripti (MissionObjective) varsa onu da tetikle
            MissionObjective missionObj = GetComponent<MissionObjective>();
            if (missionObj != null)
            {
                missionObj.OnInteracted();
            }

        }
    }
}
