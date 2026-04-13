using UnityEngine;

public class MissionObjective : MonoBehaviour
{
    [Header("Bu obje hangi görevdeyken işe yaramal�?")]
    [SerializeField] public MissionSO requiredMission;

    [Header("Etkileşimden sonra obje yok olsun mu? (Örn: Anahtar)")]
    [SerializeField] private bool destroyAfterInteract = false;

    // Senin kendi Interaction Script'in E'ye bas�l�nca BU fonksiyonu �a��racak
    public void OnInteracted()
    {
        // 1. G�venlik kontrol�: Manager sahnede var m�?
        if (MissionManager.Instance == null) return;

        // 2. Do�ru g�revde miyiz?
        if (MissionManager.Instance.CurrentMission == requiredMission)
        {
            // G�revi tamamla ve sonrakine ge�
            MissionManager.Instance.CompleteCurrentMission();

            Debug.Log($"{gameObject.name} ile etkileşime girildi, görev ilerledi.");

            // E�er bu bir toplama g�reviyse (anahtar vb.) objeyi sahneden kald�r
            if (destroyAfterInteract)
            {
                gameObject.SetActive(false);
            }
        }
        else
        {
            // Opsiyonel: Oyuncuya "Bunu �u an almama gerek yok" gibi bir feedback verebilirsin.
            Debug.Log("Bu obje bu anki görev için gerekli değil.");
        }
    }
}