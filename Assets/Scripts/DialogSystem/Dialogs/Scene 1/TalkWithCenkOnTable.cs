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

    public void StartTheDialog()
    {
        dialogSystem.StartDialog(cenkStartsNode);
    }
    
    void BuildDialogTree()
    {
        cenkStartsNode = DialogBuilder.CreateNode
        ("You are not belonging here, brother. Deads was right, last night. Mother is a cracky. Look, what she made you do.",
        "Abi... Sen buraya ait değilsin. Ölüler haklıydı, dün gece. Anne bir deli. Bak neler yaptırdı sana.",
        "Cenk");

        DialogNode enginSaysNode = DialogBuilder.CreateNode
        ("Cenk, She is mother.",
        "O anne Cenk.",
        "Engin");

        // Ana Düğüm - 1: Cenk'ten Engin'e otomatik geçiş
        DialogOption toCenk = DialogBuilder.CreateOption("...", "...", enginSaysNode, true);
        DialogBuilder.AddOption(cenkStartsNode, toCenk);

        DialogNode sheKilledTwoNode = DialogBuilder.CreateNode
        ("Mother made you kill two people. And this is not the first time. Are you a murderer?",
        "Anne sana iki kişiyi öldürttü. Ve bu ilk kez olmuyor. Sen bir katil misin?",
        "Cenk");

        // Ana Düğüm - 2: Engin'den Cenk'e otomatik geçiş
        DialogOption toEngin = DialogBuilder.CreateOption("...", "...", sheKilledTwoNode, true);
        DialogBuilder.AddOption(enginSaysNode, toEngin);



        ///////////////////////////////////////////////////////////////////////////////////////////////


        //1.Dal "AĞZINI TOPLA" SEÇENEĞİ
        DialogNode enginSaysWatchYourWordsNode = DialogBuilder.CreateNode
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

        // 1 - Devam
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
        DialogOption silentOption = DialogBuilder.CreateOption("...", "...", cenkSaysYouKilled, true);
        DialogOption silentOption2 = DialogBuilder.CreateOption("...", "...", cenksRemindsOldsNode, true);
        DialogOption silentOption3 = DialogBuilder.CreateOption("...", "...", enginSaysWhyHeKilledNode, true);
        DialogOption silentOption4 = DialogBuilder.CreateOption("...", "...", cenkSaysDoYouBelieveNode, true);
        DialogOption silentOption5 = DialogBuilder.CreateOption("...", "...", enginSaysNoNode, true);
        DialogBuilder.AddOption(enginSaysWatchYourWordsNode, silentOption);
        DialogBuilder.AddOption(motherSaidThat, silentOption2);
        DialogBuilder.AddOption(theyWouldHarmUs, silentOption2);
        DialogBuilder.AddOption(cenksRemindsOldsNode, silentOption3);
        DialogBuilder.AddOption(enginSaysWhyHeKilledNode, silentOption4);
        DialogBuilder.AddOption(cenkSaysDoYouBelieveNode, silentOption5);


        //Bu dalı ana dala bağlayan seçeneği ekler.
        DialogOption enginSaysWatchYourWordsOption = DialogBuilder.CreateOption("Watch your words! Who is murderer?", "Ağzını topla. Kim katilmiş?", enginSaysWatchYourWordsNode);
        DialogBuilder.AddOption(sheKilledTwoNode, enginSaysWatchYourWordsOption);

        ///////////////////////////////////////////////////////////////////////////////////////////////


        //2.Dal "BEN KATİL OLMAK İSTEMİYORUM" SEÇENEĞİ
        DialogNode enginSaysIdontWantNode = DialogBuilder.CreateNode
        ("I don't want to be a murderer, Cenk. I don't want to kill. I don't want to be a bad person.",
        "Ben katil olmak istemiyorum Cenk. Öldürmek istemiyorum. Kötü biri olmak istemiyorum.",
        "Engin");

        DialogNode cenkSaysDoYouRememberNode = DialogBuilder.CreateNode
        ("You will murder until you leave this house, brother. You will kill a lot. Do you remember that man, the fat one?",
        "Bu evde kaldığın sürece öldüreceksin abi. Hem de çok öldüreceksin. Hatırlıyor musun o şişman adamı?",
        "Cenk");

        
        //2.1 "BOĞAZINI KESTİM" CEVABI
        DialogNode cutTheThroatNode = DialogBuilder.CreateNode
        ("I slited his throat. With a paring knife. It was hard.",
        "Boğazını kestim. Bir meyve bıçağıyla. Çok zordu.",
        "Engin");

        DialogOption cutTheThroatOption = DialogBuilder.CreateOption("I slited his throat", "Boğazını kestim", cutTheThroatNode);
        DialogBuilder.AddOption(cenkSaysDoYouRememberNode, cutTheThroatOption);


        //2.2 "HATIRLAMAK İSTEMİYORUM" CEVABI
        DialogNode dontWantRememberNode = DialogBuilder.CreateNode
        ("I don't even want to think about it. He had writhed a lot.",
        "Hatırlamak bile istemiyorum. Çok debelenmişti.",
        "Engin");

        DialogOption dontWantRememberOption = DialogBuilder.CreateOption("I don't want to remember.", "Hatırlamak istemiyorum", dontWantRememberNode);
        DialogBuilder.AddOption(cenkSaysDoYouRememberNode, dontWantRememberOption);


        // 2 - Devam
        DialogNode motherDontLikeFatsNode = DialogBuilder.CreateNode
        ("You killed someone simply because Mother doesn't like fat people as if he could harm our family.",
        "Sırf Anne şişman insanları sevmiyor diye birini öldürdün sen. Güya ailemize zarar verebilir diye.",
        "Cenk");

        DialogNode DontTortureMeNode = DialogBuilder.CreateNode
        ("Cenk! Do you want to torture me?",
        "Cenk! Bana işkence etmek mi istiyorsun",
        "Engin");

        DialogNode cenkSaysNo = DialogBuilder.CreateNode
        ("No brother.",
        "Hayır abi.",
        "Cenk");

        DialogNode whatDoYouWantNode = DialogBuilder.CreateNode
        ("So what do you want me to do then?",
        "O zaman ne yapmamı istiyorsun",
        "Engin");

        //Enginin diyalogundan sonra cenke ilk geçiş.
        DialogOption fromEnginToCenk = DialogBuilder.CreateOption("...", "...", cenkSaysDoYouRememberNode, true);
        DialogBuilder.AddOption(enginSaysIdontWantNode, fromEnginToCenk);

        //Cenk ve Engin'in 2.1 ve 2.2 ve 2
        DialogOption silentOption6 = DialogBuilder.CreateOption("...", "...", motherDontLikeFatsNode, true);
        DialogOption silentOption7 = DialogBuilder.CreateOption("...", "...", motherDontLikeFatsNode, true);
        DialogOption silentOption8 = DialogBuilder.CreateOption("...", "...", DontTortureMeNode, true);
        DialogOption silentOption9 = DialogBuilder.CreateOption("...", "...", cenkSaysNo, true);
        DialogOption silentOption10 = DialogBuilder.CreateOption("...", "...", whatDoYouWantNode, true);
        DialogBuilder.AddOption(cutTheThroatNode, silentOption6);
        DialogBuilder.AddOption(dontWantRememberNode, silentOption7);
        DialogBuilder.AddOption(motherDontLikeFatsNode, silentOption8);
        DialogBuilder.AddOption(DontTortureMeNode, silentOption9);
        DialogBuilder.AddOption(cenkSaysNo, silentOption10);


        //Bu dalı ana dala bağlayan seçeneği ekler.
        DialogOption enginSaysIdontWantOption = DialogBuilder.CreateOption("I don't want to be a murderer.", "Ben katil olmak istemiyorum.", enginSaysIdontWantNode);
        DialogBuilder.AddOption(sheKilledTwoNode, enginSaysIdontWantOption);



        ///////////////////////////////////////////////////////////////////////////////////////////////
        


        //ANA DAL DEVAM
        DialogNode leaveHereNode = DialogBuilder.CreateNode
        ("You need to leave this house. Here isn't right for you.",
        "Bu evden git abi. Sana göre değil bu ev.",
        "Cenk");

        // Ana düğüm - 2.1: Birinci dalın ana düğüme bağlantısı
        DialogOption whatDoYouWantSilentOption = DialogBuilder.CreateOption("...", "...", leaveHereNode);
        DialogBuilder.AddOption(whatDoYouWantNode, whatDoYouWantSilentOption);

        // Ana düğüm - 2.2: İkinci dalın ana düğüme bağlantısı
        DialogOption enginSaysNoSilentOption = DialogBuilder.CreateOption("...", "...", leaveHereNode);
        DialogBuilder.AddOption(enginSaysNoNode, enginSaysNoSilentOption);

        DialogNode toWhereNode = DialogBuilder.CreateNode
        ("To where?",
        "Nereye",
        "Engin");
        
        // Ana düğüm - 3: Bu evden git ile nereye bağlantısı
        DialogOption leaveHereSilentOp = DialogBuilder.CreateOption("...", "...", toWhereNode);
        DialogBuilder.AddOption(leaveHereNode, leaveHereSilentOp);

        DialogNode goToCousinNode = DialogBuilder.CreateNode
        ("Here, I've put the cousin's number from Bursa right here. He will take you in, mom won't find out. And take this, here some money I set aside for you.",
        "Bursa'daki kuzenin telefon numarasını buraya yazdım. O seni evine alır, anne duymaz. Senin için ayırdığım biraz para. Al bu parayı da.",
        "Cenk");
        
        // Ana düğüm - 4: Nereye ile kuzene git bağlantısı
        DialogOption toWhereSilentOp = DialogBuilder.CreateOption("...", "...", goToCousinNode);
        DialogBuilder.AddOption(toWhereNode, toWhereSilentOp);

        //Engin parayı aldıktan sonra
        DialogNode cenksLastWords = DialogBuilder.CreateNode
        ("The neighbor's cake must be almost finished by now, hurry up and get out of here.",
        "Üst komşunun keki bitmek üzeredir abi acele et. Kaç buradan.",
        "Cenk");
        
        // Ana düğüm - 5: Kuzene git ile cenkin son sözleri bağlantısı
        DialogOption toCousinSilentOp = DialogBuilder.CreateOption("...", "...", cenksLastWords);
        DialogBuilder.AddOption(goToCousinNode, toCousinSilentOp);


        

    }
}
