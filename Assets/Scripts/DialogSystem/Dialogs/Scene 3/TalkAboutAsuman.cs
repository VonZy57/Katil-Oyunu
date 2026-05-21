using UnityEngine;
using DG.Tweening;

public class TalkAboutAsuman : MonoBehaviour
{
    [SerializeField] private DialogSystem dialogSystem;
    [SerializeField] private DialogNode oldManDidYouSee;
    [SerializeField] private MissionObjective missionObj;

    [Header("Kamera ve Bakış Referansları")]
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private Transform amcaTransform;
    [SerializeField] private Transform kamilEfendiTransform;

    private DialogNode oldManAfraid;
    private FirstPersonController playerFPS;

    void Start()
    {
        if (playerCamera != null)
        {
            playerFPS = playerCamera.GetComponentInParent<FirstPersonController>();
        }
        BuildDialog();
    }

    public void StartTalkAboutAsumanDialog()
    {
        if (dialogSystem != null && oldManDidYouSee != null)
        {
            if (playerFPS != null) playerFPS.enabled = false; // Diyalog esnasında oyuncu kontrolünü kapat

            if (playerCamera != null && amcaTransform != null)
            {
                Camera cam = playerCamera.GetComponent<Camera>();
                if (cam == null) cam = playerCamera.GetComponentInChildren<Camera>();
                if (cam == null) cam = Camera.main; // Fallback

                Transform camTransform = cam != null ? cam.transform : playerCamera.transform;
                camTransform.DOKill();
                camTransform.DOLookAt(amcaTransform.position, 1f).SetEase(Ease.InOutSine); // Kamerayı Amca'ya çevir
            }

            StartCoroutine(DialogSequence());
        }
    }

    private System.Collections.IEnumerator DialogSequence()
    {
        dialogSystem.StartDialog(oldManDidYouSee);

        yield return null;
        yield return new WaitUntil(() => dialogSystem.dialogPanel.activeSelf);
        yield return new WaitUntil(() => !dialogSystem.dialogPanel.activeSelf);

        // PLACEHOLDER: Amcanın yemek yeme animasyonu burada çalışacak
        Debug.Log("Amca yemek yeme animasyonu başladı...");
        yield return new WaitForSeconds(2f); // TODO: Animasyon kodları buraya yazılacak
        Debug.Log("Amca yemek yeme animasyonu bitti.");

        if (oldManAfraid != null)
        {
            dialogSystem.StartDialog(oldManAfraid);
            
            yield return null;
            yield return new WaitUntil(() => dialogSystem.dialogPanel.activeSelf);
            yield return new WaitUntil(() => !dialogSystem.dialogPanel.activeSelf);
        }

        // Diyalog tamamen bittiğinde oyuncu kontrollerini geri ver
        if (playerFPS != null)
        {
            playerFPS.SyncCameraRotation(); // Açıyı eşitle, anlık sıçrama olmasın
            playerFPS.enabled = true; 
        }

        if (missionObj != null)
        {
            missionObj.OnInteracted();
        }
    }

    private System.Collections.IEnumerator LookAtKamilSequence()
    {
        if (playerCamera != null && kamilEfendiTransform != null && amcaTransform != null)
        {
            Camera cam = playerCamera.GetComponent<Camera>();
            if (cam == null) cam = playerCamera.GetComponentInChildren<Camera>();
            if (cam == null) cam = Camera.main;

            Transform camTransform = cam != null ? cam.transform : playerCamera.transform;

            // 1. Kâmil Efendi'ye dön
            camTransform.DOKill();
            camTransform.DOLookAt(kamilEfendiTransform.position, 0.7f).SetEase(Ease.InOutSine);
            
            // Mırıldanma süresi boyunca bekle
            yield return new WaitForSeconds(2.5f);
            
            // 2. Tekrar Amca'ya dön
            camTransform.DOKill();
            camTransform.DOLookAt(amcaTransform.position, 0.7f).SetEase(Ease.InOutSine);
        }
    }

