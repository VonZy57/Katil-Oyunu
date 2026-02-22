using UnityEngine;

public class MissionObjective : MonoBehaviour
{
    [Header("Bu obje hangi görevdeyken iþe yaramalý?")]
    [SerializeField] public MissionSO requiredMission;

    [Header("Etkileþimden sonra obje yok olsun mu? (Örn: Anahtar)")]
    [SerializeField] private bool destroyAfterInteract = false;

    // Senin kendi Interaction Script'in E'ye basýlýnca BU fonksiyonu çaðýracak
    public void OnInteracted()
    {
        // 1. Güvenlik kontrolü: Manager sahnede var mý?
        if (MissionManager.Instance == null) return;

        // 2. Doðru görevde miyiz?
        // Oyuncu oyunun baþýnda anahtarý alamasýn diye bu kontrolü yapýyoruz.
        if (MissionManager.Instance.CurrentMission == requiredMission)
        {
            // Görevi tamamla ve sonrakine geç
            MissionManager.Instance.CompleteCurrentMission();

            Debug.Log($"{gameObject.name} ile etkileþime girildi, görev ilerledi.");

            // Eðer bu bir toplama göreviyse (anahtar vb.) objeyi sahneden kaldýr
            if (destroyAfterInteract)
            {
                gameObject.SetActive(false);
            }
        }
        else
        {
            // Opsiyonel: Oyuncuya "Bunu þu an almama gerek yok" gibi bir feedback verebilirsin.
            Debug.Log("Bu obje þu anki görev için gerekli deðil.");
        }
    }
}