using Lofelt.NiceVibrations;
using MoreMountains.Feedbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameManager;
using static PlayerData;

public class ScreenManager : MonoBehaviour
{
    public static ScreenManager Instance;
    // Init Managers
    private PlayerData PlayerData;
    private BikeController BikeController;
    private TrailManager TrailManager;
    private ShopManager ShopManager;
    private CameraController CameraController;
    private LevelManager LevelManager;
    private AudioManager AudioManager;
    private GameManager GameManager;
    private BackgroundParalax BackgroundParalax;
    private LeaderboardManager LeaderboardManager;

    #region Variables

    private float _lastButtonClickTime = 0f;
    private float _buttonClickCooldown = 0.45f;


    [Header("Player Profile")]
    public GameObject UsernamePanel;
    public TMP_InputField Input_PlayerUsername;
    public Image OverlayUsernamePanel;
    public Button Btn_ConfirmName;

    /////////////////////////////////////////////////////////////////////////////////////
    /////// LEVEL SELECTION MENU
    /////////////////////////////////////////////////////////////////////////////////////

    [Header("Levels Section")]
    public GameObject levelUnitPrefab;
    public GameObject LevelsBG;
    public GameObject LevelsSection;
    public Transform LevelsView;
    public Button B_LevelsMenuBack;
    private List<GameObject> instantiatedLevels = new();
    public TextMeshProUGUI Txt_LB_LevelName;


    private LevelStats CurrentLevelStats;

    /////////////////////////////////////////////////////////////////////////////////////
    /////// ALL GAME SCREENS
    /////////////////////////////////////////////////////////////////////////////////////

    [Header("Game Panels")]
    public GameObject Panel_MainMenu;
    public GameObject Panel_GameHUD;
    public GameObject Panel_GameOver;
    public GameObject Panel_Paused;
    public GameObject Panel_Levels;
    public GameObject Panel_Shop;
    public GameObject Panel_About;
    public GameObject Panel_Settings;
    public GameObject Panel_Leaderboard;
    public GameObject Panel_PlayerProfile;

    /////////////////////////////////////////////////////////////////////////////////////
    /////// GAME HUD UI
    /////////////////////////////////////////////////////////////////////////////////////

    [Header("Game HUD Elements")]
    public Button Btn_HUD_PauseGame;
    public GameObject HUD_FaultsBar;
    public GameObject HUD_TimerBar;
    public GameObject HUD_WheelieBar;
    public GameObject HUD_FlipsBar;
    public MMFeedbackFloatingText FlipFeedback;
    public MMFloatingTextMeshPro FlipFeedbackText;

    /////////////////////////////////////////////////////////////////////////////////////
    /////// MAIN MENU SCREEN
    /////////////////////////////////////////////////////////////////////////////////////

    [Header("Main Menu Elements")]
    public Button Btn_ReconnectPlayfab;
    public TextMeshProUGUI Txt_PlayerName;
    public GameObject Online_Status, Offline_Status, PlayFabStatusParent;
    public GameObject LBMainPanel;
    public Button Btn_LB_Back;
    public Button Btn_Menu_MainLeaderboard;
    public Button Btn_Menu_Start;
    public Button Btn_Menu_Settings;
    public Button Btn_Menu_Shop;
    public Button Btn_Menu_About;
    public GameObject MENU_CoinsBar;
    public GameObject MENU_GameLogo;
    public GameObject MENU_LvlsFinishedBar;
    public GameObject MENU_XP;
    public GameObject MENU_Level;
    public GameObject Overlay_Menu;
    public TextMeshProUGUI Txt_Menu_Coins;
    public TextMeshProUGUI Txt_Menu_XP;
    public TextMeshProUGUI Txt_Menu_Level;
    public TextMeshProUGUI Txt_Menu_LvlsFinished;
    public GameObject AboutPanel;
    public Button Btn_About_Back;
    public float menuBikeSpeed = 0f;
    public float menuBikeMaxSpeed = 9.5f;
    public float accelerationTimer;
    public float accelerationTime;
    bool hasAnimatedIn = false;
    public Coroutine BikeAnimationInCoroutine, BikeAnimationOutCoroutine;


    /////////////////////////////////////////////////////////////////////////////////////
    /////// SETTINGS SCREEN MENU
    /////////////////////////////////////////////////////////////////////////////////////

    [Header("Settings Section")]
    public GameObject Settings_Stats;
    public GameObject Settings_Main;
    public GameObject temp_PanelDev;
    public TextMeshProUGUI Txt_Stats1, Txt_Stats2;
    public TextMeshProUGUI Txt_PlayerLevel, Txt_PlayerXP;
    public Toggle Settings_Toggle_Mute, Settings_Toggle_Haptic;
    public TextMeshProUGUI Txt_ToggleMuteStatus;
    public Sprite MuteSprite, UnmuteSprite, HapticOffSprite, HapticOnSprite;
    public Image ToggleMuteImage, ToggleHapticImage, ToggleMuteBG, ToggleHapticBG;
    public Slider Settings_Slider_MainVol, Settings_Slider_SFXVol, Settings_Slider_LevelProgress;

    // Dev tools
    public Button btn_ResetCoins, btn_AddCoins, btn_ResetSavedata, btn_UnlockAll, btn_AddXP, btn_ShuffleSoundtrack;
    public Button B_SettingsBackMenu;
    public Toggle Toggle_GameSettings;
    public Toggle Toggle_OnlineSettings;

    public GameObject Section_OnlineSettings, Section_GameSettings;

    // Online Settings
    public TMP_InputField Input_NewUsername;
    public TextMeshProUGUI Txt_UsernamePlaceholder;
    public Button Btn_NewUsernameSave;
    public Button Btn_SyncOfflineData;

    /////////////////////////////////////////////////////////////////////////////////////
    /////// SHOP SCREEN MENU
    /////////////////////////////////////////////////////////////////////////////////////

    [Header("Shop Section")]
    public Button Btn_Shop_BackMenu;
    public Button Btn_Shop_RightBtn;
    public Button Btn_Shop_LeftBtn;

    public Button Btn_Shop_BuyButton;
    public GameObject ShopSelectionObject;
    public GameObject TopMenuHeader;
    public GameObject TopOverlayHeader;
    public TextMeshProUGUI Txt_Shop_Coins, Txt_Shop_UnlockedBikes, Txt_Shop_UnlockedTrails, Txt_Shop_ID;
    public GameObject PrefabPrice;

    /////////////////////////////////////////////////////////////////////////////////////
    /////// LEVEL ENDING SCREEN
    /////////////////////////////////////////////////////////////////////////////////////

    [Header("Level Finish Menu")]
    public Button Btn_Finish_Leaderboard;
    public Button Btn_Finish_Restart;
    public Button Btn_Finish_NextLvl;
    public Button Btn_LevelFinish_Back;
    public GameObject FinishStatsPanel;
    public GameObject Trophy1, Trophy2, Trophy3;
    public Image LevelFinishOverlay;

    [Header("Level End Text")]
    public TextMeshProUGUI Txt_LevelFinish_LevelTime;
    public TextMeshProUGUI Txt_LevelFinish_Faults;
    public TextMeshProUGUI Txt_LevelFinish_Wheelie;
    public TextMeshProUGUI Txt_LevelFinish_Flips;
    public TextMeshProUGUI Txt_LevelFinish_XPGained;

    /////////////////////////////////////////////////////////////////////////////////////
    /////// GAME PAUSED SCREEN
    /////////////////////////////////////////////////////////////////////////////////////

    [Header("Paused Screen Elements")]
    public Image Overlay_Paused;
    public TextMeshProUGUI T_PausedText;
    public Button B_Paused_Restart;
    public Button B_Paused_Resume;
    public Button B_Paused_Menu;

    /////////////////////////////////////////////////////////////////////////////////////
    /////// SCREEN TRANSITION SYSTEM
    /////////////////////////////////////////////////////////////////////////////////////

    [Header("Screen Transitions")]
    public GameObject startTransition;
    public GameObject endTransition;
    public Animator startTransitionAnimator;
    public Animator endTransitionAnimator;
    const float startTransitionDuration = 1f;
    const float endTransitionDuration = 1f;

    /////////////////////////////////////////////////////////////////////////////////////
    /////// MENU BIKE STUFF
    /////////////////////////////////////////////////////////////////////////////////////

    [NonSerialized] public GameObject PlayerMenuBike; // The current displayed prefab
    [NonSerialized] public Rigidbody2D PlayerMenuBikeRb;
    public Rigidbody2D MenuPlatformRb;
    [NonSerialized] public ParticleSystem PlayerMenuBikeParticleSystem;
    [NonSerialized] public GameObject PlayerMenuBikeTrail;
    public GameObject MenuPlatformObject;
    private Vector2 bikePosition;
    public GameObject MenuBikeObjectParent;

    #endregion

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Init Managers
        BikeController = BikeController.Instance;
        TrailManager = TrailManager.Instance;
        ShopManager = ShopManager.Instance;
        CameraController = CameraController.Instance;
        LevelManager = LevelManager.Instance;
        AudioManager = AudioManager.Instance;
        GameManager = GameManager.Instance;
        BackgroundParalax = BackgroundParalax.Instance;
        LeaderboardManager = LeaderboardManager.Instance;
        PlayerData = SaveSystem.LoadPlayerData();
        //Debug.Log("Loaded Bike ID: " + PlayerData.SELECTED_BIKE_ID);


        /////////////////////////////////////////////////////////////////////////////////////
        /////// INITIATE MENU PLAYER BIKE & TRAIL
        /////////////////////////////////////////////////////////////////////////////////////

