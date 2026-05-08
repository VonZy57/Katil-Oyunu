using DG.Tweening;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class HaveDinner : Interactable
{
    [SerializeField] private DialogNode putTheGamePadNode;
    [SerializeField] private DialogSystem dialogSystem;
    [SerializeField] private MissionObjective missionObj;

    [Header("Kalkma Görevi")]
    public MissionSO leaveHomeMission;
    
    [Header("Oturma/Kalkma Referansları")]
    public Transform sitReference;
    public Transform standReference;

    [Header ("Animasyon Ayarları")]
    public float transitionDuration = 1.0f;

    [Header("Oyuncu Referansı")]
    public GameObject player;
    public GameObject playerCamera;

    [Header("NPC Referansları")]
    public Transform motherTransform;
    public Transform cenkTransform;

    [Header("Tabak Referansları")]
    public List<GameObject> emptyPlates; // Boş tabakların listesi
    public List<GameObject> fullPlates;  // Dolu tabakların listesi

    private bool isSitting = false;
    private bool isMoving = false;

    private FirstPersonController playerFPS;
    private CharacterController playerController;
    private PlayerControls controls;

    private void Awake()
    {
        controls = new PlayerControls(); // Input system ile oluşturulmuş PlayerControls isimli script referansı

        controls.Player.Interact.performed += ctx => InteractInputForStandUp(); //Hangi tuş hangi etkiyi yapacak. Interact butonu kalkma eylemini başlatacak.
    }

    private void Start()
    {
        BuildDialogTree();

        if (player == null) // Atanmamışsa player nesnesini ata
        {
            player = GameObject.FindGameObjectWithTag("Player");
            Debug.Log("Player objesi otomatik olarak atandı");
        }   
        
        if(player != null) // Oyuncu kontrolleri refransları (Hareketleri kısıtlamak için)
        {
            playerFPS = player.GetComponent<FirstPersonController>();
            playerController = player.GetComponent<CharacterController>(); 
        }
        
    }

    void Update()
    {   
        if (MissionManager.Instance == null) return;

        if (MissionManager.Instance.CurrentMission == missionObj.requiredMission && !isSitting)
            promptMessage = "E - Otur";
        else if (isSitting && MissionManager.Instance.CurrentMission == leaveHomeMission)
            promptMessage = "E - Kalk";
        else
            promptMessage = "";
    }

    private void OnEnable()
    {
        controls.Enable(); // Script aktifse dinle
    }

    private void OnDisable()
    {
        controls.Disable(); // Script aktif değilse dinlemeyi bırak. (Performans için)
    }

    protected override void Interact()
    {
        if (!isSitting && !isMoving && MissionManager.Instance.CurrentMission == missionObj.requiredMission)
        {
            StartCoroutine(SitDown());
        }

    }

    private void InteractInputForStandUp() // Kalkmayı başlatmak için
    {
        if(isSitting && !isMoving && MissionManager.Instance.CurrentMission == leaveHomeMission)
        {
            StandUp();
        }
    }
    
    private IEnumerator SitDown()
    {
        isMoving = true;
        isSitting = true;

        if (emptyPlates != null && emptyPlates.Count > 0)
        {
            foreach (GameObject plate in emptyPlates)
                if (plate != null) plate.SetActive(true); // Oturma işlemi başladığında boş tabakları aktif et
        }

        if (playerController)
            playerController.enabled = false;

        if (playerFPS)
            playerFPS.enabled = false;


        player.transform.DOMove(sitReference.position, transitionDuration).SetEase(Ease.InOutSine); // Oturma pozisyonuna doğru hareket et
        player.transform.DORotateQuaternion(sitReference.rotation, transitionDuration).SetEase(Ease.InOutSine) // Oturma rotasyonuna doğru dön
            .OnComplete(() => // Dönme ve hareket eylemleri tamamlandığında ..... yap
            {
                isMoving = false; // Hareket bitti
                if (playerFPS)
                {
                    playerFPS.enabled = true;
                    playerFPS.SetSittingState(true);
                }
            });
        yield return new WaitForSeconds(2f);
        dialogSystem.StartDialog(putTheGamePadNode);
        RotateCameraToSpeaker(motherTransform);

        // Diyalog panelinin aktif olmasını (açılmasını) bekle
        yield return new WaitUntil(() => dialogSystem.dialogPanel.activeSelf);
        
        // Diyalog panelinin kapanmasını (diyaloğun bitmesini) bekle
        yield return new WaitUntil(() => !dialogSystem.dialogPanel.activeSelf);

        // Diyalog tamamen bittikten sonra tabakları değiştir
        if (emptyPlates != null && emptyPlates.Count > 0)
        {
            foreach (GameObject plate in emptyPlates)
                if (plate != null) plate.SetActive(false);
        }
        if (fullPlates != null && fullPlates.Count > 0)
        {
            foreach (GameObject plate in fullPlates)
                if (plate != null) plate.SetActive(true);
        }
    }

    private void StandUp()
    {
        isMoving = true;
        isSitting = false;

        if(playerFPS)
        {
            playerFPS.SetSittingState(false);
            playerFPS.enabled = false;
        }

        player.transform.DOMove(standReference.position, transitionDuration).SetEase(Ease.InOutSine); // Kalkma pozisyonuna doğru hareket et
        player.transform.DORotateQuaternion(standReference.rotation, transitionDuration).SetEase(Ease.InOutSine) // Kalkma rotasyonuna doğru dön
            .OnComplete(() => // Dönme ve hareket eylemleri tamamlandığında ..... yap
            {
                isMoving = false; // Hareket bitti
                if (playerController) playerController.enabled = true;
                if (playerFPS) playerFPS.enabled = true;
            });
    }

    void RotateCameraToSpeaker(Transform speakerTransform)
    {
        if (playerFPS != null) playerFPS.enabled = false; // DOTween ile çakışmaması için kontrolleri geçici kapat

        playerCamera.transform.DOKill(); // Varsa önceki kamera animasyonunu durdur
        playerCamera.transform.DOLookAt(speakerTransform.position, 1f).OnComplete(() =>
        {
            if (playerFPS != null)
            {
                playerFPS.SyncCameraRotation(); // Kameranın yeni açısını FPS controller'a kaydet
                playerFPS.enabled = true; // Kontrolleri geri ver
            }
        });
    }

    void BuildDialogTree()
    {
        putTheGamePadNode = DialogBuilder.CreateNode
        ("Put that damn thing and eat. Thankless bastard! Do you know how much mother worked hard to put food on the table?",
        "Anne masaya yemek koymak için ne kadar debelendi haberiniz var mı?",
        "Anne");

        DialogNode brotherKilledNode = DialogBuilder.CreateNode
        ("Big brother killed betrayers well. Right mother? Liars!",
        "Abim çok güzel öldürdü hainleri. Değil mi anne?",
        "Cenk");

        DialogNode motherSaysLiarsNode = DialogBuilder.CreateNode
        ("Liars! I'll shit on the leg of anyone who tries to insult our family.",
        "Yalancılar! Ailemize dil uzatmaya çalışnanın bacağına sıçarım.",
        "Anne");

        DialogNode cenkSaysEnginStrong = DialogBuilder.CreateNode
        ("Big brother is so strong. Isn't he mother? Engin is the strongest",
        "Abim çok güçlü! Değil mi anne? En güçlü Engin.",
        "Cenk");

        DialogNode motherSaysShutNode = DialogBuilder.CreateEndNode
        ("Shut the f*ck up. Eat!",
        "Kapa çeneni. Yemeğini ye!",
        "Anne");
        
        DialogOption silentOption = DialogBuilder.CreateOptionWithEvent("...", "...", brotherKilledNode,
        () => 
            {
                RotateCameraToSpeaker(cenkTransform);
            }, true);
        DialogOption silentOption2 = DialogBuilder.CreateOptionWithEvent("...", "...", motherSaysLiarsNode,
        () =>
            {
                RotateCameraToSpeaker(motherTransform);
            }, true);
        DialogOption silentOption3 = DialogBuilder.CreateOptionWithEvent("...", "...", cenkSaysEnginStrong,
        () =>
            {
                RotateCameraToSpeaker(cenkTransform);
            }, true);
        DialogOption silentOption4 = DialogBuilder.CreateOptionWithEvent("...", "...", motherSaysShutNode,
        () =>
            {
                RotateCameraToSpeaker(motherTransform);
            }, true);

        DialogBuilder.AddOption(putTheGamePadNode, silentOption);
        DialogBuilder.AddOption(brotherKilledNode, silentOption2);
        DialogBuilder.AddOption(motherSaysLiarsNode, silentOption3);
        DialogBuilder.AddOption(cenkSaysEnginStrong, silentOption4);

    }

}