    void BuildDialog()
    {
        // ==========================================
        // ORTAK DEVAM: Talk About Asuman
        // ==========================================
        oldManDidYouSee = DialogBuilder.CreateNode(
            "Did you see the one sitting in the car?",
            "Arabada oturan biri vardı. Onu gördün mü?",
            "Amca"
        );

        // ==========================================
        // 2. SEÇİM DALLARI: A Woman / A Man
        // ==========================================
        DialogNode enginWoman = DialogBuilder.CreateNode(
            "There was a woman in the driver's seat, a blonde. She kept honking the horn non-stop.",
            "Sürücü koltuğunda bir kadın vardı, sarışın. Kornaya bastı durmadan.",
            "Engin"
        );
        DialogNode oldManYeahShe = DialogBuilder.CreateNode(
            "Yeah, that's her.",
            "He yaşa, o.",
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
            "Görmedim diyebilirsin, sallamana gerek yok.",
            "Amca"
        );
        DialogBuilder.AddOption(enginMan, DialogBuilder.CreateOption("...", "...", oldManMakeThingsUp, true));

        DialogBuilder.AddOption(oldManDidYouSee, DialogBuilder.CreateOption("A Woman.", "Bir kadın.", enginWoman));
        DialogBuilder.AddOption(oldManDidYouSee, DialogBuilder.CreateOption("A Man.", "Bir erkek.", enginMan));

        // ==========================================
        // ORTAK DEVAM: Who is that?
        // ==========================================
        DialogNode enginWhoIsShe = DialogBuilder.CreateNode(
            "Who is that?",
            "Kim ki o?",
            "Engin"
        );
        DialogBuilder.AddOption(oldManYeahShe, DialogBuilder.CreateOption("...", "...", enginWhoIsShe, true));
        DialogBuilder.AddOption(oldManMakeThingsUp, DialogBuilder.CreateOption("...", "...", enginWhoIsShe, true));

        DialogNode oldManMyDaughter = DialogBuilder.CreateEndNode(
            "She is my daughter. Asuman. One day she called me 'Dad' and said, 'I'm going to Istanbul to study.' With some punk on her arm. That Lavuk from this morning. I didn't like the guy the second I saw him anyway. It had only been about two months since she lost her mother. I loved my wife very much.",
            "O benim kızım. Asuman. Zamanında baba dedi, ben İstanbul’a gidiyorum okumaya. Kolunda da bir lavuk. İşte sabahki Lavuk. Herifi gördüğüm gibi gözüm tutmadıydı zaten. Annesini kaybedeli 2 ay mı ne olmuştu. Eşimi çok severdim.",
            "Amca"
        );
        // Burada amca bir lokma yiyecek. Ondan sonra diğer diyalog açılacak.
        oldManAfraid = DialogBuilder.CreateNode(
            "I was afraid of losing my daughter. When she suddenly said that... I beat them both up that day. I wish my hands had broken, I wish I had died so I couldn't have done it. Couldn't have brought myself to do it. I was afraid of losing my little girl. And because of that, I lost her.",
            "Kızımı kaybetmekten korktum. Bir anda öyle deyince… Ben ikisini de dövdüm o gün. Ellerim kırılsaydı da, ölseydim de yapmasaydım. Yapamasaydım. Küçük kızımı kaybetmekten korktum. Bu yüzden de onu kaybettim.",
            "Amca"
        );
        DialogNode enginPunkTreat = DialogBuilder.CreateNode(
            "What about what Lavuk said? How does he treat your daughter?",
            "Peki Lavuk’un dedikleri. Kızına nasıl davranıyor?",
            "Engin"
        );
        DialogNode oldManPimping = DialogBuilder.CreateNode(
            "You know what hurts me the most? The bastard is pimping my daughter out. And he gambles away the money he gets. The scumbag owes money to every gambling den around here. Everyone knows him here. And nobody likes him. Oh, if I could just talk to Asuman...",
            "En çok da bu canımı yakıyor biliyon mu? Pezevenk kızı satıyor. Aldığı parayla da kumar oynuyor. Yakın çevredeki tüm batakhanelere borcu var itin. Herkes tanır burada onu. Kimse de sevmez. Ah bir Asuman’la konuşabilsem...",
            "Amca"
        );
        DialogNode enginWhatTell = DialogBuilder.CreateNode(
            "What are you going to tell her?",
            "Ona ne söyleyeceksin?",
            "Engin"
        );
        DialogNode oldManImSorry = DialogBuilder.CreateNode(
            "'I'm sorry. For everything I've done. Come back home. Let this misery end.' I've been chasing her in Istanbul for 6 months; this place is a hellhole. 'Come on, let's go back to Bursa together. To our home, our neighborhood.' If she said yes right now, I'd hit the road to Bursa this very minute.",
            "'Özür dilerim. Yaptığım her şey için. Evine dön. Bitsin bu çile.' 6 aydır senin peşine İstanbul'dayım; berbat bir yer burası. 'Gel birlikte Bursa'ya dönelim. Evimize, mahallemize.' Eğer şimdi he dese, o andan tezi yok çıkarım yola, Bursa'ya.",
            "Amca"
        );

        DialogBuilder.AddOption(enginWhoIsShe, DialogBuilder.CreateOption("...", "...", oldManMyDaughter, true));
        DialogBuilder.AddOption(oldManAfraid, DialogBuilder.CreateOption("...", "...", enginPunkTreat, true));
        DialogBuilder.AddOption(enginPunkTreat, DialogBuilder.CreateOption("...", "...", oldManPimping, true));
        DialogBuilder.AddOption(oldManPimping, DialogBuilder.CreateOption("...", "...", enginWhatTell, true));
        DialogBuilder.AddOption(enginWhatTell, DialogBuilder.CreateOption("...", "...", oldManImSorry, true));

        // ==========================================
        // 3. SEÇİM DALLARI: My cousin lives in Bursa too / You won't return to Bursa
        // ==========================================
        DialogNode enginCousin = DialogBuilder.CreateNode(
            "What a coincidence. My cousin lives in Bursa too. In Minareliçavuş.",
            "Ne büyük tesadüf. Benim de kuzenim Bursa da yaşıyor. Minareliçavuş’ta.",
            "Engin"
        );
        DialogNode oldMan20Mins = DialogBuilder.CreateNode(
            "That's 20 minutes away from our house.",
            "Bizim eve 20 dakika uzaklıkta.",
            "Amca"
        );
        DialogBuilder.AddOption(enginCousin, DialogBuilder.CreateOption("...", "...", oldMan20Mins, true));

        DialogNode enginWontReturn = DialogBuilder.CreateNode(
            "Amca, is there no other way for you to return to Bursa? Won't you let your daughter go?",
            "Amca, başka yolu yok mu Bursa’ya dönmen için. Kızının peşini bırakmaz mısın?",
            "Engin"
        );
        DialogNode oldManCantLive = DialogBuilder.CreateNode(
            "I can't live without my daughter. My house feels like a prison. Either she comes to that house, or I'll crawl after her on the roads she walks until the day I die.",
            "Ben kızım olmadan yaşayamam. Evim dar gelir oldu. Ya o eve gelecek ya da ben onun yürüdüğü yollarda onun ardında sürüneceğim ölene kadar.",
            "Amca"
        );
        DialogBuilder.AddOption(enginWontReturn, DialogBuilder.CreateOption("...", "...", oldManCantLive, true));

        DialogBuilder.AddOption(oldManImSorry, DialogBuilder.CreateOption("My cousin lives in Bursa too.", "Benim kuzenim de Bursa'da.", enginCousin));
        DialogBuilder.AddOption(oldManImSorry, DialogBuilder.CreateOption("You won't return without Asuman?", "Asuman olmadan dönmeyecek misin?", enginWontReturn));

        // ==========================================
        // ORTAK DEVAM: I'll convince Asuman...
        // ==========================================
        DialogNode enginConvince = DialogBuilder.CreateNode(
            "Maybe I can convince Asuman. Since she won't talk to you, I'll talk to her and fix this. Surely she'd prefer returning home over the life she's living now.",
            "Belki ben Asuman'ı ikna ederim. Madem seninle konuşmuyor, ben onla konuşur çözerim bu işi. Yaşadığı hayattansa eve dönmeyi tercih edecektir elbette.",
            "Engin"
        );
        DialogBuilder.AddOption(oldMan20Mins, DialogBuilder.CreateOption("...", "...", enginConvince, true));
        DialogBuilder.AddOption(oldManCantLive, DialogBuilder.CreateOption("...", "...", enginConvince, true));

        DialogNode oldManWhyHelp = DialogBuilder.CreateNode(
            "Why would you help me?",
            "Neden bana yardım edesin ki?",
            "Amca"
        );
        DialogNode enginTakeMe = DialogBuilder.CreateNode(
            "I need you to take me to Bursa too. Take me away from this wretched city.",
            "Beni de Bursa’ya götürmene ihtiyacım var. Bu berbat şehirden al götür beni.",
            "Engin"
        );
        DialogNode kamilGrunt2 = DialogBuilder.CreateNode(
            "Hmmmm... (Grunts in an approving tone)",
            "Hmmmm... (Onaylayan bir tonda mırıldanır)",
            "Kamil Efendi"
        );
        DialogNode oldManRightKamil = DialogBuilder.CreateNode(
            "You're right, Kamil Efendi. What do I have to lose? Alright young man, let's go see the punk together. But he shouldn't see me. Find a way to meet Asuman and get him to agree. Then you can talk to Asuman and try to persuade her.",
            "Haklısın Kâmil Efendi. Ne kaybedebilirim ki? Genç adam o zaman seninle Lavuk’un yanına gidelim. Ama beni görmesin. Asuman’la görüşmek için bir yol bul onu ikna et. Ardından Asuman’la konuşup onu ikna etmeye çalışırsın.",
            "Amca"
        );
        DialogNode enginAlrightGo = DialogBuilder.CreateNode(
            "Alright then, let's go see the Punk.",
            "Tamam o zaman, hadi gidelim şu Lavuk’un yanına.",
            "Engin"
        );
        DialogNode oldManTeaFirst = DialogBuilder.CreateNode(
            "We should have had a tea first.",
            "Önce bir çay içseydik.",
            "Amca"
        );
        DialogNode enginFastEnd = DialogBuilder.CreateEndNode(
            "Let's get this sorted out quickly.",
            "Acele çözelim bu işi.",
            "Engin"
        );

        // Düğümleri birbirine bağlama
        DialogBuilder.AddOption(enginConvince, DialogBuilder.CreateOption("...", "...", oldManWhyHelp, true));
        DialogBuilder.AddOption(oldManWhyHelp, DialogBuilder.CreateOption("...", "...", enginTakeMe, true));
        DialogOption enginToKamilOpt = DialogBuilder.CreateOptionWithEvent("...", "...", kamilGrunt2, () => { StartCoroutine(LookAtKamilSequence()); }, true);
        DialogBuilder.AddOption(enginTakeMe, enginToKamilOpt);
        DialogBuilder.AddOption(kamilGrunt2, DialogBuilder.CreateOption("...", "...", oldManRightKamil, true));
        DialogBuilder.AddOption(oldManRightKamil, DialogBuilder.CreateOption("...", "...", enginAlrightGo, true));
        DialogBuilder.AddOption(enginAlrightGo, DialogBuilder.CreateOption("...", "...", oldManTeaFirst, true));
        DialogBuilder.AddOption(oldManTeaFirst, DialogBuilder.CreateOption("...", "...", enginFastEnd, true));
    }
}