        int selectedBikeId = PlayerData.SELECTED_BIKE_ID;
        int selectedTrailId = PlayerData.SELECTED_TRAIL_ID;
        Bike selectedBikeData = BikeController.GetBikeById(selectedBikeId);
        Trail selectedTrailData = TrailManager.GetTrailById(selectedTrailId);
        /*#region Debugging
            if (selectedBikeData == null)
            {
                Debug.LogError("No Bike found with ID: " + selectedBikeId);
                return;
            }
            if (selectedTrailData == null)
            {
                Debug.LogError("No Trail found with ID: " + selectedTrailId);
                return;
            }
        #endregion*/
        GameObject selectedBike = selectedBikeData.BikePrefab;
        GameObject selectedTrail = selectedTrailData.TrailPrefab;
        /*#region Debugging
            if (selectedBike == null)
            {
                Debug.LogError("No Bike prefab found for ID: " + selectedBikeId);
                return;
            }
            if (selectedTrail == null)
            {
                Debug.LogError("No Trail prefab found for ID: " + selectedTrailId);
                return;
            }
        #endregion*/

        Debug.Log("Selected Bike ID: " + selectedBikeId);
        Debug.Log("Selected Trail ID: " + selectedTrailId);
        PlayerMenuBike = ShopManager.DisplayBikePrefab(selectedBike);
        PlayerMenuBikeTrail = ShopManager.DisplayTrailPrefab(selectedTrail);
        PlayerMenuBikeRb = PlayerMenuBike.GetComponent<Rigidbody2D>();

        CameraController.MenuCamera.Follow = PlayerMenuBike.transform;
        CameraController.ShopCamera.Follow = PlayerMenuBike.transform;
        CameraController.SettingsCamera.Follow = PlayerMenuBike.transform;

        /////////////////////////////////////////////////////////////////////////////////////
        /////// INITIATE SETTINGS DATA
        /////////////////////////////////////////////////////////////////////////////////////

        if (PlayerData.SETTINGS_isMuted)
        {
            Settings_Toggle_Mute.isOn = true;
            AudioManager.MainAudioSource.volume = 0;
            AudioManager.SFXAudioSource.volume = 0;
            Settings_Slider_MainVol.value = 0f;
            Settings_Slider_SFXVol.value = 0;
            Color currentColor = ToggleMuteBG.color;
            ToggleMuteBG.color = new Color(currentColor.r, currentColor.g, currentColor.b, 0.85f);
            Txt_ToggleMuteStatus.text = "Unmute";
            Settings_Slider_MainVol.interactable = false;
            Settings_Slider_SFXVol.interactable = false;
        }

        else
        {
            Settings_Toggle_Mute.isOn = false;
            AudioManager.MainAudioSource.volume = 0.45f;
            AudioManager.SFXAudioSource.volume = 0.45f;
            Settings_Slider_MainVol.value = 0.45f;
            Settings_Slider_SFXVol.value = 0.45f;
            Color currentColor = ToggleMuteBG.color;
            ToggleMuteBG.color = new Color(currentColor.r, currentColor.g, currentColor.b, 0.35f);
            Txt_ToggleMuteStatus.text = "Mute";
            Settings_Slider_MainVol.interactable = true;
            Settings_Slider_SFXVol.interactable = true;
        }

        UpdateMuteToggleImage();


        if (PlayerData.SETTINGS_isHapticEnabled)
        {
            Settings_Toggle_Haptic.isOn = true;
            HapticController.hapticsEnabled = PlayerData.SETTINGS_isHapticEnabled;
            Color currentColor = ToggleHapticBG.color;
            ToggleHapticBG.color = new Color(currentColor.r, currentColor.g, currentColor.b, 0.85f);
        }
        else
        {
            Settings_Toggle_Haptic.isOn = false;
            HapticController.hapticsEnabled = PlayerData.SETTINGS_isHapticEnabled;
            Color currentColor = ToggleHapticBG.color;
            ToggleHapticBG.color = new Color(currentColor.r, currentColor.g, currentColor.b, 0.30f);
        }

        UpdateHapticToggleImage();


        Settings_Slider_MainVol.value = PlayerData.SETTINGS_isMuted ? 0 : PlayerData.SETTINGS_mainVolume;
        Settings_Slider_SFXVol.value = PlayerData.SETTINGS_isMuted ? 0 : PlayerData.SETTINGS_sfxVolume;
        Settings_Slider_LevelProgress.interactable = false;
        // Stats Data
        RefreshTextValuesFromPlayerData();
        UpdateXPBar();

        /////////////////////////////////////////////////////////////////////////////////////
        /////// INITIAL UI POSITIONS
        /////////////////////////////////////////////////////////////////////////////////////

        #region UI Initial Positions

        // Welcome Panel
        UsernamePanel.transform.localRotation = Quaternion.Euler(-90, 0, 0);

        // Main Menu
        var obj = Btn_Menu_Start.transform.localPosition;
        Btn_Menu_Start.transform.localPosition =
            new Vector2(obj.x, obj.y - 900f);

        obj = Btn_Menu_MainLeaderboard.transform.localPosition;
        Btn_Menu_MainLeaderboard.transform.localPosition =
            new Vector2(obj.x, obj.y - 900f);

        obj = Btn_Menu_Settings.transform.localPosition;
        Btn_Menu_Settings.transform.localPosition =
            new Vector2(obj.x, obj.y - 900f);

        obj = Btn_Menu_About.transform.localPosition;
        Btn_Menu_About.transform.localPosition =
            new Vector2(obj.x, obj.y - 400);

        obj = Btn_Menu_Shop.transform.localPosition;
        Btn_Menu_Shop.transform.localPosition =
            new Vector2(obj.x, obj.y - 900f);

        obj = MENU_CoinsBar.transform.position;
        MENU_CoinsBar.transform.position =
            new Vector2(obj.x - 400f, obj.y);

        obj = MENU_LvlsFinishedBar.transform.position;
        MENU_LvlsFinishedBar.transform.position =
            new Vector2(obj.x - 400f, obj.y);

        obj = MENU_Level.transform.position;
        MENU_Level.transform.position =
            new Vector2(obj.x + 400f, obj.y);

        obj = MENU_XP.transform.position;
        MENU_XP.transform.position =
            new Vector2(obj.x + 400f, obj.y);

        obj = MENU_GameLogo.transform.localPosition;
        MENU_GameLogo.transform.localPosition =
            new Vector2(obj.x, obj.y + 450);

        obj = Btn_About_Back.transform.localPosition;
        Btn_About_Back.transform.localPosition =
            new Vector2(obj.x, obj.y - 500);

        obj = AboutPanel.transform.localPosition;
        AboutPanel.transform.localPosition =
            new Vector2(obj.x + 900, obj.y);

        obj = PlayFabStatusParent.transform.localPosition;
        PlayFabStatusParent.transform.localPosition =
            new Vector2(obj.x + 400, obj.y);

        obj = LBMainPanel.transform.localPosition;
        LBMainPanel.transform.localPosition =
            new Vector2(obj.x + 1200, obj.y);

        // Game HUD
        obj = Btn_HUD_PauseGame.transform.localPosition;
        Btn_HUD_PauseGame.transform.localPosition =
            new Vector2(obj.x - 450f, obj.y);

        obj = HUD_FaultsBar.transform.localPosition;
        HUD_FaultsBar.transform.localPosition =
            new Vector2(obj.x + 200f, obj.y);

        obj = HUD_TimerBar.transform.localPosition;
        HUD_TimerBar.transform.localPosition =
            new Vector2(obj.x - 300f, obj.y);

        obj = HUD_WheelieBar.transform.localPosition;
        HUD_WheelieBar.transform.localPosition =
            new Vector2(obj.x - 300f, obj.y);

        obj = HUD_FlipsBar.transform.localPosition;
        HUD_FlipsBar.transform.localPosition =
            new Vector2(obj.x - 300f, obj.y);


        // Paused Game
        obj = B_Paused_Restart.transform.localPosition;
        B_Paused_Restart.transform.localPosition =
            new Vector2(obj.x - 550f, obj.y);

        obj = B_Paused_Resume.transform.localPosition;
        B_Paused_Resume.transform.localPosition =
            new Vector2(obj.x + 550f, obj.y);

        obj = B_Paused_Menu.transform.localPosition;
        B_Paused_Menu.transform.localPosition =
            new Vector2(obj.x, obj.y - 300f);

        obj = T_PausedText.transform.localPosition;
        T_PausedText.transform.localPosition =
            new Vector2(obj.x, obj.y + 700);

        Overlay_Paused.color = new Color(0, 0, 0, 0);


        // Levels Section
        obj = LevelsBG.transform.localPosition;
        LevelsBG.transform.localPosition =
            new Vector2(850f, obj.y);

        obj = LevelsSection.transform.localPosition;
        LevelsSection.transform.localPosition =
            new Vector2(-800f, obj.y);


        // Shop Section
        obj = Btn_Shop_BackMenu.transform.localPosition;
        Btn_Shop_BackMenu.transform.localPosition =
            new Vector2(0, obj.y - 400f);

        obj = Btn_Shop_RightBtn.transform.localPosition;
        Btn_Shop_RightBtn.transform.localPosition =
            new Vector2(obj.x + 400, obj.y);

        obj = Btn_Shop_LeftBtn.transform.localPosition;
        Btn_Shop_LeftBtn.transform.localPosition =
            new Vector2(obj.x - 400, obj.y);

        obj = Btn_Shop_BuyButton.transform.localScale;
        Btn_Shop_BuyButton.transform.localScale =
            new Vector2(0, 0);

