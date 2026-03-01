using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // Coroutine kütüphanesi
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


        // Eðer görev sonunda sahne deðiþecekse (sahne ismi boþ deðilse), yeni sahneyi yükle
        if (!string.IsNullOrEmpty(CurrentMission.loadSceneName))
        {
            Debug.Log($"Yeni Sahne Yükleniyor: {CurrentMission.loadSceneName}");
            SceneManager.LoadScene(CurrentMission.loadSceneName);
            return; // Sahne yüklenecekse geri kalan kodu okuma
        }


        // Eðer bir sonraki görev varsa ve sahne deðiþmeyecekse sonraki göreve geç, yoksa oyunu bitir
        if (CurrentMission.nextMission != null && !CurrentMission.isFinalMission)
        {
            StartMission(CurrentMission.nextMission);
        }
        else
        {
            FinishGame();
        }
    }

    private IEnumerator HandleSceneTransition(string sceneName)
    {
        // 1. AÞAMA: Sahne yüklenmeden önce yapýlacaklar bu kýsma yazýlacak
        // Örneðin; müzik baþlatma, ekran kararma animasyonlarý...
        // ...deðiþken deðiþtirme, kayýt alma, oyuncu hareket kýsýtlamalarý gibi iþlemler burada yapýlabilir
        Debug.Log("Sahne geçiþi öncesi iþlemler tamamlandý, sahne yükleniyor...");


        // 2. AÞAMA: Sahneyi yükleme
        SceneManager.LoadScene(sceneName);

        // Bu aþamadana sonra yeni sahne yüklenirken yapýlacak iþlemleri...
        // ...bu script yeni sahnede yok olup sýfýrdan baþlayacaðýndan dolayý...
        // ...yeni sahnede yer alacak baþka bir script ile kontrol etmek gerekiyor.

        yield return null;
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