using System.Collections;
using UnityEngine;

public class BodyDragController : MonoBehaviour
{
    [Header("Referanslar")]
    public SkillCheckSystem skillCheckSystem;
    public MonoBehaviour playerMovementScript;

    public bool isDragging { get; private set; } = false;
    private bool isInDropZone = false;
    private Coroutine dragCoroutine;

    public void StartDraggingTask()
    {
        if (isDragging) return;

        isDragging = true;
        isInDropZone = false;
        Debug.Log("Ceset taşıma görevi başladı!");

        if (playerMovementScript != null)
            playerMovementScript.enabled = false;

        dragCoroutine = StartCoroutine(DragRoutine());
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
        Debug.Log("Banyoya girildi! Ceset bırakma alanına gir...");
        // Oyuncu alana girdiğinde
        if (other.CompareTag("DropZone") && isDragging)
        {
            isInDropZone = true;

            // 1. Skill check döngüsünü ve UI'ı tamamen durdur
            if (dragCoroutine != null)
            {
                StopCoroutine(dragCoroutine);
            }
            skillCheckSystem.ForceStop();

            // 2. Oyuncunun küvete yürüyebilmesi için hareket KİLİDİNİ AÇ
            if (playerMovementScript != null)
                playerMovementScript.enabled = true;

            Debug.Log("DropZone'a girildi! Skill check bitti. Cesedi bırakmak için küvete yürüyüp etkileşime geçin.");
        }
    }

    public void DropBodyAndFinish()
    {
        if (!isDragging) return;

        isDragging = false;

        // 3. Küvetle etkileşime girildi, ceset bırakıldı. (Emin olmak için hareket kilidini tekrar açıyoruz)
        if (playerMovementScript != null)
            playerMovementScript.enabled = true;

        Debug.Log("Ceset küvete bırakıldı. Görev Bitti!");
    }
}