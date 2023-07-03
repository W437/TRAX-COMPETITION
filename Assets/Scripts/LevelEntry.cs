using UnityEngine;
using UnityEngine.UI;

public class LevelEntry : MonoBehaviour 
{
    public int Level;
    public Button Btn_Leaderboard;
    public Level.Category Category;
    public Button Btn_Play;
    public TMPro.TextMeshProUGUI Txt_LevelName;
    public TMPro.TextMeshProUGUI Txt_Faults;
    public TMPro.TextMeshProUGUI Txt_Timer;
    public GameObject Trophy1, Trophy2, Trophy3;

    private void Start()
    {
        Btn_Leaderboard.onClick.AddListener(OnLeaderboardButtonClick);
        Btn_Play.onClick.AddListener(OnPlayButtonClick);
    }

    public void SetLevel(int level)
    {
        this.Level = level;
    }

    private void OnLeaderboardButtonClick()
    {
        // Show leaderboard for this level
    }

    private void OnPlayButtonClick()
    {
        // Start the level associated with this button
        ScreenManager.Instance.TweenLevelsSection(false);
        LevelManager.Instance.StartLevel(Level);
    }
}
