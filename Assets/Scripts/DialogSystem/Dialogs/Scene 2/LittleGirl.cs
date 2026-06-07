using System.Collections.Generic;
using UnityEngine;

public class LittleGirl : Interactable
{
    private DialogSystem dialogSystem;
    private MissionObjective missionObj;
    public GameObject checkOutSideTrigger;
    private bool hasInteracted = false; // Kız ile konuşuldu mu?
    private bool isDialogActive = false; // Dialog şu anda aktif mi?
    private bool dialogCompleted = false; // Ana dialog tamamlandı mı?

    [System.NonSerialized] private DialogNode mainDialogNode;
    [System.NonSerialized] private DialogNode afterDialogNode;
    public GameObject objectToActivate;

    [Header("Ses Referansları")]
    [SerializeField] private List<SpeakerAudio> speakerAudios;
    [Header("Little Girl Sesleri")]
    [SerializeField] private AudioClip clip_girl_main;
    [SerializeField] private AudioClip clip_girl_opt1Response;
    [SerializeField] private AudioClip clip_girl_sameDream;
    [SerializeField] private AudioClip clip_girl_opt11Response;
    [SerializeField] private AudioClip clip_girl_opt12Response;
    [SerializeField] private AudioClip clip_girl_opt12End;
    [SerializeField] private AudioClip clip_girl_opt2Response;
    [SerializeField] private AudioClip clip_girl_opt21Response;
    [SerializeField] private AudioClip clip_girl_opt21No;
    [SerializeField] private AudioClip clip_girl_opt22Response;
    [SerializeField] private AudioClip clip_girl_commonAnswer;
    [SerializeField] private AudioClip clip_girl_commonEnd;
    [Header("Engin Sesleri")]
    [SerializeField] private AudioClip clip_engin_didYouSee;
    [SerializeField] private AudioClip clip_engin_wasDreaming;
    [SerializeField] private AudioClip clip_engin_maybeWhyNot;
    [SerializeField] private AudioClip clip_engin_ifYouSawMe;
    [SerializeField] private AudioClip clip_engin_dontTell;
    [SerializeField] private AudioClip clip_engin_talking;
    [SerializeField] private AudioClip clip_engin_sawMother;
    [SerializeField] private AudioClip clip_engin_sheWasHere;
    [SerializeField] private AudioClip clip_engin_jumped;
    [SerializeField] private AudioClip clip_engin_whereMom;
    [Header("Kuru Sesleri")]
    [SerializeField] private AudioClip clip_kuru_lavuk;

    void Start()
    {
        // DialogSystem'i otomatik bul
        dialogSystem = FindFirstObjectByType<DialogSystem>();
        missionObj = GetComponent<MissionObjective>();

        checkOutSideTrigger.SetActive(false);

        // Prompt message'ı ayarla
        promptMessage = "E - Talk";

        BuildDialogTree();
    }

    protected override void Interact()
    {
        // E tuşuna basıldığında çağrılır
        if (dialogSystem != null && !isDialogActive)
        {
            if (!dialogCompleted)
            {
                // İlk kez konuşma
                hasInteracted = true;
                dialogSystem.SetSpeakers(speakerAudios);
                dialogSystem.StartDialog(mainDialogNode);
                isDialogActive = true;
                StartCoroutine(CheckDialogEnd());
            }
            else
            {
                // Dialog tamamlandıktan sonra after dialog
                dialogSystem.SetSpeakers(speakerAudios);
                dialogSystem.StartDialog(afterDialogNode);
                isDialogActive = true;
                StartCoroutine(CheckDialogEnd());
            }
        }
    }

    private System.Collections.IEnumerator CheckDialogEnd()
    {
        // Dialog paneli kapanana kadar bekle
        while (dialogSystem != null && dialogSystem.dialogPanel.activeSelf)
        {
            yield return null;
        }

        isDialogActive = false;

        // İlk dialog tamamlandıysa dialogCompleted'ı true yap
        if (!dialogCompleted && hasInteracted)
        {
            dialogCompleted = true;

            // Sonraki görevi tetikler
            if (missionObj != null)
            {
                missionObj.OnInteracted();
                checkOutSideTrigger.SetActive(true);
            }
                
        }
    }

