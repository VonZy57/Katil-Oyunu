using UnityEngine;

[CreateAssetMenu(fileName = "NewMission", menuName = "Mission System/Mission")]
public class MissionSO : ScriptableObject
{
    [Header("Görev Bilgileri")]
    public string missionID; // Benzersiz kimlik (örn: "Gorev_01_EveGir")

    [TextArea(3, 5)]
    public string description; // Ekranda yazacak yazý (örn: "Anahtarý bul ve eve gir.")

    [Header("Zincirleme Yapý")]
    public MissionSO nextMission; // Bu görev bitince otomatik baþlayacak görev (Opsiyonel)

    [Header("Ayarlar")]
    public bool isFinalMission = false; // Oyun sonu mu?
}