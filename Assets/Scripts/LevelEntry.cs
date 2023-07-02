using UnityEngine;
using UnityEngine.UI;

public class LevelEntry : MonoBehaviour 
{
    public int level;
    public Button leaderboardButton;
    public Level.Category category;
    public Button playButton;
    public TMPro.TextMeshProUGUI T_LevelName;
    public TMPro.TextMeshProUGUI T_Faults;
    public TMPro.TextMeshProUGUI T_Timer;

    private void Start()
    {
        leaderboardButton.onClick.AddListener(OnLeaderboardButtonClick);
        playButton.onClick.AddListener(OnPlayButtonClick);
    }

    public void SetLevel(int level)
    {
        this.level = level;
    }

    private void OnLeaderboardButtonClick()
    {
        // Show leaderboard for this level
       // LevelManager.Instance.ShowLeaderboardForLevel(level);
    }

    private void OnPlayButtonClick()
    {
        // Start the level associated with this button
        ScreenManager.Instance.TweenLevelsSection(false);
        LevelManager.Instance.StartLevel(level);
    }
}
