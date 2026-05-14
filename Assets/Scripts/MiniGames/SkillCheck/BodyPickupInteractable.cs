using UnityEngine;
using System.Collections;
using TMPro;

public class BodyPickupInteractable : Interactable
{
    [Header("Oyuncu Bağlantıları")]
    [Tooltip("Oyuncunun üzerindeki BodyDragController scripti buraya atanacak")]
    public BodyDragController dragController;
    private MissionObjective missionObj;

    [Header("Ön Koşul (Opsiyonel)")]
    [Tooltip("Eğer bu cesedi almak için önce bir diyalogun bitmesi gerekiyorsa buraya atayın (Örn: Cenk GetFeetDown)")]
    public GetFeetDown requiredDialog;
    [Tooltip("Ön koşul sağlanmadığında ekranda çıkacak bildirim metni")]
    public string requiredMessage = "Cenk'in ayaklarını indirmesi gerekiyor!";
    public TextMeshProUGUI notificationText;

    void Start()
    {
        promptMessage = "E - Cesedi Al";   

        missionObj = GetComponent<MissionObjective>();
    }

    protected override void Interact()
    {
        if(MissionManager.Instance != null && MissionManager.Instance.CurrentMission == missionObj.requiredMission)
        {
            // Eğer bir diyalog ön koşulu atanmışsa ve henüz bitmemişse
            if (requiredDialog != null && !requiredDialog.isDialogCompleted)
            {
                if (notificationText != null)
                {
                    StartCoroutine(ShowNotification(requiredMessage));
                }
                return; // Cesedi alma işlemini iptal et
            }

            // Taşımayı başlat
            if (dragController != null)
                dragController.StartDraggingTask();

            gameObject.SetActive(false);
        }
    }

    private IEnumerator ShowNotification(string message)
    {
        notificationText.text = message;
        notificationText.gameObject.SetActive(true);
        notificationText.transform.parent.gameObject.SetActive(true); // Paneli de aktif et
        yield return new WaitForSeconds(3f);
        notificationText.text = "";
        notificationText.gameObject.SetActive(false);
        notificationText.transform.parent.gameObject.SetActive(false); // Paneli de kapat
    }
}
