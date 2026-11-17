using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private static ScoreManager instance;

    public static ScoreManager Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<ScoreManager>();
            if (instance == null) instance = new GameObject("ScoreManager").AddComponent<ScoreManager>();
            return instance;
        }
    }

    [field: SerializeField] public int Score { get; private set; }
    [field: SerializeField] public int HighScore { get; private set; }
    [field: SerializeField] public int KillCount { get; private set; }

    public event Action<int, int> OnScoreChanged;

    private const string HighScoreKey = "HighScore";

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        DontDestroyOnLoad(gameObject);
        HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
    }

    public void AddKillScore(int amount)
    {
        KillCount++;
        AddScore(amount);
    }

    public void AddScore(int amount)
    {
        if (amount <= 0) return;

        Score += amount;

        if (Score > HighScore)
        {
            HighScore = Score;
            PlayerPrefs.SetInt(HighScoreKey, HighScore);
            PlayerPrefs.Save();
        }

        OnScoreChanged?.Invoke(Score, HighScore);
    }

    public void ResetScore()
    {
        Score = 0;
        KillCount = 0;
        OnScoreChanged?.Invoke(Score, HighScore);
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }
}
