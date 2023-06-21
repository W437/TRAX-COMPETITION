using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameManager;

public class ScreenManager : MonoBehaviour
{
    public static ScreenManager Instance;


    [Header("Game Panels")]
    public GameObject Panel_MainMenu;
    public GameObject Panel_GameHUD;
    public GameObject Panel_GameOver;
    public GameObject Panel_Paused;

    [Header("Game HUD Elements")]
    public Button B_PauseGame;
    public GameObject FaultsBar;
    public GameObject TimerBar;
    public GameObject WheelieBar;
    public GameObject FlipsBar;

    [Header("Main Menu Elements")]
    public Button B_MainLeaderboard;
    public Button B_Start;
    public Button B_Settings;
    public Button B_Shop;
    public GameObject CoinsBar;
    public GameObject GameLogo;
    public GameObject LvlsFinishedBar;
    public GameObject Overlay_Menu;
    public TMPro.TextMeshProUGUI T_Coins;
    public TMPro.TextMeshProUGUI T_LvlsFinished;

    [Header("Level End Buttons")]
    public Button B_Leaderboard;
    public Button B_Restart;
    public Button B_NextLvl;
    public Button B_Back;

    [Header("Level End Text")]
    public TMPro.TextMeshProUGUI T_LevelTime;
    public TMPro.TextMeshProUGUI T_Faults;
    public TMPro.TextMeshProUGUI T_Wheelie;
    public TMPro.TextMeshProUGUI T_Flips;

    [Header("Paused Screen Elements")]
    public Image Overlay_Paused;
    public TMPro.TextMeshProUGUI T_PausedText;
    public Button B_Paused_Restart;
    public Button B_Paused_Resume;
    public Button B_Paused_Menu;


