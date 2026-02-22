using UnityEngine;
using System.Collections;

public class BodyDragController : MonoBehaviour
{
    [Header("Referanslar")]
    public SkillCheckSystem skillCheckSystem;
    public MonoBehaviour playerMovementScript;

    private bool isDragging = false;
    private bool isInDropZone = false;

    void Start()
    {
        // Başlangıçta T tuşunu bekliyoruz
    }

    void Update()
    {
        //BİTİRME KONTROLÜ (DropZone içindeyken kilitliyken E'ye basma)
        if (isInDropZone && isDragging && Input.GetKeyDown(KeyCode.E))
        {
            DropBodyAndFinish();
        }
    }

    public void StartDraggingTask()
    {
        if (isDragging) return;

        isDragging = true;
        isInDropZone = false;
        Debug.Log("Ceset taşıma görevi başladı!");

        if (playerMovementScript != null)
            playerMovementScript.enabled = false;

        StartCoroutine(DragRoutine());
    }

    IEnumerator DragRoutine()
    {
        while (isDragging && !isInDropZone)
        {
            if (playerMovementScript != null)
                playerMovementScript.enabled = false;

            bool? checkResult = null;

            skillCheckSystem.StartSkillCheck((result) => { checkResult = result; });

            yield return new WaitUntil(() => checkResult.HasValue);

            if (checkResult.Value == true)
            {
                // Başarılı olunca yürümeye izin ver
                if (playerMovementScript != null)
                    playerMovementScript.enabled = true;

                yield return new WaitForSeconds(2f);
            }
            else
            {
                // Başarısız olunca yürümeyi kes
                if (playerMovementScript != null)
                    playerMovementScript.enabled = false;

                Debug.Log("Skill check kaçırıldı! Kısa bir sersemleme...");
                yield return new WaitForSeconds(1f);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Oyuncu alana girdiğinde
        if (other.CompareTag("DropZone") && isDragging)
        {
            isInDropZone = true;

            // 1. Skill check döngüsünü ve UI'ı tamamen durdur
            StopAllCoroutines();
            skillCheckSystem.ForceStop();

            // 2. Oyuncunun hareketini E'ye basana kadar KESİN OLARAK KİLİTLE
            if (playerMovementScript != null)
                playerMovementScript.enabled = false;

            Debug.Log("DropZone'a girildi! Hareket kilitlendi. Cesedi bırakmak için 'E' tuşuna basın.");
        }
    }

    private void DropBodyAndFinish()
    {
        isDragging = false;

        // 3. E'ye basıldı, ceset bırakıldı ve hareket KİLİDİ AÇILDI
        if (playerMovementScript != null)
            playerMovementScript.enabled = true;

        Debug.Log("Ceset bırakıldı. Hareket kilidi açıldı. Görev Bitti!");

        if (MissionManager.Instance != null)
            MissionManager.Instance.CompleteCurrentMission();
    }
}