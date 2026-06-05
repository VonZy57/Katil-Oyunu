using System;
using UnityEngine;

public class BathtubDropInteractable : Interactable
{
    [Header("Referanslar")]
    public BodyDragController dragController;
    public MissionSO nextMissionSO;
    public GameObject chairInteractable; // Eğer bu nesneyle etkileşim sağlanırsa sandalyeye oturma eylemi de aktifleşecekse, sandalyenin scriptini referans olarak alıyoruz.
    public GameObject maleBodyRef;
    public GameObject femaleBodyRef;
    public GameObject maleBodyCarry; // Erkek cesedi taşıma modeli
    public GameObject femaleBodyCarry; // Kadın cesedi taşıma modeli     
    private MissionObjective missionObj;
    public MissionSO maleMission;
    public MissionSO femaleMission;
    public static event Action OnLastBodyDropped; // Son ceset bırakıldığında tetiklenecek event
    void Start()
    {
        promptMessage = "E - Drop the Body";
    }

    protected override void Interact()
    {
        // Sadece ceset taşınıyorsa etkileşime izin ver
        if (dragController != null && dragController.isDragging)
        {
            missionObj= GetComponent<MissionObjective>();
            dragController.DropBodyAndFinish();

            // Hangi ceset bırakıldığına göre ilgili modeli aktif et
            if (missionObj.requiredMission == maleMission)
            {
                maleBodyRef.SetActive(true);
                maleBodyCarry.SetActive(false); // Eğer erkek cesedi bırakılıyorsa, taşıma modelini kapat
            }
            else if (missionObj.requiredMission == femaleMission)
            {                
                femaleBodyRef.SetActive(true);
                femaleBodyCarry.SetActive(false); // Eğer kadın cesedi bırakılıyorsa, taşıma modelini kapat
                OnLastBodyDropped?.Invoke(); // Son ceset bırakıldığında event'i tetikle
            }

            // Eğer küvete bırakınca çalışacak ekstra bir MissionObjective varsa tetikle
            
            if (missionObj != null)
            {
                missionObj.OnInteracted();
            }

            // Bu nesne ile bir sonraki görevde tekrar etkileşim sağlanabilmesi için gerekli görevi sonraki görevle değiştiriyoruz.
            missionObj.requiredMission = nextMissionSO;
        }
    }
}