# Voiced Dialog & 3D Audio — Design Spec

**Date:** 2026-06-07  
**Scope:** Scene 1 (test), architecture reusable for Scenes 2 & 3  
**Status:** Approved

---

## Özet

Mevcut dialog sistemine sesli dublaj desteği eklenir. NPC sesleri 3D AudioSource üzerinden oynatılır; player (Engin) sesleri 2D olarak çalınır. Her dialog node'una isteğe bağlı `AudioClip` atanabilir. Clip varsa typewriter efektinin tamamlanma süresi clip uzunluğuyla eşleşecek şekilde `typewriterSpeed` dinamik hesaplanır.

---

## Kararlar

| Soru | Karar |
|------|-------|
| Ses dili | Sadece Türkçe (altyazı TR + EN) |
| Player (Engin) sesi | 2D (spatialBlend = 0) |
| NPC sesleri | 3D (spatialBlend = 1) |
| Space ile atlama | Ses de durur, opsiyonlar çıkar |
| Clip yoksa | Varsayılan `typewriterSpeed` kullanılır |
| İlk kapsam | Sahne 1 (test), diğer sahneler aynı pattern ile genişler |

---

## Bölüm 1: Veri Modeli

### DialogNode (mevcut plain C# class)

```csharp
public class DialogNode
{
    public string speakerName;
    public LocalizedText dialogText;
    public List<DialogOption> optionsList;
    public bool isEndDialog;
    public AudioClip voiceClip; // YENİ — null ise varsayılan hız kullanılır
}
```

### SpeakerAudio (yeni, DialogSystem.cs içinde tanımlanır)

```csharp
[System.Serializable]
public class SpeakerAudio
{
    public string speakerName;      // "Cenk", "Amca" — DialogNode.speakerName ile eşleşmeli
    public AudioSource audioSource; // NPC veya player body'sindeki AudioSource
    public bool isPlayer;           // true → spatialBlend = 0 (2D)
}
```

---

## Bölüm 2: DialogSystem Değişiklikleri

### Yeni alanlar

```csharp
[Header("Ses Ayarları")]
public List<SpeakerAudio> speakers;
public float defaultTypewriterSpeed = 0.05f; // clip yoksa fallback
private AudioSource currentAudioSource;
```

`typewriterSpeed` alanı artık `defaultTypewriterSpeed`'in fallback'i olarak kalır; clip varken dinamik hesaplanır.

### SetSpeakers()

```csharp
public void SetSpeakers(List<SpeakerAudio> speakerList)
{
    speakers = speakerList;
}
```

Dialog scripti `StartDialog()` öncesinde çağırır. Aynı DialogSystem birden fazla diyalog için farklı speaker setleri alabilir.

### ShowDialog() — ses başlatma

```csharp
private void ShowDialog()
{
    // ... mevcut speakerNameText kodu ...

    string textToShow = currentNode.dialogText.GetText(isTurkish);
    PlayNodeAudio(currentNode, textToShow.Length);

    if (useTypewriterEffect)
        StartCoroutine(TypewriterEffect(textToShow));
    else
    {
        dialogText.text = textToShow;
        dialogText.maxVisibleCharacters = textToShow.Length;
        ShowOptions();
    }
}

private void PlayNodeAudio(DialogNode node, int textLength)
{
    // Önceki sesi durdur
    currentAudioSource?.Stop();
    currentAudioSource = null;

    if (node.voiceClip == null || speakers == null)
    {
        typewriterSpeed = defaultTypewriterSpeed; // clip yoksa hızı sıfırla
        return;
    }

    SpeakerAudio speaker = speakers.Find(s => s.speakerName == node.speakerName);
    if (speaker == null || speaker.audioSource == null)
    {
        typewriterSpeed = defaultTypewriterSpeed;
        return;
    }

    speaker.audioSource.spatialBlend = speaker.isPlayer ? 0f : 1f;
    speaker.audioSource.clip = node.voiceClip;
    speaker.audioSource.Play();
    currentAudioSource = speaker.audioSource;

    // Typewriter hızını clip süresine göre ayarla
    if (textLength > 0)
        typewriterSpeed = node.voiceClip.length / textLength;
}
```

### ShowOptions() — ses durdurma

`ShowOptions()` başına eklenir:
```csharp
currentAudioSource?.Stop();
currentAudioSource = null;
```

Bu sayede hem doğal bitiş (typewriter tamamlandı) hem Space ile atlama aynı noktada sesi durdurur.

### EndDialog()

```csharp
public void EndDialog()
{
    currentAudioSource?.Stop();
    currentAudioSource = null;
    // ... mevcut kod ...
}
```

---

## Bölüm 3: Dialog Scriptleri (Scene 1)

Etkilenen dosyalar:
- `Assets/Scripts/DialogSystem/Dialogs/Scene 1/TalkWithCenkOnTable.cs`
- `Assets/Scripts/DialogSystem/Dialogs/Scene 1/TalkWithNeighbor.cs`
- `Assets/Scripts/DialogSystem/Dialogs/Scene 1/GetFeetDown.cs`
- `Assets/Scripts/DialogSystem/Dialogs/Scene 1/HaveDinner.cs`
- `Assets/Scripts/DialogSystem/Dialogs/Scene 1/MotherCooking.cs`

### Her script'e eklenecek pattern

```csharp
[Header("Ses Referansları")]
[SerializeField] private List<SpeakerAudio> speakerAudios;

// Her node için (BuildDialog içinde):
someNode.voiceClip = someClip; // [SerializeField] AudioClip someClip;

// StartDialog öncesinde:
dialogSystem.SetSpeakers(speakerAudios);
dialogSystem.StartDialog(startNode);
```

AudioClip alanları Inspector'da sürükle-bırak ile atanır. Clip atanmamış node'lar `defaultTypewriterSpeed` ile çalışmaya devam eder.

---

## Bölüm 4: Unity Inspector Kurulumu (Kod dışı)

### NPC GameObjects (Scene 1)
Her konuşan NPC'ye `AudioSource` component eklenir:
- `Spatial Blend`: 1 (tam 3D)
- `Volume Rolloff`: Logarithmic Distance
- `Play On Awake`: false

### Player (Engin)
- Player body'sine `AudioSource` component eklenir
- `Spatial Blend`: 0 (2D) — kod tarafından da `isPlayer = true` ile override edilir
- `Play On Awake`: false

### SpeakerAudio Listesi (her dialog script için)
Inspector'da `speakerAudios` listesi doldurulur:
```
[0] speakerName: "Cenk",  audioSource: CenkObject/AudioSource,  isPlayer: false
[1] speakerName: "Engin", audioSource: PlayerBody/AudioSource,  isPlayer: true
[2] speakerName: "Amca",  audioSource: AmcaObject/AudioSource,  isPlayer: false
```

---

## Genişleme Notu (Scene 2 & 3)

Aynı pattern: dialog script'e `speakerAudios` listesi + node'lara `voiceClip` ataması. `DialogSystem` kodu değişmez.

---

## Başarı Kriterleri

- [ ] NPC konuşurken ses o NPC'nin pozisyonundan gelir (headphone ile test edilebilir)
- [ ] Typewriter efekti clip ile aynı anda biter
- [ ] Clip yoksa eski hız devreye girer, hata yok
- [ ] Opsiyonlar göründüğünde ses durur
- [ ] Space ile atlayınca ses durur
- [ ] Dialog bitince ses durur