    void BuildDialogTree()
    {
        // === MAIN DIALOG START ===
        mainDialogNode = DialogBuilder.CreateNode(
            "Sir, sir, sir. Why did you yell at me last night? You scared me so much. Who were you talking to? I've never seen that phone work before?",
            "Abi, abi, abi. Dün gece bana niye bağırdın. Çok korkuttun beni. Kiminle konuşuyordun? Ben bu telefonun daha önce çalıştığını görmemiştim?",
            "Little Girl"
        );

        // === OPTION 1 BRANCH: Did you really see me? ===
        DialogNode option1Engin = DialogBuilder.CreateNode(
            "Did you really see me talking on the phone?",
            "Beni gerçekten gördün mü telefonla konuşurken?",
            "Engin"
        );

        DialogNode option1GirlResponse = DialogBuilder.CreateNode(
            "Yes sir. You screamed in my face and went upstairs. Don't you remember?",
            "Evet abi. Suratıma çığlık atıp yukarı çıktın. Hatırlamıyor musun?",
            "Little Girl"
        );

        DialogNode option1EnginThought = DialogBuilder.CreateNode(
            "I thought I was dreaming.",
            "Ben rüya gördüğümü sanmıştım.",
            "Engin"
        );

        // (DÜZELTME 1) Burası daha önce kullanılmıyordu, şimdi akışa dahil edildi.
        DialogNode option1GirlSameDream = DialogBuilder.CreateNode(
            "We couldn't have had the same dream, right?",
            "Aynı rüyayı görmüş olamayız değil mi?",
            "Little Girl"
        );

        // Option 1.1: Maybe, why not
        DialogNode option11Engin = DialogBuilder.CreateNode(
            "Maybe, why not.",
            "Olabilir neden olmasın.",
            "Engin"
        );

        DialogNode option11GirlResponse = DialogBuilder.CreateNode(
            "My mom would say it's not possible. Everyone only lives inside their own head. No one loves or understands anyone because of this. And that's also why my b*stard of a father left us.",
            "Annem olamaz derdi. Herkes sadece kendi kafasının içinde yaşarmış. Kimse kimseyi bu yüzden sevmez ve anlamazmış. Yavşak babam da bu yüzden bizi terk etmiş.",
            "Little Girl"
        );

        // Option 1.2: If you saw me...
        DialogNode option12Engin = DialogBuilder.CreateNode(
            "If you saw me, that means I really was talking to my cousin.",
            "Beni gördüysen ben gerçekten kuzenimle konuştum demektir.",
            "Engin"
        );

        DialogNode option12GirlResponse = DialogBuilder.CreateNode(
            "So you talked with your cousin. Does your cousin live in Bursa, in Nilüfer, Minareliçavuş?",
            "Demek kuzeninle konuştun. Bursa'da mı yaşıyor kuzenin, Nilüfer, Minareliçavuş'ta?",
            "Little Girl"
        );

        DialogNode option12EnginPanic = DialogBuilder.CreateNode(
            "Please don't tell anyone you heard that.",
            "Lütfen bunu duyduğunu kimseye söyleme.",
            "Engin"
        );

        DialogNode option12GirlEnd = DialogBuilder.CreateNode(
            "But my mom said keeping secrets is a shameful thing?",
            "Ama annem sır saklamanın ayıp bir şey olduğunu söylerdi?",
            "Little Girl"
        );

        // === OPTION 2 BRANCH: I was talking to my cousin ===
        DialogNode option2Engin = DialogBuilder.CreateNode(
            "I was talking to my cousin on the phone. He was the one that called me.",
            "Telefonda kuzenimle konuşuyordum. O beni aramıştı.",
            "Engin"
        );

        DialogNode option2GirlResponse = DialogBuilder.CreateNode(
            "You yelled at me and ran off because of your cousin? Not that it's a problem for me. My mom always yells at me. And she already warned me about men who yell first and then leave.",
            "Kuzenin yüzünden mi benim yüzüme bağırdın ve koştun? Benim için sorun olduğundan değil. Annem bana sürekli bağırır. Beni de önce bağıran sonra giden erkekler hakkında uyarmıştı.",
            "Little Girl"
        );

        // Option 2.1
        DialogNode option21Engin = DialogBuilder.CreateNode(
            "I saw my mother. I heard her voice.",
            "Annemi gördüm. Onun sesini duydum.",
            "Engin"
        );

        DialogNode option21GirlResponse = DialogBuilder.CreateNode(
            "I'm scared of my mom too. Was that your mom on the phone?",
            "Ben de annemden korkarım. Annen telefonda mıydı?",
            "Little Girl"
        );

        DialogNode option21EnginNo = DialogBuilder.CreateNode(
            "No, she was here with us. She told me to wake up. Didn't you see her?",
            "Hayır bizimle buradaydı. Bana uyanmamı söyledi. Onu görmedin mi?",
            "Engin"
        );

        DialogNode option21GirlNo = DialogBuilder.CreateNode(
            "No sir. It was only us here. If there was a mother here, I would definitely remember that. My mom hasn't come back for a while either.",
            "Hayır abi. Burada sadece biz vardık. Bir anne olsa kesinlikle hatırlardım. Benim annem de bir süredir geri dönmedi.",
            "Little Girl"
        );

        // Option 2.2
        DialogNode option22Engin = DialogBuilder.CreateNode(
            "I jumped in fear when I saw you. You don't suddenly appear in front of people at night like that. Didn't your father teach you that?",
            "Seni görünce korkudan zıpladım. İnsanların karşısına aniden çıkılmaz öyle gece gece. Baban sana bunu öğretmedi mi?",
            "Engin"
        );

        DialogNode option22GirlResponse = DialogBuilder.CreateNode(
            "My father left us before I was born. My mom says her love and body were taken over by a b*stard and he left me behind. Only I remain from him.",
            "Benim babam bizi ben doğmadan önce bırakmış. Annem sevgisinin ve bedeninin bir yavşak tarafından yendiğini geriye de beni bıraktığını söylüyor. Ondan geriye yalnızca ben kalmışım.",
            "Little Girl"
        );

        // === COMMON ENDING ===
        DialogNode commonEnginQuestion = DialogBuilder.CreateNode(
            "Where is your mother?",
            "Annen nerde senin?",
            "Engin"
        );

        DialogNode commonGirlAnswer = DialogBuilder.CreateNode(
            "She went to work two nights ago. She told me to wait here until she comes back.",
            "İki gece önce işe çıktı. O gelene kadar burada beklememi söyledi.",
            "Little Girl"
        );

        DialogNode commonKuru = DialogBuilder.CreateNode(
            "Lavuk, don't do it. The old man has no bad intentions.",
            "Lavuk, yapma etme. Yaşlı başlı adam, kötü niyeti yok.",
            "Kuru"
        );

        DialogNode commonGirlEnd = DialogBuilder.CreateEndNode(
            "Asuman has come to the neighborhood again. A big incident is about to take place.",
            "Asuman gene mahalleye gelmiş. Büyük olay çıkmak üzere.",
            "Little Girl"
        );

        // After dialog
        afterDialogNode = DialogBuilder.CreateEndNode(
            "Asuman has come to the neighborhood again. A big incident is about to take place.",
            "Asuman gene mahalleye gelmiş. Büyük olay çıkmak üzere.",
            "Little Girl"
        );


        // ========================
        // === BUILD CONNECTIONS ===
        // ========================

        // Main Node Options
        DialogOption mainOption1 = DialogBuilder.CreateOption(
            "Did you really see me?",
            "Beni gerçekten gördün mü?",
            option1Engin
        );
        DialogOption mainOption2 = DialogBuilder.CreateOption(
            "I was talking to my cousin.",
            "Kuzenimle konuşuyordum.",
            option2Engin
        );
        DialogBuilder.AddOption(mainDialogNode, mainOption1);
        DialogBuilder.AddOption(mainDialogNode, mainOption2);

        // --- OPTION 1 PATH ---
        // Engin -> Girl
        DialogOption opt1ToGirl = DialogBuilder.CreateOption("...", "...", option1GirlResponse, true);
        DialogBuilder.AddOption(option1Engin, opt1ToGirl);

        // Girl -> Engin Thought ("I thought I was dreaming")
        DialogOption opt1ToThought = DialogBuilder.CreateOption("...", "...", option1EnginThought, true);
        DialogBuilder.AddOption(option1GirlResponse, opt1ToThought);

        // (DÜZELTME 1 UYGULAMASI)
        // Engin Thought -> Girl "Same Dream" (Araya giren yeni bağlantı)
        DialogOption optThoughtToGirlDream = DialogBuilder.CreateOption("...", "...", option1GirlSameDream, true);
        DialogBuilder.AddOption(option1EnginThought, optThoughtToGirlDream);

        // Artık seçenekler "GirlSameDream" noduna bağlanıyor (Eskiden EnginThought'a bağlıydı)
        DialogOption opt11 = DialogBuilder.CreateOption(
            "Maybe, why not.",
            "Olabilir neden olmasın.",
            option11Engin
        );
        DialogOption opt12 = DialogBuilder.CreateOption(
            "If you saw me, I really talked to my cousin.",
            "Beni gördüysen ben gerçekten kuzenimle konuştum demektir.",
            option12Engin
        );
        // Seçenekleri kıza bağla
        DialogBuilder.AddOption(option1GirlSameDream, opt11);
        DialogBuilder.AddOption(option1GirlSameDream, opt12);

        // 1.1 Path continues
        DialogOption opt11ToGirl = DialogBuilder.CreateOption("...", "...", option11GirlResponse, true);
        DialogBuilder.AddOption(option11Engin, opt11ToGirl);
        DialogOption opt11ToCommon = DialogBuilder.CreateOption("...", "...", commonEnginQuestion, true);
        DialogBuilder.AddOption(option11GirlResponse, opt11ToCommon);

        // 1.2 Path continues
        DialogOption opt12ToGirl = DialogBuilder.CreateOption("...", "...", option12GirlResponse, true);
        DialogBuilder.AddOption(option12Engin, opt12ToGirl);
        DialogOption opt12ToPanic = DialogBuilder.CreateOption("...", "...", option12EnginPanic, true);
        DialogBuilder.AddOption(option12GirlResponse, opt12ToPanic);
        DialogOption opt12ToEnd = DialogBuilder.CreateOption("...", "...", option12GirlEnd, true);
        DialogBuilder.AddOption(option12EnginPanic, opt12ToEnd);
        DialogOption opt12ToCommon = DialogBuilder.CreateOption("...", "...", commonEnginQuestion, true);
        DialogBuilder.AddOption(option12GirlEnd, opt12ToCommon);


        // --- OPTION 2 PATH ---
        DialogOption opt2ToGirl = DialogBuilder.CreateOption("...", "...", option2GirlResponse, true);
        DialogBuilder.AddOption(option2Engin, opt2ToGirl);

        DialogOption opt21 = DialogBuilder.CreateOption(
            "I saw my mother. I heard her voice.",
            "Annemi gördüm. Onun sesini duydum.",
            option21Engin
        );
        DialogOption opt22 = DialogBuilder.CreateOption(
            "I jumped in fear when I saw you.",
            "Seni görünce korkudan zıpladım.",
            option22Engin
        );
        DialogBuilder.AddOption(option2GirlResponse, opt21);
        DialogBuilder.AddOption(option2GirlResponse, opt22);

        // 2.1 Path
        DialogOption opt21ToGirl = DialogBuilder.CreateOption("...", "...", option21GirlResponse, true);
        DialogBuilder.AddOption(option21Engin, opt21ToGirl);
        DialogOption opt21ToNo = DialogBuilder.CreateOption("...", "...", option21EnginNo, true);
        DialogBuilder.AddOption(option21GirlResponse, opt21ToNo);
        DialogOption opt21ToGirlNo = DialogBuilder.CreateOption("...", "...", option21GirlNo, true);
        DialogBuilder.AddOption(option21EnginNo, opt21ToGirlNo);
        DialogOption opt21ToCommon = DialogBuilder.CreateOption("...", "...", commonEnginQuestion, true);
        DialogBuilder.AddOption(option21GirlNo, opt21ToCommon);

        // 2.2 Path
        DialogOption opt22ToGirl = DialogBuilder.CreateOption("...", "...", option22GirlResponse, true);
        DialogBuilder.AddOption(option22Engin, opt22ToGirl);
        DialogOption opt22ToCommon = DialogBuilder.CreateOption("...", "...", commonEnginQuestion, true);
        DialogBuilder.AddOption(option22GirlResponse, opt22ToCommon);


        // --- COMMON ENDING PATH ---
        DialogOption commonToAnswer = DialogBuilder.CreateOption("...", "...", commonGirlAnswer, true);
        DialogBuilder.AddOption(commonEnginQuestion, commonToAnswer);

        DialogOption commonToKuru = DialogBuilder.CreateOption("...", "...", commonKuru, true);
        DialogBuilder.AddOption(commonGirlAnswer, commonToKuru);

        DialogOption commonToEnd = DialogBuilder.CreateOption("...", "...", commonGirlEnd, true);
        DialogBuilder.AddOption(commonKuru, commonToEnd);

        if (objectToActivate != null)
        {
            DialogOption endToActivate = DialogBuilder.CreateOption("...", "...", afterDialogNode, true);
            DialogBuilder.AddOption(commonGirlEnd, endToActivate);
        }

        mainDialogNode.voiceClip = clip_girl_main;
        option1Engin.voiceClip = clip_engin_didYouSee;
        option1GirlResponse.voiceClip = clip_girl_opt1Response;
        option1EnginThought.voiceClip = clip_engin_wasDreaming;
        option1GirlSameDream.voiceClip = clip_girl_sameDream;
        option11Engin.voiceClip = clip_engin_maybeWhyNot;
        option11GirlResponse.voiceClip = clip_girl_opt11Response;
        option12Engin.voiceClip = clip_engin_ifYouSawMe;
        option12GirlResponse.voiceClip = clip_girl_opt12Response;
        option12EnginPanic.voiceClip = clip_engin_dontTell;
        option12GirlEnd.voiceClip = clip_girl_opt12End;
        option2Engin.voiceClip = clip_engin_talking;
        option2GirlResponse.voiceClip = clip_girl_opt2Response;
        option21Engin.voiceClip = clip_engin_sawMother;
        option21GirlResponse.voiceClip = clip_girl_opt21Response;
        option21EnginNo.voiceClip = clip_engin_sheWasHere;
        option21GirlNo.voiceClip = clip_girl_opt21No;
        option22Engin.voiceClip = clip_engin_jumped;
        option22GirlResponse.voiceClip = clip_girl_opt22Response;
        commonEnginQuestion.voiceClip = clip_engin_whereMom;
        commonGirlAnswer.voiceClip = clip_girl_commonAnswer;
        commonKuru.voiceClip = clip_kuru_lavuk;
        commonGirlEnd.voiceClip = clip_girl_commonEnd;
        afterDialogNode.voiceClip = clip_girl_commonEnd;
    }
}