    private void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60;
    }

    void Start()
    {
        // ---------- Initial UI pos



        // Main Menu
        var obj = B_Start.transform.localPosition;
        B_Start.transform.localPosition = 
            new Vector2(obj.x, obj.y - 900f);

        obj = B_MainLeaderboard.transform.localPosition;
        B_MainLeaderboard.transform.localPosition = 
            new Vector2(obj.x, obj.y - 900f);

        obj = B_Settings.transform.localPosition;
        B_Settings.transform.localPosition = 
            new Vector2(obj.x, obj.y - 900f);

        obj = B_Shop.transform.localPosition;
        B_Shop.transform.localPosition = 
            new Vector2(obj.x, obj.y - 900f);

        obj = CoinsBar.transform.position;
        CoinsBar.transform.position = 
            new Vector2(obj.x - 250f, obj.y);

        obj = LvlsFinishedBar.transform.position;
        LvlsFinishedBar.transform.position = 
            new Vector2(obj.x - 220f, obj.y);

        obj = GameLogo.transform.position;
        GameLogo.transform.position = 
            new Vector2(obj.x, obj.y + 350f);

        // Game HUD
        obj = B_PauseGame.transform.localPosition;
        B_PauseGame.transform.localPosition = 
            new Vector2(obj.x-300f, obj.y);

        obj = FaultsBar.transform.localPosition;
        FaultsBar.transform.localPosition = 
            new Vector2(obj.x + 200f, obj.y);

        obj = TimerBar.transform.localPosition;
        TimerBar.transform.localPosition = 
            new Vector2(obj.x - 300f, obj.y);

        obj = WheelieBar.transform.localPosition;
        WheelieBar.transform.localPosition = 
            new Vector2(obj.x - 300f, obj.y);

        obj = FlipsBar.transform.localPosition;
        FlipsBar.transform.localPosition = 
            new Vector2(obj.x - 300f, obj.y);


        // Paused Game
        obj = B_Paused_Restart.transform.localPosition;
        B_Paused_Restart.transform.localPosition = 
            new Vector2(obj.x-550f, obj.y);

        obj = B_Paused_Resume.transform.localPosition;
        B_Paused_Resume.transform.localPosition = 
            new Vector2(obj.x+550f, obj.y);

        obj = B_Paused_Menu.transform.localPosition;
        B_Paused_Menu.transform.localPosition = 
            new Vector2(obj.x, obj.y-300f);

        obj = T_PausedText.transform.localPosition;
        T_PausedText.transform.localPosition = 
            new Vector2(obj.x, obj.y+700);

        Overlay_Paused.color = new Color(0, 0, 0, 0);


        // ---------- ON GAME LAUNCH

        // Tween Menu
        TweenMainMenu(true);

        // Main Menu
        B_Start.onClick.AddListener(delegate { LevelManager.Instance.StartLevel(0); });

        // Paused Screen
        B_PauseGame.onClick.AddListener( PauseGame );

        B_Paused_Resume.onClick.AddListener( ResumeGame );

        B_Paused_Restart.onClick.AddListener(delegate { LevelManager.Instance.StartLevel(1); });

        B_Paused_Menu.onClick.AddListener(delegate { GoToMainMenu(); });


        // Level End
        B_Leaderboard.onClick.AddListener( OnRestartClicked );
        B_Restart.onClick.AddListener(OnRestartClicked);
        B_NextLvl.onClick.AddListener(delegate { OnBackClicked(Panel_GameOver); });
        B_Back.onClick.AddListener(OnRestartClicked);
    }

    private void GoToMainMenu()
    {
        TweenPauseGame(false);
        TweenGameHUD(false);
        TweenMainMenu(true);
    }

    public void PauseGame()
    {
        GameManager.Instance.SetGameState(GameState.Paused);
        TweenPauseGame(true);
    }

    public void ResumeGame()
    {
        GameManager.Instance.SetGameState(GameState.Playing);
        TweenPauseGame(false);
    }

    public void TweenPauseGame(bool In)
    {
        if (In)
        {
            LeanTween.moveX(B_PauseGame.GetComponent<RectTransform>(), -550f, 0.5f).setEase(LeanTweenType.easeOutExpo);
            Panel_Paused.SetActive(true);
            LeanTween.alpha(Overlay_Paused.GetComponent<RectTransform>(), 0.5f, 1f);
            LeanTween.moveX(B_Paused_Resume.GetComponent<RectTransform>(), 5f, 0.85f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.moveX(B_Paused_Restart.GetComponent<RectTransform>(), -5f, 0.95f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.moveY(B_Paused_Menu.GetComponent<RectTransform>(), -100f, 0.9f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.moveY(T_PausedText.GetComponent<RectTransform>(), -540f, 0.75f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
        }
        else
        {
            LeanTween.alpha(Overlay_Paused.GetComponent<RectTransform>(), 0f, 1f);
            LeanTween.moveX(B_Paused_Resume.GetComponent<RectTransform>(), 460f, 0.85f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveX(B_Paused_Restart.GetComponent<RectTransform>(), -360f, 0.95f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveY(B_Paused_Menu.GetComponent<RectTransform>(), -370f, 0.9f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveY(T_PausedText.GetComponent<RectTransform>(), 125f, 0.75f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f).setOnComplete(
                delegate ()
                {
                    Panel_Paused.SetActive(false);
                    LeanTween.moveX(B_PauseGame.GetComponent<RectTransform>(), -370f, 0.8f).setEase(LeanTweenType.easeOutExpo).setDelay(0.6f);
                });
        }
    }



    public void TweenGameHUD(bool In)
    {
        if (In)
        {
            Panel_GameHUD.SetActive(true);
            LeanTween.moveX(TimerBar.GetComponent<RectTransform>(), 0, 0.75f).setEase(LeanTweenType.easeOutExpo).setDelay(0.2f);
            LeanTween.moveX(WheelieBar.GetComponent<RectTransform>(), -45.79999f, 0.85f).setEase(LeanTweenType.easeOutExpo).setDelay(0.3f);
            LeanTween.moveX(FlipsBar.GetComponent<RectTransform>(), -67.5f, 0.95f).setEase(LeanTweenType.easeOutExpo).setDelay(0.4f);
            LeanTween.moveX(FaultsBar.GetComponent<RectTransform>(), 100f, 0.9f).setEase(LeanTweenType.easeOutExpo).setDelay(0.4f);
            LeanTween.moveX(B_PauseGame.GetComponent<RectTransform>(), -370f, 0.8f).setEase(LeanTweenType.easeOutExpo).setDelay(0.6f);
        }
        else
        {
            LeanTween.moveX(TimerBar.GetComponent<RectTransform>(), 0, 0.5f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveX(WheelieBar.GetComponent<RectTransform>(), -45.79999f, 0.5f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveX(FlipsBar.GetComponent<RectTransform>(), -67.5f, 0.5f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveX(FaultsBar.GetComponent<RectTransform>(), 100f, 0.5f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveX(B_PauseGame.GetComponent<RectTransform>(), -370f, 0.5f).setEase(LeanTweenType.easeOutExpo).setOnComplete(
                delegate ()
                {
                    Panel_GameHUD.SetActive(false);
                });
        }
    }



    public void TweenMainMenu(bool In)
    {
        if (In)
        {
            Panel_MainMenu.SetActive(true);
            LeanTween.alpha(Overlay_Menu.GetComponent<RectTransform>(), 0.5f, 1f);
            LeanTween.moveY(GameLogo.GetComponent<RectTransform>(), 650f, 0.5f).setEase(LeanTweenType.easeInOutQuad);
            LeanTween.scale(GameLogo, new Vector2(0.35f, 0.35f), 0.7f)
                   .setEaseOutBounce() 
                   .setOnComplete(() =>
                   {
                       LeanTween.scale(GameLogo, new Vector2(0.3f, 0.3f), 0.5f).setEase(LeanTweenType.easeInOutQuad)
                           .setDelay(0.3f);
                   });
            LeanTween.moveY(B_Start.GetComponent<RectTransform>(), 57f, 0.8f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveY(B_MainLeaderboard.GetComponent<RectTransform>(), -110f, 0.9f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveY(B_Shop.GetComponent<RectTransform>(), -244f, 0.95f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveY(B_Settings.GetComponent<RectTransform>(), -110f, 0.96f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveX(CoinsBar.GetComponent<RectTransform>(), -300f, 0.95f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveX(LvlsFinishedBar.GetComponent<RectTransform>(), -300f, 0.95f).setEase(LeanTweenType.easeOutExpo).setDelay(0.2f);
        }
        else
        {
            LeanTween.alpha(Overlay_Menu.GetComponent<RectTransform>(), 0f, 1f);
            LeanTween.moveX(CoinsBar.GetComponent<RectTransform>(), -550f, 0.95f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveX(LvlsFinishedBar.GetComponent<RectTransform>(), -550f, 0.95f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.moveY(GameLogo.GetComponent<RectTransform>(), 1050f, 0.8f).setEase(LeanTweenType.easeOutExpo);
   
            LeanTween.moveY(B_Shop.GetComponent<RectTransform>(), -1100f, 0.75f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveY(B_Settings.GetComponent<RectTransform>(), -1100f, 0.7f).setEase(LeanTweenType.easeOutExpo).setDelay(0.05f);
            LeanTween.moveY(B_MainLeaderboard.GetComponent<RectTransform>(), -1100f, 0.8f).setEase(LeanTweenType.easeOutExpo).setDelay(0.05f);
         
            LeanTween.moveY(B_Start.GetComponent<RectTransform>(), -1100f, 0.85f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f).setOnComplete(
                delegate ()
                {
                    Panel_MainMenu.SetActive(false);
                });
        }
    }

    private void OnRestartClicked()
    {
        SceneManager.LoadScene(0);
    }

    private void OnBackClicked(GameObject currentPanel)
    {
        currentPanel.SetActive(false);
        TweenMainMenu(true);
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
