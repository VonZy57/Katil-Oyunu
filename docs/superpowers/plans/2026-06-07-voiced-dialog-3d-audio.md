# Voiced Dialog & 3D Audio — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Dialog nodlarına isteğe bağlı AudioClip ekle; NPC sesleri 3D AudioSource'tan çal, typewriter hızını clip süresine göre hesapla.

**Architecture:** `DialogNode`'a `voiceClip` alanı eklenir. `DialogSystem`'e `SpeakerAudio` (speakerName → AudioSource mapping) + `PlayNodeAudio()` metodu eklenir. Her Scene 1 dialog scripti `SetSpeakers()` + `AudioClip` SerializeField'larıyla güncellenir. `TalkWithNeighbor` subtitle sistemine ayrıca `voiceClip` eklenir.

**Tech Stack:** Unity C#, Unity AudioSource (3D Spatial Blend), UnityEngine.AudioClip

---

## Dosya Haritası

| Dosya | Değişiklik |
|-------|-----------|
| `Assets/Scripts/DialogSystem/DialogSystem.cs` | `SpeakerAudio` class, `voiceClip` on `DialogNode`, `SetSpeakers()`, `PlayNodeAudio()`, `ShowOptions`/`EndDialog` ses durdurma |
| `Assets/Scripts/DialogSystem/Dialogs/Scene 1/TalkWithCenkOnTable.cs` | `speakerAudios` + per-node `AudioClip` SerializeField'lar + `SetSpeakers()` çağrısı |
| `Assets/Scripts/DialogSystem/Dialogs/Scene 1/GetFeetDown.cs` | Aynı pattern |
| `Assets/Scripts/DialogSystem/Dialogs/Scene 1/HaveDinner.cs` | Aynı pattern |
| `Assets/Scripts/DialogSystem/Dialogs/Scene 1/MotherCooking.cs` | Aynı pattern |
| `Assets/Scripts/DialogSystem/Dialogs/Scene 1/TalkWithNeighbor.cs` | `SubtitleLine`'a `voiceClip` + speaker AudioSource oynatma |

---

## Task 1: DialogSystem.cs — Temel Ses Altyapısı

**Files:**
- Modify: `Assets/Scripts/DialogSystem/DialogSystem.cs`

- [ ] **Adım 1.1: `DialogNode`'a `voiceClip` ekle**

`DialogNode` class'ının sonuna (`isEndDialog`'dan sonra) şunu ekle:

```csharp
public class DialogNode
{
    public string speakerName;
    public LocalizedText dialogText;
    public List<DialogOption> optionsList;
    public bool isEndDialog;
    public AudioClip voiceClip; // YENİ
}
```

- [ ] **Adım 1.2: `SpeakerAudio` class'ını ekle**

`DialogNode` class'ının hemen üstüne (dosyanın tepesine değil, `DialogNode`'dan önce) ekle:

```csharp
[System.Serializable]
public class SpeakerAudio
{
    public string speakerName;
    public AudioSource audioSource;
    public bool isPlayer; // true ise spatialBlend = 0 (2D)
}
```

- [ ] **Adım 1.3: `DialogSystem` class'ına yeni alanlar ekle**

Mevcut `[Header("Görsel Ayarlar")]` bloğunu bul ve şöyle güncelle:

```csharp
[Header("Görsel Ayarlar")]
public float typewriterSpeed = 0.05f;
public bool useTypewriterEffect = true;

[Header("Ses Ayarları")]
public List<SpeakerAudio> speakers;

private float _defaultTypewriterSpeed;
private AudioSource _currentAudioSource;
```

- [ ] **Adım 1.4: `Start()`'a default hız kaydını ekle**

`Start()` metodunun başına:

```csharp
private void Start()
{
    _defaultTypewriterSpeed = typewriterSpeed; // YENİ
    if (dialogPanel != null)
        dialogPanel.SetActive(false);
    playerController = FindFirstObjectByType<FirstPersonController>();
}
```

- [ ] **Adım 1.5: `SetSpeakers()` metodunu ekle**

`StartDialog()` metodunun hemen üstüne:

```csharp
public void SetSpeakers(List<SpeakerAudio> speakerList)
{
    speakers = speakerList;
}
```

- [ ] **Adım 1.6: `PlayNodeAudio()` metodunu ekle**

