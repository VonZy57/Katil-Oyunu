using System.Collections;
using UnityEngine;


[System.Serializable]
public struct FoodStage
{
    public Mesh foodMesh;
    public Material[] foodMaterials;
}
public class FoodInteractable : Interactable
{
    [Header("Mesh ve MateryalAyarları")]
    [Tooltip("Sırasıyla yiyeceğin halleri (Örn: Tam, 1. Isırık, 2. Isırık, Bitiş)")]
    public FoodStage[] foodStages = new FoodStage[4];
    
    [Header("Yeme/İçme Ayarları")]
    [Tooltip("Tüketilebilirden kaç saniye aralıkla ısırık/yudum alınabileceği verisini tutar.")]
    public float eatCooldown = 5f; // İki ısırık arası bekleme süresi
    [Tooltip("Etkileşime geçilecek tüketilebilirin türüne göre gösterilecek prompt mesajı. Örn: 'Eat' veya 'Drink'")]
    public string interactionPromt; 

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private int currentBiteIndex = 0; // Kaçıncı ısırıkta olduğumuzu tutar
    [SerializeField] private bool canEat = true;    // Oyuncunun yeme izni var mı (Cooldown kontrolü)

    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        

        if (foodStages.Length > 0 && foodStages[0].foodMesh != null)
        {
            if(meshFilter != null)
                meshFilter.mesh = foodStages[0].foodMesh;
            if(meshRenderer != null)
                meshRenderer.materials = foodStages[0].foodMaterials;
        }

        promptMessage = interactionPromt; // Prompt mesajını başlangıçta ayarla
    }

    protected override void Interact()
    {
        // Eğer yiyebiliyorsak ve yiyecek henüz bitmediyse
        if (canEat && currentBiteIndex < foodStages.Length - 1)
        {
            StartCoroutine(EatRoutine());
        }
    }

    private IEnumerator EatRoutine()
    {
        canEat = false;
        currentBiteIndex++;
        DoEventsWhileEating();

        // Meshi bir sonraki ısırık meshiyle değiştir
        if (meshFilter != null && currentBiteIndex < foodStages.Length && foodStages[currentBiteIndex].foodMesh != null)
        {
            if (meshFilter != null)
                meshFilter.mesh = foodStages[currentBiteIndex].foodMesh;
            if (meshRenderer != null && foodStages[currentBiteIndex].foodMaterials != null)
                meshRenderer.materials = foodStages[currentBiteIndex].foodMaterials;
            promptMessage = ""; // Yeme işlemi sırasında etkileşim promptunu gizle
        }


        // Son ısırık alındıysa (Yiyecek bittiyse)
        if (currentBiteIndex >= foodStages.Length - 1)
        {
            promptMessage = ""; // Artık etkileşim promptu çıkmasın
            yield return new WaitForSeconds(eatCooldown);
            OnFinishedEating(); // Senin daha sonradan içini dolduracağın metot!
        }
        else
        {
            // Yiyecek bitmediyse, bir sonraki ısırık için cooldown süresini bekle
            yield return new WaitForSeconds(eatCooldown);
            canEat = true;
            promptMessage = interactionPromt; // Cooldown bittikten sonra tekrar yeme promptunu göster
        }
    }

    //Yemek bittiğinde hangi olaylar gerçekleşeceği buraya yazılacak.
    protected virtual void OnFinishedEating()
    {
        Debug.Log("Yiyecek tamamen yenildi!");
    }

    //Yemek yerken gerçekleşecek olaylar buraya yazılacak.
    protected virtual void DoEventsWhileEating()
    {
        
    }
}
