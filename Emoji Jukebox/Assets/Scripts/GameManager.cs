using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public enum RoundPhase
{
    MainMenu,
    PlayerSetup,
    SongDraft,
    LookAway,
    RoundSetup,
    PassToGuesser,
    Guessing,
    Result
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Players")]
    public PlayerData player1;
    public PlayerData player2;

    [Header("Phase")]
    public RoundPhase currentPhase = RoundPhase.MainMenu;

    [Header("Song Draft")]
    public List<string> songPool = new List<string>();
    public int maxSongsInPool = 10;
    public int currentDraftPlayerIndex = 0;

    [Header("Turn State")]
    public int currentClueGiverIndex = 0;

    [Header("Round Data")]
    public string currentSong = "";
    public List<Sprite> currentEmojiClues = new List<Sprite>();

    [Header("Pending (Round Setup)")]
    public string pendingSong = "";
    public List<Sprite> pendingEmojiClues = new List<Sprite>();

    [Header("Emoji System")]
    public List<Sprite> masterEmojiLibrary = new List<Sprite>();
    public List<Sprite> currentRoundEmojiOptions = new List<Sprite>();

    [Header("Emoji Options")]
    [SerializeField] private int emojisPerRound = 50;
    [SerializeField] private int maxEmojisPerClue = 3;

    [Header("Timer Settings")]
    [SerializeField] private float roundSetupTime = 30f;
    [SerializeField] private float guessTime = 20f;

    [Header("Guess Settings")]
    [SerializeField] private int maxGuessesPerRound = 2;

    private float currentTimer = 0f;
    private bool timerRunning = false;
    private int guessesRemaining = 0;

    public float CurrentTimer => currentTimer;
    public bool TimerRunning => timerRunning;
    public int MaxEmojisPerClue => maxEmojisPerClue;
    public int EmojisPerRound => emojisPerRound;
    public int MaxGuessesPerRound => maxGuessesPerRound;
    public int GuessesRemaining => guessesRemaining;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Update()
    {
        HandleTimer();
    }

    private void OnValidate()
    {
        if (emojisPerRound < 1)
            emojisPerRound = 1;

        if (maxEmojisPerClue < 1)
            maxEmojisPerClue = 1;

        if (roundSetupTime < 1f)
            roundSetupTime = 1f;

        if (guessTime < 1f)
            guessTime = 1f;

        if (maxGuessesPerRound < 1)
            maxGuessesPerRound = 1;
    }

    private void HandleTimer()
    {
        if (!timerRunning)
            return;

        currentTimer -= Time.deltaTime;

        if (currentTimer < 0f)
            currentTimer = 0f;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateTimerDisplay(Mathf.CeilToInt(currentTimer));
        }

        if (currentTimer <= 0f)
        {
            timerRunning = false;
            OnTimerExpired();
        }
    }

    public void StartTimer(float seconds)
    {
        currentTimer = seconds;
        timerRunning = true;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateTimerDisplay(Mathf.CeilToInt(currentTimer));
        }
    }

    public void StopTimer()
    {
        timerRunning = false;
    }

    private void OnTimerExpired()
    {
        switch (currentPhase)
        {
            case RoundPhase.RoundSetup:
                FailRoundSetupFromTimer();
                break;

            case RoundPhase.Guessing:
                SkipGuessFromTimer();
                break;
        }
    }

    private void FailRoundSetupFromTimer()
    {
        StopTimer();
        currentPhase = RoundPhase.Result;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowResult(false, "Time ran out", player1, player2);
        }
    }

    private void SkipGuessFromTimer()
    {
        StopTimer();

        // 🔴 TIMER FAILURE FLASH
        if (TransitionManager.Instance != null)
            TransitionManager.Instance.PlayRedFlash(0.08f, 0.3f);

        GetClueGiver().score += 1;
        songPool.Remove(currentSong);

        currentPhase = RoundPhase.Result;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowResult(false, currentSong, player1, player2);
        }
    }

    // --------------------------------------------------
    // GAME START
    // --------------------------------------------------

    public void StartGame(string p1Name, string p2Name)
    {
        if (string.IsNullOrWhiteSpace(p1Name)) p1Name = "Player 1";
        if (string.IsNullOrWhiteSpace(p2Name)) p2Name = "Player 2";

        player1 = new PlayerData(p1Name);
        player2 = new PlayerData(p2Name);

        songPool.Clear();
        currentDraftPlayerIndex = 0;
        currentClueGiverIndex = 0;
        guessesRemaining = 0;

        currentSong = "";
        currentEmojiClues.Clear();
        pendingSong = "";
        pendingEmojiClues.Clear();
        currentRoundEmojiOptions.Clear();

        StopTimer();

        currentPhase = RoundPhase.SongDraft;
    }

    // --------------------------------------------------
    // PLAYER HELPERS
    // --------------------------------------------------

    public PlayerData GetClueGiver()
    {
        return currentClueGiverIndex == 0 ? player1 : player2;
    }

    public PlayerData GetGuesser()
    {
        return currentClueGiverIndex == 0 ? player2 : player1;
    }

    public string GetCurrentDraftPlayerName()
    {
        return currentDraftPlayerIndex == 0 ? player1.playerName : player2.playerName;
    }

    public string GetCurrentTurnPlayerName()
    {
        return GetClueGiver().playerName;
    }

    public string GetGuesserName()
    {
        return GetGuesser().playerName;
    }

    // --------------------------------------------------
    // SONG DRAFT SYSTEM
    // --------------------------------------------------

    public bool AddSongToPool(string songTitle)
    {
        if (string.IsNullOrWhiteSpace(songTitle)) return false;
        if (songPool.Count >= maxSongsInPool) return false;

        string cleaned = songTitle.Trim();

        foreach (string song in songPool)
        {
            if (song.ToLower() == cleaned.ToLower())
                return false;
        }

        songPool.Add(cleaned);
        AdvanceDraftTurn();
        return true;
    }

    public void SkipSongDraftTurn()
    {
        AdvanceDraftTurn();
    }

    private void AdvanceDraftTurn()
    {
        currentDraftPlayerIndex = currentDraftPlayerIndex == 0 ? 1 : 0;
    }

    public bool IsSongPoolFull()
    {
        return songPool.Count >= maxSongsInPool;
    }

    // --------------------------------------------------
    // ROUND SETUP
    // --------------------------------------------------

    public void StartRoundSetup()
    {
        pendingSong = "";
        pendingEmojiClues.Clear();

        currentSong = "";
        currentEmojiClues.Clear();

        GenerateRandomEmojiOptions(emojisPerRound);

        StopTimer();
        currentPhase = RoundPhase.LookAway;
    }

    public void BeginClueSetup()
    {
        currentPhase = RoundPhase.RoundSetup;
        StartTimer(roundSetupTime);
    }

    public void SetPendingSong(string song)
    {
        pendingSong = song;
    }

    public void ClearPendingSong()
    {
        pendingSong = "";
    }

    public bool AddPendingEmoji(Sprite emoji)
    {
        if (emoji == null) return false;

        if (pendingEmojiClues.Count >= maxEmojisPerClue)
        {
            Debug.Log($"Max emojis reached. Limit is {maxEmojisPerClue}.");
            return false;
        }

        pendingEmojiClues.Add(emoji);
        return true;
    }

    public bool CanAddMorePendingEmojis()
    {
        return pendingEmojiClues.Count < maxEmojisPerClue;
    }

    public void ClearPendingEmojis()
    {
        pendingEmojiClues.Clear();
    }

    public bool ConfirmRoundSetup()
    {
        if (string.IsNullOrWhiteSpace(pendingSong)) return false;
        if (pendingEmojiClues.Count == 0) return false;
        if (!songPool.Contains(pendingSong)) return false;

        currentSong = pendingSong;
        currentEmojiClues = new List<Sprite>(pendingEmojiClues);

        StopTimer();
        currentPhase = RoundPhase.PassToGuesser;
        return true;
    }

    // --------------------------------------------------
    // EMOJI SYSTEM
    // --------------------------------------------------

    public void GenerateRandomEmojiOptions(int count)
    {
        currentRoundEmojiOptions.Clear();

        if (masterEmojiLibrary == null || masterEmojiLibrary.Count == 0)
        {
            Debug.LogWarning("Emoji library is empty!");
            return;
        }

        count = Mathf.Min(count, masterEmojiLibrary.Count);

        List<Sprite> shuffled = new List<Sprite>(masterEmojiLibrary);

        for (int i = 0; i < shuffled.Count; i++)
        {
            int rand = Random.Range(i, shuffled.Count);
            (shuffled[i], shuffled[rand]) = (shuffled[rand], shuffled[i]);
        }

        for (int i = 0; i < count; i++)
        {
            currentRoundEmojiOptions.Add(shuffled[i]);
        }
    }

    public void RemoveLastEmoji()
    {
        if (pendingEmojiClues == null || pendingEmojiClues.Count == 0)
        {
            Debug.Log("No emojis to remove");
            return;
        }

        pendingEmojiClues.RemoveAt(pendingEmojiClues.Count - 1);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.RefreshClueSetupPanel();
            UIManager.Instance.RefreshEmojiPickerPanel();
        }
    }

    // --------------------------------------------------
    // GUESS PHASE
    // --------------------------------------------------

    public void BeginGuessing()
    {
        currentPhase = RoundPhase.Guessing;
        guessesRemaining = maxGuessesPerRound;
        StartTimer(guessTime);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.RefreshGuessIcons();
        }
    }

    public void SubmitGuess(string guess)
    {
        if (string.IsNullOrWhiteSpace(guess))
            return;

        bool correct = string.Equals(
            guess.Trim(),
            currentSong.Trim(),
            System.StringComparison.OrdinalIgnoreCase
        );

        if (correct)
        {
            StopTimer();

            if (TransitionManager.Instance != null)
                TransitionManager.Instance.PlayWhiteFlash();

            // 📳 SCREEN SHAKE
            if (ScreenShakeManager.Instance != null)
                ScreenShakeManager.Instance.Shake(0.1f, 20f);

            int pointsAwarded = guessesRemaining == maxGuessesPerRound ? 2 : 1;
            GetGuesser().score += pointsAwarded;

            songPool.Remove(currentSong);
            currentPhase = RoundPhase.Result;

            StartCoroutine(ShowCorrectResultDelayed(pointsAwarded));
            return;
        }

        guessesRemaining--;

        if (guessesRemaining > 1)
        {
            if (TransitionManager.Instance != null)
                TransitionManager.Instance.PlayRedFlash();
        }
        else if (guessesRemaining == 1)
        {
            if (TransitionManager.Instance != null)
                TransitionManager.Instance.PlayRedFlash(0.06f, 0.25f);
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.RefreshGuessIcons();
        }

        if (guessesRemaining <= 0)
        {
            StopTimer();

            if (TransitionManager.Instance != null)
                TransitionManager.Instance.PlayRedFlash(0.08f, 0.3f);

            songPool.Remove(currentSong);
            currentPhase = RoundPhase.Result;

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowResult(false, currentSong, player1, player2);
            }
        }
    }

    private IEnumerator ShowCorrectResultDelayed(int pointsAwarded)
    {
        yield return new WaitForSecondsRealtime(0.1f);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowResult(true, currentSong, player1, player2, pointsAwarded);
        }
    }



    public void SkipGuess()
    {
        StopTimer();

        GetClueGiver().score += 1;
        songPool.Remove(currentSong);

        currentPhase = RoundPhase.Result;
        UIManager.Instance.ShowResult(false, currentSong, player1, player2);
    }

    // --------------------------------------------------
    // MATCH STATE
    // --------------------------------------------------

    public bool IsMatchOver()
    {
        return songPool.Count == 0;
    }

    public void ResetGame()
    {
        player1.score = 0;
        player2.score = 0;

        player1.playerName = "";
        player2.playerName = "";

        songPool.Clear();

        currentSong = "";
        currentEmojiClues.Clear();
        pendingSong = "";
        pendingEmojiClues.Clear();
        currentRoundEmojiOptions.Clear();

        currentDraftPlayerIndex = 0;
        currentClueGiverIndex = 0;
        guessesRemaining = 0;

        StopTimer();

        currentPhase = RoundPhase.MainMenu;
    }

    // --------------------------------------------------
    // NEXT ROUND
    // --------------------------------------------------

    public void NextRound()
    {
        currentClueGiverIndex = currentClueGiverIndex == 0 ? 1 : 0;

        if (IsMatchOver())
        {
            UIManager.Instance.ShowFinalResults(player1, player2);
            return;
        }

        StartRoundSetup();
        UIManager.Instance.ShowLookAwayPanel();
    }
}