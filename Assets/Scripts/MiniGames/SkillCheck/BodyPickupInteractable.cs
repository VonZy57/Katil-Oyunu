using UnityEngine;

public class BodyPickupInteractable : Interactable
{
    [Header("Oyuncu Bağlantıları")]
    [Tooltip("Oyuncunun üzerindeki BodyDragController scripti buraya atanacak")]
    public BodyDragController dragController;
    private MissionObjective missionObj;

    void Start()
    {
        promptMessage = "E - Cesedi Al";   

        missionObj = GetComponent<MissionObjective>();
    }

    protected override void Interact()
    {
        if(MissionManager.Instance != null && MissionManager.Instance.CurrentMission == missionObj.requiredMission)
        {
            // Taşımayı başlat
            if (dragController != null)
                dragController.StartDraggingTask();

            gameObject.SetActive(false);
        }
    }
}
