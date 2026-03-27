using System.Collections.Generic;
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

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
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

        currentSong = "";
        currentEmojiClues.Clear();
        pendingSong = "";
        pendingEmojiClues.Clear();
        currentRoundEmojiOptions.Clear();

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

        GenerateRandomEmojiOptions(20);

        currentPhase = RoundPhase.LookAway;
    }

    public void BeginClueSetup()
    {
        currentPhase = RoundPhase.RoundSetup;
    }

    public void SetPendingSong(string song)
    {
        pendingSong = song;
    }

    public void ClearPendingSong()
    {
        pendingSong = "";
    }

    public void AddPendingEmoji(Sprite emoji)
    {
        if (emoji == null) return;

        pendingEmojiClues.Add(emoji);
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

        currentPhase = RoundPhase.PassToGuesser;
        return true;
    }

    // --------------------------------------------------
    // EMOJI SYSTEM
    // --------------------------------------------------

    public void GenerateRandomEmojiOptions(int count)
    {
        currentRoundEmojiOptions.Clear();

        List<Sprite> temp = new List<Sprite>(masterEmojiLibrary);

        for (int i = 0; i < count && temp.Count > 0; i++)
        {
            int rand = Random.Range(0, temp.Count);
            currentRoundEmojiOptions.Add(temp[rand]);
            temp.RemoveAt(rand);
        }
    }

    // --------------------------------------------------
    // GUESS PHASE
    // --------------------------------------------------

    public void BeginGuessing()
    {
        currentPhase = RoundPhase.Guessing;
    }

    public void SubmitGuess(string guess)
    {
        bool correct = string.Equals(
            guess.Trim(),
            currentSong.Trim(),
            System.StringComparison.OrdinalIgnoreCase
        );

        if (correct)
        {
            GetGuesser().score += 1;
        }

        songPool.Remove(currentSong);

        currentPhase = RoundPhase.Result;
        UIManager.Instance.ShowResult(correct, currentSong, player1, player2);
    }

    public void SkipGuess()
    {
        GetClueGiver().score += 1;

        songPool.Remove(currentSong);

        currentPhase = RoundPhase.Result;
        UIManager.Instance.ShowResult(false, currentSong, player1, player2);
    }

    // --------------------------------------------------
    // MATCH STATE (NEW)
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

        currentPhase = RoundPhase.MainMenu;
    }

    // --------------------------------------------------
    // NEXT ROUND (UPDATED)
    // --------------------------------------------------

    public void NextRound()
    {
        currentClueGiverIndex = currentClueGiverIndex == 0 ? 1 : 0;

        // 🔥 THIS IS THE FIX
        if (IsMatchOver())
        {
            UIManager.Instance.ShowFinalResults(player1, player2);
            return;
        }

        StartRoundSetup();
        UIManager.Instance.ShowLookAwayPanel();
    }
}