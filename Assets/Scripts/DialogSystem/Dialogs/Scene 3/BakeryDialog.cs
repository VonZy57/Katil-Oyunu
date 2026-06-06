using UnityEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class BakeryDialog : SittingInteraction
{
    [SerializeField] private DialogSystem dialogSystem;
    [SerializeField] private DialogNode bakeryStartNode;
    [SerializeField] private MissionObjective missionObj;
    [SerializeField] private OrderSubtitle orderSubtitle; // Sipariş altyazısının bitip bitmediğini kontrol etmek için
    [SerializeField] private GameObject notificationTextObject; // Altyazı metni referansı
    [SerializeField] private TextMeshProUGUI notificationText; // Altyazı metni referansı

    [Header("Kamera ve Bakış Referansları")]
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private Transform amcaTransform;

    [Header("Look At Referansları")]
    [SerializeField] private Transform amcaLookAt;         // Amca'ya kamera bakış hedefi
    [SerializeField] private Transform kamilEfendiLookAt;  // Kamil Efendi'ye kamera bakış hedefi

    [Header("Kamil Efendi Animasyon Referansları")]
    [SerializeField] private Transform kamilEfendiTransform;
    [SerializeField] private Transform kamilTablePosition;
    private Vector3 kamilOriginalPosition;
    private Quaternion kamilOriginalRotation;

    [Header("Yiyecek Referansları")]
    [SerializeField] private GameObject plateAmca;
    [SerializeField] private GameObject plateEngin;

    public bool IsUncleAnimationFinished { get; private set; } = false;
    private bool canStandUp = false;

    protected override void Start()
    {
        base.Start(); // Player, playerFPS ve playerController atamalarını yapar
        BuildDialog();
    }

    protected override void Update()
    {
        base.Update();
        if (MissionManager.Instance != null && missionObj != null)
        {
            // Yalnızca görev "GoToBakery (3)" iken sandalyeye oturma eylemi çıksın
            // Ve OrderSubtitle atandıysa altyazıların bitmesini beklesin
            if (MissionManager.Instance.CurrentMission == missionObj.requiredMission && !isSitting && !isMoving && (orderSubtitle == null || orderSubtitle.isFinished))
                promptMessage = "E - Sit Down";
            else if (isSitting && canStandUp)
            {
                promptMessage = "E - Stand Up";
                notificationTextObject.SetActive(true);
                notificationText.text = "E - Stand Up";
            }       
            else
                promptMessage = "";
                notificationText.text = "";
        }
    }

    protected override void Interact()
    {
        if (!isSitting && !isMoving && MissionManager.Instance.CurrentMission == missionObj.requiredMission && (orderSubtitle == null || orderSubtitle.isFinished))
        {
            SitDown();
        }
    }

    protected override void InteractInputForStandUp()
    {
        if (canStandUp)
        {
            base.InteractInputForStandUp();
        }
    }

    protected override void SitDown()
    {
        isMoving = true;
        isSitting = true;
        promptMessage = ""; // Kalkma yazısı çıkmasın

        if (playerController)
            playerController.enabled = false;

        if (playerFPS)
            playerFPS.enabled = false;

        player.transform.DOMove(sitReference.position, transitionDuration).SetEase(Ease.InOutSine);
        player.transform.DORotateQuaternion(sitReference.rotation, transitionDuration).SetEase(Ease.InOutSine)
            .OnComplete(() => 
            {
                isMoving = false; 
                if (playerFPS)
                {
                    playerFPS.SetSittingState(true);
                    // Diyalog başlayacağı için hareket kontrollerini açmıyoruz!
                }

                // 3. Görev (GoToBakery) sandalyeye oturunca tamamlanır ve 4. göreve (TalkToUncle) geçilir
                if (missionObj != null)
                {
                    missionObj.OnInteracted();
                }

                // Sonrasında otomatik olarak amca animasyonu ve diyalog başlar
                StartBakeryDialog();
            });
    }

    public void StartBakeryDialog()
    {
        if (dialogSystem != null && bakeryStartNode != null)
        {
            if (playerFPS != null) playerFPS.enabled = false;
            StartCoroutine(PreDialogAnimationSequence());
        }
    }

    void RotateCameraToSpeaker(Transform speakerTransform)
    {
        if (playerCamera == null || speakerTransform == null) return;
        Camera cam = playerCamera.GetComponent<Camera>()
            ?? playerCamera.GetComponentInChildren<Camera>()
            ?? Camera.main;
        Transform camTransform = cam != null ? cam.transform : playerCamera.transform;
        camTransform.DOKill();
        camTransform.DOLookAt(speakerTransform.position, 1f);
    }

    private IEnumerator PreDialogAnimationSequence()
    {
        // Oyuncu oturur oturmaz kamerayı Amca'ya çevir
        RotateCameraToSpeaker(amcaLookAt);

        Debug.Log("Placeholder Animasyon: Engin sandalyeye oturduktan sonra Amca'nın yapacağı hareket (Örn: Çayından yudum alma, nefes verme vs.)");
        yield return new WaitForSeconds(2f);
        Debug.Log("Amca'nın diyalog öncesi animasyonu bitti.");

        StartCoroutine(DialogSequence());
        StartCoroutine(KamilBringsAnimations()); // Diyalogla aynı anda Kamil Efendi'nin poğaçaları getirip masaya koyma animasyonunu başlat
    }

    private IEnumerator KamilBringsAnimations()
    {
        Debug.Log("Animasyon: Kamil Efendi tepsideki poğaçaları getirip masaya koyuyor...");
        
        if (kamilEfendiTransform != null && kamilTablePosition != null)
        {
            kamilOriginalPosition = kamilEfendiTransform.position;
            kamilOriginalRotation = kamilEfendiTransform.rotation;

            // Kâmil Efendi masaya doğru baksın ve yürüsün; kamera da Kamil'e dönsün
            kamilEfendiTransform.DOKill();
            kamilEfendiTransform.DOLookAt(kamilTablePosition.position, 0.5f, AxisConstraint.Y);
            kamilEfendiLookAt.gameObject.GetComponentInParent<Animator>().SetTrigger("CarryTrigger"); // Kamil Efendi'nin yürüyüş animasyonunu tetikle
            RotateCameraToSpeaker(kamilEfendiLookAt);
            Vector3 targetPosition = new Vector3(kamilTablePosition.position.x, kamilOriginalPosition.y, kamilTablePosition.position.z);
            yield return kamilEfendiTransform.DOMove(targetPosition, 3f).SetEase(Ease.InOutSine).WaitForCompletion();

            // Tabakları bırakma bekleme süresi
            yield return new WaitForSeconds(0.5f);
            kamilEfendiLookAt.gameObject.GetComponentInParent<Animator>().SetTrigger("IdleTrigger"); // Kamil Efendi'nin poğaçaları bırakma animasyonunu tetikle
            plateAmca.SetActive(true);
            plateEngin.SetActive(true);
            Debug.Log("Kamil Efendi poğaçaları masaya bıraktı.");
            yield return new WaitForSeconds(0.5f);

            // Geri dönüş; kamera Amca'ya dönsün
            kamilEfendiTransform.DOLookAt(kamilOriginalPosition, 0.5f, AxisConstraint.Y);
            RotateCameraToSpeaker(amcaLookAt);
            kamilEfendiLookAt.gameObject.GetComponentInParent<Animator>().SetTrigger("WalkingTrigger"); // Kamil Efendi'nin geri yürüyüş animasyonunu tetikle
            yield return kamilEfendiTransform.DOMove(kamilOriginalPosition, 3f).SetEase(Ease.InOutSine).WaitForCompletion();

            // Orijinal rotasyona dön
            kamilEfendiTransform.DORotateQuaternion(kamilOriginalRotation, 0.5f);
            kamilEfendiLookAt.gameObject.GetComponentInParent<Animator>().SetTrigger("IdleTrigger"); // Kamil Efendi'nin normal idle animasyonuna dönmesini tetikle
        }
        else
        {
            plateAmca.SetActive(true);
            plateEngin.SetActive(true);
            Debug.LogWarning("Kamil Efendi referansları eksik, poğaçalar direkt masada belirdi.");
        }
    }

    private IEnumerator DialogSequence()
    {
        dialogSystem.StartDialog(bakeryStartNode);

        yield return null;
        yield return new WaitUntil(() => dialogSystem.dialogPanel.activeSelf);
        yield return new WaitUntil(() => !dialogSystem.dialogPanel.activeSelf);

        // Diyalog bittiğinde hemen oyuncuya kontrolü veriyoruz ki tabakla etkileşime geçebilsin.
        if (playerFPS != null)
        {
            playerFPS.SyncCameraRotation();
            playerFPS.enabled = true; 
        }

        Debug.Log("Placeholder Animasyon: BakeryDialog bittikten sonra çalışacak animasyon (Örn: Amca poğaçadan bir ısırık alır)");
        yield return new WaitForSeconds(2f);
        Debug.Log("BakeryDialog sonrası animasyon bitti.");
        IsUncleAnimationFinished = true; // Amcanın animasyonu bitti, diğer script artık diyaloğu başlatabilir.
    }

    public void EnableStandUp()
    {
        canStandUp = true;
    }

    void BuildDialog()
    {
        // ==========================================
        // BAŞLANGIÇ
        // ==========================================
        bakeryStartNode = DialogBuilder.CreateNode(
            "Ughhh...",
            "Offff...",
            "Amca"
        );

        // ==========================================
        // 1. SEÇİM DALLLARI: Are you okay / Who was that?
        // ==========================================
        DialogNode enginAreYouOkay = DialogBuilder.CreateNode(
            "You look pretty banged up, old man. Are you okay?",
            "Amca epeyce hırpalanmış gibisin. İyi misin? ",
            "Engin"
        );
        DialogNode oldManBeenThrough = DialogBuilder.CreateNode(
            "If you only knew what I've been through back in the day. This is nothing.",
            "Ben zamanında neler gördüm bir bilsen. Bu hiçbir şey.",
            "Amca"
        );
        DialogNode enginSeeDoctor = DialogBuilder.CreateNode(
            "You should see a doctor if you need to.",
            "Doktora git istersen.",
            "Engin"
        );
        DialogNode oldManNonsense = DialogBuilder.CreateEndNode(
            "Nonsense, no way. What for? I never take a punch I can't get back up from.",
            "Yok canım daha neler. Ne hacet var. Ben sonrasında ayağa kalkamayacağım hiçbir yumruğu yemem.",
            "Amca"
        );

        DialogBuilder.AddOption(enginAreYouOkay, DialogBuilder.CreateOption("...", "...", oldManBeenThrough, true));
        DialogBuilder.AddOption(oldManBeenThrough, DialogBuilder.CreateOption("...", "...", enginSeeDoctor, true));
        DialogBuilder.AddOption(enginSeeDoctor, DialogBuilder.CreateOption("...", "...", oldManNonsense, true));

        // Branch 1 - 2. Seçenek
        DialogNode enginWhoPunk = DialogBuilder.CreateNode(
            "Amca, sorry for asking, but who was that 'Lavuk'?",
            "Amca ayıptır sorması kimdi o Lavuk?",
            "Engin"
        );
        DialogNode oldManLongStory = DialogBuilder.CreateNode(
            "Long story. Just some punk.",
            "Uzun hikâye. Lavuğun biri işte.",
            "Amca"
        );
        DialogNode enginFilthyMouth = DialogBuilder.CreateNode(
            "He had a filthy mouth.",
            "Ağzı pis biriydi.",
            "Engin"
        );
        DialogNode oldManSoap = DialogBuilder.CreateEndNode(
            "If you gathered all the soap in the Eminönü, you couldn't wash that mouth clean. Let's eat something, I'll tell you all thats happened, don't worry.",
            "Eminönü’ndeki tüm sabunları toplasan onun ağzını temizleyemezsin. Bir şeyler yiyelim anlatacağım sana olanı biteni merak etme.",
            "Amca"
        );

        DialogBuilder.AddOption(enginWhoPunk, DialogBuilder.CreateOption("...", "...", oldManLongStory, true));
        DialogBuilder.AddOption(oldManLongStory, DialogBuilder.CreateOption("...", "...", enginFilthyMouth, true));
        DialogBuilder.AddOption(enginFilthyMouth, DialogBuilder.CreateOption("...", "...", oldManSoap, true));

        // Kamil Efendi mırıldandıktan sonra oyuncu seçim yapar
        DialogBuilder.AddOption(bakeryStartNode, DialogBuilder.CreateOption("Are you okay, Amca?", "İyi misin Amca?", enginAreYouOkay));
        DialogBuilder.AddOption(bakeryStartNode, DialogBuilder.CreateOption("Who was that?", "Kimdi o?", enginWhoPunk));
    }
}
