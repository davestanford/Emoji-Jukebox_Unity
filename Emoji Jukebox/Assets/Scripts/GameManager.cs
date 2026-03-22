using System.Collections.Generic;
using UnityEngine;

public enum RoundPhase
{
    Start,
    ClueSetup,
    PassDevice,
    Guessing,
    Result
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Players")]
    public PlayerData player1;
    public PlayerData player2;

    [Header("Round State")]
    public int currentClueGiverIndex = 0;
    public string currentSongAnswer = "";
    public List<Sprite> currentEmojiClues = new List<Sprite>();
    public RoundPhase currentPhase = RoundPhase.Start;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void StartGame(string p1Name, string p2Name)
    {
        if (string.IsNullOrWhiteSpace(p1Name)) p1Name = "Player 1";
        if (string.IsNullOrWhiteSpace(p2Name)) p2Name = "Player 2";

        player1 = new PlayerData(p1Name);
        player2 = new PlayerData(p2Name);

        BeginClueSetup();
    }

    public PlayerData GetClueGiver()
    {
        return currentClueGiverIndex == 0 ? player1 : player2;
    }

    public PlayerData GetGuesser()
    {
        return currentClueGiverIndex == 0 ? player2 : player1;
    }

    public void BeginClueSetup()
    {
        currentSongAnswer = "";
        currentEmojiClues.Clear();
        currentPhase = RoundPhase.ClueSetup;
        UIManager.Instance.ShowClueSetup();
    }

    public void AddEmojiClue(Sprite emojiSprite)
    {
        if (emojiSprite == null) return;

        currentEmojiClues.Add(emojiSprite);
        UIManager.Instance.UpdateClueDisplay(currentEmojiClues);
    }

    public void ClearClues()
    {
        currentEmojiClues.Clear();
        UIManager.Instance.UpdateClueDisplay(currentEmojiClues);
    }

    public void FinishClueSetup(string songAnswer)
    {
        currentSongAnswer = songAnswer.Trim();
        currentPhase = RoundPhase.PassDevice;
        UIManager.Instance.ShowPassDevice();
    }

    public void BeginGuessing()
    {
        currentPhase = RoundPhase.Guessing;
        UIManager.Instance.ShowGuessPanel();
    }

    public void SubmitGuess(string guess)
    {
        bool correct = string.Equals(
            guess.Trim(),
            currentSongAnswer.Trim(),
            System.StringComparison.OrdinalIgnoreCase
        );

        if (correct)
        {
            GetGuesser().score += 1;
        }

        currentPhase = RoundPhase.Result;
        UIManager.Instance.ShowResult(correct, currentSongAnswer, player1, player2);
    }

    public void NextRound()
    {
        currentClueGiverIndex = currentClueGiverIndex == 0 ? 1 : 0;
        BeginClueSetup();
    }
}