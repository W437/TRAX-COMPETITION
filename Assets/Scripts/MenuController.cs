using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public static MenuController Instance;
    public GameObject Panel_MainMenu;
    public GameObject Panel_GameHUD;
    public GameObject Panel_GameOver;

    public Button B_Leaderboard;
    public Button B_Restart;
    public Button B_NextLvl;
    public Button B_Back;

    public TMPro.TextMeshProUGUI T_LevelTime;
    public TMPro.TextMeshProUGUI T_Faults;
    public TMPro.TextMeshProUGUI T_Wheelie;
    public TMPro.TextMeshProUGUI T_Flips;


    
    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {

        // Button listeners
        B_Leaderboard.onClick.AddListener( OnRestartClicked );
        B_Restart.onClick.AddListener(OnRestartClicked);

        B_NextLvl.onClick.AddListener(delegate { OnBackClicked(Panel_GameOver); });

        B_Back.onClick.AddListener(OnRestartClicked);
    }

    private void OnRestartClicked()
    {
        SceneManager.LoadScene(0);
    }

    private void OnBackClicked(GameObject currentPanel)
    {
        currentPanel.SetActive(false);
        Panel_MainMenu.SetActive(true);
    }

    public void OnLevelEnd()
    {
        Panel_GameHUD.SetActive(false);
        Panel_GameOver.SetActive(true);

        var levelTime = GameManager.Instance.timerText.text;
        T_LevelTime.text = "" + levelTime;

        var levelFaults = GameManager.Instance.faultCountText.text;
        T_Faults.text = "" + levelFaults;

        var levelWheelie = GameManager.Instance.wheelieTimeText.text;
        T_Wheelie.text = "" + levelWheelie;

        var flipCount = GameManager.Instance.flipCountText.text;
        T_Flips.text = "" + flipCount;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
