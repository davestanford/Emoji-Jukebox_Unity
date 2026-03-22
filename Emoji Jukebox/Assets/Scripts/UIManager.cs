using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Panels")]
    public GameObject startPanel;
    public GameObject clueSetupPanel;
    public GameObject passPanel;
    public GameObject guessPanel;
    public GameObject resultPanel;

    [Header("Start Panel")]
    public TMP_InputField player1Input;
    public TMP_InputField player2Input;

    [Header("Clue Setup Panel")]
    public TMP_Text turnText;
    public TMP_Text instructionText;
    public TMP_InputField songAnswerInput;
    public Transform currentClueDisplayArea;
    public GameObject clueImagePrefab;

    [Header("Pass Panel")]
    public TMP_Text passText;

    [Header("Guess Panel")]
    public TMP_Text guessTurnText;
    public Transform guessClueDisplayArea;
    public TMP_InputField guessInput;

    [Header("Result Panel")]
    public TMP_Text resultText;
    public TMP_Text scoreText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        ShowOnly(startPanel);
    }

    void ShowOnly(GameObject panelToShow)
    {
        startPanel.SetActive(false);
        clueSetupPanel.SetActive(false);
        passPanel.SetActive(false);
        guessPanel.SetActive(false);
        resultPanel.SetActive(false);

        panelToShow.SetActive(true);
    }

    void ClearImageContainer(Transform container)
    {
        if (container == null) return;

        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Destroy(container.GetChild(i).gameObject);
        }
    }

    void PopulateImageContainer(Transform container, List<Sprite> clues)
    {
        if (container == null || clueImagePrefab == null) return;

        ClearImageContainer(container);

        foreach (Sprite clueSprite in clues)
        {
            GameObject newImageObj = Instantiate(clueImagePrefab, container);
            Image imageComp = newImageObj.GetComponent<Image>();

            if (imageComp != null)
            {
                imageComp.sprite = clueSprite;
                imageComp.preserveAspect = true;
            }
        }
    }

    public void OnStartGamePressed()
    {
        GameManager.Instance.StartGame(player1Input.text, player2Input.text);
    }

    public void ShowClueSetup()
    {
        ShowOnly(clueSetupPanel);

        var giver = GameManager.Instance.GetClueGiver();
        turnText.text = giver.playerName + "'s Turn";
        instructionText.text = "Enter a song and choose emoji clues.";
        songAnswerInput.text = "";
        ClearImageContainer(currentClueDisplayArea);
    }

    public void UpdateClueDisplay(List<Sprite> clues)
    {
        PopulateImageContainer(currentClueDisplayArea, clues);
    }

    public void OnDoneCluesPressed()
    {
        string answer = songAnswerInput.text;

        if (string.IsNullOrWhiteSpace(answer))
            return;

        if (GameManager.Instance.currentEmojiClues.Count == 0)
            return;

        GameManager.Instance.FinishClueSetup(answer);
    }

    public void OnClearCluesPressed()
    {
        GameManager.Instance.ClearClues();
    }

    public void ShowPassDevice()
    {
        ShowOnly(passPanel);

        var guesser = GameManager.Instance.GetGuesser();
        passText.text = "Pass the device to " + guesser.playerName + ".";
    }

    public void OnReadyPressed()
    {
        GameManager.Instance.BeginGuessing();
    }

    public void ShowGuessPanel()
    {
        ShowOnly(guessPanel);

        var guesser = GameManager.Instance.GetGuesser();
        guessTurnText.text = guesser.playerName + ", guess the song!";
        PopulateImageContainer(guessClueDisplayArea, GameManager.Instance.currentEmojiClues);
        guessInput.text = "";
    }

    public void OnSubmitGuessPressed()
    {
        if (string.IsNullOrWhiteSpace(guessInput.text))
            return;

        GameManager.Instance.SubmitGuess(guessInput.text);
    }

    public void ShowResult(bool correct, string answer, PlayerData p1, PlayerData p2)
    {
        ShowOnly(resultPanel);

        resultText.text = correct
            ? "Correct! The answer was: " + answer
            : "Wrong! The answer was: " + answer;

        scoreText.text = p1.playerName + ": " + p1.score + "\n" +
                         p2.playerName + ": " + p2.score;
    }

    public void OnNextRoundPressed()
    {
        GameManager.Instance.NextRound();
    }
    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.enterKey.wasPressedThisFrame ||
            Keyboard.current.numpadEnterKey.wasPressedThisFrame)
        {
            if (TMP_InputFieldFocused()) return;

            HandleEnterPressed();
        }
    }
    void HandleEnterPressed()
    {
        switch (GameManager.Instance.currentPhase)
        {
            case RoundPhase.Start:
                OnStartGamePressed();
                break;

            case RoundPhase.ClueSetup:
                OnDoneCluesPressed();
                break;

            case RoundPhase.PassDevice:
                OnReadyPressed();
                break;

            case RoundPhase.Guessing:
                OnSubmitGuessPressed();
                break;

            case RoundPhase.Result:
                OnNextRoundPressed();
                break;
        }
    }

    bool TMP_InputFieldFocused()
    {
        if (player1Input != null && player1Input.isFocused) return true;
        if (player2Input != null && player2Input.isFocused) return true;
        if (songAnswerInput != null && songAnswerInput.isFocused) return true;
        if (guessInput != null && guessInput.isFocused) return true;

        return false;
    }
}