`ShowDialog()` metodunun hemen üstüne:

```csharp
private void PlayNodeAudio(DialogNode node, int textLength)
{
    _currentAudioSource?.Stop();
    _currentAudioSource = null;

    if (node.voiceClip == null || speakers == null)
    {
        typewriterSpeed = _defaultTypewriterSpeed;
        return;
    }

    SpeakerAudio speaker = speakers.Find(s => s.speakerName == node.speakerName);
    if (speaker == null || speaker.audioSource == null)
    {
        typewriterSpeed = _defaultTypewriterSpeed;
        return;
    }

    speaker.audioSource.spatialBlend = speaker.isPlayer ? 0f : 1f;
    speaker.audioSource.clip = node.voiceClip;
    speaker.audioSource.Play();
    _currentAudioSource = speaker.audioSource;

    if (textLength > 0)
        typewriterSpeed = node.voiceClip.length / textLength;
}
```

- [ ] **Adım 1.7: `ShowDialog()` içinde `PlayNodeAudio()` çağır**

Mevcut `ShowDialog()`:

```csharp
private void ShowDialog()
{
    if (speakerNameText != null && !string.IsNullOrEmpty(currentNode.speakerName))
        speakerNameText.text = currentNode.speakerName;

    string textToShow = currentNode.dialogText.GetText(isTurkish);

    if (useTypewriterEffect)
        StartCoroutine(TypewriterEffect(textToShow));
    else
    {
        dialogText.text = textToShow;
        dialogText.maxVisibleCharacters = textToShow.Length;
        ShowOptions();
    }
}
```

Şöyle güncelle (`PlayNodeAudio` çağrısı `textToShow` sonrasına):

```csharp
private void ShowDialog()
{
    if (speakerNameText != null && !string.IsNullOrEmpty(currentNode.speakerName))
        speakerNameText.text = currentNode.speakerName;

    string textToShow = currentNode.dialogText.GetText(isTurkish);
    PlayNodeAudio(currentNode, textToShow.Length); // YENİ

    if (useTypewriterEffect)
        StartCoroutine(TypewriterEffect(textToShow));
    else
    {
        dialogText.text = textToShow;
        dialogText.maxVisibleCharacters = textToShow.Length;
        ShowOptions();
    }
}
```

- [ ] **Adım 1.8: `ShowOptions()` başına ses durdurma ekle**

Mevcut `ShowOptions()`:

```csharp
private void ShowOptions()
{
    ClearOptions();
    // ...
```

Şöyle güncelle:

```csharp
private void ShowOptions()
{
    _currentAudioSource?.Stop(); // YENİ
    _currentAudioSource = null;  // YENİ
    ClearOptions();
    // ...
```

- [ ] **Adım 1.9: `EndDialog()` başına ses durdurma ekle**

Mevcut `EndDialog()`:

```csharp
public void EndDialog()
{
    dialogPanel.SetActive(false);
    // ...
```

Şöyle güncelle:

```csharp
public void EndDialog()
{
    _currentAudioSource?.Stop(); // YENİ
    _currentAudioSource = null;  // YENİ
    dialogPanel.SetActive(false);
    // ...
```

- [ ] **Adım 1.10: Unity'de derlemeyi kontrol et**

Unity Editor'ı aç (veya zaten açıksa Console'u kontrol et). Hiç hata olmamalı. Uyarılar kabul edilebilir.

- [ ] **Adım 1.11: Commit**

```
git add "Assets/Scripts/DialogSystem/DialogSystem.cs"
git commit -m "feat: add 3D voiced dialog support to DialogSystem (SpeakerAudio, PlayNodeAudio)"
```

---

## Task 2: TalkWithCenkOnTable.cs — Ses Entegrasyonu

**Files:**
- Modify: `Assets/Scripts/DialogSystem/Dialogs/Scene 1/TalkWithCenkOnTable.cs`

Bu diyalogda konuşmacılar: **Cenk** ve **Engin** (player).

- [ ] **Adım 2.1: `speakerAudios` listesi ve AudioClip alanlarını ekle**

`[SerializeField] private DialogSystem dialogSystem;` satırının altına:

```csharp
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
```

- [ ] **Adım 2.2: `BuildDialogTree()` içinde node'lara clip ata**

