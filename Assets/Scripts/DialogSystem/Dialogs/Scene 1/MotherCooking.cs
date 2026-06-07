using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MotherCooking : MonoBehaviour
{
    [SerializeField] private DialogSystem dialogSystem;
    [SerializeField] private DialogNode carryTheBodies;
    [SerializeField] private MissionObjective missionObj;
    public Transform motherTableTransform; // Anne'nin yemek masasında duracağı pozisyon
    public GameObject cookingPot; // Masadaki tencere modeli

    [Header("Ses Referansları")]
    [SerializeField] private List<SpeakerAudio> speakerAudios;

    [Header("Anne Dialog Sesleri")]
    [SerializeField] private AudioClip clip_carryTheBodies;
    [SerializeField] private AudioClip clip_carryTheBodies2;

    [Header("Şarkı")]
    public AudioClip songClip;
    private AudioSource audioSource;

    [Header("Animasyon Ayarları")]
    [SerializeField] private Transform motherTransform;
    [SerializeField] private Transform motherLookTarget; // Kameranın bakacağı nokta (kafa vb.)
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
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        BuildDialogTree();
        StartCoroutine(PlaySongAfterStartDialog());
    }

    IEnumerator PlaySongAfterStartDialog()
    {
        if (songClip != null)
        {
            audioSource.PlayOneShot(songClip);
            yield return new WaitForSeconds(songClip.length);
        }

        yield return new WaitForSeconds(2f);


        // Karakter kontrollerini geçici kapat ki DOTween ile çakışmasın
        FirstPersonController fps = playerBody.GetComponent<FirstPersonController>();
        if (fps != null) fps.enabled = false;

        // GÖVDEYİ (playerBody) DÖNDÜRMÜYORUZ! (Gövde dönerse koltuğun yönü/0 noktası bozulur).
        // Sadece kamerayı anneye çeviriyoruz.
        Transform lookTarget = motherLookTarget != null ? motherLookTarget : motherTransform;
        playerCamera.transform.DOLookAt(lookTarget.position, 1f).OnComplete(() =>
        {
            if (fps != null) { fps.SyncCameraRotation(); fps.enabled = true; } // Yeni açıyı sisteme kaydet ve kontrolleri geri ver
            dialogSystem.SetSpeakers(speakerAudios);
            dialogSystem.StartDialog(carryTheBodies); // Diyalog başlar.
            StartCoroutine(WaitForDialogEnd()); // Diyalogun bitmesini bekleyecek sistemi başlat
        });
    }

    private IEnumerator WaitForDialogEnd()
    {
        // Panelin açılması için 1 frame bekle (erken tetiklenmeyi önlemek için)
        yield return null;
        
        // Diyalog paneli kapanana kadar (EndDialog çağrılana kadar) döngüyü beklet
        yield return new WaitUntil(() => !dialogSystem.dialogPanel.activeSelf);
        
        // Diyalog kapandığında görevi ilerlet
        if (missionObj != null)
            missionObj.OnInteracted();
    }

    void BuildDialogTree()
    {
        carryTheBodies = DialogBuilder.CreateNode
        ("We're gonna eat, you son of a b*tch. Don't just sit there all curled up!",
        "Yemek yicez soysuzun çocuğu. Ne diye büzüşüp oturuyon!",
        "Mother");

        DialogNode carryTheBodies2 = DialogBuilder.CreateEndNode
        ("Stand up, do something useful you lazy piece of sh*t. Carry the bodies to the bathroom. We can't eat like this. My dear son, just like his father. Come on, help yor poor mother.",
        "Kalk da bir boka yara. Cesetleri sırtlan da banyoya koy. Midemiz kaldırmaz ellam. Babası kılıklı canım oğluşum benim. Hadi garip anana bir yardım et.",
        "Mother");

        DialogOption silentOption = DialogBuilder.CreateOption("...", "...", carryTheBodies2, true);
        DialogBuilder.AddOption(carryTheBodies, silentOption);

        carryTheBodies.voiceClip  = clip_carryTheBodies;
        carryTheBodies2.voiceClip = clip_carryTheBodies2;
    }
}
