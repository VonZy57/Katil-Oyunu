using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class TalkWithCenkOnTable : MonoBehaviour
{
    [SerializeField] private DialogSystem dialogSystem;

    [Header("Ses Referansları")]
    [SerializeField] private List<SpeakerAudio> speakerAudios;

    [Header("Cenk Sesleri")]
    [SerializeField] private AudioClip clip_cenkStarts;
    [SerializeField] private AudioClip clip_sheKilledTwo;
    [SerializeField] private AudioClip clip_cenkSaysYouKilled;
    [SerializeField] private AudioClip clip_cenksRemindsOlds;
    [SerializeField] private AudioClip clip_cenkSaysDoYouBelieve;
    [SerializeField] private AudioClip clip_cenkSaysDoYouRemember;
    [SerializeField] private AudioClip clip_motherDontLikeFats;
    [SerializeField] private AudioClip clip_cenkSaysNo;
    [SerializeField] private AudioClip clip_leaveHere;
    [SerializeField] private AudioClip clip_goToCousin;
    [SerializeField] private AudioClip clip_cenksLastWords;

    [Header("Engin Sesleri")]
    [SerializeField] private AudioClip clip_enginSays;
    [SerializeField] private AudioClip clip_enginSaysWatchYourWords;
    [SerializeField] private AudioClip clip_motherSaidThat;
    [SerializeField] private AudioClip clip_theyWouldHarmUs;
    [SerializeField] private AudioClip clip_enginSaysWhyHeKilled;
    [SerializeField] private AudioClip clip_enginSaysNo;
    [SerializeField] private AudioClip clip_enginSaysIdontWant;
    [SerializeField] private AudioClip clip_cutTheThroat;
    [SerializeField] private AudioClip clip_dontWantRemember;
    [SerializeField] private AudioClip clip_dontTortureMe;
    [SerializeField] private AudioClip clip_whatDoYouWant;
    [SerializeField] private AudioClip clip_toWhere;
    [SerializeField] private DialogNode cenkStartsNode;
    [SerializeField] private MissionObjective missionObj;

    [Header("Kamera ve Bakış Referansları")]
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private Transform cenkTransform;
    [SerializeField] private Transform moneyTransform; // Paranın (veya masadaki paranın olduğu yerin) referansı
    [SerializeField] private TalkWithNeighbor talkWithNeighbor;

    private FirstPersonController playerFPS;


    void Start()
    {
        BuildDialogTree();

        // Başlangıçta parayı gizle (Cenk çıkarana kadar görünmesin)
        if (moneyTransform != null)
        {
            moneyTransform.gameObject.SetActive(false);
        }

        // Oyuncu kamerasından FPS kontrolcüsünü bul
        if (playerCamera != null)
        {
            playerFPS = playerCamera.GetComponentInParent<FirstPersonController>();
        }
    }

    void Update()
    {
        
    }

    public void StartTheDialog()
    {
        StartCoroutine(WaitAndStartDialog());
    }

    private IEnumerator WaitAndStartDialog()
    {
        // Eğer komşu diyaloğu atanmışsa ve henüz bitmemişse, bitmesini bekle
        if (talkWithNeighbor != null)
        {
            yield return new WaitUntil(() => talkWithNeighbor.isFinished);
        }

        if (dialogSystem != null)
        {
            if (playerFPS != null) playerFPS.enabled = false; // Kontrolleri kapat ki kamera dönmesi çakışmasın

            dialogSystem.SetSpeakers(speakerAudios);
            dialogSystem.StartDialog(cenkStartsNode);
            
            // Diyalog başladığında kamerayı doğrudan Cenk'e döndür
            if (playerCamera != null && cenkTransform != null)
            {
                Camera cam = playerCamera.GetComponent<Camera>();
                if (cam == null) cam = playerCamera.GetComponentInChildren<Camera>();
                if (cam == null) cam = Camera.main; // Fallback

                Transform camTransform = cam != null ? cam.transform : playerCamera.transform;
                camTransform.DOKill(); // Olası önceki animasyonları durdur
                camTransform.DOLookAt(cenkTransform.position, 1f).SetEase(Ease.InOutSine);
            }

            StartCoroutine(CheckDialogEnd());
        }
    }

    private IEnumerator LookAtMoneySequence()
    {
        // Kamera paraya ve Cenk'e bakmak için hazırsa
        if (playerCamera != null && moneyTransform != null && cenkTransform != null)
        {
            Camera cam = playerCamera.GetComponent<Camera>();
            if (cam == null) cam = playerCamera.GetComponentInChildren<Camera>();
            if (cam == null) cam = Camera.main; // Fallback olarak ana kamerayı bul

            Transform camTransform = cam != null ? cam.transform : playerCamera.transform;
            float originalFOV = cam != null ? cam.fieldOfView : 60f;
            float zoomFOV = originalFOV - 20f; // Yaklaşmak için FOV değerini küçült

            // Parayı sahnede görünür yap
            moneyTransform.gameObject.SetActive(true);

            // 1. Önce paraya bak
            camTransform.DOKill();
            if (cam != null) { cam.DOKill(); cam.DOFieldOfView(zoomFOV, 1f).SetEase(Ease.InOutSine); }
            camTransform.DOLookAt(moneyTransform.position, 1f).SetEase(Ease.InOutSine);
            yield return new WaitForSeconds(2.5f); // Paraya tam odaklanabilmesi için süreyi uzattık
            
            // 2. Tekrar Cenk'e dön
            camTransform.DOKill();
            if (cam != null) { cam.DOKill(); cam.DOFieldOfView(originalFOV, 1f).SetEase(Ease.InOutSine); }
            camTransform.DOLookAt(cenkTransform.position, 1f).SetEase(Ease.InOutSine);
        }
    }

    private IEnumerator CheckDialogEnd()
    {
        // Önce diyalog panelinin aktif olmasını bekle
        yield return new WaitUntil(() => dialogSystem.dialogPanel.activeSelf);

        // Şimdi de diyalog panelinin kapanmasını bekle
        yield return new WaitUntil(() => !dialogSystem.dialogPanel.activeSelf);

        if (playerFPS != null)
        {
            playerFPS.SyncCameraRotation(); // Açıyı eşitle ki ani atlama olmasın
            playerFPS.enabled = true; // Oyuncuya kontrolü geri ver
        }

        // Diyalog bitti, görevi tamamla
        if (missionObj != null)
        {
            missionObj.OnInteracted();
        }
    }
    
    void BuildDialogTree()
    {
        cenkStartsNode = DialogBuilder.CreateNode
        ("You do not belong here, brother. The dead were right, last night. Mother is crazy. Look, what she made you do.",
        "Abi... Sen buraya ait değilsin. Ölüler haklıydı, dün gece. Anne bir deli. Bak neler yaptırdı sana.",
        "Cenk");

        DialogNode enginSaysNode = DialogBuilder.CreateNode
        ("Cenk, She is my mother.",
        "O benim annem Cenk.",
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
        ("Watch your words, you're talking to your elder brother. What the hell do you think you're talking about? Murderer???",
        "Ağzını topla, abinle konuşuyorsun. Katil matil ne zırvalıyorsun?!",
        "Engin");

        DialogNode cenkSaysYouKilled = DialogBuilder.CreateNode
        ("So, is it a lie? Didn't you kill those two?",
        "Ama abi, yalan mı? Sen öldürmedin mi onları?",
        "Cenk");


        //1.1 "ANNE SÖYLEDİ" CEVABI
        DialogNode motherSaidThat = DialogBuilder.CreateNode
        ("Mother said that, Cenk. Mother never wants bad things. That means they were bad.",
        "Anne söyledi Cenk Anne asla kötü bir şey istemez. Demek ki kötü insanlarmış.",
        "Engin");

        DialogOption motherSaidOption = DialogBuilder.CreateOption("Mother said that.", "Anne söyledi.", motherSaidThat);
        DialogBuilder.AddOption(cenkSaysYouKilled, motherSaidOption);


        //1.2 "AİLEMİZE ZARAR VERECEKLERDİ" CEVABI
        DialogNode theyWouldHarmUs = DialogBuilder.CreateNode
        ("They would have harmed our family, Cenk. You know what they said about us. They weren't good people.",
        "Ailemize zarar vereceklerdi Cenk. Bizim hakkımızda ne dediklerini duydun. İyi insan değillerdi.",
        "Engin");

        DialogOption theyWouldHarmOption = DialogBuilder.CreateOption("They would have harmed our family.", "Ailemize zarar vereceklerdi.", theyWouldHarmUs);
        DialogBuilder.AddOption(cenkSaysYouKilled, theyWouldHarmOption);

        // 1 - Devam
        DialogNode cenksRemindsOldsNode = DialogBuilder.CreateNode
        ("Okay, let's say these people were bad. But why did she made you kill a kid on the street, two months ago?",
        "Hadi bu insanlar kötü. 2 ay önce, sokaktaki çocuğu niye öldürttü sana?",
        "Cenk"); 

        DialogNode enginSaysWhyHeKilledNode = DialogBuilder.CreateNode
        ("He was walking crookedly. He was never gonna be strong and handsome like you and me.",
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
        DialogOption enginSaysWatchYourWordsOption = DialogBuilder.CreateOption("Watch your mouth! Who's a murderer?", "Ağzını topla. Kim katilmiş?", enginSaysWatchYourWordsNode);
        DialogBuilder.AddOption(sheKilledTwoNode, enginSaysWatchYourWordsOption);

        ///////////////////////////////////////////////////////////////////////////////////////////////


        //2.Dal "BEN KATİL OLMAK İSTEMİYORUM" SEÇENEĞİ
        DialogNode enginSaysIdontWantNode = DialogBuilder.CreateNode
        ("I don't want to be a murderer, Cenk. I don't want to kill. I don't want to be a bad person.",
        "Ben katil olmak istemiyorum Cenk. Öldürmek istemiyorum. Kötü biri olmak istemiyorum.",
        "Engin");

        DialogNode cenkSaysDoYouRememberNode = DialogBuilder.CreateNode
        ("You will murder until you leave this house, brother. And you will murder a lot. Do you remember that man, the fat one?",
        "Bu evde kaldığın sürece öldüreceksin abi. Hem de çok öldüreceksin. Hatırlıyor musun o şişman adamı?",
        "Cenk");

        
        //2.1 "BOĞAZINI KESTİM" CEVABI
        DialogNode cutTheThroatNode = DialogBuilder.CreateNode
        ("I slit his throat. With a fruit knife. It was hard.",
        "Boğazını kestim. Bir meyve bıçağıyla. Çok zordu.",
        "Engin");

        DialogOption cutTheThroatOption = DialogBuilder.CreateOption("I slit his throat", "Boğazını kestim", cutTheThroatNode);
        DialogBuilder.AddOption(cenkSaysDoYouRememberNode, cutTheThroatOption);


        //2.2 "HATIRLAMAK İSTEMİYORUM" CEVABI
        DialogNode dontWantRememberNode = DialogBuilder.CreateNode
        ("I don't even want to think about it. He was struggling a lot.",
        "Hatırlamak bile istemiyorum. Çok debelenmişti.",
        "Engin");

        DialogOption dontWantRememberOption = DialogBuilder.CreateOption("I don't want to remember.", "Hatırlamak istemiyorum", dontWantRememberNode);
        DialogBuilder.AddOption(cenkSaysDoYouRememberNode, dontWantRememberOption);


        // 2 - Devam
        DialogNode motherDontLikeFatsNode = DialogBuilder.CreateNode
        ("You killed someone simply because Mother doesn't like fat people. Supposedly because he could 'harm' our family.",
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
        ("You need to leave this house. This house isn't for you.",
        "Bu evden git abi. Sana göre değil bu ev.",
        "Cenk");

        // Ana düğüm - 2.1: Birinci dalın ana düğüme bağlantısı
        DialogOption whatDoYouWantSilentOption = DialogBuilder.CreateOption("...", "...", leaveHereNode, true);
        DialogBuilder.AddOption(whatDoYouWantNode, whatDoYouWantSilentOption);

        // Ana düğüm - 2.2: İkinci dalın ana düğüme bağlantısı
        DialogOption enginSaysNoSilentOption = DialogBuilder.CreateOption("...", "...", leaveHereNode, true);
        DialogBuilder.AddOption(enginSaysNoNode, enginSaysNoSilentOption);

        DialogNode toWhereNode = DialogBuilder.CreateNode
        ("To where?",
        "Nereye",
        "Engin");
        
        // Ana düğüm - 3: Bu evden git ile nereye bağlantısı
        DialogOption leaveHereSilentOp = DialogBuilder.CreateOption("...", "...", toWhereNode, true);
        DialogBuilder.AddOption(leaveHereNode, leaveHereSilentOp);

        DialogNode goToCousinNode = DialogBuilder.CreateNode
        ("Here, I've put cousin's number from Bursa here. He will take you in, mother won't find out. And take this, here is some money I set aside for you.",
        "Bursa'daki kuzenin telefon numarasını buraya yazdım. O seni evine alır, anne duymaz. Senin için ayırdığım biraz para. Al bu parayı da.",
        "Cenk");
        
        // Ana düğüm - 4: Nereye ile kuzene git bağlantısı
        // Cenk parayı çıkardığı an çalışacak şekilde güncellendi (CreateOptionWithEvent kullanıldı)
        DialogOption toWhereSilentOp = DialogBuilder.CreateOptionWithEvent("...", "...", goToCousinNode, () => { StartCoroutine(LookAtMoneySequence()); }, true);
        DialogBuilder.AddOption(toWhereNode, toWhereSilentOp);

        //Engin parayı aldıktan sonra
        DialogNode cenksLastWords = DialogBuilder.CreateEndNode
        ("The neighbor's cake must be almost finished by now, hurry up and get out of here.",
        "Üst komşunun keki bitmek üzeredir abi acele et. Kaç buradan.",
        "Cenk");
        
        // Ana düğüm - 5: Kuzene git ile cenkin son sözleri bağlantısı
        DialogOption toCousinSilentOp = DialogBuilder.CreateOption("...", "...", cenksLastWords, true);
        DialogBuilder.AddOption(goToCousinNode, toCousinSilentOp);

        // Cenk cliplerini ata
        cenkStartsNode.voiceClip                = clip_cenkStarts;
        sheKilledTwoNode.voiceClip              = clip_sheKilledTwo;
        cenkSaysYouKilled.voiceClip             = clip_cenkSaysYouKilled;
        cenksRemindsOldsNode.voiceClip          = clip_cenksRemindsOlds;
        cenkSaysDoYouBelieveNode.voiceClip      = clip_cenkSaysDoYouBelieve;
        cenkSaysDoYouRememberNode.voiceClip     = clip_cenkSaysDoYouRemember;
        motherDontLikeFatsNode.voiceClip        = clip_motherDontLikeFats;
        cenkSaysNo.voiceClip                    = clip_cenkSaysNo;
        leaveHereNode.voiceClip                 = clip_leaveHere;
        goToCousinNode.voiceClip                = clip_goToCousin;
        cenksLastWords.voiceClip                = clip_cenksLastWords;

        // Engin cliplerini ata
        enginSaysNode.voiceClip                 = clip_enginSays;
        enginSaysWatchYourWordsNode.voiceClip   = clip_enginSaysWatchYourWords;
        motherSaidThat.voiceClip                = clip_motherSaidThat;
        theyWouldHarmUs.voiceClip               = clip_theyWouldHarmUs;
        enginSaysWhyHeKilledNode.voiceClip      = clip_enginSaysWhyHeKilled;
        enginSaysNoNode.voiceClip               = clip_enginSaysNo;
        enginSaysIdontWantNode.voiceClip        = clip_enginSaysIdontWant;
        cutTheThroatNode.voiceClip              = clip_cutTheThroat;
        dontWantRememberNode.voiceClip          = clip_dontWantRemember;
        DontTortureMeNode.voiceClip             = clip_dontTortureMe;
        whatDoYouWantNode.voiceClip             = clip_whatDoYouWant;
        toWhereNode.voiceClip                   = clip_toWhere;
    }
}