`BuildDialogTree()` metodunun sonuna (son `DialogBuilder.AddOption` çağrısından sonra) ekle:

```csharp
// Cenk cliplerini ata
cenkStartsNode.voiceClip          = clip_cenkStarts;
sheKilledTwoNode.voiceClip        = clip_sheKilledTwo;
cenkSaysYouKilled.voiceClip       = clip_cenkSaysYouKilled;
cenksRemindsOldsNode.voiceClip    = clip_cenksRemindsOlds;
cenkSaysDoYouBelieveNode.voiceClip = clip_cenkSaysDoYouBelieve;
cenkSaysDoYouRememberNode.voiceClip = clip_cenkSaysDoYouRemember;
motherDontLikeFatsNode.voiceClip  = clip_motherDontLikeFats;
cenkSaysNo.voiceClip              = clip_cenkSaysNo;
leaveHereNode.voiceClip           = clip_leaveHere;
goToCousinNode.voiceClip          = clip_goToCousin;
cenksLastWords.voiceClip          = clip_cenksLastWords;

// Engin cliplerini ata
enginSaysNode.voiceClip              = clip_enginSays;
enginSaysWatchYourWordsNode.voiceClip = clip_enginSaysWatchYourWords;
motherSaidThat.voiceClip             = clip_motherSaidThat;
theyWouldHarmUs.voiceClip            = clip_theyWouldHarmUs;
enginSaysWhyHeKilledNode.voiceClip   = clip_enginSaysWhyHeKilled;
enginSaysNoNode.voiceClip            = clip_enginSaysNo;
enginSaysIdontWantNode.voiceClip     = clip_enginSaysIdontWant;
cutTheThroatNode.voiceClip           = clip_cutTheThroat;
dontWantRememberNode.voiceClip       = clip_dontWantRemember;
DontTortureMeNode.voiceClip          = clip_dontTortureMe;
whatDoYouWantNode.voiceClip          = clip_whatDoYouWant;
toWhereNode.voiceClip                = clip_toWhere;
```

- [ ] **Adım 2.3: `StartTheDialog()` içinde `SetSpeakers()` çağır**

`WaitAndStartDialog()` coroutine içinde `dialogSystem.StartDialog(cenkStartsNode)` satırının hemen **üstüne** ekle:

```csharp
dialogSystem.SetSpeakers(speakerAudios);
dialogSystem.StartDialog(cenkStartsNode);
```

- [ ] **Adım 2.4: Unity'de derlemeyi kontrol et**

Console'da hata olmamalı.

- [ ] **Adım 2.5: Commit**

```
git add "Assets/Scripts/DialogSystem/Dialogs/Scene 1/TalkWithCenkOnTable.cs"
git commit -m "feat: add voice clip support to TalkWithCenkOnTable"
```

---

## Task 3: GetFeetDown.cs — Ses Entegrasyonu

**Files:**
- Modify: `Assets/Scripts/DialogSystem/Dialogs/Scene 1/GetFeetDown.cs`

Bu diyalogda konuşmacılar: **Engin** (player), **Cenk**, **Mother**.

- [ ] **Adım 3.1: Alanları ekle**

`[SerializeField] private DialogSystem dialogSystem;` altına:

```csharp
[Header("Ses Referansları")]
[SerializeField] private List<SpeakerAudio> speakerAudios;

[Header("Engin Sesleri")]
[SerializeField] private AudioClip clip_getFeetDown;
[SerializeField] private AudioClip clip_afterDinner;
[SerializeField] private AudioClip clip_enginsAnswer;
[SerializeField] private AudioClip clip_whichLevel;
[SerializeField] private AudioClip clip_enginsAnswerLevel;

[Header("Cenk Sesleri")]
[SerializeField] private AudioClip clip_playingGame;
[SerializeField] private AudioClip clip_cenksAnswer;
[SerializeField] private AudioClip clip_cenksAnswerLevel;

[Header("Anne Sesleri")]
[SerializeField] private AudioClip clip_motherWarns;
```

- [ ] **Adım 3.2: `BuildDialogTree()` sonuna clip atamaları ekle**

