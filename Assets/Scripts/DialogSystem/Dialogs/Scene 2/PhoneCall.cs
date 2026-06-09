using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class PhoneCall : Interactable
{
    [Header("References")]
    public Transform phoneLookAtTarget; //Telefona bakış hedefi // Sonrasında hedef anneye geçecek ve anneye dönecek
    public Transform playerBody;      // Karakterin ana gövdesi (Sağa/Sola dönüş için)
    public Transform playerCamera;    // Karakterin kamerası (Yukarı/Aşağı bakış için)
    public GameObject phoneHandSet; // Telefonun ahizesi
    public Transform playerHandSetRef; // Ahizenin gideceği referans
    public Transform hangUpRef; // Oyuncunun telefonu açtığında DOTween ile gideceği konum ve rotasyon
    [SerializeField] private Vector3 initialHandSetPos; // Telefon kapanınca ahizenin geri döneceği yer
    [SerializeField] private Quaternion initialHandSetRot;

    [Header("Jumpscare & Wake Up References")]
    public GameObject MotherFacedGirl; // Küçük kız GameObject'i (başlangıçta deaktif)
    public Transform jumpscareLookTarget; // Jumpscare esnasında kameranın bakacağı nokta
    public GameObject littleGirlToTalk;
    public Transform wakePosition; // Uyanılacak pozisyon
    public GameObject[] objectsToDisableOnWakeUp; // Işınlandıktan sonra kapatılacak objeler

    [Header("UI References")]
    public GameObject playerObjectToTeleport; // Işınlanma için
    public Image clockImage;
    public TextMeshProUGUI clockText;
    public GameObject objectToActivateOnBlackscreen; // ClockImage açılınca aktif edilecek obje
    public Material skyboxMaterial; // ClockImage açılınca geçilecek skybox materyali
    

    [Header("Rotation Settings")]
    public float rotationDuration = 2f;  // Telefona dönüş hızı

    [Header("Audio Settings")]
    public AudioClip phoneHangupSound; // Telefon kapatma sesi
    public AudioClip phonePickupSound; // Telefon açma sesi
    private AudioSource audioSource;

    [Header("TV Video Settings")]
    public VideoPlayer tvVideoPlayer; // TV'deki VideoPlayer bileşeni
    public Material nightMaterial;
    public Material dayMaterial;


    [Header("Diyalog Sesleri")]
    [SerializeField] private List<SpeakerAudio> speakerAudios;
    [Header("Cousin at Bursa Sesleri")]
    [SerializeField] private AudioClip clip_cousin_hello;
    [SerializeField] private AudioClip clip_cousin_address;
    [SerializeField] private AudioClip clip_cousin_noBus;
    [SerializeField] private AudioClip clip_cousin_noFerry;
    [SerializeField] private AudioClip clip_cousin_needCar;
    [SerializeField] private AudioClip clip_cousin_signs;
    [SerializeField] private AudioClip clip_cousin_listen;
    [SerializeField] private AudioClip clip_cousin_goodLuck;
    [Header("Engin Sesleri")]
    [SerializeField] private AudioClip clip_engin_yes;
    [SerializeField] private AudioClip clip_engin_busOpt;
    [SerializeField] private AudioClip clip_engin_ferryOpt;
    [SerializeField] private AudioClip clip_engin_howToGet;
    [SerializeField] private AudioClip clip_engin_noMoney;
    [SerializeField] private AudioClip clip_engin_cantDrive;
    [SerializeField] private AudioClip clip_engin_thanks;
    [Header("Little Girl Sesleri")]
    [SerializeField] private AudioClip clip_girl_abiabi;
    [Header("Jumpscare Sesleri")]
    [SerializeField] private AudioClip clip_wakeUp;
    [SerializeField] private AudioClip clip_jumpscare;

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
        if (MotherFacedGirl != null) MotherFacedGirl.SetActive(false); // Kızı kaldır
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
        buttonSmashGame.EndGame();

        // E tuşuna basıldığında çağrılır (PlayerInteraction raycast ile kontrol eder)
        if (dialogSystem != null && !isDialogActive && !hasInteracted)
        {
            hasInteracted = true; // Bir daha telefon kullanılamaz
            promptMessage = ""; // Prompt'u kaldır

            StartCoroutine(PhoneInteractionSequence());
        }
    }

    private IEnumerator PhoneInteractionSequence()
    {
        // Oyuncuyu HangUpRef konumuna ve rotasyonuna DOTween ile ilerlet
        if (playerObjectToTeleport != null && hangUpRef != null)
        {
            CharacterController cc = playerObjectToTeleport.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            // Hedefin sadece X ve Z koordinatlarını alıp, karakterin kendi Y (yükseklik) değerini koruyoruz
            Vector3 targetPos = new Vector3(hangUpRef.position.x, playerObjectToTeleport.transform.position.y, hangUpRef.position.z);
            playerObjectToTeleport.transform.DOMove(targetPos, 1f).SetEase(Ease.InOutSine);
            playerObjectToTeleport.transform.DORotateQuaternion(hangUpRef.rotation, 1f).SetEase(Ease.InOutSine);

            // Oyuncu ilerlerken kameraya yürüme (Head Bobbing) hissi vermek için Y ekseninde sarsıntı ekle
            if (playerCamera != null)
            {
                playerCamera.DOPunchPosition(new Vector3(0f, -0.08f, 0f), 1f, 3, 0.5f);
            }

            // Yürüme hareketinin bitmesini (1 saniye) bekle
            yield return new WaitForSeconds(1f);

            if (cc != null) cc.enabled = true;
        }
        
        // Hareket tamamen bittikten sonra kamerayı telefona döndür
        RotateCameraToTarget(rotationDuration);

        // Küçük kızı aktif et ve animasyonu tetikle
        if (MotherFacedGirl != null)
        {
            MotherFacedGirl.SetActive(true);
        }

        // Telefon ahizesini kulağa götür.
        if (phoneHandSet != null)
        {
            phoneHandSet.transform.DOMove(playerHandSetRef.transform.position, 1f);
            phoneHandSet.transform.DORotateQuaternion(playerHandSetRef.transform.rotation, 1f);
        }

        // Telefon açma sesi çal
        if (audioSource != null && phonePickupSound != null)
        {
            audioSource.PlayOneShot(phonePickupSound);
        }

        dialogSystem.SetSpeakers(speakerAudios);
        dialogSystem.StartDialog(phoneCallNode);
        isDialogActive = true;
        StartCoroutine(CheckDialogEnd());
    }

    void RotateCameraToTarget(float rotateDuration)
    {
        if (phoneLookAtTarget == null) return;

        // A. Karakterin Gövdesini (Body) hedefe döndür (Sadece Y ekseninde)
        // Böylece karakterin vücudu hedefe döner ama havaya bakmaz.
        if (playerBody != null)
        {
            playerBody.DOKill();
            playerBody.DOLookAt(phoneLookAtTarget.position, rotateDuration, AxisConstraint.Y).SetEase(Ease.InOutSine);
        }

        if (playerCamera != null)
        {
            playerCamera.DOKill();
            playerCamera.DOLookAt(phoneLookAtTarget.position, rotateDuration).SetEase(Ease.InOutSine);
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
        StartCoroutine(JumpscareSequence());
    }

    private IEnumerator TrackJumpscareTarget(float duration)
    {
        if (jumpscareLookTarget == null) yield break;

        // Slerp'in FirstPersonController ile savaşmasını KESİN olarak önlemek için kapatıyoruz
        FirstPersonController fpc = playerObjectToTeleport != null ? playerObjectToTeleport.GetComponent<FirstPersonController>() : null;
        if (fpc != null) fpc.enabled = false;

        float timer = 0f;
        while (timer < duration)
        {
            // FPC kapalı olduğu için artık frame sonunu beklemeye gerek yok, doğrudan Update hızında çalışır
            yield return null;

            if (playerCamera != null && playerBody != null)
            {
                // 1. Önce gövdeyi hedefe döndür (Sadece Y ekseni)
                Vector3 bodyDir = new Vector3(jumpscareLookTarget.position.x - playerBody.position.x, 0, jumpscareLookTarget.position.z - playerBody.position.z).normalized;
                if (bodyDir != Vector3.zero) 
                    playerBody.rotation = Quaternion.Slerp(playerBody.rotation, Quaternion.LookRotation(bodyDir), Time.deltaTime * 15f);

                // 2. Gövde döndükten SONRA kameranın yeni pozisyonundan hedefe olan yönü hesapla
                Vector3 dirToTarget = (jumpscareLookTarget.position - playerCamera.position).normalized;
                
                // 3. Kamerayı tam hedefe döndür
                if (dirToTarget != Vector3.zero) 
                    playerCamera.rotation = Quaternion.Slerp(playerCamera.rotation, Quaternion.LookRotation(dirToTarget), Time.deltaTime * 15f);
            }
            timer += Time.deltaTime;
        }
    }

    private IEnumerator JumpscareSequence()
    {
        Sequence jumpscareSeq = DOTween.Sequence();

        jumpscareSeq.Append(playerBody.DOLookAt(jumpscareLookTarget.position, 0.15f, AxisConstraint.Y).SetEase(Ease.InOutSine));
        jumpscareSeq.Join(playerCamera.DOLookAt(jumpscareLookTarget.position, 0.15f).SetEase(Ease.InOutSine));
        jumpscareSeq.Join(playerCamera.DOShakePosition(0.15f, strength: 0.5f, vibrato:10, randomness:90).SetEase(Ease.InOutSine));
        jumpscareSeq.OnComplete(() => {Debug.Log("Jumpscare Rotasyonu Tamamlandı");});
        yield return jumpscareSeq.WaitForCompletion();
        
        // Dönüş biter bitmez Jumpscare animasyonunu tetikle!
        if (MotherFacedGirl != null)
        {
            Animator anim = MotherFacedGirl.GetComponent<Animator>();
            if (anim != null) anim.SetTrigger("JumpScareTrigger");
        }

        // "UYAN!" yazısını ekranda altyazı şeklinde göster
        if (clockText != null)
        {
            bool isTurkish = dialogSystem != null ? dialogSystem.GetCurrentLanguage() : true;
            clockText.text = isTurkish ? "UYAN!" : "WAKE UP!";
            clockText.gameObject.SetActive(true);
        }

        if(audioSource != null && clip_jumpscare != null)
            audioSource.PlayOneShot(clip_jumpscare);

        if (audioSource != null && clip_wakeUp != null)
            audioSource.PlayOneShot(clip_wakeUp);

        // Jumpscare animasyonu süresince kameranın hareket eden yüzü takip etmesini sağla
        StartCoroutine(TrackJumpscareTarget(1f));

        // Oyuncunun jumpscare'i ve yazıyı göreceği süre
        yield return new WaitForSeconds(1f);

        // Siyah ekranı aç
        if (clockImage != null) clockImage.gameObject.SetActive(true);
        if (objectToActivateOnBlackscreen != null) objectToActivateOnBlackscreen.SetActive(true);
        if (skyboxMaterial != null) RenderSettings.skybox = skyboxMaterial;
        
        // "UYAN!" yazısını kaldır
        if (clockText != null) clockText.gameObject.SetActive(false);

        // 2 saniye karanlıkta bekle
        yield return new WaitForSeconds(3f);

        // Işınlanmadan hemen önce kapı açıksa kapalı konuma getir
        if (motelRoomDoor != null)
        {
            motelRoomDoor.CloseDoor();
        }
        else if (buttonSmashGame != null && buttonSmashGame.motelRoomDoor != null)
        {
            buttonSmashGame.motelRoomDoor.CloseDoor(); // Alternatif (Fallback) kontrol
        }

        yield return new WaitForSeconds(2f); // Kapının kapanma animasyonunu bekle

        // Oyuncuyu yatağa/uyanma noktasına ışınla
        if (playerObjectToTeleport != null && wakePosition != null)
        {
            // CharacterController varsa ışınlamadan önce kapatmak gerekebilir (çakışma olmaması için)
            CharacterController controller = playerObjectToTeleport.GetComponent<CharacterController>();
            FirstPersonController fpc = playerObjectToTeleport.GetComponent<FirstPersonController>();
            if (controller != null) controller.enabled = false;

            playerObjectToTeleport.transform.position = wakePosition.position;
            Destroy(MotherFacedGirl);
            if (littleGirlToTalk != null) littleGirlToTalk.SetActive(true);

            foreach (var obj in objectsToDisableOnWakeUp)
                obj?.SetActive(false);

            if (controller != null) controller.enabled = true;
            if (fpc != null)
            {
                fpc.enabled = true;
                fpc.SyncCameraRotation(); // Uyanınca kamerada sıçrama olmaması için
            }
        }

        // Sonraki görevi tetikler
        MissionObjective missionObj = GetComponent<MissionObjective>();
        if (missionObj != null)
        {
            missionObj.OnInteracted();
        }

        // Saat metnini eski formatına (9:40 AM) döndür ve göster
        if (clockText != null)
        {
            clockText.text = "9:40 AM";
            clockText.gameObject.SetActive(true);
        }
        
        yield return new WaitForSeconds(4f);

        tvVideoPlayer.gameObject.GetComponent<MeshRenderer>().material = dayMaterial;
        // Siyah ekranı kapat
        if (clockImage != null) clockImage.gameObject.SetActive(false);
        if (clockText != null) clockText.gameObject.SetActive(false);
        tvVideoPlayer.enabled = true;
        tvVideoPlayer.Play();
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
        DialogNode girlArrives = DialogBuilder.CreateEndNode(
            "Sir, sir, sir!",
            "Abi, abi, abi!",
            "Little Girl"
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
            },
            true
        );
        DialogBuilder.AddOption(goodLuck, goodLuckToGirl);

        phoneCallNode.voiceClip = clip_cousin_hello;
        enginResponse1.voiceClip = clip_engin_yes;
        cousinAddress.voiceClip = clip_cousin_address;
        busOption1.voiceClip = clip_engin_busOpt;
        busResponse.voiceClip = clip_cousin_noBus;
        ferryOption1.voiceClip = clip_engin_ferryOpt;
        ferryResponse.voiceClip = clip_cousin_noFerry;
        howToGetThere.voiceClip = clip_engin_howToGet;
        needCar.voiceClip = clip_cousin_needCar;
        noMoneyOption1.voiceClip = clip_engin_noMoney;
        signsWillAppear.voiceClip = clip_cousin_signs;
        cantDriveOption1.voiceClip = clip_engin_cantDrive;
        listenToVoice.voiceClip = clip_cousin_listen;
        enginThanks.voiceClip = clip_engin_thanks;
        goodLuck.voiceClip = clip_cousin_goodLuck;
        girlArrives.voiceClip = clip_girl_abiabi;
    }

}