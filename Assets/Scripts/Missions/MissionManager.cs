using UnityEngine;
using System; // Action eventleri için gerekli

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance { get; private set; }

    [Header("Baþlangýç Ayarý")]
    [SerializeField] private MissionSO firstMission; // Oyun açýlýnca baþlayacak ilk görev

    // Þu anki aktif görevi tutar
    public MissionSO CurrentMission { get; private set; }

    // UI veya ses sistemlerinin dinlemesi için Eventler
    public event Action<MissionSO> OnMissionStart; // Görev baþladýðýnda tetiklenir
    public event Action<MissionSO> OnMissionComplete; // Görev bittiðinde tetiklenir
    public event Action OnAllMissionsComplete; // Tüm oyun bittiðinde

    private void Awake()
    {
        // Singleton Pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Sahne deðiþse de yok olmasýn
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Oyun baþladýðýnda ilk görevi yükle
        if (firstMission != null)
        {
            StartMission(firstMission);
        }
    }

    // Yeni bir görevi baþlatýr
    public void StartMission(MissionSO mission)
    {
        CurrentMission = mission;
        Debug.Log($"Görev Baþladý: {mission.description}");

        // Event'i tetikle (UI bunu duyup güncelleyecek)
        OnMissionStart?.Invoke(mission);
    }

    // Mevcut görevi tamamlar ve sýradakine geçer
    public void CompleteCurrentMission()
    {
        if (CurrentMission == null) return;

        Debug.Log($"Görev Tamamlandý: {CurrentMission.description}");
        OnMissionComplete?.Invoke(CurrentMission);

        // Eðer bir sonraki görev varsa ona geç, yoksa oyunu bitir
        if (CurrentMission.nextMission != null && !CurrentMission.isFinalMission)
        {
            StartMission(CurrentMission.nextMission);
        }
        else
        {
            FinishGame();
        }
    }

    // Belirli bir görevi direkt atamak için (Örn: Save dosyasýndan yüklerken)
    public void ForceSetMission(MissionSO mission)
    {
        StartMission(mission);
    }

    private void FinishGame()
    {
        Debug.Log("Tüm görevler bitti! Hikaye sonu.");
        CurrentMission = null;
        OnAllMissionsComplete?.Invoke();
    }
}