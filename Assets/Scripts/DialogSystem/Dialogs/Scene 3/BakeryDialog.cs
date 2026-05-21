using UnityEngine;

public class BakeryDialog : MonoBehaviour
{
    [SerializeField] private DialogSystem dialogSystem;
    [SerializeField] private DialogNode bakeryStartNode;
    [SerializeField] private MissionObjective missionObj;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        BuildDialog();
    }

    void BuildDialog()
    {
        // ==========================================
        // BAŞLANGIÇ
        // ==========================================
        bakeryStartNode = DialogBuilder.CreateNode(
            "Ughhh...",
            "Offff...",
            "Amca"
        );

        // ==========================================
        // 1. SEÇİM DALLLARI: Are you okay / Who was that?
        // ==========================================
        DialogNode enginAreYouOkay = DialogBuilder.CreateNode(
            "You look pretty banged up, old man. Are you okay?",
            "Bayağı hırpalanmış görünüyorsun amca. İyi misin?",
            "Engin"
        );
        DialogNode oldManBeenThrough = DialogBuilder.CreateNode(
            "If you only knew what I've been through back in the day. This is nothing.",
            "Eskiden neler yaşadığımı bir bilsen. Bu hiçbir şey.",
            "Amca"
        );
        DialogNode enginSeeDoctor = DialogBuilder.CreateNode(
            "You should see a doctor if you need to.",
            "Gerekirse bir doktora görünmelisin.",
            "Engin"
        );
        DialogNode oldManNonsense = DialogBuilder.CreateNode(
            "Nonsense, no way. What for? I never take a punch I can't get back up from.",
            "Saçmalama, olmaz öyle şey. Ne gerek var? Ben kalkamayacağım yumruğu yemem.",
            "Amca"
        );

        DialogBuilder.AddOption(enginAreYouOkay, DialogBuilder.CreateOption("...", "...", oldManBeenThrough, true));
        DialogBuilder.AddOption(oldManBeenThrough, DialogBuilder.CreateOption("...", "...", enginSeeDoctor, true));
        DialogBuilder.AddOption(enginSeeDoctor, DialogBuilder.CreateOption("...", "...", oldManNonsense, true));

        // Branch 1 - 2. Seçenek
        DialogNode enginWhoPunk = DialogBuilder.CreateNode(
            "Old man, pardon my asking, but who was that punk?",
            "Amca affına sığınarak soruyorum, kimdi o serseri?",
            "Engin"
        );
        DialogNode oldManLongStory = DialogBuilder.CreateNode(
            "Long story. Just some punk.",
            "Uzun hikaye. Serserinin teki işte.",
            "Amca"
        );
        DialogNode enginFilthyMouth = DialogBuilder.CreateNode(
            "He had a filthy mouth.",
            "Ağzı çok bozuktu.",
            "Engin"
        );
        DialogNode oldManSoap = DialogBuilder.CreateNode(
            "If you gathered all the soap in the world, you couldn't wash that mouth clean. Let's eat something, I'll tell you what happened, don't worry.",
            "Dünyadaki bütün sabunları toplasan o ağzı yıkayıp temizleyemezsin. Hadi bir şeyler yiyelim, anlatacağım sana ne olduğunu, merak etme.",
            "Amca"
        );

        DialogBuilder.AddOption(enginWhoPunk, DialogBuilder.CreateOption("...", "...", oldManLongStory, true));
        DialogBuilder.AddOption(oldManLongStory, DialogBuilder.CreateOption("...", "...", enginFilthyMouth, true));
        DialogBuilder.AddOption(enginFilthyMouth, DialogBuilder.CreateOption("...", "...", oldManSoap, true));

        // Kamil Efendi mırıldandıktan sonra oyuncu seçim yapar
        DialogBuilder.AddOption(bakeryStartNode, DialogBuilder.CreateOption("Are you okay, Old Man?", "İyi misin Amca?", enginAreYouOkay));
        DialogBuilder.AddOption(bakeryStartNode, DialogBuilder.CreateOption("Who was that?", "Kimdi o?", enginWhoPunk));

        // ==========================================
        // ORTAK DEVAM: The pastries arrive...
        // ==========================================
        DialogNode actionPastries = DialogBuilder.CreateNode(
            "(The pastries arrive. After Engin takes his first bite...)",
            "(Poğaçalar gelir. Engin ilk ısırığını aldıktan sonra...)",
            "Action"
        );
        DialogNode oldManDidYouSee = DialogBuilder.CreateNode(
            "Did you see the person sitting in the car?",
            "Arabada oturan kişiyi gördün mü?",
            "Amca"
        );

        DialogBuilder.AddOption(oldManNonsense, DialogBuilder.CreateOption("...", "...", actionPastries, true));
        DialogBuilder.AddOption(oldManSoap, DialogBuilder.CreateOption("...", "...", actionPastries, true));
        DialogBuilder.AddOption(actionPastries, DialogBuilder.CreateOption("...", "...", oldManDidYouSee, true));

        // ==========================================
        // 2. SEÇİM DALLARI: A Woman / A Man
        // ==========================================
        DialogNode enginWoman = DialogBuilder.CreateNode(
            "There was a woman in the driver's seat, a blonde. She kept honking the horn non-stop.",
            "Sürücü koltuğunda bir kadın vardı, sarışın. Sürekli kornaya basıyordu.",
            "Engin"
        );
        DialogNode oldManYeahShe = DialogBuilder.CreateNode(
            "Yeah, that's her.",
            "Evet, ta kendisi.",
            "Amca"
        );
        DialogBuilder.AddOption(enginWoman, DialogBuilder.CreateOption("...", "...", oldManYeahShe, true));

        DialogNode enginMan = DialogBuilder.CreateNode(
            "There was a big, bearded guy.",
            "İriyarı, sakallı bir adam vardı.",
            "Engin"
        );
        DialogNode oldManMakeThingsUp = DialogBuilder.CreateNode(
            "You can just say you didn't see, no need to make things up.",
            "Görmedim diyebilirsin, uydurmana gerek yok.",
            "Amca"
        );
        DialogBuilder.AddOption(enginMan, DialogBuilder.CreateOption("...", "...", oldManMakeThingsUp, true));

        DialogBuilder.AddOption(oldManDidYouSee, DialogBuilder.CreateOption("A Woman.", "Bir kadın.", enginWoman));
        DialogBuilder.AddOption(oldManDidYouSee, DialogBuilder.CreateOption("A Man.", "Bir erkek.", enginMan));

        // ==========================================
        // ORTAK DEVAM: Who is she?
        // ==========================================
        DialogNode enginWhoIsShe = DialogBuilder.CreateNode(
            "Who is she?",
            "Kim o?",
            "Engin"
        );
        DialogBuilder.AddOption(oldManYeahShe, DialogBuilder.CreateOption("...", "...", enginWhoIsShe, true));
        DialogBuilder.AddOption(oldManMakeThingsUp, DialogBuilder.CreateOption("...", "...", enginWhoIsShe, true));

        DialogNode oldManMyDaughter = DialogBuilder.CreateNode(
            "She is my daughter. Asuman. One day she called me 'Dad' and said, 'I'm going to Istanbul to study.' With some punk on her arm. That punk from this morning. I didn't like the guy the second I saw him anyway. It had only been about two months since she lost her mother. I loved my wife very much.",
            "O benim kızım. Asuman. Bir gün bana 'Baba' dedi, 'Ben okumaya İstanbul'a gidiyorum.' Kolunda bir serseriyle. Sabahki serseriyle. Zaten görür görmez sevmemiştim herifi. Annesini kaybedeli daha iki ay olmuştu. Karımı çok severdim.",
            "Amca"
        );
        DialogNode actionSilence1 = DialogBuilder.CreateNode(
            "(Silence falls. The Old Man takes a bite. Engin takes a second bite.)",
            "(Sessizlik çöker. Amca bir ısırık alır. Engin ikinci ısırığını alır.)",
            "Action"
        );
        DialogNode oldManAfraid = DialogBuilder.CreateNode(
            "I was afraid of losing my daughter. When she suddenly said that... I beat them both up that day. I wish my hands had broken, I wish I had died so I couldn't have done it. Couldn't have brought myself to do it. I was afraid of losing my little girl. And because of that, I lost her.",
            "Kızımı kaybetmekten korktum. O aniden öyle diyince... O gün ikisini de dövdüm. Keşke ellerim kırılsaydı da, keşke ölseydim de yapamasaydım. Kendime yediremedim. Küçük kızımı kaybetmekten korktum. Ve bu yüzden, onu kaybettim.",
            "Amca"
        );
        DialogNode enginPunkTreat = DialogBuilder.CreateNode(
            "What about what the Punk said? How does he treat your daughter?",
            "Peki ya Serserinin dedikleri? Kızına nasıl davranıyor?",
            "Engin"
        );
        DialogNode actionSilence2 = DialogBuilder.CreateNode(
            "(Silence falls again. They each take another bite.)",
            "(Tekrar sessizlik çöker. İkisi de birer ısırık daha alır.)",
            "Action"
        );
        DialogNode oldManPimping = DialogBuilder.CreateNode(
            "You know what hurts me the most? The bastard is pimping my daughter out. And he gambles away the money he gets. The scumbag owes money to every gambling den around here. Everyone knows him here. And nobody likes him. Oh, if I could just talk to Asuman...",
            "Benim canımı en çok ne yakıyor biliyor musun? Şerefsiz benim kızımı pazarlıyor. Ve kazandığı parayı kumarda yiyor. Pisliğin buralardaki her kumarhaneye borcu var. Herkes onu tanır burada. Ve kimse de sevmez. Ah, Asuman'la bir konuşabilsem...",
            "Amca"
        );
        DialogNode enginWhatTell = DialogBuilder.CreateNode(
            "What are you going to tell her, old man?",
            "Ona ne söyleyeceksin amca?",
            "Engin"
        );
        DialogNode actionSilence3 = DialogBuilder.CreateNode(
            "(Another silence. Engin eats the last bite on his plate.)",
            "(Yine bir sessizlik. Engin tabağındaki son lokmayı da yer.)",
            "Action"
        );
        DialogNode oldManImSorry = DialogBuilder.CreateNode(
            "'I'm sorry. For everything I've done. Come back home. Let this misery end.' I've been chasing her in Istanbul for 6 months; this place is a hellhole. 'Come on, let's go back to Bursa together. To our home, our neighborhood.' If she said yes right now, I'd hit the road to Bursa this very minute.",
            "'Özür dilerim. Yaptığım her şey için. Eve dön. Bitsin bu sefalet.' İstanbul'da 6 aydır peşinden koşuyorum; burası bir cehennem. 'Gel, beraber dönelim Bursa'ya. Evimize, mahallemize.' Şimdi evet dese, şu dakika düşerim Bursa yollarına.",
            "Amca"
        );

        DialogBuilder.AddOption(enginWhoIsShe, DialogBuilder.CreateOption("...", "...", oldManMyDaughter, true));
        DialogBuilder.AddOption(oldManMyDaughter, DialogBuilder.CreateOption("...", "...", actionSilence1, true));
        DialogBuilder.AddOption(actionSilence1, DialogBuilder.CreateOption("...", "...", oldManAfraid, true));
        DialogBuilder.AddOption(oldManAfraid, DialogBuilder.CreateOption("...", "...", enginPunkTreat, true));
        DialogBuilder.AddOption(enginPunkTreat, DialogBuilder.CreateOption("...", "...", actionSilence2, true));
        DialogBuilder.AddOption(actionSilence2, DialogBuilder.CreateOption("...", "...", oldManPimping, true));
        DialogBuilder.AddOption(oldManPimping, DialogBuilder.CreateOption("...", "...", enginWhatTell, true));
        DialogBuilder.AddOption(enginWhatTell, DialogBuilder.CreateOption("...", "...", actionSilence3, true));
        DialogBuilder.AddOption(actionSilence3, DialogBuilder.CreateOption("...", "...", oldManImSorry, true));

        // ==========================================
        // 3. SEÇİM DALLARI: My cousin lives in Bursa too / You won't return to Bursa
        // ==========================================
        DialogNode enginCousin = DialogBuilder.CreateNode(
            "What a coincidence. My cousin lives in Bursa too. In Minareliçavuş.",
            "Ne tesadüf. Benim kuzenim de Bursa'da yaşıyor. Minareliçavuş'ta.",
            "Engin"
        );
        DialogNode oldMan20Mins = DialogBuilder.CreateNode(
            "That's 20 minutes away from our house.",
            "Bizim eve 20 dakika uzaklıkta orası.",
            "Amca"
        );
        DialogBuilder.AddOption(enginCousin, DialogBuilder.CreateOption("...", "...", oldMan20Mins, true));

        DialogNode enginWontReturn = DialogBuilder.CreateNode(
            "Old man, is there no other way for you to return to Bursa? Won't you let your daughter go?",
            "Amca, senin Bursa'ya dönmenin başka yolu yok mu? Kızını bırakmayacak mısın?",
            "Engin"
        );
        DialogNode oldManCantLive = DialogBuilder.CreateNode(
            "I can't live without my daughter. My house feels like a prison. Either she comes to that house, or I'll crawl after her on the roads she walks until the day I die.",
            "Ben kızımsız yaşayamam. Evim bana zindan. Ya o eve gelir, ya da ben ölene kadar onun yürüdüğü yollarda peşinden sürünürüm.",
            "Amca"
        );
        DialogBuilder.AddOption(enginWontReturn, DialogBuilder.CreateOption("...", "...", oldManCantLive, true));

        DialogBuilder.AddOption(oldManImSorry, DialogBuilder.CreateOption("My cousin lives in Bursa too.", "Benim kuzenim de Bursa'da.", enginCousin));
        DialogBuilder.AddOption(oldManImSorry, DialogBuilder.CreateOption("You won't return without Asuman?", "Asuman olmadan dönmeyecek misin?", enginWontReturn));

        // ==========================================
        // ORTAK DEVAM: I'll convince Asuman...
        // ==========================================
        DialogNode enginConvince = DialogBuilder.CreateNode(
            "(An idea suddenly pops into Engin's mind.) I'll convince Asuman. Since she won't talk to you, I'll talk to her and fix this. Surely she'd prefer returning home over the life she's living now.",
            "(Engin'in aklına aniden bir fikir gelir.) Asuman'ı ben ikna edeceğim. Seninle konuşmadığına göre, ben onunla konuşur ve bu işi hallederim. Eminim şu an yaşadığı hayattansa eve dönmeyi tercih edecektir.",
            "Engin"
        );
        DialogBuilder.AddOption(oldMan20Mins, DialogBuilder.CreateOption("...", "...", enginConvince, true));
        DialogBuilder.AddOption(oldManCantLive, DialogBuilder.CreateOption("...", "...", enginConvince, true));

        DialogNode oldManWhyHelp = DialogBuilder.CreateNode(
            "Why would you help me?",
            "Bana neden yardım ediyorsun ki?",
            "Amca"
        );
        DialogNode enginTakeMe = DialogBuilder.CreateNode(
            "I need you to take me to Bursa too. Take me away from this wretched city.",
            "Benim de senin beni Bursa'ya götürmene ihtiyacım var. Beni bu lanet olası şehirden kurtar.",
            "Engin"
        );
        DialogNode kamilGrunt2 = DialogBuilder.CreateNode(
            "Hmmmm... (Grunts in an approving tone)",
            "Hmmmm... (Onaylayan bir tonda mırıldanır)",
            "Kamil Efendi"
        );
        DialogNode oldManRightKamil = DialogBuilder.CreateNode(
            "You're right, Kamil Efendi. What do I have to lose? Alright young man, let's go see the punk together. But he shouldn't see me. Find a way to meet Asuman and get him to agree. Then you can talk to Asuman and try to persuade her.",
            "Haklısın Kamil Efendi. Kaybedecek neyim var? Pekala delikanlı, hadi gidip şu serseriyi birlikte görelim. Ama beni görmemeli. Bir yolunu bul, serseriyi ikna et. Sonra da Asuman'la konuşup onu ikna etmeye çalışırsın.",
            "Amca"
        );
        DialogNode enginAlrightGo = DialogBuilder.CreateNode(
            "Alright then, let's go see the Punk.",
            "Pekala o zaman, gidip şu Serseri'yi bulalım.",
            "Engin"
        );
        DialogNode oldManTeaFirst = DialogBuilder.CreateNode(
            "We should have had a tea first.",
            "Önce bir çay içseydik.",
            "Amca"
        );
        DialogNode enginFastEnd = DialogBuilder.CreateEndNode(
            "Let's get this sorted out quickly.",
            "Şu işi hemen halledelim.",
            "Engin"
        );

        // Düğümleri birbirine bağlama
        DialogBuilder.AddOption(enginConvince, DialogBuilder.CreateOption("...", "...", oldManWhyHelp, true));
        DialogBuilder.AddOption(oldManWhyHelp, DialogBuilder.CreateOption("...", "...", enginTakeMe, true));
        DialogBuilder.AddOption(enginTakeMe, DialogBuilder.CreateOption("...", "...", kamilGrunt2, true));
        DialogBuilder.AddOption(kamilGrunt2, DialogBuilder.CreateOption("...", "...", oldManRightKamil, true));
        DialogBuilder.AddOption(oldManRightKamil, DialogBuilder.CreateOption("...", "...", enginAlrightGo, true));
        DialogBuilder.AddOption(enginAlrightGo, DialogBuilder.CreateOption("...", "...", oldManTeaFirst, true));
        DialogBuilder.AddOption(oldManTeaFirst, DialogBuilder.CreateOption("...", "...", enginFastEnd, true));
    }
}
