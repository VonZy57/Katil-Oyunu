using UnityEngine;
using System.Collections;
using TMPro;
using DG.Tweening;

public class HouseDoorInteraction : Interactable
{
    [Header("Bağlantılar")]
    public MoneyInteraction moneyInteraction; // Paranın script referansı

    [Header("Uyarı Mesajı")]
    public TextMeshProUGUI notificationText;
    public string noMoneyMessage = "Önce parayı almalıyım."; // I should take the money first.
    public float messageDisplayTime = 3f;
    private bool isShowingMessage = false;

    [Header("Kapı Ayarları")]
    public float openAngle = 90f;
    public float doorDuration = 1f;
    public AudioClip doorOpenSound;
    
    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private AudioSource audioSource;

    private void Start()
    {
        closedRotation = transform.rotation;
        openRotation = closedRotation * Quaternion.Euler(0, 0, openAngle);

        promptMessage = "E - Evden Çık";

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
    }

    protected override void Interact()
    {
        // Eğer para alınmadıysa kapı açılmasın ve uyarı versin
        if (moneyInteraction != null && !moneyInteraction.isMoneyCollected)
        {
            if (!isShowingMessage)
            {
                StartCoroutine(ShowNoMoneyMessage());
            }
            return;
        }

        // Para alındıysa kapıyı aç
        if (!isOpen)
        {
            isOpen = true;
            promptMessage = ""; // Artık kapı açıldı, etkileşim yazısını sil

            if (audioSource != null && doorOpenSound != null)
            {
                audioSource.PlayOneShot(doorOpenSound);
            }

            transform.DOKill();
            transform.DORotateQuaternion(openRotation, doorDuration).SetEase(Ease.InOutQuad);

            // Evden çıkma görevini tamamla
            MissionObjective missionObj = GetComponent<MissionObjective>();
            if (missionObj != null)
            {
                missionObj.OnInteracted();
            }
        }
    }

    private IEnumerator ShowNoMoneyMessage()
    {
        isShowingMessage = true;

        if (notificationText != null)
        {
            notificationText.text = noMoneyMessage;
            notificationText.gameObject.SetActive(true);
            notificationText.transform.parent.gameObject.SetActive(true); // Paneli de aktif et
        }

        yield return new WaitForSeconds(messageDisplayTime);

        if (notificationText != null)
        {
            notificationText.gameObject.SetActive(false);
            notificationText.transform.parent.gameObject.SetActive(false); // Paneli de kapat
        }

        isShowingMessage = false;
    }
}