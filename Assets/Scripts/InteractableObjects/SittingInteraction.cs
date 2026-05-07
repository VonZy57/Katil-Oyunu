using DG.Tweening;
using UnityEngine;

public class SittingInteraction : Interactable
{
    [Header("Oturma/Kalkma Referansları")]
    public Transform sitReference;
    public Transform standReference;

    [Header ("Animasyon Ayarları")]
    public float transitionDuration = 1.0f;

    [Header("Oyuncu Referansı")]
    public GameObject player;

    protected bool isSitting = false;
    protected bool isMoving = false;

    protected FirstPersonController playerFPS;
    protected CharacterController playerController;
    protected PlayerControls controls;

    protected virtual void Awake()
    {
        controls = new PlayerControls(); // Input system ile oluşturulmuş PlayerControls isimli script referansı

        controls.Player.Interact.performed += ctx => InteractInputForStandUp(); //Hangi tuş hangi etkiyi yapacak. Interact butonu kalkma eylemini başlatacak.
    }

    protected virtual void Start()
    {
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

        promptMessage = "E - Otur";
    }

    protected virtual void OnEnable()
    {
        controls.Enable(); // Script aktifse dinle
    }

    protected virtual void OnDisable()
    {
        controls.Disable(); // Script aktif değilse dinlemeyi bırak. (Performans için)
    }

    protected override void Interact()
    {
        if (!isSitting && !isMoving)
        {
            SitDown();
        }

    }

    protected virtual void InteractInputForStandUp() // Kalkmayı başlatmak için
    {
        if(isSitting && !isMoving)
        {
            StandUp();
        }
    }

    protected virtual void SitDown()
    {
        isMoving = true;
        isSitting = true;
        promptMessage = "E - Kalk";

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
    
    }

    protected virtual void StandUp()
    {
        isMoving = true;
        isSitting = false;
        promptMessage = "E - Otur";

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

}