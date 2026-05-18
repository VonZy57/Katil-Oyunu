using UnityEngine;
using System;

public abstract class Interactable : MonoBehaviour
{
    public string promptMessage;

    [Header("Outline (Vurgu) Ayarları")]
    [Tooltip("Oyuncu bu mesafeye girdiğinde Outline açılır.")]
    public float outlineShowDistance = 3f;

    private Transform playerTransform;
    private Behaviour outlineComponent;

    protected virtual void Awake()
    {
        // Oyuncuyu bul (Player etiketine sahip olduğunu varsayıyoruz)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Obje üzerindeki Outline eklentisini bul (QuickOutline veya EPOOutline destekli)
        outlineComponent = GetComponent("Outline") as Behaviour;
        if (outlineComponent == null)
        {
            outlineComponent = GetComponent("Outlinable") as Behaviour;
        }

        // Eğer hala bulunamadıysa, otomatik olarak eklemeyi dene
        if (outlineComponent == null)
        {
            // Önce Outlinable (EPO Outline) eklemeyi dene
            Type outlinableType = FindTypeAcrossAssemblies("Outlinable");
            if (outlinableType != null)
            {
                outlineComponent = gameObject.AddComponent(outlinableType) as Behaviour;
                Debug.Log($"'{gameObject.name}' objesine 'Outlinable' componenti otomatik olarak eklendi.", this);
            }
            else
            {
                // Alternatif olarak Outline (Quick Outline) eklemeyi dene
                Type outlineType = FindTypeAcrossAssemblies("Outline");
                if (outlineType != null)
                {
                    outlineComponent = gameObject.AddComponent(outlineType) as Behaviour;
                    Debug.Log($"'{gameObject.name}' objesine 'Outline' componenti otomatik olarak eklendi.", this);
                }
            }
        }

        // Başlangıçta Outline'ı kapalı tut
        if (outlineComponent != null)
        {
            outlineComponent.enabled = false;
        }
    }

    protected virtual void Update()
    {
        // Oyuncu ve outline scripti varsa mesafe ölçümü yap
        if (playerTransform != null && outlineComponent != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            
            // Mesafenin içindeyse outline'ı aktif et, dışındaysa kapat
            outlineComponent.enabled = (distance <= outlineShowDistance);
        }
    }

    protected virtual void Interact()
    {

    }

    public void BaseInteract()
    {
        Interact();
    }

    /// <summary>
    /// Projedeki tüm Assembly'ler içinde belirtilen isme sahip bir tipi arar.
    /// Bu, eklentilerin farklı Assembly'lerde olabilmesi sorununu çözer.
    /// </summary>
    private static Type FindTypeAcrossAssemblies(string typeName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = assembly.GetType(typeName);
            if (type != null)
                return type;
        }
        return null;
    }

}