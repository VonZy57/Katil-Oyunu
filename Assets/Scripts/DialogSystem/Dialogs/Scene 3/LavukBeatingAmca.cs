using UnityEngine;

public class LavukBeatingAmca : MonoBehaviour
{
    [SerializeField] private DialogSystem dialogSystem;
    [SerializeField] private DialogNode LavukStartNode;
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

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && MissionManager.Instance.CurrentMission == missionObj.requiredMission)
        {
            dialogSystem.StartDialog(LavukStartNode);
        }
    }

    void BuildDialog()
    {
        LavukStartNode = DialogBuilder.CreateNode
        ("Didn't I tell you I never want to see you around here again, you **** *****! Huh, if I **** your liver right now, would it be enough, huh!",
        "Sana bir daha seni burada görmeyeceğim demedim mi **** *****. He şimdi senin ciğerini ****** az mı yapmış olurum l*n. *******.",
        "Lavuk");

        // SilentOpt - 1 Place

        DialogNode LavukContinueNode = DialogBuilder.CreateNode
        ("You geezerrr, didn't I tell you to leave this woman alone, huuuuuh! Why are you roaming around here like a sleazebag, you ****.",
        "Moruuukk sana bu kadının peşini bırak demedim mi haaaa!? Ne gevşek gevşek dolanıyorsun buralarda *****.",
        "Lavuk");

        // SilentOpt - 1
        DialogOption lavuksContinueOpt = DialogBuilder.CreateOption("...", "...", LavukContinueNode, true);
        DialogBuilder.AddOption(LavukStartNode, lavuksContinueOpt);
        
        // SilentOpt - 2 Place

        DialogNode KuruTryStopNode = DialogBuilder.CreateNode
        ("Just walk away, man, he's gonna die in your hands. Alright, you're the toughest around, now just hit the road.",
        "Çek git oğlum, elinde kalacak adam. Tamam en kral sensin bak yoluna.",
        "Kuru");

        // SilentOpt - 2
        DialogOption kuruTryStopOpt = DialogBuilder.CreateOption("...", "...", KuruTryStopNode, true);
        DialogBuilder.AddOption(LavukContinueNode, kuruTryStopOpt);

        // SilentOpt - 3 Place

        DialogNode lavukResponseToKuruNode = DialogBuilder.CreateNode
        ("What do you mean hit the road, man! I'll **** your neighborhood and **** you too! It's easy to talk from up there. Come on, come here!",
        "Ne bakayım lan yoluma! Olum sizin mahallenizi de ***** sizi de ******. Oradan konuşmak kolay. Gelsene lan buraya.",
        "Lavuk");

        // SilentOpt - 3
        DialogOption lavukResponseToKuruOpt = DialogBuilder.CreateOption("...", "...", lavukResponseToKuruNode, true);
        DialogBuilder.AddOption(KuruTryStopNode, lavukResponseToKuruOpt);
        
        // SilentOpt - 4 Place

        DialogNode kuruDontPushYourLuckNode = DialogBuilder.CreateNode
        ("Don't push your luck, boy!",
        "Oğlum kaşınma bak!",
        "Kuru");
        
        // SilentOpt - 4
        DialogOption kuruPushOpt = DialogBuilder.CreateOption("...", "...", kuruDontPushYourLuckNode, true);
        DialogBuilder.AddOption(lavukResponseToKuruNode, kuruPushOpt);

        // SilentOpt - 5 Place

        DialogNode lavukTellsAmca = DialogBuilder.CreateNode
        ("Are you hitting on my wife, my property, my bread and butter, you ***! Don't I have the right to beat the **** out of you right in front of this neighborhood, huh!",
        "Sen benim karıma, sen benim malıma, ekmek tekneme laf mı atıyon ***. Bu mahallelinin önünde seni çatır çutur **** hakkım değil mi lan!",
        "Lavuk");

        // SilentOpt - 5
        DialogOption lavukTellsAmcaOpt = DialogBuilder.CreateOption("...", "...", lavukTellsAmca, true);
        DialogBuilder.AddOption(kuruDontPushYourLuckNode, lavukTellsAmcaOpt);

        // SilentOpt - 6 Place

        DialogNode amcaResponseToLavuk = DialogBuilder.CreateNode
        ("Please, let me speak to her just once. I missed her so much. I love her. She is my everything.",
        "Ne olur bir kez konuşayım onunla onu çok özledim. Onu seviyorum. O benim her şeyim",
        "Amca");

        // SilentOpt - 6
        DialogOption amcaResponseToLavukOpt = DialogBuilder.CreateOption("...", "...", amcaResponseToLavuk, true);
        DialogBuilder.AddOption(lavukTellsAmca, amcaResponseToLavukOpt);

        // SilentOpt - 7 Place

        DialogNode lavuksLastWords = DialogBuilder.CreateEndNode
        ("Get the hell out of here, man. Look, I'm swearing in front of everyone. I swear to God on everything holy, if I see you around Asuman again, I'll choke the life out of you. If I don't, I'm the biggest *** ** *****.",
        "Yürrrü git lan. Bak herkesin önünde yemin ediyorum. Ekmek musap Kuran çarpsın, seni bir daha Asuman’ın etrafında görürsem senin ümüğünü sıkarım. Yapmazsam en adi ****** *******.",
        "Lavuk");

        // SilentOpt - 7
        DialogOption lavuksLastWordsOpt = DialogBuilder.CreateOption("...", "...", lavuksLastWords, true);
        DialogBuilder.AddOption(amcaResponseToLavuk, lavuksLastWordsOpt);

    }
}
