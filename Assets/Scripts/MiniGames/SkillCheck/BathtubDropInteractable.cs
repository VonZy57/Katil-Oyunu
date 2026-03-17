using UnityEngine;

public class BathtubDropInteractable : Interactable
{
    [Header("Referanslar")]
    public BodyDragController dragController;
    public MissionSO nextMissionSO;

    void Start()
    {
        promptMessage = "E - Cesedi Bırak";
    }

    protected override void Interact()
    {
        // Sadece ceset taşınıyorsa etkileşime izin ver
        if (dragController != null && dragController.isDragging)
        {
            dragController.DropBodyAndFinish();
            
            // Eğer küvete bırakınca çalışacak ekstra bir MissionObjective varsa tetikle
            MissionObjective missionObj = GetComponent<MissionObjective>();
            if (missionObj != null)
            {
                missionObj.OnInteracted();
            }

            // Bu nesne ile bir sonraki görevde tekrar etkileşim sağlanabilmesi için gerekli görevi sonraki görevle değiştiriyoruz.
            missionObj.requiredMission = nextMissionSO;
        }
    }
}