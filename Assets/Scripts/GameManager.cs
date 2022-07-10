using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    // General
    public bool gameStarted = false;
    public bool gameOver = false;
    //public TextMeshProUGUI pressSpaceText;
    public GameObject infoPanel;
    public GameObject finishPanel;

    // Score
    [SerializeField] private int scorePerSecondRemaining = 50;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI finishScoreText;
    private int score;
    private int fullScore;

    // Timer
    public float timeRemaining;
    public TextMeshProUGUI timeRemainingText;
    public bool timerIsRunning = false;

    // Checkpoint
    public string p1LastCheckpoint;

    // Save score
    [SerializeField] public GameObject highScoresTable;
    [SerializeField] public GameObject inputField;
    private HighScoreTable highScoreTable;

    void Start()
    {
        score = 0;
        scoreText.text = "" + score;
        // timerIsRunning = true;
        highScoreTable = highScoresTable.GetComponent<HighScoreTable>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            gameStarted = true;
            infoPanel.SetActive(false);
            scoreText.enabled = true;
            timeRemainingText.enabled = true;
        }

        DisplayTime(timeRemaining);

        // Countdown
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
            }
            else
            {
                Debug.Log("Time has run out!");
                timeRemaining = 0;
                timerIsRunning = false;
                gameOver = true;
                Finish();
            }
        }
    }

    public void UpdateScore(int scoreToAdd)
    {
        score += scoreToAdd;
        scoreText.text = "" + score;
    }

    public void AddTime(float timeToAdd)
    {
        timeRemaining += timeToAdd;
    }

    private void DisplayTime(float timeToDisplay)
    {
        int seconds = Mathf.FloorToInt(timeToDisplay);
        timeRemainingText.text = seconds.ToString();
    }

    public void Finish()
    {
        fullScore = (Mathf.FloorToInt(timeRemaining) * scorePerSecondRemaining);
        fullScore += score;

        timerIsRunning = false;
        gameStarted = false;
        scoreText.enabled = false;
        timeRemainingText.enabled = false;

        finishPanel.SetActive(true);
        finishScoreText.text = "" + fullScore;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void MainMenuButton()
    {
        SceneManager.LoadScene(0);
    }

    public void SaveButton()
    {
        string playerName = inputField.GetComponent<TMP_InputField>().text;
        highScoreTable.AddHighScoreEntry(fullScore, playerName.ToUpper());
    }
}
