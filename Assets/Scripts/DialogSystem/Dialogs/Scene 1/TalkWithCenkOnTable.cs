using UnityEngine;

public class TalkWithCenkOnTable : MonoBehaviour
{
    [SerializeField] private DialogSystem dialogSystem;
    [SerializeField] private DialogNode cenkStartsNode;
    [SerializeField] private MissionObjective missionObj;

    void Start()
    {
        BuildDialogTree();
    }

    void Update()
    {
        
    }
    
    void BuildDialogTree()
    {
        cenkStartsNode = DialogBuilder.CreateNode
        ("You are not belonging here, brother. Deads was right, last night. Mother is a cracky. Look, what she made you do.",
        "Abi... Sen buraya ait değilsin. Ölüler haklıydı, dün gece. Anne bir deli. Bak neler yaptırdı sana.",
        "Cenk");

        DialogNode enginSaysNode = DialogBuilder.CreateNode
        ("Cenk, She is my mother.",
        "O benim annem Cenk.",
        "Engin");

        // Ana Düğüm - 1: Cenk'ten Engin'e otomatik geçiş
        DialogOption toCenkOpt = DialogBuilder.CreateOption("...", "...", enginSaysNode, true);
        DialogBuilder.AddOption(cenkStartsNode, toCenkOpt);

        DialogNode sheKilledTwoNode = DialogBuilder.CreateNode
        ("Mother made you kill two people. And this is not the first time. Are you a murderer?",
        "Anne sana iki kişiyi öldürttü. Ve bu ilk kez olmuyor. Sen bir katil misin?",
        "Cenk");

        // Ana Düğüm - 2: Engin'den Cenk'e otomatik geçiş
        DialogOption toEngin = DialogBuilder.CreateOption("...", "...", sheKilledTwoNode, true);
        DialogBuilder.AddOption(enginSaysNode, toEngin);



        ///////////////////////////////////////////////////////////////////////////////////////////////


        //1.Dal "AĞZINI TOPLA" SEÇENEĞİ
        DialogNode enginSaysNoImNot = DialogBuilder.CreateNode
        ("Watch your words, you're talking with your elder brother. What do you think you're talking about? Murderer???",
        "Ağzını topla, abinle konuşuyorsun. Katil matil ne zırvalıyorsun?!",
        "Engin");

        DialogNode cenkSaysYouKilled = DialogBuilder.CreateNode
        ("So, is it a lie? Didn't you kill those two?",
        "Ama abi, yalan mı? Sen öldürmedin mi onları?",
        "Cenk");


        //1.1 "ANNE SÖYLEDİ" CEVABI
        DialogNode motherSaidThat = DialogBuilder.CreateNode
        ("Mother said that, Cenk. Mother never wants bad things. So, they were bad.",
        "Anne söyledi Cenk Anne asla kötü bir şey istemez. Demek ki kötü insanlarmış.",
        "Engin");

        DialogOption motherSaidOption = DialogBuilder.CreateOption("Mother said that.", "Anne söyledi.", motherSaidThat);
        DialogBuilder.AddOption(cenkSaysYouKilled, motherSaidOption);


        //1.2 "AİLEMİZE ZARAR VERECEKLERDİ" CEVABI
        DialogNode theyWouldHarmUs = DialogBuilder.CreateNode
        ("They would harm our family, Cenk. You know what they said about us. They weren't good.",
        "Ailemize zarar vereceklerdi Cenk. Bizim hakkımızda ne dediklerini duydun. İyi insan değillerdi.",
        "Engin");

        DialogOption theyWouldHarmOption = DialogBuilder.CreateOption("They would harm our family.", "Ailemize zarar vereceklerdi.", theyWouldHarmUs);
        DialogBuilder.AddOption(cenkSaysYouKilled, theyWouldHarmOption);

        
        DialogNode cenksRemindsOldsNode = DialogBuilder.CreateNode
        ("Okay, let's say these people were bad. But why did she made you kill a kid on the street, two months ago?",
        "Hadi bu insanlar kötü. 2 ay önce, sokaktaki çocuğu niye öldürttü sana?",
        "Cenk"); 

        DialogNode enginSaysWhyHeKilledNode = DialogBuilder.CreateNode
        ("He was walking crookedly. He was never gonna be a strong and handsome like you and me.",
        "Yamuk yürüyordu. Senin ve benim gibi güçlü ve yakışıklı olamayacaktı.",
        "Engin");

        DialogNode cenkSaysDoYouBelieveNode = DialogBuilder.CreateNode
        ("Do you believe that?",
        "Sen inanıyor musun buna?",
        "Cenk");

        DialogNode enginSaysNoNode = DialogBuilder.CreateNode
        ("No.", "Hayır.", "Engin");

        //Cenk ve Engin'in 1.1 ve 1.2 cevaplarından sonraki konuşmalarındaki geçişleri sağlayan sessiz seçenekler
        DialogOption silentOption = DialogBuilder.CreateOption("...", "...", enginSaysWhyHeKilledNode, true);
        DialogOption silentOption2 = DialogBuilder.CreateOption("...", "...", cenkSaysDoYouBelieveNode, true);
        DialogOption silentOption3 = DialogBuilder.CreateOption("...", "...", enginSaysNoNode, true);
        DialogBuilder.AddOption(cenksRemindsOldsNode, silentOption);
        DialogBuilder.AddOption(enginSaysWhyHeKilledNode, silentOption2);
        DialogBuilder.AddOption(cenkSaysDoYouBelieveNode, silentOption3);



        ///////////////////////////////////////////////////////////////////////////////////////////////


        //2.Dal "BEN KATİL OLMAK İSTEMİYORUM" SEÇENEĞİ
        DialogNode enginSaysIdontWantNode = DialogBuilder.CreateNode
        ("I don't want to be a murderer, Cenk. I don't want to kill. I don't want to be a bad person.",
        "Ben katil olmak istemiyorum Cenk. Öldürmek istemiyorum. Kötü biri olmak istemiyorum.",
        "Engin");

        DialogNode cenkSaysDoYouRememberNode = DialogBuilder.CreateNode
        ("You will murder until you leave this home, brother. You will kill a lot. Do you remember that man, the fat one?",
        "Bu evde kaldığın sürece öldüreceksin abi. Hem de çok öldüreceksin. Hatırlıyor musun o şişman adamı?",
        "Cenk");

        DialogOption fromEnginToCenk = DialogBuilder.CreateOption("...", "...", cenkSaysDoYouRememberNode, true);
        DialogBuilder.AddOption(enginSaysIdontWantNode, fromEnginToCenk);

        
        //2.1 "BOĞAZINI KESTİM" CEVABI
    }
}
