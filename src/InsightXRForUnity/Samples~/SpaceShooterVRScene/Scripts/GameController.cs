using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameController : MonoBehaviour
{
    [SerializeField] private Image timerImage;
    [SerializeField] private float gameTime;

    [Header("Score Components")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Game Over Components")]
    [SerializeField] private GameObject gameOverScreen;

    [Header("Gameplay Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] gameplayAudio;

    private int playerScore;

    public enum GameState
    {
        Waiting,
        Playing,
        GameOver
    }

    public static GameState currentGameStatus;

    private void Awake()
    {
        currentGameStatus = GameState.Waiting;
    }

    private float sliderCurrentFillAmount = 1f;

    private void Update()
    {
        if(currentGameStatus == GameState.Playing)
        {
            AdjustTimer();
        }
    }

    private void AdjustTimer()
    {
        timerImage.fillAmount = sliderCurrentFillAmount - (Time.deltaTime / gameTime);

        sliderCurrentFillAmount = timerImage.fillAmount;

        if(sliderCurrentFillAmount <= 0f)
        {
            GameOver();
        }
    }

    public void UpdatePlayerScore(int asteroidHitPoints)
    {
        if (currentGameStatus != GameState.Playing)
            return;

        playerScore += asteroidHitPoints;
        scoreText.text = playerScore.ToString();
    }

    public void StartGame()
    {
        currentGameStatus = GameState.Playing;
        audioSource.clip = gameplayAudio[1];
        audioSource.Play();
    }

    private void GameOver()
    {
        currentGameStatus = GameState.GameOver;

        //show the game over screen
        gameOverScreen.SetActive(true);

        audioSource.clip = gameplayAudio[2];
        audioSource.Play();
        audioSource.loop = false;
    }

    public void ResetGame()
    {
        currentGameStatus = GameState.Waiting;

        //set timer to 1
        sliderCurrentFillAmount = 1;
        timerImage.fillAmount = 1f;

        //reset score
        playerScore = 0;
        scoreText.text = "0";

        //play intro music
        audioSource.clip = gameplayAudio[0];
        audioSource.Play();
        audioSource.loop = true;
    }

}