```csharp
// Engin
getFeetDownNode.voiceClip      = clip_getFeetDown;
afterDinnerNode.voiceClip      = clip_afterDinner;
enginsAnswerNode.voiceClip     = clip_enginsAnswer;
whichLevelNode.voiceClip       = clip_whichLevel;
enginsAnswerLevelNode.voiceClip = clip_enginsAnswerLevel;

// Cenk
playingGameNode.voiceClip       = clip_playingGame;
cenksAnswerNode.voiceClip       = clip_cenksAnswer;
cenksAnswerLevelNode.voiceClip  = clip_cenksAnswerLevel;

// Anne
motherWarnsNode.voiceClip = clip_motherWarns;
```

- [ ] **Adım 3.3: `DialogRoutine()` içinde `SetSpeakers()` çağır**

`dialogSystem.StartDialog(getFeetDownNode)` satırının üstüne:

```csharp
dialogSystem.SetSpeakers(speakerAudios);
dialogSystem.StartDialog(getFeetDownNode);
```

- [ ] **Adım 3.4: Derleme kontrolü ve commit**

```
git add "Assets/Scripts/DialogSystem/Dialogs/Scene 1/GetFeetDown.cs"
git commit -m "feat: add voice clip support to GetFeetDown"
```

---

## Task 4: HaveDinner.cs — Ses Entegrasyonu

**Files:**
- Modify: `Assets/Scripts/DialogSystem/Dialogs/Scene 1/HaveDinner.cs`

Bu diyalogda konuşmacılar: **Mother**, **Cenk**.

- [ ] **Adım 4.1: Alanları ekle**

`[SerializeField] private DialogSystem dialogSystem;` altına:

```csharp
[Header("Ses Referansları")]
[SerializeField] private List<SpeakerAudio> speakerAudios;

[Header("Anne Sesleri")]
[SerializeField] private AudioClip clip_putTheGamePad;
[SerializeField] private AudioClip clip_motherSaysLiars;
[SerializeField] private AudioClip clip_motherSaysShut;

[Header("Cenk Sesleri")]
[SerializeField] private AudioClip clip_brotherKilled;
[SerializeField] private AudioClip clip_cenkSaysEnginStrong;
```

- [ ] **Adım 4.2: `BuildDialogTree()` sonuna clip atamaları ekle**

```csharp
putTheGamePadNode.voiceClip       = clip_putTheGamePad;
motherSaysLiarsNode.voiceClip     = clip_motherSaysLiars;
motherSaysShutNode.voiceClip      = clip_motherSaysShut;
brotherKilledNode.voiceClip       = clip_brotherKilled;
cenkSaysEnginStrong.voiceClip     = clip_cenkSaysEnginStrong;
```

- [ ] **Adım 4.3: `SitDown()` içinde `SetSpeakers()` çağır**

`dialogSystem.StartDialog(putTheGamePadNode)` satırının üstüne:

```csharp
dialogSystem.SetSpeakers(speakerAudios);
dialogSystem.StartDialog(putTheGamePadNode);
```

- [ ] **Adım 4.4: Derleme kontrolü ve commit**

```
git add "Assets/Scripts/DialogSystem/Dialogs/Scene 1/HaveDinner.cs"
git commit -m "feat: add voice clip support to HaveDinner"
```

---

## Task 5: MotherCooking.cs — Ses Entegrasyonu

**Files:**
- Modify: `Assets/Scripts/DialogSystem/Dialogs/Scene 1/MotherCooking.cs`

Bu diyalogda konuşmacı: **Mother**. Script'te zaten bir `AudioSource` var (şarkı için). Dialog için ayrı bir AudioSource kullanılacak.

- [ ] **Adım 5.1: Alanları ekle**

Mevcut `[Header("Şarkı")]` bloğunun üstüne:

```csharp
[Header("Ses Referansları")]
[SerializeField] private List<SpeakerAudio> speakerAudios;

[Header("Anne Dialog Sesleri")]
[SerializeField] private AudioClip clip_carryTheBodies;
[SerializeField] private AudioClip clip_carryTheBodies2;
```

- [ ] **Adım 5.2: `BuildDialogTree()` sonuna clip atamaları ekle**

```csharp
carryTheBodies.voiceClip  = clip_carryTheBodies;
carryTheBodies2.voiceClip = clip_carryTheBodies2;
```

- [ ] **Adım 5.3: `PlaySongAfterStartDialog()` içinde `SetSpeakers()` çağır**

