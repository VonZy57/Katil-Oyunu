using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlackjackManager : MonoBehaviour
{
    [Header("Görev Ayarları")]
    public MissionSO storyMission; // Bu görev aktifse hileli mod açılır

    [Header("Görsel Ayarlar")]
    public List<Sprite> allCardSprites;
    public Sprite cardBackSprite;
    public GameObject cardPrefab;
    public float cardMoveSpeed = 1500f; // Kartın uçuş hızı

    [Header("Deste Pozisyonu")]
    public Transform deckSpawnPoint; // Sahnedeki DeckReference objesi (Bo� GameObject)
    private List<GameObject> visualDeckStack = new List<GameObject>(); // Masadaki görsel deste objeleri

    [Header("UI Referansları")]
    public Transform dealerCardArea;
    public Transform playerCardArea;
    public TextMeshProUGUI playerScoreText;
    public TextMeshProUGUI dealerScoreText;
    public TextMeshProUGUI resultText;
    public GameObject gamePanel;

    [Header("Butonlar")]
    public Button hitButton;
    public Button standButton;
    public Button playAgainButton;

    private BlackjackTableInteractable currentTable;

    // Oyun Mant���
    private List<Card> deck = new List<Card>();
    private List<Card> playerHand = new List<Card>();
    private List<Card> dealerHand = new List<Card>();

    // Krupiyerin kapalı kartı (Animasyon sonrası referans tutmak için)
    private Card dealerHiddenCardData;
    private GameObject dealerHiddenCardObj;
    private bool isDealerCardHidden = false;

    // Animasyon kilidi (Kart uçarken butonlara basılmasın)
    private bool isDealingAnimationPlaying = false;

    void Start()
    {
        hitButton.onClick.AddListener(() => StartCoroutine(PlayerHitRoutine()));
        standButton.onClick.AddListener(PlayerStand);
        playAgainButton.onClick.AddListener(StartRound);
        gamePanel.SetActive(false);
        SetGameButtonsActive(false);
        playAgainButton.gameObject.SetActive(false);
    }

    public void OpenTable(BlackjackTableInteractable tableScript)
    {
        currentTable = tableScript;
        gamePanel.SetActive(true);

        CreateDeck();
        ShuffleDeck();

        // G�rsel desteyi olu�tur (Masadaki y���n)
        GenerateVisualDeck();

        StartRound();
    }

    void StartRound()
    {
        // Deste azald�ysa yenile
        if (deck.Count < 10)
        {
            CreateDeck();
            ShuffleDeck();
            GenerateVisualDeck(); // G�rsel y���n� da tazele
            resultText.text = "Deste Kar��t�r�ld�!";
        }
        else
        {
            resultText.text = "";
        }

        playerHand.Clear();
        dealerHand.Clear();
        ClearTable();

        playAgainButton.gameObject.SetActive(false);
        SetGameButtonsActive(false);
        playerScoreText.text = "";
        dealerScoreText.text = "";
        isDealerCardHidden = true;

        StartCoroutine(DealInitialCardsRoutine());
    }

    // --- GÖRSEL DESTE OLUŞTURMA ---
    void GenerateVisualDeck()
    {
        // Önce eskileri temizle
        foreach (GameObject obj in visualDeckStack) Destroy(obj);
        visualDeckStack.Clear();

        int visualCount = Mathf.Min(deck.Count, 52);

        for (int i = 0; i < visualCount; i++)
        {
            GameObject cardObj = Instantiate(cardPrefab, deckSpawnPoint);
            // LayoutGroup tarafından yönetilmesin, serbest dursun
            Destroy(cardObj.GetComponent<LayoutElement>());

            cardObj.transform.localPosition = new Vector3(0, i * 2f, 0); // Hafif üst üste bindir
            cardObj.GetComponent<Image>().sprite = cardBackSprite;

            // Deste referansının içinde dursun
            cardObj.transform.SetParent(deckSpawnPoint, false);

            visualDeckStack.Add(cardObj);
        }
    }

    // --- KART DA�ITMA AN�MASYONU ---
    IEnumerator DealCardAnimated(List<Card> hand, Transform targetArea, bool faceDown)
    {
        if (deck.Count == 0) yield break;

        isDealingAnimationPlaying = true; // Kilit vur

        // 1. Veriyi �ek
        Card cardToDeal = deck[0];
        deck.RemoveAt(0);
        hand.Add(cardToDeal);

        // 2. G�rsel desteden bir kart eksilt
        GameObject flyingCard = null;
        if (visualDeckStack.Count > 0)
        {
            // En �stteki kart� al
            int lastIndex = visualDeckStack.Count - 1;
            flyingCard = visualDeckStack[lastIndex];
            visualDeckStack.RemoveAt(lastIndex);
        }
        else
        {
            // E�er g�rsel deste bittiyse (ama oyun devam ediyorsa) yeni bir tane olu�tur
            flyingCard = Instantiate(cardPrefab, deckSpawnPoint);
            flyingCard.GetComponent<Image>().sprite = cardBackSprite;
        }

        // 3. Kart� GamePanel'in �ocu�u yap (b�ylece el alan�ndan ba��ms�z u�ar)
        flyingCard.transform.SetParent(gamePanel.transform);
        flyingCard.transform.position = deckSpawnPoint.position; // Ba�lang�� noktas�

        // 4. Hedefe u�ur (Lerp)
        Vector3 targetPos = targetArea.position; // Elin oldu�u yere u�acak
        float flightTime = 0.4f; // Saniye cinsinden u�u� s�resi
        float elapsedTime = 0f;
        Vector3 startPos = flyingCard.transform.position;

        while (elapsedTime < flightTime)
        {
            // Hedefe do�ru git
            flyingCard.transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime / flightTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 5. Hedefe vard�, art�k Layout Grubuna (Elin i�ine) girsin
        flyingCard.transform.SetParent(targetArea, false);

        // U�u� s�ras�nda scale bozulabilir, d�zeltelim
        flyingCard.transform.localScale = Vector3.one;

        // Layout grubunun onu s�raya dizmesini beklemeden g�rseli ayarla
        Image cardImg = flyingCard.GetComponent<Image>();

        if (faceDown)
        {
            cardImg.sprite = cardBackSprite;
            // E�er bu krupiyerin kapal� kart�ysa referans�n� sakla
            if (hand == dealerHand)
            {
                dealerHiddenCardObj = flyingCard;
                dealerHiddenCardData = cardToDeal;
            }
        }
        else
        {
            // Kart� a� (Flip efekti eklenebilir ama �imdilik direkt de�i�elim)
            cardImg.sprite = cardToDeal.cardSprite;
        }

        isDealingAnimationPlaying = false; // Kilidi a�
    }

    // --- OYUN AKI�I ---

    IEnumerator DealInitialCardsRoutine()
    {
        // Animasyon bitene kadar bekle (yield return StartCoroutine...)
        yield return StartCoroutine(DealCardAnimated(playerHand, playerCardArea, false));
        UpdateScores();

        yield return StartCoroutine(DealCardAnimated(dealerHand, dealerCardArea, false));
        UpdateScores();

        yield return StartCoroutine(DealCardAnimated(playerHand, playerCardArea, false));
        UpdateScores();

        // Dealer'ın kapalı kartı
        yield return StartCoroutine(DealCardAnimated(dealerHand, dealerCardArea, true));
        UpdateScores();

        // Blackjack Kontrolü
        if (CalculateHandValue(playerHand) == 21)
        {
            StartCoroutine(RevealHiddenCardAndFinish());
        }
        else
        {
            SetGameButtonsActive(true);
        }
    }

    // Player Hit artık Coroutine olmalı çünkü animasyonu bekleyeceğiz
    IEnumerator PlayerHitRoutine()
    {
        // Butonları kapat (spam yapılmasın)
        SetGameButtonsActive(false);

        yield return StartCoroutine(DealCardAnimated(playerHand, playerCardArea, false));
        UpdateScores();

        if (CalculateHandValue(playerHand) > 21)
        {
            EndGame("Busted! (Battın)");
        }
        else if (CalculateHandValue(playerHand) == 21)
        {
            PlayerStand();
        }
        else
        {
            // Eğer batmadıysa butonları tekrar aç
            SetGameButtonsActive(true);
        }
    }

    void PlayerStand()
    {
        SetGameButtonsActive(false);
        StartCoroutine(DealerTurnRoutine());
    }

    IEnumerator DealerTurnRoutine()
    {
        // Önce kapalı kartı aç
        if (isDealerCardHidden)
        {
            RevealHiddenCard();
            yield return new WaitForSeconds(0.5f);
        }

        while (CalculateHandValue(dealerHand) < 17)
        {
            yield return StartCoroutine(DealCardAnimated(dealerHand, dealerCardArea, false));
            UpdateScores();
            yield return new WaitForSeconds(0.2f);
        }

        DetermineWinner();
    }

    void RevealHiddenCard()
    {
        isDealerCardHidden = false;
        if (dealerHiddenCardObj != null)
        {
            dealerHiddenCardObj.GetComponent<Image>().sprite = dealerHiddenCardData.cardSprite;
        }
        UpdateScores();
    }

    // Özel durum: Blackjack olunca veya oyun bitince kart açıp bitirme
    IEnumerator RevealHiddenCardAndFinish()
    {
        yield return new WaitForSeconds(0.5f);
        RevealHiddenCard();
        DetermineWinner();
    }

    void DetermineWinner()
    {
        int playerVal = CalculateHandValue(playerHand);
        int dealerVal = CalculateHandValue(dealerHand);

        if (playerVal > 21) EndGame("Kaybettin.");
        else if (dealerVal > 21) EndGame("Kasa Battı! Kazandın!");
        else if (playerVal > dealerVal) EndGame("Kazandın!");
        else if (playerVal < dealerVal) EndGame("Kaybettin.");
        else EndGame("Berabere.");
    }

    void EndGame(string message)
    {
        resultText.text = message;
        SetGameButtonsActive(false);
        playAgainButton.gameObject.SetActive(true);
    }

    void UpdateScores()
    {
        playerScoreText.text = "Skor: " + CalculateHandValue(playerHand);

        if (isDealerCardHidden && dealerHand.Count > 0)
        {
            int visibleVal = dealerHand[0].value;
            if (visibleVal == 11) visibleVal = 11;
            dealerScoreText.text = "Skor: " + visibleVal + " + ?";
        }
        else
        {
            dealerScoreText.text = "Skor: " + CalculateHandValue(dealerHand);
        }
    }

    int CalculateHandValue(List<Card> hand)
    {
        int total = 0;
        int aceCount = 0;
        foreach (Card card in hand)
        {
            total += card.value;
            if (card.value == 11) aceCount++;
        }
        while (total > 21 && aceCount > 0)
        {
            total -= 10;
            aceCount--;
        }
        return total;
    }

    void CreateDeck()
    {
        deck.Clear();
        string[] suits = { "Sinek", "Karo", "Kupa", "Maca" };
        int spriteIndex = 0;
        foreach (string suit in suits)
        {
            for (int i = 1; i <= 13; i++)
            {
                int val = i;
                if (val > 10) val = 10;
                if (val == 1) val = 11;

                Card newCard = new Card
                {
                    cardName = suit + " " + i,
                    value = val,
                    cardSprite = allCardSprites.Count > spriteIndex ? allCardSprites[spriteIndex] : null
                };
                deck.Add(newCard);
                spriteIndex++;
            }
        }
    }

    void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            Card temp = deck[i];
            int randomIndex = Random.Range(i, deck.Count);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    void ClearTable()
    {
        foreach (Transform child in playerCardArea) Destroy(child.gameObject);
        foreach (Transform child in dealerCardArea) Destroy(child.gameObject);
    }

    void SetGameButtonsActive(bool isActive)
    {
        // Animasyon oynuyorsa butonları zorla kapalı tut
        if (isDealingAnimationPlaying) isActive = false;

        hitButton.interactable = isActive;
        standButton.interactable = isActive;

        // Buton objelerini kapatmak yerine interactable kapatmak daha şık durur
        // Ama görünürlük kapatmak istersen gameObject.SetActive kullanabilirsin
        hitButton.gameObject.SetActive(isActive);
        standButton.gameObject.SetActive(isActive);
    }

    public void ExitTable()
    {
        gamePanel.SetActive(false);
        if (currentTable != null)
        {
            currentTable.EndInteraction();
            currentTable = null;
        }
    }
}