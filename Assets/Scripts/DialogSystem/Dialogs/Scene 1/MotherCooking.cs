using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class MotherCooking : MonoBehaviour
{
    [SerializeField] private DialogSystem dialogSystem;
    [SerializeField] private DialogNode carryTheBodies;
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private MissionObjective missionObj;
    public Transform motherTableTransform; // Anne'nin yemek masasında duracağı pozisyon
    public GameObject cookingPot; // Masadaki tencere modeli

    [System.Serializable]
    public struct SongLine
    {
        public LocalizedText lineText;
        public float displayDuration;
    }

    [Header("Şarkı Sözleri")]
    [SerializeField] private List<SongLine> songLines;


    [Header("Animasyon Ayarları")]
    [SerializeField] private Transform motherTransform;
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private GameObject playerBody;

    void OnEnable()
    {
        BathtubDropInteractable.OnLastBodyDropped += TeleportMotherToTable;
    }
    void OnDisable()
    {
        BathtubDropInteractable.OnLastBodyDropped -= TeleportMotherToTable;
    }

    void TeleportMotherToTable()
    {
        gameObject.transform.position = motherTableTransform.position;
        gameObject.transform.rotation = motherTableTransform.rotation;
        cookingPot.SetActive(true); // Tencereyi görünür yap
        this.enabled = false; // Teleport işlemi tamamlandıktan sonra bu scripti devre dışı bırakıyoruz, böylece oyuncu tekrar etkileşime giremez.
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        BuildDialogTree();   
        StartCoroutine(PlaySongAfterStartDialog()); // Şuanlık 5 saniye sonra diyalog başlıyor. Şarkı bitince başlayacak. Şarkı altyazı şeklinde olacak.
    }

    IEnumerator PlaySongAfterStartDialog()
    {
        foreach (SongLine line in songLines)
        {
            bool isTurkish = dialogSystem.GetCurrentLanguage();
            subtitleText.text = line.lineText.GetText(isTurkish);
            yield return new WaitForSeconds(line.displayDuration);
        }
        subtitleText.text = "";

        yield return new WaitForSeconds(2f); // Şarkı bittiğinde kısa bir bekleme süresi.
        
        // Karakter kontrollerini geçici kapat ki DOTween ile çakışmasın
        FirstPersonController fps = playerBody.GetComponent<FirstPersonController>();
        if (fps != null) fps.enabled = false;

        // GÖVDEYİ (playerBody) DÖNDÜRMÜYORUZ! (Gövde dönerse koltuğun yönü/0 noktası bozulur).
        // Sadece kamerayı anneye çeviriyoruz.
        playerCamera.transform.DOLookAt(motherTransform.position, 1f).OnComplete(() =>
        {
            if (fps != null) { fps.SyncCameraRotation(); fps.enabled = true; } // Yeni açıyı sisteme kaydet ve kontrolleri geri ver
            dialogSystem.StartDialog(carryTheBodies); // Diyalog başlar.
        });
    }

    void BuildDialogTree()
    {
        carryTheBodies = DialogBuilder.CreateNode
        ("We're gonna eat, you son of a b*tch. Don't just sit there all curled up!",
        "Yemek yicez soysuzun çocuğu. Ne diye büzüşüp oturuyon!",
        "Mother");

        DialogNode carryTheBodies2 = DialogBuilder.CreateEndNode
        ("Stand up, do something usefull you lazy piece of sh*t. Carry the bodies to the bathroom. We can't eat like this. My dear son, just like his father. Come on, help yor poor mother.",
        "Kalk da bir boka yara. Cesetleri sırtlan da banyoya koy. Midemiz kaldırmaz ellam. Babası kılıklı canım oğluşum benim. Hadi garip anana bir yardım et.",
        "Mother");

        DialogOption silentOption = DialogBuilder.CreateOptionWithEvent("...", "...", carryTheBodies2, () => {missionObj.OnInteracted();}, true);
        DialogBuilder.AddOption(carryTheBodies, silentOption);
    }
}
