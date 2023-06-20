using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScreenManager : MonoBehaviour
{
    public static ScreenManager Instance;


    [Header("Game Panels")]
    public GameObject Panel_MainMenu;
    public GameObject Panel_GameHUD;
    public GameObject Panel_GameOver;
    private GameObject Panel_Paused;

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
    public Image Paused_Overlay;
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
        // Initial UI pos
   
        // Paused Elements
        var obj = B_Paused_Restart.transform.localPosition;
        B_Paused_Restart.transform.localPosition = new Vector2 (obj.x-450f, obj.y);
        obj = B_Paused_Resume.transform.localPosition;
        B_Paused_Resume.transform.localPosition = new Vector2 (obj.x+550f, obj.y);
        obj = B_Paused_Menu.transform.position;
        B_Paused_Menu.transform.position = new Vector2 (obj.x, obj.y-350f);
        obj = CoinsBar.transform.position;
        CoinsBar.transform.position = new Vector2(obj.x - 250f, obj.y);
        obj = LvlsFinishedBar.transform.position;
        LvlsFinishedBar.transform.position = new Vector2 (obj.x-220f, obj.y);
        obj = GameLogo.transform.position;
        GameLogo.transform.position = new Vector2(obj.x, obj.y+350f);

        Paused_Overlay.color = new Color(0,0,0,0);

        // Main Menu
        obj = B_Start.transform.localPosition;
        B_Start.transform.localPosition = new Vector3(obj.x, obj.y - 900f);
        obj = B_MainLeaderboard.transform.localPosition;
        B_MainLeaderboard.transform.localPosition = new Vector3(obj.x, obj.y - 900f);
        obj = B_Settings.transform.localPosition;
        B_Settings.transform.localPosition = new Vector3(obj.x, obj.y - 900f);
        obj = B_Shop.transform.localPosition;
        B_Shop.transform.localPosition = new Vector3(obj.x, obj.y - 900f);



        // Tween Menu
        TweenMainMenu(true);


        B_Start.onClick.AddListener(delegate { LevelManager.Instance.StartLevel(0); });

        // Button listeners
        B_Leaderboard.onClick.AddListener( OnRestartClicked );
        B_Restart.onClick.AddListener(OnRestartClicked);
        B_NextLvl.onClick.AddListener(delegate { OnBackClicked(Panel_GameOver); });
        B_Back.onClick.AddListener(OnRestartClicked);
    }



    public void TweenMainMenu(bool In)
    {
        if (In)
        {
            Panel_MainMenu.SetActive(true);
            LeanTween.alpha(Overlay_Menu.GetComponent<RectTransform>(), 0.5f, 1f);
            LeanTween.moveY(GameLogo.GetComponent<RectTransform>(), 508f, 0f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.scale(GameLogo, new Vector2(0.35f, 0.35f), 1f)
                   .setEaseOutBounce() 
                   .setOnComplete(() =>
                   {
                       LeanTween.scale(GameLogo, new Vector2(0.3f, 0.3f), 0.5f)
                           .setDelay(0.2f);
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
            LeanTween.moveX(CoinsBar.GetComponent<RectTransform>(), -550f, 0.95f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveX(LvlsFinishedBar.GetComponent<RectTransform>(), -550f, 0.95f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.moveY(GameLogo.GetComponent<RectTransform>(), 850f, 0.8f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveY(B_Settings.GetComponent<RectTransform>(), -900f, 0.7f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveY(B_Shop.GetComponent<RectTransform>(), -900f, 0.75f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveY(B_MainLeaderboard.GetComponent<RectTransform>(), -900f, 0.8f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.alpha(Overlay_Menu.GetComponent<RectTransform>(), 0f, 1f);
            LeanTween.moveY(B_Start.GetComponent<RectTransform>(), -900f, 0.85f).setEase(LeanTweenType.easeOutExpo).setOnComplete(
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
