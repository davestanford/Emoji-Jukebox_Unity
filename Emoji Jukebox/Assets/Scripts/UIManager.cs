using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject startPanel;
    public GameObject songDraftPanel;
    public GameObject lookAwayPanel;
    public GameObject clueSetupPanel;
    public GameObject passPanel;
    public GameObject guessPanel;
    public GameObject resultPanel;
    public GameObject songLibraryPanel;
    public GameObject emojiPickerPanel;
    public GameObject pausePanel;
    public GameObject howToPlayPanel;
    public GameObject aboutPanel;

    [Header("Start Panel")]
    public TMP_InputField player1Input;
    public TMP_InputField player2Input;
    public Button startGameButton;
    [SerializeField] private float enabledStartButtonAlpha = 1f;
    [SerializeField] private float disabledStartButtonAlpha = 0.45f;

    [Header("Song Draft Panel")]
    public TMP_Text draftTurnText;
    public TMP_Text draftPoolCountText;
    public TMP_Text draftErrorText;
    public TMP_InputField draftSongInput;
    public Transform draftSongListContainer;
    public GameObject draftSongListItemPrefab;
    public Button addSongButton;
    public Button skipSongButton;
    public Button startMatchButton;
    [SerializeField] private float draftErrorDuration = 2.5f;
    [SerializeField] private float enabledDraftButtonAlpha = 1f;
    [SerializeField] private float disabledDraftButtonAlpha = 0.4f;
    private Coroutine draftErrorRoutine;
    private bool wasStartMatchReadyLastRefresh = false;

    [Header("Look Away Panel")]
    public TMP_Text lookAwayText;
    public Button continueButton;

    [Header("Clue Setup Panel")]
    public TMP_Text clueSetupTurnText;
    public TMP_Text clueSetupInstructionText;
    public TMP_Text selectedSongText;
    public Transform selectedEmojiDisplayArea;
    public GameObject clueImagePrefab;
    public Button confirmRoundButton;

    [Header("Song Library Panel")]
    public TMP_Text songLibraryTitleText;
    public TMP_Text songLibraryCountText;
    public Transform songLibraryListContainer;
    public GameObject songLibraryButtonPrefab;

    [Header("Emoji Picker Panel")]
    public TMP_Text emojiPickerTitleText;
    public TMP_Text emojiPickerCountText;
    public Transform emojiPickerOptionsContainer;
    public Transform emojiPickerSelectedContainer;
    public GameObject emojiButtonPrefab;
    public Button backspaceButton;

    [Header("Pass Panel")]
    public TMP_Text passText;
    public Button readyToGuessButton;

    [Header("Guess Panel")]
    public TMP_Text guessTurnText;
    public Transform guessClueDisplayArea;
    public Transform guessSongListContainer;
    public GameObject guessSongButtonPrefab;
    public TMP_Text selectedGuessText;
    public Button submitGuessButton;

    [Header("Emoji Reveal Timing")]
    [SerializeField] private float emojiRevealStagger = 0.25f;

    [Header("Guess Icons")]
    public Image guessOneIcon;
    public Image guessTwoIcon;
    [SerializeField] private float activeGuessAlpha = 1f;
    [SerializeField] private float usedGuessAlpha = 0.25f;

    [Header("Result Panel")]
    public TMP_Text resultText;
    public TMP_Text scoreText;

    [Header("Result Panel Buttons")]
    public GameObject nextRoundButton;
    public GameObject playAgainButton;
    public GameObject mainMenuFromResultsButton;

    [Header("Pause Panel")]
    public TMP_Text pauseTitleText;

    [Header("Timer UI")]
    public TMP_Text clueSetupTimerText;
    public TMP_Text chooseSongTimerText;
    public TMP_Text chooseEmojiTimerText;
    public TMP_Text guessTimerText;

    private bool pauseVisible = false;
    private string selectedGuessSong = "";
    private Dictionary<Button, Coroutine> buttonPulseRoutines = new Dictionary<Button, Coroutine>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (player1Input != null)
            player1Input.onValueChanged.AddListener(_ => ValidateStartButton());

        if (player2Input != null)
            player2Input.onValueChanged.AddListener(_ => ValidateStartButton());

        ShowMainMenu();
        ValidateStartButton();

        if (draftErrorText != null)
        {
            draftErrorText.text = "";
            draftErrorText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        HandleKeyboardInput();
    }

    // --------------------------------------------------
    // START PANEL VALIDATION
    // --------------------------------------------------

    private void ValidateStartButton()
    {
        if (startGameButton == null)
            return;

        bool p1Valid = player1Input != null && !string.IsNullOrWhiteSpace(player1Input.text);
        bool p2Valid = player2Input != null && !string.IsNullOrWhiteSpace(player2Input.text);

        startGameButton.interactable = p1Valid && p2Valid;
        UpdateStartGameButtonVisual();

        if (startGameButton.interactable)
            StartButtonPulse(startGameButton);
        else
            StopButtonPulse(startGameButton);
    }

    private void UpdateStartGameButtonVisual()
    {
        if (startGameButton == null)
            return;

        float targetAlpha = startGameButton.interactable
            ? enabledStartButtonAlpha
            : disabledStartButtonAlpha;

        Image buttonImage = startGameButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            Color c = buttonImage.color;
            c.a = targetAlpha;
            buttonImage.color = c;
        }

        TMP_Text buttonText = startGameButton.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            Color c = buttonText.color;
            c.a = targetAlpha;
            buttonText.color = c;
        }
    }

    // --------------------------------------------------
    // DRAFT ERROR MESSAGE
    // --------------------------------------------------

    public void ShowDraftError(string message)
    {
        if (draftErrorText == null) return;

        if (draftErrorRoutine != null)
            StopCoroutine(draftErrorRoutine);

        draftErrorText.text = message;
        draftErrorText.gameObject.SetActive(true);

        Color c = draftErrorText.color;
        c.a = 1f;
        draftErrorText.color = c;

        draftErrorRoutine = StartCoroutine(HideDraftErrorAfterTime());
    }

    private IEnumerator HideDraftErrorAfterTime()
    {
        yield return new WaitForSeconds(draftErrorDuration);

        if (draftErrorText == null)
        {
            draftErrorRoutine = null;
            yield break;
        }

        float fadeTime = 0.4f;
        float t = 0f;

        Color startColor = draftErrorText.color;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / fadeTime);

            draftErrorText.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        draftErrorText.text = "";
        draftErrorText.gameObject.SetActive(false);
        draftErrorText.color = new Color(startColor.r, startColor.g, startColor.b, 1f);

        draftErrorRoutine = null;
    }

    // --------------------------------------------------
    // GENERIC BUTTON PULSE
    // --------------------------------------------------

    private void StartButtonPulse(Button button, float scaleMultiplier = 1.08f, float halfCycleDuration = 0.5f)
    {
        if (button == null) return;
        if (buttonPulseRoutines.ContainsKey(button)) return;

        Coroutine routine = StartCoroutine(PulseButtonRoutine(button, scaleMultiplier, halfCycleDuration));
        buttonPulseRoutines.Add(button, routine);
    }

    private void StopButtonPulse(Button button)
    {
        if (button == null) return;

        if (buttonPulseRoutines.TryGetValue(button, out Coroutine routine))
        {
            StopCoroutine(routine);
            buttonPulseRoutines.Remove(button);
        }

        button.transform.localScale = Vector3.one;
    }

    private void StopAllButtonPulses()
    {
        foreach (var pair in buttonPulseRoutines)
        {
            if (pair.Value != null)
                StopCoroutine(pair.Value);

            if (pair.Key != null)
                pair.Key.transform.localScale = Vector3.one;
        }

        buttonPulseRoutines.Clear();
    }

    private IEnumerator PulseButtonRoutine(Button button, float scaleMultiplier, float halfCycleDuration)
    {
        Vector3 startScale = Vector3.one;
        Vector3 endScale = Vector3.one * scaleMultiplier;

        while (button != null)
        {
            float t = 0f;

            while (t < halfCycleDuration)
            {
                t += Time.deltaTime;
                float lerp = Mathf.SmoothStep(0f, 1f, t / halfCycleDuration);
                if (button != null)
                    button.transform.localScale = Vector3.Lerp(startScale, endScale, lerp);
                yield return null;
            }

            t = 0f;

            while (t < halfCycleDuration)
            {
                t += Time.deltaTime;
                float lerp = Mathf.SmoothStep(0f, 1f, t / halfCycleDuration);
                if (button != null)
                    button.transform.localScale = Vector3.Lerp(endScale, startScale, lerp);
                yield return null;
            }
        }
    }

    // --------------------------------------------------
    // PANEL VISIBILITY
    // --------------------------------------------------

    private void HideAllPanels()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (startPanel != null) startPanel.SetActive(false);
        if (songDraftPanel != null) songDraftPanel.SetActive(false);
        if (lookAwayPanel != null) lookAwayPanel.SetActive(false);
        if (clueSetupPanel != null) clueSetupPanel.SetActive(false);
        if (passPanel != null) passPanel.SetActive(false);
        if (guessPanel != null) guessPanel.SetActive(false);
        if (resultPanel != null) resultPanel.SetActive(false);
        if (songLibraryPanel != null) songLibraryPanel.SetActive(false);
        if (emojiPickerPanel != null) emojiPickerPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        if (aboutPanel != null) aboutPanel.SetActive(false);
    }

    private void ShowOnly(GameObject panelToShow)
    {
        HideAllPanels();
        StopAllButtonPulses();

        if (panelToShow != null)
            panelToShow.SetActive(true);

        pauseVisible = false;
        Time.timeScale = 1f;
    }

    private void ShowPopup(GameObject popupToShow)
    {
        if (popupToShow != null)
            popupToShow.SetActive(true);
    }

    private void HidePopup(GameObject popupToHide)
    {
        if (popupToHide != null)
            popupToHide.SetActive(false);
    }

    // --------------------------------------------------
    // IMAGE / EMOJI DISPLAY
    // --------------------------------------------------

    private void ClearObjectContainer(Transform container)
    {
        if (container == null) return;

        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Destroy(container.GetChild(i).gameObject);
        }
    }

    private void PopulateImageContainer(Transform container, List<Sprite> clues)
    {
        if (container == null || clueImagePrefab == null) return;

        ClearObjectContainer(container);

        if (clues == null) return;

        foreach (Sprite clueSprite in clues)
        {
            if (clueSprite == null) continue;

            GameObject newImageObj = Instantiate(clueImagePrefab, container);
            Image imageComp = newImageObj.GetComponent<Image>();

            if (imageComp != null)
            {
                imageComp.sprite = clueSprite;
                imageComp.preserveAspect = true;
            }
        }
    }

    private void PopulateAnimatedImageContainer(Transform container, List<Sprite> clues)
    {
        if (container == null)
        {
            Debug.LogError("PopulateAnimatedImageContainer: container is NULL");
            return;
        }

        if (clueImagePrefab == null)
        {
            Debug.LogError("PopulateAnimatedImageContainer: clueImagePrefab is NULL");
            return;
        }

        ClearObjectContainer(container);

        if (clues == null)
        {
            Debug.LogWarning("PopulateAnimatedImageContainer: clues list is NULL");
            return;
        }

        for (int i = 0; i < clues.Count; i++)
        {
            Sprite clueSprite = clues[i];
            if (clueSprite == null)
                continue;

            GameObject newImageObj = Instantiate(clueImagePrefab, container);

            Image imageComp = newImageObj.GetComponent<Image>();
            if (imageComp != null)
            {
                imageComp.sprite = clueSprite;
                imageComp.preserveAspect = true;
            }

            EmojiClueAnim anim = newImageObj.GetComponent<EmojiClueAnim>();
            if (anim != null)
            {
                float delay = i * emojiRevealStagger;
                anim.Play(delay);
            }
        }
    }

    private void SetButtonAlpha(Button button, float alpha)
    {
        if (button == null) return;

        Image img = button.GetComponent<Image>();
        if (img != null)
        {
            Color c = img.color;
            c.a = alpha;
            img.color = c;
        }

        TMP_Text txt = button.GetComponentInChildren<TMP_Text>();
        if (txt != null)
        {
            Color c = txt.color;
            c.a = alpha;
            txt.color = c;
        }
    }

    // --------------------------------------------------
    // MAIN MENU
    // --------------------------------------------------

    public void ShowMainMenu()
    {
        ShowOnly(mainMenuPanel);

        if (GameManager.Instance != null)
            GameManager.Instance.currentPhase = RoundPhase.MainMenu;
    }

    public void OnMainMenuStartPressed()
    {
        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.PlayBlackFade(() =>
            {
                ShowStartPanel();
            });
        }
        else
        {
            ShowStartPanel();
        }
    }

    public void ShowStartPanel()
    {
        ShowOnly(startPanel);

        if (GameManager.Instance != null)
            GameManager.Instance.currentPhase = RoundPhase.PlayerSetup;

        ValidateStartButton();
    }

    public void OnBeginPlayerSetupPressed()
    {
        if (startGameButton != null && !startGameButton.interactable)
            return;

        string p1 = player1Input != null ? player1Input.text : "Player 1";
        string p2 = player2Input != null ? player2Input.text : "Player 2";

        if (string.IsNullOrWhiteSpace(p1)) p1 = "Player 1";
        if (string.IsNullOrWhiteSpace(p2)) p2 = "Player 2";

        StopButtonPulse(startGameButton);

        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.PlayBlackFade(() =>
            {
                GameManager.Instance.StartGame(p1, p2);
                ShowSongDraftPanel();
            });
        }
        else
        {
            GameManager.Instance.StartGame(p1, p2);
            ShowSongDraftPanel();
        }
    }

    public void OnHowToPlayPressed()
    {
        ShowOnly(howToPlayPanel);
    }

    public void OnAboutPressed()
    {
        ShowOnly(aboutPanel);
    }

    public void OnBackToMainMenuPressed()
    {
        ShowMainMenu();
    }

    public void OnExitGamePressed()
    {
        Application.Quit();
    }

    // --------------------------------------------------
    // SONG DRAFT PANEL
    // --------------------------------------------------

    public void ShowSongDraftPanel()
    {
        ShowOnly(songDraftPanel);
        RefreshSongDraftPanel();
    }

    public void RefreshSongDraftPanel()
    {
        if (GameManager.Instance == null) return;

        bool isFull = GameManager.Instance.IsSongPoolFull();

        if (draftTurnText != null)
        {
            if (isFull)
            {
                draftTurnText.text = "Song list complete!";
            }
            else
            {
                draftTurnText.text = GameManager.Instance.GetCurrentDraftPlayerName() + ", add a song";
            }
        }

        if (draftPoolCountText != null)
            draftPoolCountText.text = "Songs: " + GameManager.Instance.songPool.Count + " / " + GameManager.Instance.maxSongsInPool;

        if (startMatchButton != null)
            startMatchButton.interactable = isFull;

        if (addSongButton != null)
            addSongButton.interactable = !isFull;

        if (skipSongButton != null)
            skipSongButton.interactable = !isFull;

        SetButtonAlpha(addSongButton, isFull ? disabledDraftButtonAlpha : enabledDraftButtonAlpha);
        SetButtonAlpha(skipSongButton, isFull ? disabledDraftButtonAlpha : enabledDraftButtonAlpha);
        SetButtonAlpha(startMatchButton, isFull ? enabledDraftButtonAlpha : disabledDraftButtonAlpha);

        if (isFull && !wasStartMatchReadyLastRefresh)
            StartButtonPulse(startMatchButton);
        else if (!isFull)
            StopButtonPulse(startMatchButton);

        wasStartMatchReadyLastRefresh = isFull;

        if (draftErrorRoutine != null)
        {
            StopCoroutine(draftErrorRoutine);
            draftErrorRoutine = null;
        }

        if (draftErrorText != null)
        {
            draftErrorText.text = "";
            draftErrorText.gameObject.SetActive(false);

            Color c = draftErrorText.color;
            c.a = 1f;
            draftErrorText.color = c;
        }

        if (draftSongInput != null)
            draftSongInput.text = "";

        RefreshDraftSongList();
    }

    private void RefreshDraftSongList()
    {
        if (draftSongListContainer == null || draftSongListItemPrefab == null) return;

        ClearObjectContainer(draftSongListContainer);

        foreach (string song in GameManager.Instance.songPool)
        {
            GameObject item = Instantiate(draftSongListItemPrefab, draftSongListContainer);

            TMP_Text textComp = item.GetComponentInChildren<TMP_Text>();
            if (textComp != null)
                textComp.text = song;
        }
    }

    public void OnAddDraftSongPressed()
    {
        if (draftSongInput == null) return;
        if (addSongButton != null && !addSongButton.interactable) return;

        bool success = GameManager.Instance.AddSongToPool(draftSongInput.text);

        if (!success)
        {
            ShowDraftError("Enter a unique song title.");
            return;
        }

        RefreshSongDraftPanel();
    }

    public void OnSkipDraftSongPressed()
    {
        if (skipSongButton != null && !skipSongButton.interactable) return;

        GameManager.Instance.SkipSongDraftTurn();
        RefreshSongDraftPanel();
    }

    public void OnBeginMatchPressed()
    {
        if (!GameManager.Instance.IsSongPoolFull())
            return;

        StopButtonPulse(startMatchButton);
        StartCoroutine(BeginMatchWithFlash());
    }

    private IEnumerator BeginMatchWithFlash()
    {
        if (TransitionManager.Instance != null)
            TransitionManager.Instance.PlayWhiteFlash();

        yield return new WaitForSecondsRealtime(0.08f);

        GameManager.Instance.StartRoundSetup();
        ShowLookAwayPanel();
    }

    // --------------------------------------------------
    // LOOK AWAY PANEL
    // --------------------------------------------------

    public void ShowLookAwayPanel()
    {
        ShowOnly(lookAwayPanel);

        if (GameManager.Instance != null)
            GameManager.Instance.currentPhase = RoundPhase.LookAway;

        if (lookAwayText != null && GameManager.Instance != null)
        {
            string currentPlayer = GameManager.Instance.GetCurrentTurnPlayerName();
            string otherPlayer = GameManager.Instance.GetGuesserName();

            lookAwayText.text =
                
                currentPlayer;
        }

        if (continueButton != null)
            StartButtonPulse(continueButton);
    }

    public void OnContinueToClueSetupPressed()
    {
        StopButtonPulse(continueButton);

        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.PlayBlackFade(() =>
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.BeginClueSetup();

                ShowClueSetupPanel();
            });
        }
        else
        {
            if (GameManager.Instance != null)
                GameManager.Instance.BeginClueSetup();

            ShowClueSetupPanel();
        }
    }

    // --------------------------------------------------
    // CLUE SETUP PANEL
    // --------------------------------------------------

    public void ShowClueSetupPanel()
    {
        ShowOnly(clueSetupPanel);
        RefreshClueSetupPanel();

        if (songLibraryPanel != null) songLibraryPanel.SetActive(false);
        if (emojiPickerPanel != null) emojiPickerPanel.SetActive(false);
    }

    public void RefreshClueSetupPanel()
    {
        if (clueSetupTurnText != null)
            clueSetupTurnText.text = GameManager.Instance.GetCurrentTurnPlayerName() + ", set up your clue";

        if (clueSetupInstructionText != null)
            clueSetupInstructionText.text = "Choose a song and emoji clues.";

        if (selectedSongText != null)
        {
            if (string.IsNullOrWhiteSpace(GameManager.Instance.pendingSong))
                selectedSongText.text = "No song selected";
            else
                selectedSongText.text = GameManager.Instance.pendingSong;
        }

        PopulateImageContainer(selectedEmojiDisplayArea, GameManager.Instance.pendingEmojiClues);

        bool hasSong = !string.IsNullOrWhiteSpace(GameManager.Instance.pendingSong);
        bool hasEmojis = GameManager.Instance.pendingEmojiClues != null &&
                         GameManager.Instance.pendingEmojiClues.Count > 0;

        if (confirmRoundButton != null)
        {
            confirmRoundButton.interactable = hasSong && hasEmojis;

            if (confirmRoundButton.interactable)
                StartButtonPulse(confirmRoundButton);
            else
                StopButtonPulse(confirmRoundButton);
        }
    }

    public void OnOpenSongLibraryPressed()
    {
        RefreshSongLibraryPanel();
        ShowPopup(songLibraryPanel);
    }

    public void OnCloseSongLibraryPressed()
    {
        HidePopup(songLibraryPanel);
    }

    public void OnOpenEmojiPickerPressed()
    {
        RefreshEmojiPickerPanel();
        ShowPopup(emojiPickerPanel);
    }

    public void OnCloseEmojiPickerPressed()
    {
        HidePopup(emojiPickerPanel);
    }

    public void OnClearSelectedSongPressed()
    {
        GameManager.Instance.ClearPendingSong();
        RefreshClueSetupPanel();
    }

    public void OnClearSelectedEmojisPressed()
    {
        GameManager.Instance.ClearPendingEmojis();
        RefreshClueSetupPanel();
        RefreshEmojiPickerPanel();
    }

    public void OnConfirmRoundPressed()
    {
        StopButtonPulse(confirmRoundButton);

        bool success = GameManager.Instance.ConfirmRoundSetup();
        if (!success) return;

        ShowPassPanel();
    }

    // --------------------------------------------------
    // SONG LIBRARY PANEL
    // --------------------------------------------------

    public void RefreshSongLibraryPanel()
    {
        if (songLibraryTitleText != null)
            songLibraryTitleText.text = "Choose a Song";

        if (songLibraryCountText != null)
            songLibraryCountText.text = "Songs Left: " + GameManager.Instance.songPool.Count;

        if (songLibraryListContainer == null || songLibraryButtonPrefab == null) return;

        ClearObjectContainer(songLibraryListContainer);

        foreach (string song in GameManager.Instance.songPool)
        {
            GameObject buttonObj = Instantiate(songLibraryButtonPrefab, songLibraryListContainer);

            TMP_Text textComp = buttonObj.GetComponentInChildren<TMP_Text>();
            if (textComp != null)
                textComp.text = song;

            Button buttonComp = buttonObj.GetComponent<Button>();
            if (buttonComp != null)
            {
                string capturedSong = song;
                buttonComp.onClick.RemoveAllListeners();
                buttonComp.onClick.AddListener(() => OnSongLibrarySongPressed(capturedSong));
            }
        }
    }

    public void OnSongLibrarySongPressed(string songTitle)
    {
        GameManager.Instance.SetPendingSong(songTitle);
        HidePopup(songLibraryPanel);
        RefreshClueSetupPanel();
    }

    // --------------------------------------------------
    // EMOJI PICKER PANEL
    // --------------------------------------------------

    public void RefreshEmojiPickerPanel()
    {
        if (emojiPickerTitleText != null)
            emojiPickerTitleText.text = "Choose Emoji Clues";

        if (emojiPickerCountText != null)
        {
            int current = GameManager.Instance.pendingEmojiClues.Count;
            int max = GameManager.Instance.MaxEmojisPerClue;
            emojiPickerCountText.text = $"Selected: {current} / {max}";
        }

        if (backspaceButton != null)
            backspaceButton.interactable = GameManager.Instance.pendingEmojiClues != null &&
                                           GameManager.Instance.pendingEmojiClues.Count > 0;

        RefreshEmojiOptionButtons();
        PopulateImageContainer(emojiPickerSelectedContainer, GameManager.Instance.pendingEmojiClues);
    }

    private void RefreshEmojiOptionButtons()
    {
        if (emojiPickerOptionsContainer == null || emojiButtonPrefab == null) return;

        ClearObjectContainer(emojiPickerOptionsContainer);

        List<Sprite> emojiOptions = GameManager.Instance.currentRoundEmojiOptions;
        if (emojiOptions == null) return;

        foreach (Sprite emojiSprite in emojiOptions)
        {
            if (emojiSprite == null) continue;

            GameObject buttonObj = Instantiate(emojiButtonPrefab, emojiPickerOptionsContainer);

            Image imageComp = buttonObj.GetComponent<Image>();
            if (imageComp != null)
            {
                imageComp.sprite = emojiSprite;
                imageComp.preserveAspect = true;
            }

            Button buttonComp = buttonObj.GetComponent<Button>();
            if (buttonComp != null)
            {
                Sprite capturedSprite = emojiSprite;
                buttonComp.interactable = GameManager.Instance.CanAddMorePendingEmojis();
                buttonComp.onClick.RemoveAllListeners();
                buttonComp.onClick.AddListener(() => OnEmojiOptionPressed(capturedSprite));
            }
        }
    }

    public void OnEmojiOptionPressed(Sprite emojiSprite)
    {
        bool added = GameManager.Instance.AddPendingEmoji(emojiSprite);

        if (!added)
        {
            Debug.Log("Max emoji limit reached!");
            return;
        }

        RefreshEmojiPickerPanel();
        RefreshClueSetupPanel();
    }

    public void OnConfirmEmojiPickerPressed()
    {
        HidePopup(emojiPickerPanel);
        RefreshClueSetupPanel();
    }

    // --------------------------------------------------
    // PASS PANEL
    // --------------------------------------------------

    public void ShowPassPanel()
    {
        ShowOnly(passPanel);

        if (passText != null)
            passText.text = GameManager.Instance.GetGuesserName();

        if (readyToGuessButton != null)
            StartButtonPulse(readyToGuessButton);
    }

    public void OnReadyToGuessPressed()
    {
        StopButtonPulse(readyToGuessButton);

        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.PlayBlackFade(() =>
            {
                GameManager.Instance.BeginGuessing();
                ShowGuessPanel();
            });
        }
        else
        {
            GameManager.Instance.BeginGuessing();
            ShowGuessPanel();
        }
    }

    // --------------------------------------------------
    // GUESS PANEL
    // --------------------------------------------------

    public void ShowGuessPanel()
    {
        ShowOnly(guessPanel);

        selectedGuessSong = "";

        if (guessTurnText != null)
            guessTurnText.text = GameManager.Instance.GetGuesserName() + ", guess the song";

        if (selectedGuessText != null)
            selectedGuessText.text = "No song selected";

        if (submitGuessButton != null)
        {
            submitGuessButton.interactable = false;
            StopButtonPulse(submitGuessButton);
        }

        PopulateAnimatedImageContainer(guessClueDisplayArea, GameManager.Instance.currentEmojiClues);
        RefreshGuessSongList();
        RefreshGuessIcons();
    }

    public void RefreshGuessSongList()
    {
        if (guessSongListContainer == null || guessSongButtonPrefab == null) return;

        ClearObjectContainer(guessSongListContainer);

        foreach (string song in GameManager.Instance.songPool)
        {
            GameObject buttonObj = Instantiate(guessSongButtonPrefab, guessSongListContainer);

            TMP_Text textComp = buttonObj.GetComponentInChildren<TMP_Text>();
            if (textComp != null)
                textComp.text = song;

            Button buttonComp = buttonObj.GetComponent<Button>();
            if (buttonComp != null)
            {
                string capturedSong = song;
                buttonComp.onClick.RemoveAllListeners();
                buttonComp.onClick.AddListener(() => OnGuessSongSelected(capturedSong));
            }
        }
    }

    public void OnGuessSongSelected(string songTitle)
    {
        selectedGuessSong = songTitle;

        if (selectedGuessText != null)
            selectedGuessText.text = selectedGuessSong;

        if (submitGuessButton != null)
        {
            submitGuessButton.interactable = true;
            StartButtonPulse(submitGuessButton);
        }
    }

    public void OnSubmitGuessPressed()
    {
        if (string.IsNullOrWhiteSpace(selectedGuessSong)) return;

        StopButtonPulse(submitGuessButton);

        GameManager.Instance.SubmitGuess(selectedGuessSong);

        if (GameManager.Instance != null && GameManager.Instance.currentPhase == RoundPhase.Guessing)
        {
            selectedGuessSong = "";

            if (selectedGuessText != null)
                selectedGuessText.text = "No song selected";

            if (submitGuessButton != null)
            {
                submitGuessButton.interactable = false;
                StopButtonPulse(submitGuessButton);
            }
        }
    }

    public void OnSkipGuessPressed()
    {
        StopButtonPulse(submitGuessButton);
        GameManager.Instance.SkipGuess();
    }

    public void RefreshGuessIcons()
    {
        if (GameManager.Instance == null) return;

        int remaining = GameManager.Instance.GuessesRemaining;

        SetGuessIcon(guessOneIcon, remaining >= 1);
        SetGuessIcon(guessTwoIcon, remaining >= 2);
    }

    private void SetGuessIcon(Image icon, bool active)
    {
        if (icon == null) return;

        Color c = icon.color;
        c.a = active ? activeGuessAlpha : usedGuessAlpha;
        icon.color = c;
    }

    // --------------------------------------------------
    // RESULT PANEL
    // --------------------------------------------------

    private void SetRoundResultButtons()
    {
        if (nextRoundButton != null) nextRoundButton.SetActive(true);
        if (playAgainButton != null) playAgainButton.SetActive(false);
        if (mainMenuFromResultsButton != null) mainMenuFromResultsButton.SetActive(false);
    }

    private void SetFinalResultButtons()
    {
        if (nextRoundButton != null) nextRoundButton.SetActive(false);
        if (playAgainButton != null) playAgainButton.SetActive(true);
        if (mainMenuFromResultsButton != null) mainMenuFromResultsButton.SetActive(true);
    }

    public void ShowResult(bool correct, string answer, PlayerData p1, PlayerData p2, int pointsEarned = 0)
    {
        ShowOnly(resultPanel);
        SetRoundResultButtons();

        if (resultText != null)
        {
            if (correct)
                resultText.text = $"Correct! The answer was: {answer}\n+{pointsEarned} point(s)";
            else
                resultText.text = "Wrong! The answer was: " + answer;
        }

        if (scoreText != null)
        {
            scoreText.text = p1.playerName + ": " + p1.score + "\n" +
                             p2.playerName + ": " + p2.score;
        }
    }

    public void ShowFinalResults(PlayerData p1, PlayerData p2)
    {
        ShowOnly(resultPanel);
        SetFinalResultButtons();

        if (resultText != null)
        {
            if (p1.score > p2.score)
                resultText.text = "Final Results\n\n" + p1.playerName + " wins!";
            else if (p2.score > p1.score)
                resultText.text = "Final Results\n\n" + p2.playerName + " wins!";
            else
                resultText.text = "Final Results\n\nIt's a tie!";
        }

        if (scoreText != null)
        {
            scoreText.text = p1.playerName + ": " + p1.score + "\n" +
                             p2.playerName + ": " + p2.score;
        }

        if (GameManager.Instance != null)
            GameManager.Instance.currentPhase = RoundPhase.Result;
    }

    public void OnNextRoundPressed()
    {
        if (GameManager.Instance == null) return;

        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.PlayBlackFade(() =>
            {
                if (GameManager.Instance.IsMatchOver())
                {
                    ShowFinalResults(GameManager.Instance.player1, GameManager.Instance.player2);
                }
                else
                {
                    GameManager.Instance.NextRound();
                }
            });
        }
        else
        {
            if (GameManager.Instance.IsMatchOver())
            {
                ShowFinalResults(GameManager.Instance.player1, GameManager.Instance.player2);
            }
            else
            {
                GameManager.Instance.NextRound();
            }
        }
    }

    public void OnPlayAgainPressed()
    {
        if (GameManager.Instance == null) return;

        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.PlayBlackFade(() =>
            {
                GameManager.Instance.ResetGame();
                ShowStartPanel();
            });
        }
        else
        {
            GameManager.Instance.ResetGame();
            ShowStartPanel();
        }
    }

    public void OnResultsMainMenuPressed()
    {
        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.PlayBlackFade(() =>
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.ResetGame();

                ShowMainMenu();
            });
        }
        else
        {
            if (GameManager.Instance != null)
                GameManager.Instance.ResetGame();

            ShowMainMenu();
        }
    }
    // --------------------------------------------------
    // PAUSE
    // --------------------------------------------------

    public void TogglePause()
    {
        if (pauseVisible)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        if (pausePanel == null) return;

        pauseVisible = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;

        if (pauseTitleText != null)
            pauseTitleText.text = "Paused";
    }

    public void ResumeGame()
    {
        if (pausePanel == null) return;

        pauseVisible = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OnResumePressed()
    {
        ResumeGame();
    }

    public void OnPauseHowToPlayPressed()
    {
        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(true);
    }

    public void OnExitToMainMenuPressed()
    {
        Time.timeScale = 1f;
        pauseVisible = false;
        ShowMainMenu();
    }

    public void OnExitToDesktopPressed()
    {
        Application.Quit();
    }

    // --------------------------------------------------
    // KEYBOARD INPUT
    // --------------------------------------------------

    private void HandleKeyboardInput()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (CanPauseCurrentScreen())
                TogglePause();
        }

        if (Keyboard.current.enterKey.wasPressedThisFrame ||
            Keyboard.current.numpadEnterKey.wasPressedThisFrame)
        {
            if (TMP_InputFieldFocused()) return;

            HandleEnterPressed();
        }
    }

    private void HandleEnterPressed()
    {
        if (pauseVisible)
            return;

        switch (GameManager.Instance.currentPhase)
        {
            case RoundPhase.MainMenu:
                OnMainMenuStartPressed();
                break;

            case RoundPhase.PlayerSetup:
                OnBeginPlayerSetupPressed();
                break;

            case RoundPhase.SongDraft:
                OnAddDraftSongPressed();
                break;

            case RoundPhase.LookAway:
                OnContinueToClueSetupPressed();
                break;

            case RoundPhase.RoundSetup:
                OnConfirmRoundPressed();
                break;

            case RoundPhase.PassToGuesser:
                OnReadyToGuessPressed();
                break;

            case RoundPhase.Guessing:
                OnSubmitGuessPressed();
                break;

            case RoundPhase.Result:
                OnNextRoundPressed();
                break;
        }
    }

    private bool TMP_InputFieldFocused()
    {
        if (player1Input != null && player1Input.isFocused) return true;
        if (player2Input != null && player2Input.isFocused) return true;
        if (draftSongInput != null && draftSongInput.isFocused) return true;

        return false;
    }

    private bool CanPauseCurrentScreen()
    {
        if (GameManager.Instance == null) return false;

        switch (GameManager.Instance.currentPhase)
        {
            case RoundPhase.SongDraft:
            case RoundPhase.LookAway:
            case RoundPhase.RoundSetup:
            case RoundPhase.PassToGuesser:
            case RoundPhase.Guessing:
            case RoundPhase.Result:
                return true;

            default:
                return false;
        }
    }

    // --------------------------------------------------
    // TIMER DISPLAY
    // --------------------------------------------------

    public void UpdateTimerDisplay(int secondsLeft)
    {
        string formatted = $"{secondsLeft / 60}:{secondsLeft % 60:00}";

        if (clueSetupTimerText != null)
            clueSetupTimerText.text = formatted;

        if (chooseSongTimerText != null)
            chooseSongTimerText.text = formatted;

        if (chooseEmojiTimerText != null)
            chooseEmojiTimerText.text = formatted;

        if (guessTimerText != null)
            guessTimerText.text = formatted;
    }
}