`dialogSystem.StartDialog(carryTheBodies)` satırının üstüne:

```csharp
dialogSystem.SetSpeakers(speakerAudios);
dialogSystem.StartDialog(carryTheBodies);
```

- [ ] **Adım 5.4: Derleme kontrolü ve commit**

```
git add "Assets/Scripts/DialogSystem/Dialogs/Scene 1/MotherCooking.cs"
git commit -m "feat: add voice clip support to MotherCooking"
```

---

## Task 6: TalkWithNeighbor.cs — SubtitleLine Ses Entegrasyonu

**Files:**
- Modify: `Assets/Scripts/DialogSystem/Dialogs/Scene 1/TalkWithNeighbor.cs`

Bu script DialogSystem kullanmıyor — kendi subtitle sistemini kullanıyor. `SubtitleLine`'a `AudioClip` eklenir ve her satırda doğru konuşmacının AudioSource'undan çalınır. `displayDuration` zamanlamasını kontrol etmeye devam eder; kullanıcı `displayDuration` = `clip.length` olarak ayarlar.

- [ ] **Adım 6.1: `SubtitleLine` struct'ına `voiceClip` ekle**

Mevcut struct:

```csharp
[System.Serializable]
public struct SubtitleLine
{
    public string speakerName;
    public LocalizedText lineText;
    public float displayDuration;
}
```

Şöyle güncelle:

```csharp
[System.Serializable]
public struct SubtitleLine
{
    public string speakerName;
    public LocalizedText lineText;
    public float displayDuration;
    public AudioClip voiceClip; // YENİ — null ise ses çalınmaz
}
```

- [ ] **Adım 6.2: `SpeakerAudio` listesi ekle**

Class'ın alanlarına (örneğin `[SerializeField] private DialogSystem dialogSystem;` altına) ekle:

```csharp
[Header("Ses Referansları")]
[SerializeField] private List<SpeakerAudio> speakerAudios;
```

- [ ] **Adım 6.3: `PlaySubtitleDialog()` içinde ses oynatmayı ekle**

Mevcut loop:

```csharp
foreach (SubtitleLine line in dialogLines)
{
    bool isTurkish = dialogSystem.GetCurrentLanguage();
    string speaker = isTurkish ? (line.speakerName == "Mother" ? "Anne" : "Komşu") : line.speakerName;
    subtitleText.text = speaker + ": " + line.lineText.GetText(isTurkish);
    yield return new WaitForSeconds(line.displayDuration);
}
```

Şöyle güncelle:

```csharp
foreach (SubtitleLine line in dialogLines)
{
    bool isTurkish = dialogSystem.GetCurrentLanguage();
    string speaker = isTurkish ? (line.speakerName == "Mother" ? "Anne" : "Komşu") : line.speakerName;
    subtitleText.text = speaker + ": " + line.lineText.GetText(isTurkish);

    // Konuşmacının AudioSource'unu bul ve sesi çal
    if (line.voiceClip != null && speakerAudios != null)
    {
        SpeakerAudio sa = speakerAudios.Find(s => s.speakerName == line.speakerName);
        if (sa != null && sa.audioSource != null)
        {
            sa.audioSource.spatialBlend = sa.isPlayer ? 0f : 1f;
            sa.audioSource.clip = line.voiceClip;
            sa.audioSource.Play();
        }
    }

    yield return new WaitForSeconds(line.displayDuration);
}
```

- [ ] **Adım 6.4: Derleme kontrolü ve commit**

```
git add "Assets/Scripts/DialogSystem/Dialogs/Scene 1/TalkWithNeighbor.cs"
git commit -m "feat: add voice clip support to TalkWithNeighbor subtitle system"
```

---

## Task 7: Unity Inspector Kurulumu (Manuel)

Bu task kod içermiyor — Unity Editor'da yapılır.

**Files:** Yok (Inspector değişiklikleri Unity sahne dosyalarına kaydedilir)

- [ ] **Adım 7.1: NPC GameObject'lerine AudioSource ekle**

Scene 1'deki her konuşmacı NPC GameObject'ini seç ve `Add Component → Audio → Audio Source` yap:

