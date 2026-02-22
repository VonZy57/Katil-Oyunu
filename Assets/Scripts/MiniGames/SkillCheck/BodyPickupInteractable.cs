using UnityEngine;

public class BodyPickupInteractable : Interactable
{
    [Header("Oyuncu Bağlantıları")]
    [Tooltip("Oyuncunun üzerindeki BodyDragController scripti buraya atanacak")]
    public BodyDragController dragController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        promptMessage = "Cesedi Al [F] ";   
    }

    protected override void Interact()
    {
        // Taşımayı başlat
        if (dragController != null)
            dragController.StartDraggingTask();

        MissionObjective missionObj = GetComponent<MissionObjective>();
        if (missionObj != null)
            missionObj.OnInteracted();

        gameObject.SetActive(false);
    }
}
