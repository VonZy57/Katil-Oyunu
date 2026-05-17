using Unity.VisualScripting;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using System.Numerics;

public class GetFeetDown : Interactable
{
    [SerializeField] private DialogSystem dialogSystem;
    [SerializeField] private DialogNode getFeetDownNode;
    [SerializeField] private MissionObjective missionObj;

    public Transform cenkTransform; // Cenk'in küvetten kalktıktan sonra gideceği pozisyon
    public Transform cenksChairTransform; // Cenk'in sandalyeye oturacağı pozisyon
    public GameObject chairForCenk; // Cenk'in oturacağı sandalyenin modeli

    void OnEnable()
    {
        BathtubDropInteractable.OnLastBodyDropped += TeleportCenkToTable;
    }
    void OnDisable()
    {
        BathtubDropInteractable.OnLastBodyDropped -= TeleportCenkToTable;
    }
    void TeleportCenkToTable()
    {
        gameObject.transform.position = cenkTransform.position;
        gameObject.transform.rotation = cenkTransform.rotation;
        // Cenk'i sandalyeye oturtmak için sandalyenin pozisyonuna da
        chairForCenk.transform.position = cenksChairTransform.position;
        chairForCenk.transform.rotation = cenksChairTransform.rotation;
        promptMessage = ""; // Etkileşim mesajını temizle
        this.gameObject.layer = LayerMask.NameToLayer("Default"); // Cenk'in layer'ını değiştirme (örneğin "Default" yapabilirsiniz)
        this.enabled = false;
    }


