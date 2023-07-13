using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelEntry : MonoBehaviour 
{
    public static LevelEntry Instance;
    private LeaderboardManager LeaderboardManager;
    private ScreenManager ScreenManager;
    public int Level;
    public Button Btn_Leaderboard;
    public Level.Category Category;
    public Button Btn_Play;
    public TMPro.TextMeshProUGUI Txt_LevelName;
    public TMPro.TextMeshProUGUI Txt_Faults;
    public TMPro.TextMeshProUGUI Txt_Timer;
    public GameObject Trophy1, Trophy2, Trophy3;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        LeaderboardManager = LeaderboardManager.Instance;
        ScreenManager = ScreenManager.Instance;
        Btn_Leaderboard.onClick.AddListener(OnLeaderboardButtonClick);
        Btn_Play.onClick.AddListener(OnPlayButtonClick);
    }

    public void SetLevel(int level, Level.Category category)
    {
        Level = level;
        Category = category;
    }

    private void OnLeaderboardButtonClick()
    {
        string levelKey = Category.ToString() + "_" + Level;
        ScreenManager.Txt_LB_LevelName.text = "Global Toptimes for: " + Category.ToString() + " - " + (Level+1);
        LeaderboardManager.UpdateLeaderboardUI(levelKey);
        ScreenManager.TweenLevelsSection(false);
        ScreenManager.TweenLeaderboard(true);
    }

    private void OnPlayButtonClick()
    {
        // Start the level associated with this button
        ScreenManager.Instance.TweenLevelsSection(false);
        LevelManager.Instance.StartLevel(Level);
    }
}
