using System.Collections.Generic;         // 🔗 List<T> gibi koleksiyon sınıflarını kullanmak için.
using UnityEngine;                        // 🔗 Unity'nin temel sınıflarını (MonoBehaviour, GameObject vb.) kullanmak için gerekli.

public class DialogBuilder               // 💬 DialogNode ve DialogOption nesnelerini kolay oluşturmak için kullanılan yardımcı sınıf (factory).
{
    // 💬 Yeni bir diyalog düğümü (node) oluşturur.
    // 🔗 DialogNode, bir konuşma satırını temsil eder.
    public static DialogNode CreateNode(string englishText, string turkishText, string speakerName = "")
    {
        DialogNode node = new()
        {
            speakerName = speakerName,         // 💬 Konuşan karakterin ismi atanıyor.
            dialogText = new LocalizedText     // 💬 Diyalog metni iki dilde tutulacak (LocalizedText yapısı kullanılıyor).
            {
                english = englishText,              // 💬 İngilizce metin atanıyor.
                turkish = turkishText               // 💬 Türkçe metin atanıyor.
            },
            optionsList = new List<DialogOption>(), // 💬 Bu node'a ait seçeneklerin listesi başlatılıyor.
            isEndDialog = false                // 💬 Varsayılan olarak bu node, diyalog sonu değil.
        };     // 💬 Yeni bir diyalog düğümü oluşturuluyor.
        return node;                             // 💬 Oluşturulan node geri döndürülüyor.
    }

    // 💬 Diyalog sonu olan bir node oluşturur.
    // 🔗 Bitiş düğümleri oyuncunun konuşmayı kapatmasını sağlar.
    public static DialogNode CreateEndNode(string englishText, string turkishText, string speakerName = "")
    {
        DialogNode node = CreateNode(englishText, turkishText, speakerName); // 💬 Normal node gibi oluşturuluyor.
        node.isEndDialog = true;                                // 💬 Sadece bu sefer "son diyalog" işareti true yapılıyor.
        return node;
    }

    // 💬 Oyuncunun seçebileceği bir seçenek (option) oluşturur.
    // 🔗 DialogOption, bir cevabı ve devam eden node'u temsil eder.
    public static DialogOption CreateOption(string englishText, string turkishText, DialogNode response = null, bool isSilent = false)
    {
        DialogOption option = new()
        {
            optionText = new LocalizedText          // 💬 Seçeneğin iki dildeki metni atanıyor.
            {
                english = englishText,
                turkish = turkishText
            },
            responseNode = response,                // 💬 Bu seçeneğe tıklandığında gidilecek node atanıyor (null olabilir).
            isSilentOption = isSilent              // 💬 "..." gibi sessiz cevap mı?
        };      // 💬 Yeni seçenek nesnesi oluşturuluyor.
        return option;
    }

    // 💬 Bir node’a yeni bir seçenek ekler.
    // 🔗 Node’un seçenek listesine (List<DialogOption>) push yapılır.
    public static void AddOption(DialogNode node, DialogOption option)
    {
        node.optionsList.Add(option);                      // 💬 Belirtilen seçeneği bu node’un listesine ekliyor.
        Debug.Log("Listeye seçenek eklendi");
    }

    // 💬 Event (UnityAction) içeren bir seçenek oluşturur.
    // 🔗 UnityEvent ve UnityAction, butonlara veya olaylara tıklanınca fonksiyon çalıştırmak için kullanılır.
    public static DialogOption CreateOptionWithEvent(string englishText, string turkishText,
        DialogNode response, UnityEngine.Events.UnityAction eventCallback, bool isSilent = false)
    {
        DialogOption option = CreateOption(englishText, turkishText, response, isSilent); // 💬 Normal seçenek oluşturuluyor.
        option.onSelect = new UnityEngine.Events.UnityEvent();                   // 💬 Bu seçeneğe ait UnityEvent nesnesi oluşturuluyor.
        option.onSelect.AddListener(eventCallback);                              // 💬 Tıklanınca çalışacak callback fonksiyonu olaya ekleniyor.
        return option;                                                           // 💬 Olay destekli seçenek geri döndürülüyor.
    }
}
