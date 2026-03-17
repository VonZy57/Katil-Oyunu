using System; // Action eventleri için gerekli
using System.Collections; // Coroutine kütüphanesi
using UnityEngine;
using UnityEngine.SceneManagement;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance { get; private set; }

    [Header("Ba�lang�� Ayar�")]
    [SerializeField] private MissionSO firstMission; // Oyun a��l�nca ba�layacak ilk g�rev

    // �u anki aktif g�revi tutar
    public MissionSO CurrentMission { get; private set; }

    // UI veya ses sistemlerinin dinlemesi i�in Eventler
    public event Action<MissionSO> OnMissionStart; // G�rev ba�lad���nda tetiklenir
    public event Action<MissionSO> OnMissionComplete; // G�rev bitti�inde tetiklenir
    public event Action OnAllMissionsComplete; // T�m oyun bitti�inde

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
        // Oyun ba�lad���nda ilk g�revi y�kle
        if (firstMission != null)
        {
            StartMission(firstMission);
        }
    }

    // Yeni bir g�revi ba�lat�r
    public void StartMission(MissionSO mission)
    {
        CurrentMission = mission;
        Debug.Log($"G�rev Ba�lad�: {mission.description}");

        // Event'i tetikle (UI bunu duyup g�ncelleyecek)
        OnMissionStart?.Invoke(mission);
    }

    // Mevcut g�revi tamamlar ve s�radakine ge�er
    public void CompleteCurrentMission()
    {
        if (CurrentMission == null) return;

        Debug.Log($"Görev Tamamlandı: {CurrentMission.description}");
        OnMissionComplete?.Invoke(CurrentMission);


        // E�er g�rev sonunda sahne de�i�ecekse (sahne ismi bo� de�ilse), yeni sahneyi y�kle
        if (!string.IsNullOrEmpty(CurrentMission.loadSceneName))
        {
            Debug.Log($"Yeni Sahne Yükleniyor: {CurrentMission.loadSceneName}");
            SceneManager.LoadScene(CurrentMission.loadSceneName);
            return; // Sahne yüklenecekse geri kalan kodu okuma
        }


        // Eğer bir sonraki görev varsa ve sahne değişmeyecekse sonraki göreve geç, yoksa oyunu bitir
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
        // 1. AŞAMA: Sahne y�klenmeden �nce yap�lacaklar bu k�sma yaz�lacak
        // Örneğin; m�zik ba�latma, ekran kararma animasyonlar�...
        // ...değişken de�i�tirme, kay�t alma, oyuncu hareket k�s�tlamalar� gibi i�lemler burada yap�labilir
        Debug.Log("Sahne ge�i�i �ncesi i�lemler tamamland�, sahne y�kleniyor...");


        // 2. A�AMA: Sahneyi y�kleme
        SceneManager.LoadScene(sceneName);

        // Bu a�amadana sonra yeni sahne y�klenirken yap�lacak i�lemleri...
        // ...bu script yeni sahnede yok olup s�f�rdan ba�layaca��ndan dolay�...
        // ...yeni sahnede yer alacak ba�ka bir script ile kontrol etmek gerekiyor.

        yield return null;
    }

    // Belirli bir g�revi direkt atamak i�in (�rn: Save dosyas�ndan y�klerken)
    public void ForceSetMission(MissionSO mission)
    {
        StartMission(mission);
    }

    private void FinishGame()
    {
        Debug.Log("T�m g�revler bitti! Hikaye sonu.");
        CurrentMission = null;
        OnAllMissionsComplete?.Invoke();
    }
}