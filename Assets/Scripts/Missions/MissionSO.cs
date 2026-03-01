using UnityEngine;

[CreateAssetMenu(fileName = "NewMission", menuName = "Mission System/Mission")]
public class MissionSO : ScriptableObject
{
    [Header("Görev Bilgileri")]
    [Tooltip("Görevin ismi")]
    public string missionID; // Benzersiz kimlik (örn: "Gorev_01_EveGir")

    [TextArea(3, 5)]
    [Tooltip("Ekranda yazacak yazý")]
    public string description; // Ekranda yazacak yazý (örn: "Anahtarý bul ve eve gir.")

    [Header("Zincirleme Yapý")]
    [Tooltip("Görev bittiðinde baþlayacak bir sonraki görevi buraya ata")]
    public MissionSO nextMission; // Bu görev bitince otomatik baþlayacak görev (Opsiyonel)

    [Header("Sahne Geçiþi")]
    [Tooltip("Eðer bu görev bitince yeni sahneye geçilecekse, geçilecek sahnenin adýný yaz")]
    public string loadSceneName;

    [Header("Ayarlar")]
    [Tooltip("Oyunun son görevi mi?")]
    public bool isFinalMission = false; // Oyun sonu mu?
}