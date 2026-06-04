using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhoneCall : Interactable
{
    [Header("References")]
    public Transform phoneLookAtTarget; //Telefona bakış hedefi // Sonrasında hedef anneye geçecek ve anneye dönecek
    public Transform playerBody;      // Karakterin ana gövdesi (Sağa/Sola dönüş için)
    public Transform playerCamera;    // Karakterin kamerası (Yukarı/Aşağı bakış için)
    public GameObject phoneHandSet; // Telefonun ahizesi
    public Transform playerHandSetRef; // Ahizenin gideceği referans
    [SerializeField] private Vector3 initialHandSetPos; // Telefon kapanınca ahizenin geri döneceği yer
    [SerializeField] private Quaternion initialHandSetRot;

    [Header("Jumpscare & Wake Up References")]
    public GameObject girlObj_to_beMother; // Küçük kız GameObject'i (başlangıçta deaktif)
    public GameObject littleGirlToTalk;
    public Transform wakePosition; // Uyanılacak pozisyon

    [Header("UI References")]
    public GameObject playerObjectToTeleport; // Işınlanma için
    public Image clockImage;
    public TextMeshProUGUI clockText;
    public GameObject objectToActivateOnBlackscreen; // ClockImage açılınca aktif edilecek obje
    public Material skyboxMaterial; // ClockImage açılınca geçilecek skybox materyali
    

    [Header("Rotation Settings")]
    public float rotationDuration = 2f;  // Telefona dönüş hızı
    [Tooltip("Target'ın ne kadar üstüne bakılsın?")]
    public float lookOffset = 0.5f;   // Hedefin ne kadar üstüne bakılacağı


    public float autoCloseDelay = 2f; // "UYAN!" sonrası kaç saniye beklenecek

    [Header("Audio Settings")]
    public AudioClip phoneHangupSound; // Telefon kapatma sesi
    private AudioSource audioSource;

    [Header("Door References")]
    public MotelRoomDoorInteraction motelRoomDoor; // Işınlanmadan hemen önce kapanacak motel kapısı
    public ButtonSmashGame buttonSmashGame;
    private DialogSystem dialogSystem;
    

    private bool hasInteracted = false; // Telefon ile konuşuldu mu?
    private bool isDialogActive = false; // Dialog şu anda aktif mi?

    [System.NonSerialized] private DialogNode phoneCallNode;

    void Start()
    {
        // DialogSystem'i otomatik bul
        dialogSystem = FindFirstObjectByType<DialogSystem>();

        promptMessage = "E - Answer Call";
        
        clockText.text = "";

        // 2. Başlangıç Ayarları
        if (girlObj_to_beMother != null) girlObj_to_beMother.SetActive(false); // Kızı kaldır
        if (clockImage != null) clockImage.gameObject.SetActive(false); // Siyah ekranı kaldır
        if (littleGirlToTalk != null) littleGirlToTalk.SetActive(false); // Konuşulacak kız kapalı

        // Ahizenin başlangıç position ve rotation verisini kaydet
        initialHandSetPos = phoneHandSet.transform.position;
        initialHandSetRot = phoneHandSet.transform.rotation;

        // AudioSource ekle
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    

        BuildDialogTree();
    }


    protected override void Interact()
    {

        //Button Smash gamei bitir
        buttonSmashGame.EndGame(); // Telefon açma sesi bu metodun içinde

        // E tuşuna basıldığında çağrılır (PlayerInteraction raycast ile kontrol eder)
        if (dialogSystem != null && !isDialogActive && !hasInteracted)
        {
            hasInteracted = true; // Bir daha telefon kullanılamaz
            promptMessage = ""; // Prompt'u kaldır

            // Kamerayı telefona döndür
            RotateCameraToTarget(rotationDuration);

            // Telefon ahizesini kulağa götür.
            if (phoneHandSet != null)
            {
                phoneHandSet.transform.DOMove(playerHandSetRef.transform.position, 1f);
                phoneHandSet.transform.DORotateQuaternion(playerHandSetRef.transform.rotation, 1f);
            }

            dialogSystem.StartDialog(phoneCallNode);
            isDialogActive = true;
            StartCoroutine(CheckDialogEnd());
        }
    }

    void RotateCameraToTarget(float rotateDuration)
    {
        if (phoneLookAtTarget == null) return;

        // A. Karakterin Gövdesini (Body) hedefe döndür (Sadece Y ekseninde)
        // Böylece karakterin vücudu hedefe döner ama havaya bakmaz.
        if (playerBody != null)
        {
            playerBody.DOLookAt(phoneLookAtTarget.position, rotateDuration, AxisConstraint.Y);
        }

        // B. Kamerayı (Camera) hedefin biraz üstüne döndür
        // Hedef pozisyon + Offset (Yukarı)
        if (playerCamera != null)
        {
            Vector3 targetPositionWithOffset = phoneLookAtTarget.position; //+ (Vector3.up * lookOffset);
            playerCamera.DOLookAt(targetPositionWithOffset, rotateDuration);
        }

    }

    private IEnumerator CheckDialogEnd()
    {
        // Dialog paneli kapanana kadar bekle
        while (dialogSystem != null && dialogSystem.dialogPanel.activeSelf)
        {
            yield return null;
        }

        isDialogActive = false;
    }

    private IEnumerator AutoCloseDialog()
    {
        // Belirtilen süre kadar bekle
        yield return new WaitForSeconds(autoCloseDelay);

        // Dialog'u kapat
        if (dialogSystem != null)
        {
            dialogSystem.EndDialog();
        }
    }

    void BuildDialogTree()
    {
        // === PHONE CALL START ===
        phoneCallNode = DialogBuilder.CreateNode(
            "Cousin? That's you, isn't it? Cenk told me you finally escaped.",
            "Kuzen? Sensin değil mi? Cenk söyledi sonunda kaçmışsın.",
            "Cousin at Bursa"
        );

        // Engin'in cevabı
        DialogNode enginResponse1 = DialogBuilder.CreateNode(
            "Yes. I feel like I'm drunk.",
            "Evet. Sarhoş gibiyim.",
            "Engin"
        );

        // Kuzenin adresi vermesi
        DialogNode cousinAddress = DialogBuilder.CreateNode(
            "My house is in Nilüfer, Minareliçavuş. When you get there, anyone you ask can show you where I am.",
            "Benim evim Nilüfer, Minareliçavuş'ta. Oraya gelince kime sorsan beni gösterir.",
            "Cousin at Bursa"
        );

        // === OPTION 1: OTOBUS ===
        DialogNode busOption1 = DialogBuilder.CreateNode(
            "I'll catch the first bus tomorrow and make my way over.",
            "Yarın ilk otobüse atlayıp geliyorum.",
            "Engin"
        );

        DialogNode busResponse = DialogBuilder.CreateNode(
            "No. You can't use the bus. Mother will hear. They check IDs, it's too risky.",
            "Hayır. Otobüs kullanamazsın. Anne duyar. Kimliğine bakıyorlar fazla riskli.",
            "Cousin at Bursa"
        );

        // === OPTION 2: VAPUR ===
        DialogNode ferryOption1 = DialogBuilder.CreateNode(
            "I'll take the ferries going to Bursa tomorrow.",
            "Bursa'ya giden vapurlara bineceğim yarın.",
            "Engin"
        );

        DialogNode ferryResponse = DialogBuilder.CreateNode(
            "There was an accident at the dock here. Ferry services are cancelled. Plus ferries are too crowded. Too dangerous, Mother might hear.",
            "İskelede kaza oldu burada. Vapur seferleri iptal. Hem fazla kalabalık vapur. Fazla tehlikeli Anne duyabilir.",
            "Cousin at Bursa"
        );

        // === AFTER OPTIONS (SAME FOR BOTH) ===
        DialogNode howToGetThere = DialogBuilder.CreateNode(
            "How will I get there then?",
            "Nasıl geleceğim o zaman?",
            "Engin"
        );

        DialogNode needCar = DialogBuilder.CreateNode(
            "You need to find a car. A path away from crowds, a road no one knows the name of, Engin. There's no other way.",
            "Bir araba bulman gerekiyor. Kalabalıktan uzak, kimsenin adını bilmediği bir yol Engin. Başka çare yok.",
            "Cousin at Bursa"
        );

        // === SECOND SET OF OPTIONS ===
        // Option 1: No money
        DialogNode noMoneyOption1 = DialogBuilder.CreateNode(
            "How will I find a car? I have 50 YTL at most in my pocket.",
            "Nasıl bir araba bulacağım ben. Cebimde toplasan 50 YTL var",
            "Engin"
        );

        DialogNode signsWillAppear = DialogBuilder.CreateNode(
            "Don't worry, signs will appear before you. You've always been special. You'll succeed, don't be afraid.",
            "Merak etme önüne işaretler çıkacaktır. Sen her zaman özel biri oldun. Başaracaksın korkma.",
            "Cousin at Bursa"
        );

        // Option 2: Can't drive
        DialogNode cantDriveOption1 = DialogBuilder.CreateNode(
            "I can't drive well. I can't get there with a car. Plus I don't even have a car.",
            "Ben iyi araba süremem. Ben bir arabayla oraya kadar gelemem. Zaten araba da yok.",
            "Engin"
        );

        DialogNode listenToVoice = DialogBuilder.CreateNode(
            "Listen to the voice inside you. It will guide you. You are special. You've always been special. You can do it.",
            "İçindeki sesi dinle. O seni yönlendirecek. Sen özel birisin. Her zaman özel oldun. Yapabilirsin.",
            "Cousin at Bursa"
        );

        // === FINAL NODES (SAME FOR BOTH) ===
        DialogNode enginThanks = DialogBuilder.CreateNode(
            "Cousin, thank you.",
            "Kuzen, teşekkür ederim.",
            "Engin"
        );

        DialogNode goodLuck = DialogBuilder.CreateNode(
            "May your path be clear, cousin.",
            "Yolun açık olsun kuzen.",
            "Cousin at Bursa"
        );

        // === AFTER PHONE CALL - GIRL ARRIVES ===
        DialogNode girlArrives = DialogBuilder.CreateNode(
            "Sir, sir, sir!",
            "Abi, abi, abi!",
            "Little Girl"
        );

        // === MOTHER'S VOICE ===
        DialogNode motherWakeUp = DialogBuilder.CreateEndNode(
            "WAKE UP!",
            "UYAN!",
            "Mother"
        );

        // === BUILD THE TREE ===
        // Start -> Engin Response
        DialogOption phoneToEngin = DialogBuilder.CreateOption("...", "...", enginResponse1, true);
        DialogBuilder.AddOption(phoneCallNode, phoneToEngin);

        // Engin -> Cousin Address
        DialogOption enginToAddress = DialogBuilder.CreateOption("...", "...", cousinAddress, true);
        DialogBuilder.AddOption(enginResponse1, enginToAddress);

        // Cousin Address -> Options (Bus / Ferry)
        DialogOption busOpt = DialogBuilder.CreateOption(
            "I'll catch the first bus tomorrow and come.",
            "Yarın ilk otobüse atlayıp geliyorum.",
            busOption1
        );
        DialogOption ferryOpt = DialogBuilder.CreateOption(
            "I'll take the ferries going to Bursa tomorrow.",
            "Bursa'ya giden vapurlara bineceğim yarın.",
            ferryOption1
        );
        DialogBuilder.AddOption(cousinAddress, busOpt);
        DialogBuilder.AddOption(cousinAddress, ferryOpt);

        // Bus path
        DialogOption busToResponse = DialogBuilder.CreateOption("...", "...", busResponse, true);
        DialogBuilder.AddOption(busOption1, busToResponse);

        DialogOption busToHow = DialogBuilder.CreateOption("...", "...", howToGetThere, true);
        DialogBuilder.AddOption(busResponse, busToHow);

        // Ferry path
        DialogOption ferryToResponse = DialogBuilder.CreateOption("...", "...", ferryResponse, true);
        DialogBuilder.AddOption(ferryOption1, ferryToResponse);

        DialogOption ferryToHow = DialogBuilder.CreateOption("...", "...", howToGetThere, true);
        DialogBuilder.AddOption(ferryResponse, ferryToHow);

        // How to get there -> Need car
        DialogOption howToNeedCar = DialogBuilder.CreateOption("...", "...", needCar, true);
        DialogBuilder.AddOption(howToGetThere, howToNeedCar);

        // Need car -> Second set of options (No money / Can't drive)
        DialogOption noMoneyOpt = DialogBuilder.CreateOption(
            "How will I find a car?",
            "Nasıl bir araba bulacağım.",
            noMoneyOption1
        );
        DialogOption cantDriveOpt = DialogBuilder.CreateOption(
            "I can't drive well.",
            "Ben iyi araba süremem.",
            cantDriveOption1
        );
        DialogBuilder.AddOption(needCar, noMoneyOpt);
        DialogBuilder.AddOption(needCar, cantDriveOpt);

        // No money path
        DialogOption noMoneyToSigns = DialogBuilder.CreateOption("...", "...", signsWillAppear, true);
        DialogBuilder.AddOption(noMoneyOption1, noMoneyToSigns);

        DialogOption signsToThanks = DialogBuilder.CreateOption("...", "...", enginThanks, true);
        DialogBuilder.AddOption(signsWillAppear, signsToThanks);

        // Can't drive path
        DialogOption cantDriveToVoice = DialogBuilder.CreateOption("...", "...", listenToVoice, true);
        DialogBuilder.AddOption(cantDriveOption1, cantDriveToVoice);

        DialogOption voiceToThanks = DialogBuilder.CreateOption("...", "...", enginThanks, true);
        DialogBuilder.AddOption(listenToVoice, voiceToThanks);

        // Thanks -> Good luck
        DialogOption thanksToEnd = DialogBuilder.CreateOption("...", "...", goodLuck, true);
        DialogBuilder.AddOption(enginThanks, thanksToEnd);

        // Good luck -> Girl arrives (play hangup sound and activate girl)
        DialogOption goodLuckToGirl = DialogBuilder.CreateOptionWithEvent(
            "...",
            "...",
            girlArrives,
            () => {
                // Telefon kapatma sesi çal
                if (audioSource != null && phoneHangupSound != null)
                {
                    phoneHandSet.transform.DOMove(initialHandSetPos, 0.2f);
                    phoneHandSet.transform.DORotateQuaternion(initialHandSetRot, 0.2f);
                    audioSource.PlayOneShot(phoneHangupSound);
                }

                // Küçük kızı aktif et (kamera dönmez)
                if (girlObj_to_beMother != null)
                {
                    girlObj_to_beMother.SetActive(true);
                }
            },
            true
        );
        DialogBuilder.AddOption(goodLuck, goodLuckToGirl);

        // Girl -> Mother wake up (look at mother object and auto close)
        DialogOption girlToMother = DialogBuilder.CreateOptionWithEvent(
            "...",
            "...",
            motherWakeUp,
            () => {
                // Mother objesine bak
                if (girlObj_to_beMother != null)
                {
                    phoneLookAtTarget = girlObj_to_beMother.transform;
                    RotateCameraToTarget(0.25f);
                }

                // Otomatik kapanma başlat
                StartCoroutine(AutoCloseDialog());
                StartCoroutine(BlackScreenAfterJumpscare());
            },
            true
        );
        DialogBuilder.AddOption(girlArrives, girlToMother);
    }

    IEnumerator BlackScreenAfterJumpscare()
    {
        // Jumpscare anı (1 sn bekle)
        yield return new WaitForSeconds(1f);
        // Siyah ekranı aç
        clockImage.gameObject.SetActive(true);
        if (objectToActivateOnBlackscreen != null) objectToActivateOnBlackscreen.SetActive(true);
        if (skyboxMaterial != null) RenderSettings.skybox = skyboxMaterial;
        clockText.gameObject.SetActive(false);
        // 5 saniye karanlıkta bekle
        yield return new WaitForSeconds(2f);

        // Işınlanmadan hemen önce kapı açıksa kapalı konuma getir
        if (motelRoomDoor != null)
        {
            motelRoomDoor.CloseDoor();
        }
        else if (buttonSmashGame != null && buttonSmashGame.motelRoomDoor != null)
        {
            buttonSmashGame.motelRoomDoor.CloseDoor(); // Alternatif (Fallback) kontrol
        }

        // Oyuncuyu yatağa/uyanma noktasına ışınla
        if (playerObjectToTeleport != null && wakePosition != null)
        {
            // CharacterController varsa ışınlamadan önce kapatmak gerekebilir (çakışma olmaması için)
            CharacterController controller = playerObjectToTeleport.GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false;

            playerObjectToTeleport.transform.position = wakePosition.position;
            Destroy(girlObj_to_beMother);
            littleGirlToTalk.SetActive(true);
            // kuruObject.SetActive(true); // gerekirse kuru burada geri açılacak. sabah olduğunda. dışarıda spawnlanabilir.
            

            if (controller != null) controller.enabled = true;
            
        }

        //Sonraki görevi tetikler
        MissionObjective missionObj = GetComponent<MissionObjective>();
        if (missionObj != null)
        {
            // Varsa, görev sistemini tetikle!
            missionObj.OnInteracted();
        }

        clockText.text = "9:40 AM";
        clockText.gameObject.SetActive(true);
        yield return new WaitForSeconds(4f);

        //Buraya nefes nefese kalma sesleri eklenebilir veya kendi kendine rüyaymış gibi bir konuşma olabilir.
        //Yada bu kısım siyah ekranda uykudan uyanmadan enginin kendi kendine söylediği bir konuşma olabilir.

        // Siyah ekranı kapat
        if (clockImage != null)
        {
            clockImage.gameObject.SetActive(false);
        }
    }
}
