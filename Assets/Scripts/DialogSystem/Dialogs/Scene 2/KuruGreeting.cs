using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class KuruGreeting : Interactable
{
    [Header("References")]
    public Transform lookAtTarget; // Oyuncunun bakacağı hedef (Kuru karakteri)
    public Transform playerBody;      // Karakterin ana gövdesi (Sağa/Sola dönüş için)
    public Transform playerCamera;    // Karakterin kamerası (Yukarı/Aşağı bakış için)

    [Header("Rotation Settings")]
    public float rotationDuration = 2f;  // Dönüş hızı
    [Tooltip("Target'ın ne kadar üstüne bakılsın?")]
    public float lookOffset = 0f;   // Hedefin ne kadar üstüne bakılacağı

    private DialogSystem dialogSystem;
    private MissionObjective missionObj;

    private bool dialogCompleted = false; // Dialog tamamlandı mı?
    private bool isDialogActive = false; // Dialog şu anda aktif mi?

    [Header("Ses Referansları")]
    [SerializeField] private List<SpeakerAudio> speakerAudios;
    [Header("Kuru's Wife Sesleri")]
    [SerializeField] private AudioClip clip_wife_intro;
    [Header("Kuru Sesleri")]
    [SerializeField] private AudioClip clip_kuru_welcome;
    [SerializeField] private AudioClip clip_kuru_bursaResponse;
    [SerializeField] private AudioClip clip_kuru_bursaPrice;
    [SerializeField] private AudioClip clip_kuru_phoneInfo;
    [SerializeField] private AudioClip clip_kuru_whereResponse;
    [SerializeField] private AudioClip clip_kuru_whereConfirm;
    [SerializeField] private AudioClip clip_kuru_after;
    [Header("Engin Sesleri")]
    [SerializeField] private AudioClip clip_engin_bursa;
    [SerializeField] private AudioClip clip_engin_bursaOkay;
    [SerializeField] private AudioClip clip_engin_phoneQuestion;
    [SerializeField] private AudioClip clip_engin_whereIAm;
    [SerializeField] private AudioClip clip_engin_whereStay;


    [System.NonSerialized] private DialogNode introNode;
    [System.NonSerialized] private DialogNode mainNode;
    [System.NonSerialized] private DialogNode afterDialogNode;
    [System.NonSerialized] private DialogNode bursaOption1Response;
    [System.NonSerialized] private DialogNode bursaOption2Response;
    [System.NonSerialized] private DialogNode whereOption1Response;
    [System.NonSerialized] private DialogNode whereOption2Response;

    void Start()
    {
        // Referansları bul
        dialogSystem = FindFirstObjectByType<DialogSystem>();
        missionObj = GetComponent<MissionObjective>();
        promptMessage = "E - Talk";
        BuildDialogTree();
    }



    public void StartIntroDialog()
    {
        if (dialogSystem != null)
        {
            dialogSystem.SetSpeakers(speakerAudios);
            dialogSystem.StartDialog(introNode);
            isDialogActive = true;
            StartCoroutine(CheckDialogEnd());
        }
    }

    protected override void Interact()
    {
        // NPC HAREKETİ: Kuru, Oyuncuya dönsün (Sadece Y ekseninde)
        if (playerCamera != null)
        {
            // transform (NPC) oyuncuya baksın, süre aynı olsun, Y ekseni kilitli kalsın
            transform.DOLookAt(playerCamera.position, rotationDuration, AxisConstraint.Y).SetEase(Ease.OutQuad);
        }

        // E tuşuna basıldığında çağrılır (PlayerInteraction raycast ile kontrol eder)
        // Dialog tamamlanmışsa after node'u göster
        if (dialogSystem != null && !isDialogActive && dialogCompleted)
        {
            dialogSystem.SetSpeakers(speakerAudios);
            dialogSystem.StartDialog(afterDialogNode);
            isDialogActive = true;
            StartCoroutine(CheckDialogEnd());
        }
    }

    private System.Collections.IEnumerator CheckDialogEnd()
    {
        // Dialog paneli kapanana kadar bekle
        while (dialogSystem != null && dialogSystem.dialogPanel.activeSelf)
        {
            yield return null;
        }

        isDialogActive = false;

        // İlk dialog tamamlandıysa dialogCompleted'ı true yap
        if (!dialogCompleted)
        {
            dialogCompleted = true;

            // Sonraki görevi tetikler
            if(missionObj != null)
                missionObj.OnInteracted();
        }
    }


    void RotateCameraToTarget()
    {
        if (lookAtTarget == null) return;

        // A. Karakterin Gövdesini (Body) hedefe döndür (Sadece Y ekseninde)
        // Böylece karakterin vücudu hedefe döner ama havaya bakmaz.
        if (playerBody != null)
        {
            playerBody.DOLookAt(lookAtTarget.position, rotationDuration, AxisConstraint.Y);
        }

        // B. Kamerayı (Camera) hedefin biraz üstüne döndür
        // Hedef pozisyon + Offset (Yukarı)
        if (playerCamera != null)
        {
            Vector3 targetPositionWithOffset = lookAtTarget.position + (Vector3.up * lookOffset);
            playerCamera.DOLookAt(targetPositionWithOffset, rotationDuration);
        }
    }



    void BuildDialogTree()
    {
        //  (Kuru'nun eşinin bağırması) 
        introNode = DialogBuilder.CreateNode(
            "KURUUUU! Are you sleeping again, damn it. Don't make the customer wait. Be worth a damn for once.",
            "KURUUUU! Gene mi uyuyorsun Allah'ın belası. Müşteriyi bekletme. Bir boka yara.",
            "Kuru's Wife"
        );

        // === MAIN NODE (Kuru'nun karşılaması) ===
        mainNode = DialogBuilder.CreateNode(
            "Welcome to Sevda Motel. This is the most honorable motel in the neighborhood. This motel is as honorable as a girl who just entered puberty. If you try to bring a woman in, I'll rough you up badly with my stick. How many days will you stay?",
            "Sevda Motel'e hoş geldiniz. Semtin en namuslu moteli burasıdır. Ergenliğine yeni girmiş bir kız kadar namusludur bu motel. İçeri kadın sokmaya çalışırsan seni sopamla çok pis döverim. Kaç gün kalacaksın?",
            "Kuru"
        );

        // === BURSA OPTION ===
        // Bursa Seçeneği - Engin'in cevabı
        bursaOption1Response = DialogBuilder.CreateNode(
            "I need to go to Bursa. How can I get there?",
            "Bursa'ya gitmem lazım. Nasıl gidebilirim?",
            "Engin"
        );

        // Bursa Seçeneği - Kuru'nun yanıtı
        bursaOption2Response = DialogBuilder.CreateNode(
            "You can't even go out to the center at this hour. Buses to Bursa leaves from Esenler. Ferries from Eminönü. Trying to go to those places at this hour is not wise. I'd say wait for the morning.",
            "Bu saatte meydana bile çıkamazsın sen. Bursa'ya giden otobüsler Esenler'den kalkar. Vapurlar Eminönü'nden. Bu saatte o taraflara gitmeye çalışmak akıl kârı değil. Sabahı bekle derim.",
            "Kuru"
        );

        // Engin'in cevabından Kuru'nun cevabına "..." ile geç
        DialogOption bursaToContinue = DialogBuilder.CreateOption("...", "...", bursaOption2Response, true);
        DialogBuilder.AddOption(bursaOption1Response, bursaToContinue);

        // Ana seçenek
        DialogOption bursaOption = DialogBuilder.CreateOption(
            "I need to go to Bursa.",
            "Bursa'ya gitmem lazım.",

            bursaOption1Response // Direkt Engin'in cevabına git
        );

        // Bursa yanıtından sonraki seçenekler
        DialogNode bursaContinue1 = DialogBuilder.CreateNode(
            "Okay. If I stay for the night, how much would it cost?",
            "Peki. Ben bu geceyi burada geçirsem ne kadar tutar?",
            "Engin"
        );

        DialogNode bursaContinue2 = DialogBuilder.CreateNode(
            "Very honorable. A good choice. You owe 15 YTL.",
            "Tam namuslu. İyi bir seçim. Borcunuz 15 YTL'dir.",
            "Kuru"
        );

        DialogNode bursaContinue3 = DialogBuilder.CreateNode(
            "Is there a phone I can use?",
            "Kullanabileceğim bir telefon var mı?",
            "Engin"
        );

        DialogNode bursaEnd = DialogBuilder.CreateEndNode(
            "There is one in the common area, in the back. It hasn't been working for 3 months, but you can try your luck.",
            "Ortak alanda, arkada. 3 aydır çalışmıyor ama sen bir şansını dene.",
            "Kuru"
        );

        // Bursa dalının ilerlemesi: bursaOption2Response'tan sonra otomatik devam
        DialogOption bursaContinueOpt1 = DialogBuilder.CreateOption("...", "...", bursaContinue1, true);
        DialogBuilder.AddOption(bursaOption2Response, bursaContinueOpt1);

        DialogOption bursaContinueOpt2 = DialogBuilder.CreateOption("...", "...", bursaContinue2, true);
        DialogBuilder.AddOption(bursaContinue1, bursaContinueOpt2);

        DialogOption bursaContinueOpt3 = DialogBuilder.CreateOption("...", "...", bursaContinue3, true);
        DialogBuilder.AddOption(bursaContinue2, bursaContinueOpt3);

        // Son "..." seçeneği
        DialogOption bursaContinueOpt4 = DialogBuilder.CreateOption("...", "...", bursaEnd, true);
        DialogBuilder.AddOption(bursaContinue3, bursaContinueOpt4);

        // === WHERE OPTION ===
        // Neredeyim Seçeneği - Engin'in cevabı
        whereOption1Response = DialogBuilder.CreateNode(
            "Sorry man, I'm not really sure where I am.",
            "Pardon abi, ben nerde olduğumdan emin değilim sanırım.",
            "Engin"
        );

        // Neredeyim Seçeneği - Kuru'nun yanıtı
        whereOption2Response = DialogBuilder.CreateNode(
            "This is one of Istanbul's most dishonorable neighborhoods. Everyone here found their lives on the street, continues to live by chance, and there is no purpose to their lives other than pleasure. People are not trustworthy, even the cats and dogs act different here.",
            "Burası İstanbul'un en namussuz semtlerinden biridir. Burada herkes canını sokakta bulmuştur, şans eseri yaşamaya devam eder ve zevklerden başka bir gaye bulunmaz. İnsanına güvenilmez, hatta kedisi köpeği bile bir başkadır burada.",
            "Kuru"
        );

        // Engin'in cevabından Kuru'nun cevabına "..." ile geç
        DialogOption whereToContinue = DialogBuilder.CreateOption("...", "...", whereOption2Response, true);
        DialogBuilder.AddOption(whereOption1Response, whereToContinue);

        // Ana seçenek
        DialogOption whereOption = DialogBuilder.CreateOption(
            "I'm not sure where I am.",
            "Nerede olduğumdan emin değilim.",
            whereOption1Response // Direkt Engin'in cevabına git
        );

        // Neredeyim yanıtından sonraki seçenekler
        DialogNode whereContinue1 = DialogBuilder.CreateNode(
            "Then I'll spend the night at this neighborhood's most honorable motel. Exactly how honorable is this motel?",
            "Ben o zaman bu semtin en namuslu motelinde geceyi geçireyim. Tam olarak ne kadar namuslu bu motel?",
            "Engin"
        );

        DialogNode whereContinue2 = DialogBuilder.CreateNode(
            "Very honorable. A good choice. You owe 15 YTL.",
            "Very honorable. A good choice. You owe 15 YTL.",
            "Kuru"
        );

        DialogNode whereContinue3 = DialogBuilder.CreateNode(
            "Is there a phone I can use?",
            "Kullanabileceğim bir telefon var mı?",
            "Engin"
        );

        DialogNode whereEnd = DialogBuilder.CreateEndNode(
            "There is one in the common area, in the back. It hasn't been working for 3 months, but you can try your luck.",
            "Ortak alanda, arkada. 3 aydır çalışmıyor ama sen bir şansını dene.",
            "Kuru"
        );

        // Neredeyim dalının ilerlemesi
        DialogOption whereContinueOpt1 = DialogBuilder.CreateOption("...", "...", whereContinue1, true);
        DialogBuilder.AddOption(whereOption2Response, whereContinueOpt1);

        DialogOption whereContinueOpt2 = DialogBuilder.CreateOption("...", "...", whereContinue2, true);
        DialogBuilder.AddOption(whereContinue1, whereContinueOpt2);

        DialogOption whereContinueOpt3 = DialogBuilder.CreateOption("...", "...", whereContinue3, true);
        DialogBuilder.AddOption(whereContinue2, whereContinueOpt3);

        // Son "..." seçeneği
        DialogOption whereContinueOpt4 = DialogBuilder.CreateOption("...", "...", whereEnd, true);
        DialogBuilder.AddOption(whereContinue3, whereContinueOpt4);

        // Main node'a seçenekleri ekle
        DialogBuilder.AddOption(mainNode, bursaOption);
        DialogBuilder.AddOption(mainNode, whereOption);

        // === AFTER DIALOG NODE (Dialog tamamlandıktan sonra) ===
        afterDialogNode = DialogBuilder.CreateEndNode(
            "I gave you your key, go to your room.",
            "Anahtarını verdim, odana git.",
            "Kuru"
        );

        // === INTRO NODE'A "..." SEÇENEĞİNİ EKLE (EN SONDA) ===
        // Tüm node'lar oluşturulduktan sonra introNode'a bağlantı ekle
        DialogOption continueOption = DialogBuilder.CreateOptionWithEvent(
            "...",
            "...",
            mainNode, // responseNode olarak mainNode'u ver
            () => {
                // Kamerayı Kuru'ya döndür ve biraz yukarı baktır
                RotateCameraToTarget();
            },
            true // isSilentOption
        );

        DialogBuilder.AddOption(introNode, continueOption);

        introNode.voiceClip = clip_wife_intro;
        mainNode.voiceClip = clip_kuru_welcome;
        bursaOption1Response.voiceClip = clip_engin_bursa;
        bursaOption2Response.voiceClip = clip_kuru_bursaResponse;
        bursaContinue1.voiceClip = clip_engin_bursaOkay;
        bursaContinue2.voiceClip = clip_kuru_bursaPrice;
        bursaContinue3.voiceClip = clip_engin_phoneQuestion;
        bursaEnd.voiceClip = clip_kuru_phoneInfo;
        whereOption1Response.voiceClip = clip_engin_whereIAm;
        whereOption2Response.voiceClip = clip_kuru_whereResponse;
        whereContinue1.voiceClip = clip_engin_whereStay;
        whereContinue2.voiceClip = clip_kuru_whereConfirm;
        whereContinue3.voiceClip = clip_engin_phoneQuestion;
        whereEnd.voiceClip = clip_kuru_phoneInfo;
        afterDialogNode.voiceClip = clip_kuru_after;
    }
}