        obj = TopOverlayHeader.transform.localPosition;
        TopOverlayHeader.transform.localPosition =
            new Vector2(0, obj.y + 500f);

        obj = ShopSelectionObject.transform.localPosition;
        ShopSelectionObject.transform.localPosition =
            new Vector2(obj.x - 850, obj.y);


        // Settings Section
        obj = Settings_Stats.transform.localPosition;
        Settings_Stats.transform.localPosition =
            new Vector2(obj.x + 900, obj.y);

        obj = Settings_Main.transform.localPosition;
        Settings_Main.transform.localPosition =
            new Vector2(obj.x - 900, obj.y);

        obj = temp_PanelDev.transform.localPosition;
        temp_PanelDev.transform.localPosition =
            new Vector2(obj.x + 900, obj.y);

        obj = B_SettingsBackMenu.transform.localPosition;
        B_SettingsBackMenu.transform.localPosition =
            new Vector2(obj.x, obj.y - 400);

        Toggle_GameSettings.isOn = true;
        Toggle_GameSettings.Select();


        // Game Finish Section
        obj = FinishStatsPanel.transform.localPosition;
        FinishStatsPanel.transform.localPosition =
            new Vector2(obj.x, obj.y + 1500);

        obj = Btn_Finish_Leaderboard.transform.localScale;
        Btn_Finish_Leaderboard.transform.localScale =
            new Vector3(0, 0, 0);

        obj = Btn_Finish_Restart.transform.localPosition;
        Btn_Finish_Restart.transform.localPosition =
            new Vector2(obj.x - 400, obj.y);

        obj = Btn_Finish_NextLvl.transform.localPosition;
        Btn_Finish_NextLvl.transform.localPosition =
            new Vector2(obj.x + 400, obj.y);

        obj = Btn_LevelFinish_Back.transform.localPosition;
        Btn_LevelFinish_Back.transform.localPosition =
            new Vector2(obj.x, obj.y - 350);

        obj = Txt_LevelFinish_XPGained.transform.localPosition;
        Txt_LevelFinish_XPGained.transform.localPosition =
            new Vector2(obj.x, obj.y + 450);

        // Trophies
        obj = Trophy1.transform.localScale;
        Trophy1.transform.localScale =
            new Vector3(0, 0, 0);

        obj = Trophy2.transform.localScale;
        Trophy2.transform.localScale =
            new Vector3(0, 0, 0);

        obj = Trophy3.transform.localScale;
        Trophy3.transform.localScale =
            new Vector3(0, 0, 0);


        #endregion


        /////////////////////////////////////////////////////////////////////////////////////
        /////// ASSIGN BUTTON LISTENERS
        /////////////////////////////////////////////////////////////////////////////////////