| NPC | GameObject Adı (tahmini) | Spatial Blend |
|-----|--------------------------|---------------|
| Cenk | Cenk | 1 (3D) |
| Mother/Anne | Mother | 1 (3D) |
| Komşu | Neighbor | 1 (3D) |
| Engin (player) | Player | 0 (2D) |

Her AudioSource için:
- `Play On Awake`: **false**
- `Spatial Blend`: 0 (player) veya 1 (NPC)
- `Volume Rolloff`: Logarithmic Distance
- `Max Distance`: 15–20 (sahne boyutuna göre)

- [ ] **Adım 7.2: MotherCooking için ayrı dialog AudioSource**

`MotherCooking` script'i zaten `audioSource = gameObject.AddComponent<AudioSource>()` ile bir ses kaynağı ekliyor (şarkı için). Anne'nin dialog sesi için:

- Mother GameObject'ine **ikinci** bir AudioSource ekle.
- `MotherCooking` Inspector'ında `speakerAudios` listesine Mother'ı ekle ve bu ikinci AudioSource'u ata.

- [ ] **Adım 7.3: TalkWithCenkOnTable Inspector**

`TalkWithCenkOnTable` component'ini seç:
1. `Speaker Audios` listesine 2 eleman ekle:
   - `[0]`: speakerName = "Cenk", audioSource = Cenk/AudioSource, isPlayer = false
   - `[1]`: speakerName = "Engin", audioSource = Player/AudioSource, isPlayer = true
2. Cenk ve Engin ses klipleri atandığında `Clip_*` alanlarına sürükle.

- [ ] **Adım 7.4: GetFeetDown Inspector**

`GetFeetDown` component'ini seç:
1. `Speaker Audios` listesine 3 eleman ekle:
   - `[0]`: speakerName = "Engin", audioSource = Player/AudioSource, isPlayer = true
   - `[1]`: speakerName = "Cenk", audioSource = Cenk/AudioSource, isPlayer = false
   - `[2]`: speakerName = "Mother", audioSource = Mother/AudioSource, isPlayer = false
2. Klipleri ata.

- [ ] **Adım 7.5: HaveDinner Inspector**

`HaveDinner` component'ini seç:
1. `Speaker Audios` listesine 2 eleman ekle:
   - `[0]`: speakerName = "Mother", audioSource = Mother/AudioSource, isPlayer = false
   - `[1]`: speakerName = "Cenk", audioSource = Cenk/AudioSource, isPlayer = false
2. Klipleri ata.

- [ ] **Adım 7.6: MotherCooking Inspector**

`MotherCooking` component'ini seç:
1. `Speaker Audios` listesine 1 eleman ekle:
   - `[0]`: speakerName = "Mother", audioSource = Mother/ikinci AudioSource, isPlayer = false
2. Klipleri ata.

- [ ] **Adım 7.7: TalkWithNeighbor Inspector**

`TalkWithNeighbor` component'ini seç:
1. `Speaker Audios` listesine 2 eleman ekle:
   - `[0]`: speakerName = "Neighbor", audioSource = Neighbor/AudioSource, isPlayer = false
   - `[1]`: speakerName = "Mother", audioSource = Mother/AudioSource, isPlayer = false
2. `Dialog Lines` listesindeki her satıra `voiceClip` ata. `displayDuration` değerini o clip'in süresiyle eşleştir.

- [ ] **Adım 7.8: Sahnede test et**

1. Play moduna gir.
2. TalkWithCenkOnTable diyaloğunu başlat — Cenk konuşurken ses Cenk'in pozisyonundan gelmeli.
3. Headphone ile sağa-sola dön — ses 3D konumlanmalı (Cenk solda ise soldan gelir).
4. Space'e bas → ses durur, opsiyonlar çıkar.
5. Clip atanmamış node gelince typewriter eskiden kaldığı hızda devam etmeli.

---

## Başarı Kriterleri

- [ ] NPC konuşurken ses o NPC'nin dünya pozisyonundan gelir
- [ ] Typewriter efekti clip ile aynı anda biter (karakter sayısı / clip süresi)
- [ ] Clip atanmamış node'lar hatasız çalışır (default hız)
- [ ] Opsiyonlar göründüğünde ses durur
- [ ] Space ile atlayınca ses durur
- [ ] Dialog bitince ses durur
- [ ] Engin'in sesi 2D (pozisyon bağımsız) çalınır
