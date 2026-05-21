using UnityEngine;

public class EnginTalksWithCrowded : MonoBehaviour
{
    [SerializeField] private DialogSystem dialogSystem;
    [SerializeField] private DialogNode crowdStartNode;
    [SerializeField] private MissionObjective missionObj;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        BuildDialog();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    void BuildDialog()
    {
        crowdStartNode = DialogBuilder.CreateNode
        ("Somebody help the old man up.",
        "Biri amcayı kaldırsın.",
        "Someone from the Crowd");

        // Silent Opt - 1 Place

        DialogNode crowd2Node = DialogBuilder.CreateNode
        ("It's your turn, you're picking up the old guy this time.",
        "Sıra sende bu sefer sen yaşlı kaldırıyorsun.",
        "Another from the Crowd");

        // Silent Opt - 1
        DialogOption crowd2Opt = DialogBuilder.CreateOption("...", "...", crowd2Node, true);
        DialogBuilder.AddOption(crowdStartNode, crowd2Opt);

        // Silent Opt - 2 Place

        DialogNode crowd3Node = DialogBuilder.CreateNode
        ("I just picked one up last week!",
        "Daha geçen hafta ben kaldırdım.",
        "Someone from the Crowd");

        // Silent Opt - 2
        DialogOption crowd3Opt = DialogBuilder.CreateOption("...", "...", crowd3Node, true);
        DialogBuilder.AddOption(crowd2Node, crowd3Opt);

        // Silent Opt - 3 Place

        DialogNode crowd4Node = DialogBuilder.CreateNode
        ("Do you have any idea how many old people fall down around here in a week?",
        "Bir hafta da kaç yaşlı düşüyor burada haberin var mı?",
        "A Completely Different Person");

        // Silent Opt - 3
        DialogOption crowd4Opt = DialogBuilder.CreateOption("...", "...", crowd4Node, true);
        DialogBuilder.AddOption(crowd3Node, crowd4Opt);

        // Silent Opt - 4 Place
        
        DialogNode crowd5Node = DialogBuilder.CreateNode
        ("Young man, you're new here, you help him up. Join our neighborhood's tradition.",
        "Delikanlı, sen yenisin sen kaldır. Mahallemizin bu adetine dahil ol.",
        "Someone from the Crowd");

        // Silent Opt - 4
        DialogOption crowd5Opt = DialogBuilder.CreateOption("...", "...", crowd5Node, true);
        DialogBuilder.AddOption(crowd4Node, crowd5Opt);

        // ==============================
        // 1. DAL: "Sure." / "Olur"
        // ==============================
        DialogNode sureEnginNode = DialogBuilder.CreateNode
        ("Of course. Helping the elderly is our duty.",
        "Tabii ki. Yaşlıları kaldırmak boynumuzun borcu.",
        "Engin");

        DialogNode sureCrowdNode = DialogBuilder.CreateNode
        ("Good for you. They don't make young men like you anymore.",
        "Helal olsun sana. Kalmadı senin gibi delikanlılar.",
        "Someone from the Crowd");

        DialogOption sureEnginToCrowdOpt = DialogBuilder.CreateOption("...", "...", sureCrowdNode, true);
        DialogBuilder.AddOption(sureEnginNode, sureEnginToCrowdOpt);

        // ==============================
        // 2. DAL: "Why me?" / "Niye ben?"
        // ==============================
        DialogNode whyMeEnginNode = DialogBuilder.CreateNode
        ("Why do I have to do it, man? I don't even know the guy.",
        "Neden ben yapıyorum abi. Amcayı tanımam etmem.",
        "Engin");

        DialogNode whyMeCrowdNode = DialogBuilder.CreateNode
        ("Oh, sure, we all know and love him so much. Look, don't piss me off. I don't want to carry another old man this week.",
        "He biz tanırız bayılırız zaten. Bak benim tepemin tasını attırma. Ben bu hafta bir tane daha yaşlı taşımak istemiyorum.",
        "Someone from the Crowd");
        
        DialogOption whyMeEnginToCrowdOpt = DialogBuilder.CreateOption("...", "...", whyMeCrowdNode, true);
        DialogBuilder.AddOption(whyMeEnginNode, whyMeEnginToCrowdOpt);

        // Ana Düğümden Oyuncu Seçenekleri
        DialogOption sureOption = DialogBuilder.CreateOption("Sure.", "Olur", sureEnginNode);
        DialogOption whyMeOption = DialogBuilder.CreateOption("Why me?", "Niye ben?", whyMeEnginNode);

        DialogBuilder.AddOption(crowd5Node, sureOption);
        DialogBuilder.AddOption(crowd5Node, whyMeOption);

        // ==============================
        // ORTAK DEVAM: Amca İle Konuşma
        // ==============================
        DialogNode amcaStartNode = DialogBuilder.CreateNode
        ("Thank you, young man. People like you are rare these days. If it weren't for you, I would've been left on the ground. Let me treat you to a pastry and some ayran. And I won't take no for an answer.",
        "Sağ olasın delikanlı. Senin gibi insanlar az bulunur oldu. Sen de olmasan yerde kalacaktım. İzin ver sana bir poğaça ayran ısmarlayayım. Ve hayırı cevap olarak kabul etmiyorum.",
        "Amca");

        // İki dalı da Amca düğümüne bağlıyoruz.
        // Not: Burada 'Engin Amcayı kaldırır ve kalabalık dağılır' olayı için ileride DialogBuilder.CreateOptionWithEvent kullanılabilir.
        DialogOption sureToAmcaOpt = DialogBuilder.CreateOption("...", "...", amcaStartNode, true);
        DialogBuilder.AddOption(sureCrowdNode, sureToAmcaOpt);

        DialogOption whyMeToAmcaOpt = DialogBuilder.CreateOption("...", "...", amcaStartNode, true);
        DialogBuilder.AddOption(whyMeCrowdNode, whyMeToAmcaOpt);

        // ==============================
        // AMCA SEÇENEK 1: "Yes." / "Evet"
        // ==============================
        DialogNode amcaYesEnginNode = DialogBuilder.CreateNode
        ("That sounds good, I'll get to have some breakfast too.",
        "Güzel olur, ben de kahvaltı yapmış olurum.",
        "Engin");

        DialogNode amcaYesResponseNode = DialogBuilder.CreateEndNode
        ("That's my boy.",
        "He yaşa oğlum benim.",
        "Amca");

        DialogOption amcaYesEnginToResponseOpt = DialogBuilder.CreateOption("...", "...", amcaYesResponseNode, true);
        DialogBuilder.AddOption(amcaYesEnginNode, amcaYesEnginToResponseOpt);

        // ==============================
        // AMCA SEÇENEK 2: "No." / "Hayır"
        // ==============================
        DialogNode amcaNoEnginNode = DialogBuilder.CreateNode
        ("No, I'm busy, I don't have time for a pastry right now.",
        "Hayır benim işim gücüm var poğaçaya zaman ayıramam.",
        "Engin");

        DialogNode amcaNoResponseNode = DialogBuilder.CreateEndNode
        ("I won't accept that. There's always time for a pastry.",
        "Kabul etmiyorum. Her zaman poğaçaya ayrılacak zaman vardır.",
        "Amca");

        DialogOption amcaNoEnginToResponseOpt = DialogBuilder.CreateOption("...", "...", amcaNoResponseNode, true);
        DialogBuilder.AddOption(amcaNoEnginNode, amcaNoEnginToResponseOpt);

        // Amca Düğümünden Oyuncu Seçenekleri
        DialogOption yesOption = DialogBuilder.CreateOption("Yes.", "Evet", amcaYesEnginNode);
        DialogOption noOption = DialogBuilder.CreateOption("No.", "Hayır", amcaNoEnginNode);

        DialogBuilder.AddOption(amcaStartNode, yesOption);
        DialogBuilder.AddOption(amcaStartNode, noOption);
    }
}
