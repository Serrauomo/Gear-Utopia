using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;
    
    [Header("In-Game UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highscoreText;
    
    [Header("Game Over UI")]
    public TextMeshProUGUI gameOverScoreText;
    public TextMeshProUGUI gameOverHighscoreText;
    
    int score = 0;
    int highscore = 0;
    
    private void Awake()
    {
        instance = this;
    }
    
    void Start()
    {
        highscore = PlayerPrefs.GetInt("highscore", 0);
        UpdateUI();
    }
    
    public void AddPoint()
    {
        score += 1;
        UpdateUI();
        
        if(highscore < score)
        {
            highscore = score;
            PlayerPrefs.SetInt("highscore", score);
        }
    }
    
    private void UpdateUI()
    {
        // Update in-game UI
        if(scoreText != null)
            scoreText.text = score.ToString() + " POINTS";
        if(highscoreText != null)
            highscoreText.text = "HIGHSCORE: " + highscore.ToString();
            
        // Update game over UI (if references exist)
        if(gameOverScoreText != null)
            gameOverScoreText.text = score.ToString() + " POINTS";
        if(gameOverHighscoreText != null)
            gameOverHighscoreText.text = "HIGHSCORE: " + highscore.ToString();
    }
    
    // Method to call when game over is triggered
    public void UpdateGameOverUI()
    {
        if(gameOverScoreText != null)
            gameOverScoreText.text = score.ToString() + " POINTS";
        if(gameOverHighscoreText != null)
            gameOverHighscoreText.text = "HIGHSCORE: " + highscore.ToString();
    }
}