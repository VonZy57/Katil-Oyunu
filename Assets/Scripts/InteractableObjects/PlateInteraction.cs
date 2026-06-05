using System.Collections;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class PlateInteraction : FoodInteractable
{
    [Header("Referanslar")]
    [Tooltip("Yutkunma animasyonu için oyuncu camerasına uygulanacak animasyon.")]
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private GameObject player;

    [Header("Görev Referansları")]
    [SerializeField] private MissionSO talkWithCenkSO;
    
    protected override void DoEventsWhileEating()
    {
        //Basit çiğneme animasyonu
        if (playerCamera != null)
        {
            player.GetComponent<FirstPersonController>().enabled = false;

            // DOTween Sequence oluştur (Animasyonları sıraya dizmek için)
            DG.Tweening.Sequence eatSequence = DOTween.Sequence();

            // 1. Kamera 15 derece yukarı kalksın (1 saniyede)
            eatSequence.Append(playerCamera.transform.DORotate(new Vector3(-20f, 0f, 0f), 1f, RotateMode.LocalAxisAdd).SetEase(Ease.InOutQuad));

            // 2. Çiğneme efekti (4 saniye boyunca)
            // DOPunchRotation kameranın X ekseninde ufak ritmik vuruşlar yapmasını sağlar (kafa sallama/çiğneme hissi).
            // 1 saniyelik bir sarsıntıyı 4 kere tekrar ederek toplam 4 saniye boyunca yukarıda çiğniyor gibi görünür.
            //eatSequence.Append(playerCamera.transform.DOPunchRotation(new Vector3(3f, 0f, 0f), 1f, 1, 0.5f).SetLoops(4));
            eatSequence.Append(playerCamera.transform.DOShakeRotation(1f, new Vector3(1.5f, 0f, 0f), 3, 20f).SetLoops(3));

            // 3. Kamera orijinal pozisyonuna geri dönsün (+15 derece ekleyerek çıktığımız kadar iniyoruz)
            //eatSequence.Append(playerCamera.transform.DORotate(new Vector3(15f, 0f, 0f), 1f, RotateMode.LocalAxisAdd).SetEase(Ease.InOutQuad));

            // 4. Tüm süreç bittiğinde kontrolleri tekrar oyuncuya ver
            eatSequence.OnComplete(() => 
            {
                FirstPersonController fpsController = player.GetComponent<FirstPersonController>();
                fpsController.AddXRotation(-20f); // Animasyonda verdiğimiz -20 derecelik açıyı Controller'a da bildiriyoruz
                fpsController.enabled = true;
            });
        }
            
    }

    protected override void OnFinishedEating()
    {
        if(gameObject.GetComponent<TalkWithCenkOnTable>() != null)
            gameObject.GetComponent<TalkWithCenkOnTable>().StartTheDialog();

        // Yemek yeme işlemi bittiğinde bağlı olan görevi tamamla
        MissionObjective missionObj = GetComponent<MissionObjective>();
        if (missionObj != null)
        {
            missionObj.OnInteracted();
            missionObj.requiredMission = talkWithCenkSO;
        }
    }
}