    public bool isDialogCompleted { get; private set; } = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        BuildDialogTree();
    }

    // Update is called once per frame
    void Update()
    {
        if (missionObj != null && MissionManager.Instance.CurrentMission == missionObj.requiredMission && !isDialogCompleted)
        {
            promptMessage = "E - Talk";
        }
        else
        {
            promptMessage = "";
        }
    }

    protected override void Interact()
    {
        StartCoroutine(DialogRoutine()); 
    }
    
    private IEnumerator DialogRoutine()
    {
        dialogSystem.StartDialog(getFeetDownNode);
        
        // Panelin açılması için 1 frame bekle
        yield return null;
        
        // Panel kapanana kadar (diyalog bitene kadar) bekle
        yield return new WaitUntil(() => !dialogSystem.dialogPanel.activeSelf);
        
        isDialogCompleted = true;
    }

    void BuildDialogTree()
    {
        getFeetDownNode = DialogBuilder.CreateNode
        ("Can you get your feet down? Mother gave me a task.",
        "Ayaklarını indirir misin? Anne görev verdi.",
        "Engin");

        DialogNode playingGameNode = DialogBuilder.CreateNode
        ("I'm playing. Do it later.",
        "Oyun oynuyorum. Sonra hallet.",
        "Cenk");

        // Ana Düğüm: Engin'den Cenk'e otomatik geçiş
        DialogOption toCenkOpt = DialogBuilder.CreateOption("...", "...", playingGameNode, true);
        DialogBuilder.AddOption(getFeetDownNode, toCenkOpt);



        ///////////////////////////////////////////////////////////////////////////////////////////////
        

        //1.Dal "YEMEKTEN SONRA OYNARSIN" SEÇENEĞİ
        DialogNode afterDinnerNode = DialogBuilder.CreateNode
        ("You can play that after dinner. Mother said carry the bodies to the bathroom.",
        "Yemekten sonra devam edersin. Anne ölü bedenleri banyoya taşı dedi.",
        "Engin");

        DialogNode cenksAnswerNode = DialogBuilder.CreateNode
        ("You are no fun at all, bro. Or are you? Carrying the bodies to the bathroom is a game for you. Isn't it?",
        "Hiç eğlenceli değilsin abi. Ya da öylesin. Cesetleri taşımak senin için bir oyun. Öyle değil mi?",
        "Cenk");

        DialogNode enginsAnswerNode = DialogBuilder.CreateNode
        ("What game! Don't be silly. This is my duty.",
        "Ne oyunu! Saçmalama. Bu benim görevim.",
        "Engin");

        // 1. Dal içi bağlantılar
        DialogOption afterDinnerToCenk = DialogBuilder.CreateOption("...", "...", cenksAnswerNode, true);
        DialogBuilder.AddOption(afterDinnerNode, afterDinnerToCenk);

        DialogOption cenkToEngin = DialogBuilder.CreateOption("...", "...", enginsAnswerNode, true);
        DialogBuilder.AddOption(cenksAnswerNode, cenkToEngin);



        ///////////////////////////////////////////////////////////////////////////////////////////////


        //2. Dal "KAÇINCI LEVELDESİN" SEÇENEĞİ
        DialogNode whichLevelNode = DialogBuilder.CreateNode
        ("Which level are you at?",
        "Kaçıncı levela geldin?",
        "Engin");

        DialogNode cenksAnswerLevelNode = DialogBuilder.CreateNode
        ("It is not a level based game. It's a story based game where you discover and complete tasks by talking with NPCs. It is  hard to say that it is a game.",
        "Bu seviye kasarak ilerleme kat ettiğin bir oyun değil. Daha çok insanlar ile konuşarak yapman gereken görevleri tamamladığın hikaye temelli bir oyun. Oyun demek bile zor.",
        "Cenk");

        DialogNode enginsAnswerLevelNode = DialogBuilder.CreateNode
        ("Can I try it after you?",
        "Ben de oynayabilir miyim sen bitirdikten sonra?",
        "Engin");

        // 2. Dal içi bağlantılar
        DialogOption whichLevelToCenk = DialogBuilder.CreateOption("...", "...", cenksAnswerLevelNode, true);
        DialogBuilder.AddOption(whichLevelNode, whichLevelToCenk);

        DialogOption cenkLevelToEngin = DialogBuilder.CreateOption("...", "...", enginsAnswerLevelNode, true);
        DialogBuilder.AddOption(cenksAnswerLevelNode, cenkLevelToEngin);


        ///////////////////////////////////////////////////////////////////////////////////////////////
        

        // === ORTAK SON: ANNE BÖLÜYOR ===
        DialogNode motherWarnsNode = DialogBuilder.CreateEndNode
        ("MOTHERF*CKERS! DINNER IS ALMOST READY! If the bodies are still there when I come there, I'm gonna torture you two until morning!",
        "AĞZINA S*ÇTIKLARIM! YEMEK NEREDEYSE HAZIR! Oraya geldiğimde eğer o cesetler hala oradaysa, sabaha kadar döverim sizi!",
        "Mother");

        // İki dalın son cümlesini de Anne'ye bağlıyoruz
        DialogOption enginToMother1 = DialogBuilder.CreateOption("...", "...", motherWarnsNode, true);
        DialogBuilder.AddOption(enginsAnswerNode, enginToMother1);

        DialogOption enginToMother2 = DialogBuilder.CreateOption("...", "...", motherWarnsNode, true);
        DialogBuilder.AddOption(enginsAnswerLevelNode, enginToMother2);


        // === ANA DÜĞÜMÜN OYUNCU SEÇENEKLERİ ===
        // Cenk "Oyun oynuyorum" dedikten sonra oyuncuya çıkacak 2 seçenek:
        DialogOption playingGameOption = DialogBuilder.CreateOption("Play after dinner.", "Yemekten sonra oyna.", afterDinnerNode, false);
        DialogOption whichLevelOption = DialogBuilder.CreateOption("Which level?", "Kaçıncı level'a geldin?", whichLevelNode, false);
        
        DialogBuilder.AddOption(playingGameNode, playingGameOption);
        DialogBuilder.AddOption(playingGameNode, whichLevelOption);
    }
}