        #region Button Listeners
        ///// Main Menu
        //////////////////////
        ///
        Btn_Menu_Start.onClick.AddListener(delegate
        {
            if (Time.time - _lastButtonClickTime < _buttonClickCooldown)
                return;
            _lastButtonClickTime = Time.time;

            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact);
            TweenMainMenu(false);
            LoadLevelsScreen(true);
            GameManager.SavePlaytimeAndDistance();
        });

        Btn_ConfirmName.onClick.AddListener(LeaderboardManager.OnConfirmButtonPressed);

        Btn_Menu_Shop.onClick.AddListener(delegate
        {
            if (Time.time - _lastButtonClickTime < _buttonClickCooldown)
                return;
            _lastButtonClickTime = Time.time;

            GoToShop();
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact);
        });


        Btn_Menu_About.onClick.AddListener(delegate
        {
            if (Time.time - _lastButtonClickTime < _buttonClickCooldown)
                return;
            _lastButtonClickTime = Time.time;

            TweenAboutSection(true);
            TweenMainMenu(false);
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact);
        });

        Btn_Menu_MainLeaderboard.onClick.AddListener(delegate
        {
            if (Time.time - _lastButtonClickTime < _buttonClickCooldown)
                return;
            _lastButtonClickTime = Time.time;

            HapticPatterns.PlayPreset(HapticPatterns.PresetType.Failure);
            //Panel_Leaderboard.SetActive(true);
            //LeaderboardManager.UpdateLeaderboardUI();
        });

        Btn_About_Back.onClick.AddListener(delegate
        {
            if (Time.time - _lastButtonClickTime < _buttonClickCooldown)
                return;
            _lastButtonClickTime = Time.time;

            TweenAboutSection(false);
            TweenMainMenu(true);
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact);
        });


        Btn_Menu_Settings.onClick.AddListener(delegate
        {
            if (Time.time - _lastButtonClickTime < _buttonClickCooldown + 1)
                return;
            _lastButtonClickTime = Time.time;

            RefreshTextValuesFromPlayerData();
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact);
            TweenMainMenu(false);
            TweenGameLogo(false);
            TweenSettingsMenu(true);
            GameManager.SavePlaytimeAndDistance();
            CameraController.SwitchToSettingsCamera();
        });

        // Set Data
        Txt_Menu_Coins.text = "" + PlayerData.COINS;
        Txt_Menu_LvlsFinished.text = PlayerData.TOTAL_LEVELS_FINISHED + "/" + LevelManager.Levels.Length;

        // Paused Screen
        Btn_HUD_PauseGame.onClick.AddListener(delegate
        {
            if (Time.time - _lastButtonClickTime < _buttonClickCooldown)
                return;
            _lastButtonClickTime = Time.time;

            PauseGame();
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.Warning);
        });

        B_Paused_Resume.onClick.AddListener(delegate
        {
            if (Time.time - _lastButtonClickTime < _buttonClickCooldown)
                return;
            _lastButtonClickTime = Time.time;

            ResumeGame();
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact);
        });

        B_Paused_Restart.onClick.AddListener(delegate
        {
            if (Time.time - _lastButtonClickTime < _buttonClickCooldown)
                return;
            _lastButtonClickTime = Time.time;
            GameManager.SavePlaytimeAndDistance();
            TweenPauseGame(false);

            var _data = SaveSystem.LoadPlayerData();

            SaveSystem.SavePlayerData(_data);
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact);
            LevelManager.StartLevel(LevelManager.CurrentLevelID);
        });

        B_Paused_Menu.onClick.AddListener(delegate
        {
            if (Time.time - _lastButtonClickTime < _buttonClickCooldown)
                return;
            _lastButtonClickTime = Time.time;

            StartCoroutine(GoToMenuFromGame());
            GameManager.SavePlaytimeAndDistance();
            var _data = SaveSystem.LoadPlayerData();
            SaveSystem.SavePlayerData(_data);
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact);

        });

        // Level End
        Btn_Finish_Leaderboard.onClick.AddListener(delegate
        {
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact);

            string category = LevelManager.GetCurrentLevelData().LevelCategory.ToString();
            int id = LevelManager.GetCurrentLevelData().LevelID;
            string levelKey = category + "_" + id;

            Txt_LB_LevelName.text = "Global Toptimes for: " + category + " - " + (id + 1);

            LeaderboardManager.UpdateLeaderboardUI(levelKey);
            TweenLevelFinishMenu(false);
            TweenLeaderboard(true);

        });

        Btn_Finish_Restart.onClick.AddListener(delegate
        {
            if (Time.time - _lastButtonClickTime < _buttonClickCooldown)
                return;
            _lastButtonClickTime = Time.time;

            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact);

            TweenLevelFinishMenu(false);
            LevelManager.StartLevel(LevelManager.CurrentLevelID);
        });

        Btn_Finish_NextLvl.onClick.AddListener(delegate
        {
            if (Time.time - _lastButtonClickTime < _buttonClickCooldown)
                return;
            _lastButtonClickTime = Time.time;
            TweenLevelFinishMenu(false);
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact);
            StartCoroutine(SwitchScreen_LevelsFromGame());
        });

        Btn_LevelFinish_Back.onClick.AddListener(delegate
        {
            if (Time.time - _lastButtonClickTime < _buttonClickCooldown)
                return;
            _lastButtonClickTime = Time.time;

            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact);

            TweenLevelFinishMenu(false);
            StartCoroutine(GoToMenuFromFinishMenu());
        });

        // Levels Section
        B_LevelsMenuBack.onClick.AddListener(delegate
        {
            if (Time.time - _lastButtonClickTime < _buttonClickCooldown)
                return;
            _lastButtonClickTime = Time.time;

            PlayerMenuBike.SetActive(true);
            MenuPlatformObject.SetActive(true);

            PlayerMenuBikeRb.isKinematic = false;
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact);
            TweenLevelsSection(false);
            TweenGameLogo(true);
            GoToMainMenu();
        });

        // Shop Section
        Btn_Shop_BackMenu.onClick.AddListener(delegate
        {
            if (Time.time - _lastButtonClickTime < _buttonClickCooldown + 1)
                return;
            _lastButtonClickTime = Time.time;

            RefreshTextValuesFromPlayerData();
            TweenShopMenu(false);
            TweenMainMenu(true);
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact);

            var _playerData = SaveSystem.LoadPlayerData();
            var _bikeList = BikeController.GetAllBikes();
            var _trailList = TrailManager.GetAllTrails();
            ShopManager.SelectBike(ShopManager.CurrentBikeIndex);
            ShopManager.SelectTrail(ShopManager.CurrentTrailIndex);
            PlayerMenuBike = ShopManager.DisplayBikePrefab(_bikeList[_playerData.SELECTED_BIKE_ID].BikePrefab);
            PlayerMenuBikeRb = PlayerMenuBike.GetComponent<Rigidbody2D>();
            ShopManager.DisplayTrailPrefab(_trailList[_playerData.SELECTED_TRAIL_ID].TrailPrefab);
            CameraController.SwitchToMenuCamera();
        });

        // Settings
        B_SettingsBackMenu.onClick.AddListener(delegate
        {
            if (Time.time - _lastButtonClickTime < _buttonClickCooldown + 1)
                return;
            _lastButtonClickTime = Time.time;

            TweenGameLogo(true);
            TweenSettingsMenu(false);
            TweenMainMenu(true);
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact);
            var _playerData = SaveSystem.LoadPlayerData();
            _playerData.UpdateLevel();
            CameraController.SwitchToMenuCamera();
        });

        Settings_Toggle_Mute.onValueChanged.AddListener(delegate { OnMuteToggleClick(); });
        Settings_Toggle_Haptic.onValueChanged.AddListener(delegate { OnHapticToggleClick(); });

        Settings_Slider_MainVol.onValueChanged.AddListener(delegate { OnSliderMainVolumeValueChanged(Settings_Slider_MainVol.value); });
        Settings_Slider_SFXVol.onValueChanged.AddListener(delegate { OnSliderSFXVolumeValueChanged(Settings_Slider_SFXVol.value); });

        // Dev tools buttons
        btn_AddCoins.onClick.AddListener(delegate
        {
            var _data = SaveSystem.LoadPlayerData();
            Debug.Log("Coins before save: " + _data.COINS);
            HapticPatterns.PlayEmphasis(0.6f, 1.0f);
            Debug.Log("Coins before: " + _data.COINS);
            _data.COINS += 1231;
            SaveSystem.SavePlayerData(_data);
            Debug.Log("Coins after save: " + _data.COINS);
            RefreshTextValuesFromPlayerData();
        });

        btn_ResetCoins.onClick.AddListener(delegate
        {
            var _data = SaveSystem.LoadPlayerData();
            HapticPatterns.PlayEmphasis(0.6f, 1.0f);
            _data.UpdateLevel();
            Debug.Log("TotalPlayTime: " + _data.TOTAL_PLAYTIME + " current: " + GameManager.CurrentPlayTime);

            SaveSystem.SavePlayerData(_data);
            RefreshTextValuesFromPlayerData();
        });

        btn_ResetSavedata.onClick.AddListener(delegate
        {
            SaveSystem.ResetSaveFile();
            HapticPatterns.PlayEmphasis(0.6f, 1.0f);
            RefreshTextValuesFromPlayerData();
        });

        btn_UnlockAll.onClick.AddListener(delegate
        {
            var _playerData = SaveSystem.LoadPlayerData();
            Bike[] allBikes = BikeController.GetAllBikes();
            Trail[] allTrails = TrailManager.GetAllTrails();
            HapticPatterns.PlayEmphasis(0.6f, 1.0f);

            _playerData.UNLOCKED_BIKES = new int[allBikes.Length];
            _playerData.UNLOCKED_TRAILS = new int[allTrails.Length];

            for (int i = 0; i < allBikes.Length; i++)
                _playerData.UNLOCKED_BIKES[i] = allBikes[i].ID;
            for (int i = 0; i < allTrails.Length; i++)
                _playerData.UNLOCKED_TRAILS[i] = allTrails[i].ID;

            SaveSystem.SavePlayerData(_playerData);
            RefreshTextValuesFromPlayerData();
        });

        btn_AddXP.onClick.AddListener(delegate
        {
            var _playerData = SaveSystem.LoadPlayerData();
            int addedXP = 5167;
            _playerData.TOTAL_XP += addedXP;
            SaveSystem.SavePlayerData(_playerData);
            HapticPatterns.PlayEmphasis(0.6f, 1.0f);
            StatsLevelProgressRefresh();
            SaveSystem.SavePlayerData(_playerData);
        });

        btn_ShuffleSoundtrack.onClick.AddListener(delegate
        {
            HapticPatterns.PlayEmphasis(0.6f, 1.0f);
            AudioManager.PlayNextTrack();
        });

        Btn_LB_Back.onClick.AddListener(delegate
        {
            if (GameManager.gameState == GameState.Menu)
            {
                TweenLevelsSection(true);
                TweenLeaderboard(false);
            }
            else
            {
                TweenLevelFinishMenu(true);
                TweenLeaderboard(false);
            }
        });

        Btn_ReconnectPlayfab.onClick.AddListener(delegate
        {
            LeaderboardManager.Instance.PlayFabLogin();
        });

        Btn_NewUsernameSave.onClick.AddListener(delegate
        {
            string name = Input_NewUsername.text;
            if (!string.IsNullOrEmpty(name) && name.Length >= 3 && name.Length <= 14)
            {
                LeaderboardManager.Instance.UpdateDisplayName(name);
                Txt_UsernamePlaceholder.text = name;
                Txt_PlayerName.text = name;
                LeaderboardManager.PlayFabLogin();
            }
            else
            {
                Debug.Log("Invalid new name.");
            }
        });

        Toggle_GameSettings.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                if (Toggle_OnlineSettings.isOn)
                {
                    Toggle_OnlineSettings.isOn = false;
                }
                Section_GameSettings.SetActive(true);
                Section_OnlineSettings.SetActive(false);
            }
        });

        Toggle_OnlineSettings.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                if (Toggle_GameSettings.isOn)
                {
                    Toggle_GameSettings.isOn = false;
                }
                Section_GameSettings.SetActive(false);
                Section_OnlineSettings.SetActive(true);
            }
        });

        Btn_SyncOfflineData.onClick.AddListener(delegate
        {
            LeaderboardManager.Instance.UpdateLeaderboardFromOfflinePlay();
        }); 
        #endregion

    }

    void FixedUpdate()
    {
        if (GameManager.gameState == GameState.Menu)
        {
            if (!hasAnimatedIn)
            {
                int randomInt = UnityEngine.Random.Range(1, 3);
                AnimationWheelieType randomType = (AnimationWheelieType)randomInt;
                StartCoroutine(AnimateBikeIn(randomType));
                hasAnimatedIn = true;
            }
            else
            {
                PreviewPlayerBike();
            }
        }
    }

    private void Update()
    {
        // PLAYFAB
        if (LeaderboardManager.IsLoggedIn)
        {
            Online_Status.SetActive(true);
            Offline_Status.SetActive(false);
        }
        else
        {
            Online_Status.SetActive(false);
            Offline_Status.SetActive(true);
        }
    }

    public void StatsLevelProgressRefresh()
    {
        var _data = SaveSystem.LoadPlayerData();
        Settings_Slider_LevelProgress.maxValue = 1;
        Settings_Slider_LevelProgress.value = 0;
        LeanTween.value(gameObject, 0, _data.GetCurrentXPProgress(), 1f).setEaseInOutSine().setOnUpdate((float value) =>
        {
            Settings_Slider_LevelProgress.value = value;
        }).setOnComplete(() =>
        {
            UpdateXPBar();
        });

        SaveSystem.SavePlayerData(_data);
        RefreshTextValuesFromPlayerData();
    }

    public void UpdateXPBar()
    {
        var _playerData = SaveSystem.LoadPlayerData();
        float currentLevelXP = _playerData.TOTAL_XP - _playerData.XPForLevel(_playerData.PLAYER_LEVEL);
        float nextLevelXP = _playerData.XPForLevel(_playerData.PLAYER_LEVEL + 1) - _playerData.XPForLevel(_playerData.PLAYER_LEVEL);

        // SliderLevelProgress.maxValue = 1; 
        // SliderLevelProgress.value = currentLevelXP / nextLevelXP;

        Txt_PlayerLevel.text = "Level: " + _playerData.PLAYER_LEVEL.ToString() + "/100";
        Txt_PlayerXP.text = _playerData.TOTAL_XP.ToString() + " XP";

        SaveSystem.SavePlayerData(_playerData);
    }

    private void OnMuteToggleClick()
    {
        if (Time.time - _lastButtonClickTime < _buttonClickCooldown - 1f)
            return;
        _lastButtonClickTime = Time.time;

        HapticPatterns.PlayPreset(HapticPatterns.PresetType.RigidImpact);
        PlayerData = SaveSystem.LoadPlayerData();
        bool isOn = !PlayerData.SETTINGS_isMuted;

        if (isOn)
        {
            AudioManager.MainAudioSource.volume = 0;
            AudioManager.SFXAudioSource.volume = 0;
            PlayerData.SETTINGS_mainVolume = 0;
            PlayerData.SETTINGS_sfxVolume = 0;
            Settings_Slider_MainVol.value = 0f;
            Settings_Slider_SFXVol.value = 0f;
            PlayerData.SETTINGS_isMuted = true;

            Settings_Slider_MainVol.interactable = false;
            Settings_Slider_SFXVol.interactable = false;

            Color currentColor = ToggleMuteBG.color;
            ToggleMuteBG.color = new Color(currentColor.r, currentColor.g, currentColor.b, 0.85f);
            Txt_ToggleMuteStatus.text = "Unmute";
        }
        else
        {
            AudioManager.MainAudioSource.volume = 0.45f;
            AudioManager.SFXAudioSource.volume = 0.45f;
            Settings_Slider_MainVol.value = 0.45f;
            Settings_Slider_SFXVol.value = 0.45f;
            PlayerData.SETTINGS_isMuted = false;

            Settings_Slider_MainVol.interactable = true;
            Settings_Slider_SFXVol.interactable = true;

            Color currentColor = ToggleMuteBG.color;
            ToggleMuteBG.color = new Color(currentColor.r, currentColor.g, currentColor.b, 0.35f);
            Txt_ToggleMuteStatus.text = "Mute";
        }
        SaveSystem.SavePlayerData(PlayerData);
        UpdateMuteToggleImage();
    }

    private void OnHapticToggleClick()
    {
        if (Time.time - _lastButtonClickTime < _buttonClickCooldown - 1f)
            return;
        _lastButtonClickTime = Time.time;


        HapticPatterns.PlayPreset(HapticPatterns.PresetType.RigidImpact);
        PlayerData = SaveSystem.LoadPlayerData();
        bool isOn = PlayerData.SETTINGS_isHapticEnabled;

        if (isOn)
        {
            PlayerData.SETTINGS_isHapticEnabled = false;
            HapticController.hapticsEnabled = false;
            Color currentColor = ToggleHapticBG.color;
            ToggleHapticBG.color = new Color(currentColor.r, currentColor.g, currentColor.b, 0.30f);
        }
        else
        {
            PlayerData.SETTINGS_isHapticEnabled = true;
            HapticController.hapticsEnabled = true;
            Color currentColor = ToggleHapticBG.color;
            ToggleHapticBG.color = new Color(currentColor.r, currentColor.g, currentColor.b, 0.85f);
        }
        SaveSystem.SavePlayerData(PlayerData);
        UpdateHapticToggleImage();
        Debug.Log("Haptic: " + isOn + " Save: " + PlayerData.SETTINGS_isHapticEnabled);
    }

    private void OnSliderSFXVolumeValueChanged(float value)
    {
        var _playerData = SaveSystem.LoadPlayerData();
        AudioManager.SFXAudioSource.volume = value;
        _playerData.SETTINGS_sfxVolume = value;
        HapticPatterns.PlayConstant(value, 0.1f, 0.05f);
        SaveSystem.SavePlayerData(_playerData);
    }

    private void OnSliderMainVolumeValueChanged(float value)
    {
        var _playerData = SaveSystem.LoadPlayerData();
        AudioManager.MainAudioSource.volume = value;
        _playerData.SETTINGS_mainVolume = value;

        // haptic feedback
        float amplitude = Mathf.Clamp(value, 0, 1);
        HapticPatterns.PlayConstant(value, 0.1f, 0.05f);  // Short duration to mimic a brief 'buzz' feeling

        SaveSystem.SavePlayerData(_playerData);
    }

    private void UpdateMuteToggleImage()
    {
        Image toggleImage = ToggleMuteImage;
        if (toggleImage != null)
        {
            toggleImage.sprite = Settings_Toggle_Mute.isOn ? MuteSprite : UnmuteSprite;
        }
    }

    private void UpdateHapticToggleImage()
    {
        Image toggleImage = ToggleHapticImage;
        if (toggleImage != null)
        {
            toggleImage.sprite = Settings_Toggle_Haptic.isOn ? HapticOnSprite : HapticOffSprite;
        }
    }

    public void RefreshTextValuesFromPlayerData()
    {
        PlayerData = SaveSystem.LoadPlayerData();
        var _unlockedBikes = PlayerData.UNLOCKED_BIKES.Length;
        var _unlockedTrails = PlayerData.UNLOCKED_TRAILS.Length;

        // Menu
        Txt_Menu_Coins.text = PlayerData.COINS + "";
        Txt_Menu_XP.text = PlayerData.TOTAL_XP + "";
        Txt_Menu_Level.text = PlayerData.PLAYER_LEVEL + "";

        Txt_Menu_LvlsFinished.text = GameManager.GetFinishedLevelsCount(PlayerData) + "/" + LevelManager.Levels.Length;

        // Shop
        ShopManager.T_Coins.text = PlayerData.COINS + "";
        ShopManager.T_UnlockedBikes.text = _unlockedBikes
        + "/" + BikeController.GetAllBikes().Length + "";
        ShopManager.T_UnlockedTrails.text = _unlockedTrails
        + "/" + TrailManager.GetAllTrails().Length + "";

        // Settings (Stats)
        // stats1
        string text1 = $"Bikes owned: {PlayerData.UNLOCKED_BIKES.Length}/{BikeController.GetAllBikes().Length}\n";
        text1 += $"Trails owned: {PlayerData.UNLOCKED_TRAILS.Length}/{TrailManager.GetAllTrails().Length}\n";
        text1 += $"Levels finished: {GameManager.GetFinishedLevelsCount(PlayerData)}/{LevelManager.Levels.Length}\n";
        text1 += $"Level Faults: {PlayerData.TOTAL_FAULTS_ALL_LEVELS}\n";
        text1 += $"Distance: {PlayerData.TOTAL_DISTANCE.ToString("F2")}km\n";
        text1 += $"Playtime: {((int)(PlayerData.TOTAL_PLAYTIME / 3600))}h {((int)(PlayerData.TOTAL_PLAYTIME % 3600 / 60))}min\n";

        Txt_Stats1.text = text1;

        // stats2
        string text2 = $"Most Flips: {PlayerData.BEST_INTERNAL_FLIPS}\n";
        text2 += $"Total Flips: {PlayerData.TOTAL_FLIPS}\n";
        text2 += $"Most Wheelie: {PlayerData.BEST_SINGLE_WHEELIE.ToString("F2")}\n";
        text2 += $"Total Faults: {PlayerData.TOTAL_FAULTS}\n";
        text2 += $"Best Wheelie: {PlayerData.BEST_LEVEL_WHEELIE.ToString("F2")}\n";
        text2 += $"Total Wheelie: {PlayerData.TOTAL_WHEELIE.ToString("F2")}\n";
        Txt_Stats2.text = text2;


        // Level and XP
        Txt_PlayerLevel.text = "Level: " + PlayerData.PLAYER_LEVEL + "/100";
        Txt_PlayerXP.text = PlayerData.TOTAL_XP + " XP";
    }

    private void PreviewPlayerBike()
    {
        accelerationTimer += Time.fixedDeltaTime;
        // Calculate the current speed based on the elapsed time
        menuBikeSpeed = Mathf.Lerp(0, menuBikeMaxSpeed, accelerationTimer / accelerationTime);

        Vector2 force = new Vector2(menuBikeSpeed - PlayerMenuBikeRb.velocity.x, 0);
        PlayerMenuBikeRb.AddForce(force, ForceMode2D.Force);


        // Maintain positive bike rotation after coroutines.
        if ((BikeAnimationInCoroutine == null && BikeAnimationOutCoroutine == null))
        {
            float rotationAngle = menuBikeSpeed * 1.25f; // Adjust the factor as needed
            PlayerMenuBikeRb.MoveRotation(rotationAngle);
        }

        // Pin platform to payer bike
        if (PlayerMenuBikeRb != null)
        {
            bikePosition = PlayerMenuBikeRb.position;
            float platformBaseSpeed = 5.0f;  // adjust the value as needed
            float platformSpeed = platformBaseSpeed * menuBikeSpeed;  // platform speed is relative to the bike's speed
            float step = platformSpeed * Time.deltaTime; // calculate distance to move

            Vector2 targetPos = new Vector2(bikePosition.x - 4, MenuPlatformRb.position.y);
            Vector2 newPlatformPosition = Vector2.MoveTowards(MenuPlatformRb.position, targetPos, step);

            MenuPlatformRb.position = newPlatformPosition;
        }
    }

    public float GetStartTransitionDuration()
    {
        return startTransitionDuration;
    }

    public float GetEndTransitionDuration()
    {
        return endTransitionDuration;
    }

    private void GoToMainMenu()
    {
        TweenMainMenu(true);
        BackgroundParalax.ResetParallax();
        GameManager.SetGameState(GameState.Menu);
        PlayerMenuBikeRb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void GoToShop()
    {
        CameraController.SwitchToShopCamera();
        TweenMainMenu(false);
        TweenShopMenu(true);
    }

    public void PauseGame()
    {
        BikeController.PauseBike();
        GameManager.SetGameState(GameState.Paused);
        TweenPauseGame(true);
    }

    public void ResumeGame()
    {
        GameManager.SetGameState(GameState.Playing);
        BikeController.ResumeBike();
        TweenPauseGame(false);
    }

    public void TweenShopMenu(bool In)
    {
        if (In)
        {
            Panel_Shop.SetActive(true);
            LeanTween.moveX(Btn_Shop_RightBtn.GetComponent<RectTransform>(), 0, 0.75f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.moveX(Btn_Shop_LeftBtn.GetComponent<RectTransform>(), 0, 0.75f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.scale(Btn_Shop_BuyButton.GetComponent<RectTransform>(), new Vector3(1.40f, 1.40f, 1.40f), 0.75f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.moveX(ShopSelectionObject.GetComponent<RectTransform>(), 0, 0.55f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.moveY(TopOverlayHeader.GetComponent<RectTransform>(), 56f, 0.4f).setEase(LeanTweenType.easeOutExpo).setDelay(0.2f);
            LeanTween.moveY(Btn_Shop_BackMenu.GetComponent<RectTransform>(), -85f, 0.9f).setEase(LeanTweenType.easeOutExpo).setDelay(0.2f);
        }
        else
        {
            LeanTween.moveY(Btn_Shop_BackMenu.GetComponent<RectTransform>(), -400f, 0.35f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveX(Btn_Shop_RightBtn.GetComponent<RectTransform>(), 800f, 0.35f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveX(Btn_Shop_LeftBtn.GetComponent<RectTransform>(), -800f, 0.35f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.scale(Btn_Shop_BuyButton.GetComponent<RectTransform>(), new Vector3(0f, 0f, 0f), 0.35f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveX(ShopSelectionObject.GetComponent<RectTransform>(), 1100f, 0.55f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveY(TopOverlayHeader.GetComponent<RectTransform>(), 700f, 0.4f).setEase(LeanTweenType.easeOutExpo).
            setOnComplete(
                delegate ()
                {
                    Panel_Shop.SetActive(false);
                }
            );
        }
    }

    public void TweenLevelsSection(bool In)
    {
        if (In)
        {
            Panel_Levels.SetActive(true);
            LeanTween.moveX(LevelsBG.GetComponent<RectTransform>(), 0f, 0.75f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveX(LevelsSection.GetComponent<RectTransform>(), -20f, 0.75f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.moveY(B_LevelsMenuBack.GetComponent<RectTransform>(), -85f, 0.55f).setEase(LeanTweenType.easeOutExpo).setDelay(0.2f);
        }
        else
        {
            LeanTween.moveX(LevelsBG.GetComponent<RectTransform>(), 1100f, 0.45f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveX(LevelsSection.GetComponent<RectTransform>(), -800f, 0.45f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveY(B_LevelsMenuBack.GetComponent<RectTransform>(), -350f, 0.45f).setEase(LeanTweenType.easeOutExpo).setOnComplete(
            delegate ()
            {
                Panel_Levels.SetActive(false);
            });
        }
    }

    public void TweenSettingsMenu(bool In)
    {
        if (In)
        {
            Txt_UsernamePlaceholder.text = LeaderboardManager.Instance.PlayerDisplayName;
            Settings_Slider_LevelProgress.value = 0;
            Panel_Settings.SetActive(true);
            LeanTween.moveX(Settings_Stats.GetComponent<RectTransform>(), 0, 0.5f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f).setOnComplete(delegate () { StatsLevelProgressRefresh(); });
            LeanTween.moveX(Settings_Main.GetComponent<RectTransform>(), -0, 0.50f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.moveX(temp_PanelDev.GetComponent<RectTransform>(), 0, 0.5f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.moveY(B_SettingsBackMenu.GetComponent<RectTransform>(), -85f, 0.55f).setEase(LeanTweenType.easeOutExpo).setDelay(0.2f);
        }
        else
        {
            LeanTween.moveX(Settings_Stats.GetComponent<RectTransform>(), 1100f, 0.45f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveX(Settings_Main.GetComponent<RectTransform>(), -1100f, 0.45f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveX(temp_PanelDev.GetComponent<RectTransform>(), 1100, 0.45f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveY(B_SettingsBackMenu.GetComponent<RectTransform>(), -400f, 0.5f).setEase(LeanTweenType.easeOutExpo).
            setOnComplete(
                delegate ()
                {
                    Panel_Settings.SetActive(false);
                }
            );
        }
    }

    public void LoadLevelsScreen(bool In)
    {
        if (!Panel_Levels.activeSelf)
        {
            Panel_Levels.SetActive(true);
        }
        TweenGameLogo(false);
        TweenLevelsSection(true);

        // Destroy existing levels
        foreach (GameObject instantiatedLevel in instantiatedLevels)
        {
            Destroy(instantiatedLevel);
        }

        // Clear the list
        instantiatedLevels.Clear();

        int lvlID = 0;
        foreach (var item in LevelManager.Levels)
        {
            GameObject levelUnitInstance = Instantiate(levelUnitPrefab, LevelsView);
            TextMeshProUGUI[] childTexts = levelUnitInstance.GetComponentsInChildren<TextMeshProUGUI>();

            LevelEntry levelEntry = levelUnitInstance.GetComponent<LevelEntry>();

            levelEntry.SetLevel(item.LevelID, item.LevelCategory);

            levelEntry.Txt_LevelName.text = "Level " + (item.LevelID + 1);

            // Get LevelStats for this level
            var _playerData = SaveSystem.LoadPlayerData();

            LevelStats levelStats = _playerData.GetLevelStats(item.LevelCategory, item.LevelID);

            int _trophyCount = 0;

            if (levelStats != null)
                _trophyCount = LevelManager.Levels[item.LevelID].CalculateTrophies(levelStats.Time, levelStats.Faults);


            switch (_trophyCount)
            {
                case 3:
                    //Debug.Log("3 trophies");
                    levelEntry.Trophy1.SetActive(true);
                    levelEntry.Trophy2.SetActive(true);
                    levelEntry.Trophy3.SetActive(true);
                    break;
                case 2:
                    levelEntry.Trophy1.SetActive(true);
                    levelEntry.Trophy2.SetActive(true);
                    break;
                case 1:
                    levelEntry.Trophy1.SetActive(true);
                    break;
                case 0:

                    break;
                default:
                    break;
            }

            if (levelStats != null)
            {
                var timeSpan = TimeSpan.FromSeconds(levelStats.Time);
                levelEntry.Txt_Timer.text = string.Format("{0:D2}:{1:D2}:{2:D2}",
                    timeSpan.Minutes, timeSpan.Seconds, (int)Math.Round(timeSpan.Milliseconds / 10.0));


                levelEntry.Txt_Faults.text = levelStats.Faults.ToString();
            }
            else
            {
                // Display default values
                levelEntry.Txt_Timer.text = "N/A";
                levelEntry.Txt_Faults.text = "N/A";
            }

            // Add this level to our list
            instantiatedLevels.Add(levelUnitInstance);

            lvlID++;
        }
    }

    public void TweenPauseGame(bool In)
    {
        if (In)
        {
            Panel_Paused.SetActive(true);
            LeanTween.moveX(Btn_HUD_PauseGame.GetComponent<RectTransform>(), -750f, 0.5f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.alpha(Overlay_Paused.GetComponent<RectTransform>(), 0.5f, 0.5f);
            LeanTween.moveX(B_Paused_Resume.GetComponent<RectTransform>(), 5f, 0.55f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveX(B_Paused_Restart.GetComponent<RectTransform>(), -5f, 0.75f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveY(B_Paused_Menu.GetComponent<RectTransform>(), -85f, 0.5f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveY(T_PausedText.GetComponent<RectTransform>(), -540f, 0.55f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
        }
        else
        {
            LeanTween.alpha(Overlay_Paused.GetComponent<RectTransform>(), 0f, 0.5f);
            LeanTween.moveX(B_Paused_Resume.GetComponent<RectTransform>(), 550f, 0.45f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveX(B_Paused_Restart.GetComponent<RectTransform>(), -550f, 0.45f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveY(B_Paused_Menu.GetComponent<RectTransform>(), -370f, 0.45f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveY(T_PausedText.GetComponent<RectTransform>(), 125f, 0.45f).setEase(LeanTweenType.easeOutExpo).setOnComplete(
            delegate ()
            {
                Panel_Paused.SetActive(false);
                LeanTween.moveX(Btn_HUD_PauseGame.GetComponent<RectTransform>(), -470f, 0.8f).setEase(LeanTweenType.easeOutExpo).setDelay(1f);
            });
        }
    }

    public void TweenLevelFinishMenu(bool In)
    {
        if (In)
        {
            Panel_GameOver.SetActive(true);
            LeanTween.alpha(LevelFinishOverlay.GetComponent<RectTransform>(), 0.5f, 0.5f).setDelay(0.5f)
            .setOnComplete(delegate ()
            {
                LeanTween.moveY(FinishStatsPanel.GetComponent<RectTransform>(), 90, 0.75f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
                LeanTween.moveX(Btn_Finish_Restart.GetComponent<RectTransform>(), 0f, 0.75f).setEase(LeanTweenType.easeOutExpo).setDelay(1f);
                LeanTween.moveX(Btn_Finish_NextLvl.GetComponent<RectTransform>(), 0f, 0.75f).setEase(LeanTweenType.easeOutExpo).setDelay(1f);
                LeanTween.scale(Btn_Finish_Leaderboard.GetComponent<RectTransform>(), new Vector3(1.45f, 1.45f, 1.45f), 0.75f).setEase(LeanTweenType.easeOutExpo).setDelay(1f);
                LeanTween.moveY(Txt_LevelFinish_XPGained.GetComponent<RectTransform>(), -280f, 0.35f).setEase(LeanTweenType.easeOutExpo).setDelay(1f);
                LeanTween.value(Txt_LevelFinish_XPGained.gameObject, 0, PlayerData.CalculateXpForLevel(CurrentLevelStats), 2f).setEase(LeanTweenType.easeOutExpo).setDelay(1f).setOnUpdate((float value) =>
                {
                    Txt_LevelFinish_XPGained.text = Mathf.RoundToInt(value).ToString() + " XP GAINED!";
                    // Calculate haptic strength based on current value
                    float hapticStrength = value / PlayerData.CalculateXpForLevel(CurrentLevelStats);
                    // Play haptic feedback
                    HapticPatterns.PlayConstant(hapticStrength, 0.1f, 0.05f); // Juice
                });
                LeanTween.moveY(Btn_LevelFinish_Back.GetComponent<RectTransform>(), -85f, 0.75f).setEase(LeanTweenType.easeOutExpo).setDelay(1f);
            });
        }
        else
        {
            LeanTween.alpha(LevelFinishOverlay.GetComponent<RectTransform>(), 0f, 0.3f)
            .setOnComplete(
            delegate ()
            {
                LeanTween.moveY(FinishStatsPanel.GetComponent<RectTransform>(), +1500, 0.75f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
                LeanTween.moveX(Btn_Finish_Restart.GetComponent<RectTransform>(), -400f, 0.55f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
                LeanTween.moveX(Btn_Finish_NextLvl.GetComponent<RectTransform>(), 400f, 0.55f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
                LeanTween.moveY(Btn_LevelFinish_Back.GetComponent<RectTransform>(), -400f, 0.75f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
                LeanTween.scale(Btn_Finish_Leaderboard.GetComponent<RectTransform>(), new Vector3(0f, 0f, 0.65f), 0.5f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
                LeanTween.moveY(Txt_LevelFinish_XPGained.GetComponent<RectTransform>(), 400f, 0.75f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f)
                    .setOnComplete(delegate ()
                    { Panel_GameOver.SetActive(false); });
            });

        }
    }

    public void TweenGameHUD(bool In)
    {
        if (In)
        {
            Panel_GameHUD.SetActive(true);
            LeanTween.moveX(HUD_TimerBar.GetComponent<RectTransform>(), -40, 0.75f).setEase(LeanTweenType.easeOutExpo).setDelay(1f);
            LeanTween.moveX(HUD_WheelieBar.GetComponent<RectTransform>(), -92f, 0.85f).setEase(LeanTweenType.easeOutExpo).setDelay(1.2f);
            LeanTween.moveX(HUD_FlipsBar.GetComponent<RectTransform>(), -145f, 0.95f).setEase(LeanTweenType.easeOutExpo).setDelay(1.3f);
            LeanTween.moveX(HUD_FaultsBar.GetComponent<RectTransform>(), 142f, 0.9f).setEase(LeanTweenType.easeOutExpo).setDelay(1.4f);
            LeanTween.moveX(Btn_HUD_PauseGame.GetComponent<RectTransform>(), -470f, 0.8f).setEase(LeanTweenType.easeOutExpo).setDelay(1.5f);
        }
        else
        {
            LeanTween.moveX(HUD_TimerBar.GetComponent<RectTransform>(), -400f, 0.5f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveX(HUD_WheelieBar.GetComponent<RectTransform>(), -400f, 0.5f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveX(HUD_FlipsBar.GetComponent<RectTransform>(), -400f, 0.5f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveX(HUD_FaultsBar.GetComponent<RectTransform>(), 400f, 0.5f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveX(Btn_HUD_PauseGame.GetComponent<RectTransform>(), -870f, 0.5f).setEase(LeanTweenType.easeOutExpo)
            .setOnComplete(
            delegate ()
            {
                Panel_GameHUD.SetActive(false);
            });
        }
    }

    public void TweenGameLogo(bool In)
    {
        if (In)
        {
            MENU_GameLogo.SetActive(true);
            LeanTween.moveY(MENU_GameLogo.GetComponent<RectTransform>(), -150f, 0.5f).setEase(LeanTweenType.easeInOutQuad);
            LeanTween.scale(MENU_GameLogo, new Vector2(0.45f, 0.45f), 0.7f)
            .setEaseOutBounce()
            .setOnComplete(() =>
            {
                LeanTween.scale(MENU_GameLogo, new Vector2(0.47f, 0.47f), 0.5f).setEase(LeanTweenType.easeInOutQuad)
                    .setDelay(0.3f);
            });
        }
        else
        {
            LeanTween.moveY(MENU_GameLogo.GetComponent<RectTransform>(), 450f, 0.8f).setEase(LeanTweenType.easeOutExpo)
            .setOnComplete(() =>
            {
                MENU_GameLogo.SetActive(false);
            });
        }
    }

    public void TweenMainMenu(bool In)
    {
        if (In)
        {
            Panel_MainMenu.SetActive(true);
            MenuPlatformObject.SetActive(true);
            CameraController.SwitchToMenuCamera();
            LeanTween.alpha(Overlay_Menu.GetComponent<RectTransform>(), 0.5f, 0.5f).setEaseInExpo();
            LeanTween.moveY(Btn_Menu_Start.GetComponent<RectTransform>(), 80f, 0.5f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveY(Btn_Menu_MainLeaderboard.GetComponent<RectTransform>(), -140f, 0.55f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveY(Btn_Menu_Shop.GetComponent<RectTransform>(), -310f, 0.55f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveY(Btn_Menu_Settings.GetComponent<RectTransform>(), -140f, 0.55f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveX(MENU_CoinsBar.GetComponent<RectTransform>(), -82f, 0.75f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveX(MENU_LvlsFinishedBar.GetComponent<RectTransform>(), -154f, 0.75f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveY(Btn_Menu_About.GetComponent<RectTransform>(), -130f, 0.55f).setEase(LeanTweenType.easeInOutSine).setDelay(0.2f);
            LeanTween.moveX(PlayFabStatusParent.GetComponent<RectTransform>(), 0, 0.55f).setEase(LeanTweenType.easeInOutSine);
            LeanTween.moveX(MENU_XP.GetComponent<RectTransform>(), 50, 0.55f).setEase(LeanTweenType.easeInOutSine);
            LeanTween.moveX(MENU_Level.GetComponent<RectTransform>(), 119, 0.55f).setEase(LeanTweenType.easeInOutSine);
        }
        else
        {
            LeanTween.alpha(Overlay_Menu.GetComponent<RectTransform>(), 0f, 0.5f);
            LeanTween.moveX(MENU_CoinsBar.GetComponent<RectTransform>(), -550f, 0.35f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveY(Btn_Menu_About.GetComponent<RectTransform>(), -450f, 0.35f).setEase(LeanTweenType.easeInOutExpo);
            LeanTween.moveX(MENU_LvlsFinishedBar.GetComponent<RectTransform>(), -550f, 0.35f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveY(Btn_Menu_Shop.GetComponent<RectTransform>(), -1350f, 0.35f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveY(Btn_Menu_Settings.GetComponent<RectTransform>(), -1350f, 0.35f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveY(Btn_Menu_MainLeaderboard.GetComponent<RectTransform>(), -1350f, 0.35f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveX(PlayFabStatusParent.GetComponent<RectTransform>(), 400f, 0.15f).setEase(LeanTweenType.easeInOutSine);
            LeanTween.moveX(MENU_XP.GetComponent<RectTransform>(), 450, 0.15f).setEase(LeanTweenType.easeInOutSine);
            LeanTween.moveX(MENU_Level.GetComponent<RectTransform>(), 400, 0.15f).setEase(LeanTweenType.easeInOutSine);
            LeanTween.moveY(Btn_Menu_Start.GetComponent<RectTransform>(), -1350f, 0.35f).setEase(LeanTweenType.easeOutExpo).setOnComplete(
                delegate ()
                {
                    Panel_MainMenu.SetActive(false);
                });
        }
    }

    public void TweenLeaderboard(bool In)
    {
        if (In)
        {
            Panel_Leaderboard.SetActive(true);
            LeanTween.moveX(LBMainPanel.GetComponent<RectTransform>(), 0, 0.55f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveY(Btn_LB_Back.GetComponent<RectTransform>(), -85, 0.55f).setEase(LeanTweenType.easeOutExpo);
        }
        else
        {
            LeanTween.moveX(LBMainPanel.GetComponent<RectTransform>(), 1200, 0.55f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveY(Btn_LB_Back.GetComponent<RectTransform>(), -450, 0.55f).setEase(LeanTweenType.easeOutExpo).setOnComplete(
                delegate ()
                {
                    Panel_Leaderboard.SetActive(false);
                });
        }
    }

    public void TweenWelcomePanel(bool In)
    {
        if (In)
        {
            Panel_PlayerProfile.SetActive(true);
            LeanTween.alpha(OverlayUsernamePanel.GetComponent<RectTransform>(), 0.75f, 2f).setEaseInExpo().setDelay(0.1f)
                .setOnComplete(delegate ()
                {
                    LeanTween.rotateX(UsernamePanel, 0, 1.5f).setEase(LeanTweenType.easeOutExpo);
                });
        }
        else
        {
            LeanTween.rotateX(UsernamePanel, 90, 0.5f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.scale(UsernamePanel, new Vector3(0,0,0), 0.5f).setDelay(0.1f);
            LeanTween.alpha(OverlayUsernamePanel.GetComponent<RectTransform>(), 0f, 0.5f).setDelay(0.1f).setEaseInExpo();
        }
    }


    public void TweenAboutSection(bool In)
    {
        if (In)
        {
            Panel_About.SetActive(true);
            TweenGameLogo(false);
            LeanTween.moveX(AboutPanel.GetComponent<RectTransform>(), 0, 0.55f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveY(Btn_About_Back.GetComponent<RectTransform>(), -85f, 0.85f).setEase(LeanTweenType.easeInOutSine).setDelay(0.1f);
        }
        else
        {
            LeanTween.moveX(AboutPanel.GetComponent<RectTransform>(), 1100, 0.45f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.moveY(Btn_About_Back.GetComponent<RectTransform>(), -450f, 0.45f).setEase(LeanTweenType.easeInOutSine)
            .setOnComplete(delegate ()
            {
                Panel_About.SetActive(false);
                TweenGameLogo(true);
            });
        }
    }

    public void OnLevelFinish()
    {
        GameManager.SetGameState(GameState.Finished);
        // TODO: Stop player in a nicer way. Smooth out velocity to 0.
        BikeController.bikeRearWheelJoint.useMotor = false;

        HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact);
        var _playerData = SaveSystem.LoadPlayerData();
        var _currentLevelID = LevelManager.CurrentLevelID;
        var _levels = LevelManager.Levels;
        var _LevelTime = GameManager.LevelTimer;
        var _LevelFaults = BikeController.faults;
        var _LevelFlips = BikeController.flipCount;
        var _LevelWheelie = BikeController.wheeliePoints;
        var _trophyCount = _levels[_currentLevelID].CalculateTrophies(_LevelTime, _LevelFaults);

        _playerData.UpdateLevel();

        CurrentLevelStats = new LevelStats
        {
            Time = _LevelTime,
            Faults = _LevelFaults,
            Flips = _LevelFlips,
            Wheelie = _LevelWheelie,
            Trophies = _trophyCount
            // ADD OTHER STATS
        };


        int timeInMS = GameManager.ConvertSecondsToMilliseconds(_LevelTime);
        string leaderboardId = _levels[_currentLevelID].LevelCategory + "_" + _levels[_currentLevelID].LevelID;

        // SAVE DATA

        // Actions based on outcome
        Result result = _playerData.AddLevelStats(_levels[_currentLevelID].LevelCategory, _levels[_currentLevelID].LevelID, CurrentLevelStats);
        switch (result)
        {
            case Result.NoRecord:

                Debug.Log("No existing record");
                LeaderboardManager.SendAllStats(leaderboardId, timeInMS, _LevelFaults, _LevelFlips, _LevelWheelie);
                break;
            case Result.NewTimeRecord:

                Debug.Log("New time record");
                LeaderboardManager.SendAllStats(leaderboardId, timeInMS, _LevelFaults, _LevelFlips, _LevelWheelie);
                break;
            case Result.NewWheelieRecord:

                Debug.Log("New wheelie record");
                LeaderboardManager.SendAllStats(leaderboardId, timeInMS, _LevelFaults, _LevelFlips, _LevelWheelie);
                break;
            case Result.NewFlipsRecord:

                Debug.Log("New flips record");
                LeaderboardManager.SendAllStats(leaderboardId, timeInMS, _LevelFaults, _LevelFlips, _LevelWheelie);
                break;
        }


        if (_LevelFlips > _playerData.BEST_LEVEL_FLIPS)
        {
            _playerData.BEST_LEVEL_FLIPS = _LevelFlips;
        }

        if (_LevelWheelie > _playerData.BEST_LEVEL_WHEELIE)
        {
            _playerData.BEST_LEVEL_WHEELIE = _LevelWheelie;
        }

        switch (_trophyCount)
        {
            case 3:
                //Debug.Log("3 trophies");
                TrophyAnimation(Trophy1, 0.55f, 2f);
                TrophyAnimation(Trophy2, 0.375f, 2.25f);
                TrophyAnimation(Trophy3, 0.375f, 2.5f);
                break;
            case 2:
                //Debug.Log("2 trophies");
                TrophyAnimation(Trophy1, 0.55f, 2f);
                TrophyAnimation(Trophy2, 0.375f, 2.25f);
                break;
            case 1:
                //Debug.Log("1 trophy");
                TrophyAnimation(Trophy1, 0.55f, 2f);
                break;
            case 0:
                //Debug.Log("0 trophies");
                break;
            default:
                //Debug.LogError("Invalid trophy count");
                break;
        }

        GameManager.SavePlaytimeAndDistance();

        _playerData.TOTAL_FAULTS_ALL_LEVELS = _playerData.GetTotalFaultsOnFinishedLevels();
        SaveSystem.SavePlayerData(_playerData);

        //Level.Category currentLevelCategory = _levels[_currentLevelID].category;
        //LevelStats stats = _playerData.GetLevelStats(currentLevelCategory, _currentLevelID);
        //Debug.Log("Saved Data: " + stats.Time + " " + stats.Faults );

        TweenGameHUD(false);
        TweenLevelFinishMenu(true);

        // Convert the time back to seconds for display and format it as M:SS:MS
        TimeSpan _timeSpan = TimeSpan.FromSeconds(GameManager.LevelTimer);
        string _formattedTime = string.Format("{0}:{1:00}:{2:00}", _timeSpan.Minutes, _timeSpan.Seconds, _timeSpan.Milliseconds / 10);

        Txt_LevelFinish_LevelTime.text = _formattedTime;
        Txt_LevelFinish_Faults.text = GameManager.faultCountText.text;
        Txt_LevelFinish_Wheelie.text = GameManager.wheelieTimeText.text;
        Txt_LevelFinish_Flips.text = GameManager.flipCountText.text;
    }

    private void TrophyAnimation(GameObject trophy, float targetScale, float delay)
    {
        trophy.transform.localScale = Vector3.zero;
        LeanTween.scale(trophy, Vector3.one * targetScale, 0.5f).setEase(LeanTweenType.easeOutExpo).setDelay(delay)
        .setOnComplete(delegate ()
        {
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.RigidImpact);
        }
        );

    }

    public void ResetTrophiesDefaultScale()
    {
        Trophy1.transform.localScale = Vector3.zero;
        Trophy2.transform.localScale = Vector3.zero;
        Trophy3.transform.localScale = Vector3.zero;
    }

    private IEnumerator SwitchScreen_LevelsFromGame()
    {

        StartCoroutine(PlayTransition());
        yield return new WaitForSeconds(1f);
        LoadLevelsScreen(true);
    }

    private IEnumerator AnimateBikeIn(AnimationWheelieType wheelieType)
    {
        PlayerMenuBikeRb.constraints = RigidbodyConstraints2D.None;
        float _delay = 2.2f;
        float wheelieDuration = 0.8f + UnityEngine.Random.Range(-0.05f, 0.55f);
        float startRotation = 0;
        float endRotation = 55;
        float elapsedTime = 0;
        float maintainRotationTime = 5 + UnityEngine.Random.Range(-0.5f, 1.5f);


        BikeAnimationInCoroutine = StartCoroutine(CameraController.AnimateScreenX(-0.15f, 0.88f, wheelieDuration + maintainRotationTime));

        yield return new WaitForSeconds(_delay);

        while (elapsedTime < wheelieDuration)
        {
            elapsedTime += Time.deltaTime;
            float currentRotation = Mathf.SmoothStep(startRotation, endRotation, elapsedTime / wheelieDuration);
            PlayerMenuBikeRb.MoveRotation(currentRotation);
            // Continue moving the bike as normal
            PreviewPlayerBike();

            yield return null;
        }

        // After the wheelie, ensure the bike's rotation is upright
        PlayerMenuBikeRb.MoveRotation(endRotation);

        elapsedTime = 0;
        while (elapsedTime < maintainRotationTime)
        {
            elapsedTime += Time.deltaTime;

            // Calculate the rotation based on the wheelie type
            float rotation;
            switch (wheelieType)
            {
                case AnimationWheelieType.Static:
                    rotation = endRotation;
                    break;
                case AnimationWheelieType.Sine:
                    // Natural wheelie effect
                    rotation = endRotation + Mathf.Sin(elapsedTime * 2 * Mathf.PI) * 5 + UnityEngine.Random.Range(-1.5f, 1.8f);
                    break;
                case AnimationWheelieType.Cosine:
                    rotation = endRotation + Mathf.Cos(elapsedTime * 2 * Mathf.PI) * 5 + UnityEngine.Random.Range(-1.6f, 1.7f);
                    break;
                default:
                    rotation = endRotation;
                    break;
            }

            PlayerMenuBikeRb.MoveRotation(rotation);
            // Continue moving the bike as normal
            PreviewPlayerBike();
            yield return null;
        }

        if (BikeAnimationInCoroutine != null)
            StopCoroutine(BikeAnimationInCoroutine);

        BikeAnimationInCoroutine = null;
        BikeAnimationOutCoroutine = StartCoroutine(CameraController.AnimateScreenX(0.88f, 0.5f, 2));
        yield return new WaitForSeconds(1f);
        PlayerMenuBikeRb.constraints = RigidbodyConstraints2D.FreezeRotation;
        PlayerMenuBikeRb.SetRotation(1.25f);
    }

    public IEnumerator PlayStartTransition()
    {
        startTransition.SetActive(true);
        yield return new WaitForSeconds(startTransitionDuration);
    }

    public IEnumerator PlayEndTransition()
    {
        startTransition.SetActive(false);
        endTransition.SetActive(true);
        yield return new WaitForSeconds(GetEndTransitionDuration());
        endTransition.SetActive(false);
    }

    private IEnumerator GoToMenuFromFinishMenu()
    {
        StartCoroutine(PlayTransition());
        yield return new WaitForSeconds(0.5f);
        PlayerMenuBike.SetActive(true);
        MenuPlatformObject.SetActive(true);
        PlayerMenuBikeRb.constraints = RigidbodyConstraints2D.None;
        PlayerMenuBikeRb.transform.localPosition = new Vector2(0, 0.5f);
        MenuPlatformRb.transform.localPosition = new Vector2(-4, 0);
        PlayerMenuBikeRb.isKinematic = false;
        BackgroundParalax.ResetParallax();
        GoToMainMenu();
    }

    private IEnumerator GoToMenuFromGame()
    {
        StartCoroutine(PlayTransition());
        // Show Menu Platform & Bike
        PlayerMenuBike.SetActive(true);
        MenuPlatformObject.SetActive(true);
        PlayerMenuBikeRb.transform.localPosition = new Vector2(0, 0.5f);
        MenuPlatformRb.transform.localPosition = new Vector2(-4, 0);
        PlayerMenuBikeRb.isKinematic = false;
        yield return new WaitForSeconds(0.3f);
        TweenPauseGame(false);
        TweenGameHUD(false);
        GoToMainMenu();
        GameManager.ResetLevelStats();
        RefreshTextValuesFromPlayerData();
        BackgroundParalax.ResetParallax();
    }

    public IEnumerator PlayTransition()
    {
        StartCoroutine(PlayStartTransition());
        yield return new WaitForSeconds(startTransitionDuration - 0.5f);
        StartCoroutine(PlayEndTransition());
        yield return new WaitForSeconds(GetEndTransitionDuration());
    }

    public enum AnimationWheelieType
    {
        Static,
        Sine,
        Cosine
    }
}
