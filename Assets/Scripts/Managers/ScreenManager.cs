using System;
using System.Collections;
using System.Collections.Generic;
using Lofelt.NiceVibrations;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    #region Variables

    private float _lastButtonClickTime = 0f;
    private float _buttonClickCooldown = 0.75f;

    /////////////////////////////////////////////////////////////////////////////////////
    /////// LEVEL SELECTION MENU
    /////////////////////////////////////////////////////////////////////////////////////

    [Header("Levels Section")]
    public GameObject levelUnitPrefab;
    public GameObject LevelsBG;
    public GameObject LevelsSection;
    public Transform LevelsView;
    public Button B_LevelsMenuBack;
    private List<GameObject> instantiatedLevels = new List<GameObject>();


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

    /////////////////////////////////////////////////////////////////////////////////////
    /////// GAME HUD UI
    /////////////////////////////////////////////////////////////////////////////////////

    [Header("Game HUD Elements")]
    public Button Btn_HUD_PauseGame;
    public GameObject HUD_FaultsBar;
    public GameObject HUD_TimerBar;
    public GameObject HUD_WheelieBar;
    public GameObject HUD_FlipsBar;

    /////////////////////////////////////////////////////////////////////////////////////
    /////// MAIN MENU SCREEN
    /////////////////////////////////////////////////////////////////////////////////////

    [Header("Main Menu Elements")]
    public Button Btn_Menu_MainLeaderboard;
    public Button Btn_Menu_Start;
    public Button Btn_Menu_Settings;
    public Button Btn_Menu_Shop;
    public Button Btn_Menu_About;
    public GameObject MENU_CoinsBar;
    public GameObject MENU_GameLogo;
    public GameObject MENU_LvlsFinishedBar;
    public GameObject Overlay_Menu;
    public TextMeshProUGUI Txt_Menu_Coins;
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
    public Sprite MuteSprite, UnmuteSprite;
    public Image ToggleMuteImage, ToggleMuteBG, ToggleHapticBG;
    public Slider Settings_Slider_MainVol, Settings_Slider_SFXVol, Settings_Slider_LevelProgress;

    // Dev tools
    public Button btn_ResetCoins, btn_AddCoins, btn_ResetSavedata, btn_UnlockAll, btn_AddXP, btn_ShuffleSoundtrack;
    public Button B_SettingsBackMenu;

    /////////////////////////////////////////////////////////////////////////////////////
    /////// SHOP SCREEN MENU
    /////////////////////////////////////////////////////////////////////////////////////

    [Header("Shop Section")]
    public Button Btn_Shop_BackMenu;
    public Button Btn_Shop_RightBtn;
    public Button Btn_Shop_LeftBtn;
    public Button Btn_Shop_Trails;
    public Button Btn_Shop_Bikes;
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
    public Button Btn_LevelFinish_Leaderboard;
    public Button Btn_LevelFinish_Restart;
    public Button Btn_LevelFinish_NextLvl;
    public Button Btn_LevelFinish_Back;
    public GameObject FinishLevelStatsPanel;
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
    public string startTransitionName;
    public string endTransitionName;
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
    [NonSerialized] public TrailRenderer PlayerMenuBikeTrail;
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
        // Init Managers    private PlayerData PlayerData;
        BikeController = BikeController.Instance;
        TrailManager = TrailManager.Instance;
        ShopManager = ShopManager.Instance;
        CameraController = CameraController.Instance;
        LevelManager = LevelManager.Instance;
        AudioManager = AudioManager.Instance;
        GameManager = GameManager.Instance;
        BackgroundParalax = BackgroundParalax.Instance;
        PlayerData = SaveSystem.LoadPlayerData();
        Debug.Log("Loaded Bike ID : " + PlayerData.SELECTED_BIKE_ID);

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
        PlayerMenuBikeRb = PlayerMenuBike.GetComponent<Rigidbody2D>();
        ShopManager.DisplayTrailPrefab(selectedTrail);

        CameraController.MenuCamera.Follow = PlayerMenuBike.transform;
        CameraController.ShopCamera.Follow = PlayerMenuBike.transform;
        CameraController.SettingsCamera.Follow = PlayerMenuBike.transform;

        /////////////////////////////////////////////////////////////////////////////////////
        /////// INITIATE SETTINGS DATA
        /////////////////////////////////////////////////////////////////////////////////////

        Settings_Toggle_Mute.isOn = PlayerData.SETTINGS_isMuted;
        Settings_Toggle_Haptic.isOn = PlayerData.SETTINGS_isHapticEnabled;

        Settings_Toggle_Mute.onValueChanged.AddListener(OnMuteToggleClick);

        if(PlayerData.SETTINGS_isMuted)
        {
            AudioManager.MainAudioSource.volume = 0;
            AudioManager.SFXAudioSource.volume = 0;
            ToggleMuteBG.color = new Color(0,0,0,0.5f);
            Txt_ToggleMuteStatus.text = "Unmute";
        }

        else
        {
            AudioManager.MainAudioSource.volume = PlayerData.SETTINGS_mainVolume;
            AudioManager.SFXAudioSource.volume = PlayerData.SETTINGS_sfxVolume;
            ToggleMuteBG.color = new Color(0,0,0,0.2f);
            Txt_ToggleMuteStatus.text = "Mute";
        }
        UpdateMuteToggleImage();


        if(!Settings_Toggle_Haptic.isOn)
        {
            PlayerData.SETTINGS_isHapticEnabled = false;
            HapticController.hapticsEnabled = PlayerData.SETTINGS_isHapticEnabled;
            ToggleHapticBG.color = new Color(0,0,0,0.2f);
        }

        Settings_Slider_MainVol.value = PlayerData.SETTINGS_isMuted ? 0 : PlayerData.SETTINGS_mainVolume;
        Settings_Slider_SFXVol.value = PlayerData.SETTINGS_isMuted ? 0 : PlayerData.SETTINGS_sfxVolume;
        Settings_Slider_LevelProgress.interactable = false;
        // Stats Data
        RefreshTextValuesFromPlayerData();
        UpdateXPBar();

        /////////////////////////////////////////////////////////////////////////////////////
        /////// INITIAL UI POSITIONS
        /////////////////////////////////////////////////////////////////////////////////////

        #region UI Initial Position
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
            new Vector2(obj.x, obj.y-400);

        obj = Btn_Menu_Shop.transform.localPosition;
        Btn_Menu_Shop.transform.localPosition =
            new Vector2(obj.x, obj.y - 900f);

        obj = MENU_CoinsBar.transform.position;
        MENU_CoinsBar.transform.position =
            new Vector2(obj.x - 250f, obj.y);

        obj = MENU_LvlsFinishedBar.transform.position;
        MENU_LvlsFinishedBar.transform.position =
            new Vector2(obj.x - 220f, obj.y);

        obj = MENU_GameLogo.transform.localPosition;
        MENU_GameLogo.transform.localPosition =
            new Vector2(obj.x, obj.y+450);

        obj = Btn_About_Back.transform.localPosition;
        Btn_About_Back.transform.localPosition =
            new Vector2(obj.x, obj.y-500);

        obj = AboutPanel.transform.localPosition;
        AboutPanel.transform.localPosition =
            new Vector2(obj.x+900, obj.y);

        // Game HUD
        obj = Btn_HUD_PauseGame.transform.localPosition;
        Btn_HUD_PauseGame.transform.localPosition =
            new Vector2(obj.x - 300f, obj.y);

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
            new Vector2(-700f, obj.y); 


        // Shop Section
        obj = Btn_Shop_BackMenu.transform.localPosition;
        Btn_Shop_BackMenu.transform.localPosition =
            new Vector2(0, obj.y-400f); 

        obj = Btn_Shop_RightBtn.transform.localPosition;
        Btn_Shop_RightBtn.transform.localPosition =
            new Vector2(obj.x+400, obj.y); 

        obj = Btn_Shop_LeftBtn.transform.localPosition;
        Btn_Shop_LeftBtn.transform.localPosition =
            new Vector2(obj.x-400, obj.y); 

        obj = Btn_Shop_BuyButton.transform.localScale;
        Btn_Shop_BuyButton.transform.localScale =
            new Vector2(0, 0); 
            
        obj = TopOverlayHeader.transform.localPosition;
        TopOverlayHeader.transform.localPosition =
            new Vector2(0, obj.y+500f); 

        obj = ShopSelectionObject.transform.localPosition;
        ShopSelectionObject.transform.localPosition =
            new Vector2(obj.x-850, obj.y); 


        // Settings Section
        obj = Settings_Stats.transform.localPosition;
        Settings_Stats.transform.localPosition =
            new Vector2(obj.x+900, obj.y); 

        obj = Settings_Main.transform.localPosition;
        Settings_Main.transform.localPosition =
            new Vector2(obj.x-900, obj.y); 

        obj = temp_PanelDev.transform.localPosition;
        temp_PanelDev.transform.localPosition =
            new Vector2(obj.x+900, obj.y); 

        obj = B_SettingsBackMenu.transform.localPosition;
        B_SettingsBackMenu.transform.localPosition =
            new Vector2(obj.x, obj.y-400); 


        // Game Finish Section
        obj = FinishLevelStatsPanel.transform.localPosition;
        FinishLevelStatsPanel.transform.localPosition =
            new Vector2(obj.x, obj.y+1500); 

        obj = Btn_LevelFinish_Leaderboard.transform.localScale;
        Btn_LevelFinish_Leaderboard.transform.localScale =
            new Vector3(0,0,0); 

        obj = Btn_LevelFinish_Restart.transform.localPosition;
        Btn_LevelFinish_Restart.transform.localPosition =
            new Vector2(obj.x-400, obj.y); 

        obj = Btn_LevelFinish_NextLvl.transform.localPosition;
        Btn_LevelFinish_NextLvl.transform.localPosition =
            new Vector2(obj.x+400, obj.y); 

        obj = Btn_LevelFinish_Back.transform.localPosition;
        Btn_LevelFinish_Back.transform.localPosition =
            new Vector2(obj.x, obj.y-350); 

        obj = Txt_LevelFinish_XPGained.transform.localPosition;
        Txt_LevelFinish_XPGained.transform.localPosition =
            new Vector2(obj.x, obj.y+450); 

        // Trophies
        obj = Trophy1.transform.localScale;
        Trophy1.transform.localScale =
            new Vector3(0,0,0); 

        obj = Trophy2.transform.localScale;
        Trophy2.transform.localScale =
            new Vector3(0,0,0); 

        obj = Trophy3.transform.localScale;
        Trophy3.transform.localScale =
            new Vector3(0,0,0); 


        #endregion


        /////////////////////////////////////////////////////////////////////////////////////
        /////// ASSIGN BUTTON LISTENERS
        /////////////////////////////////////////////////////////////////////////////////////

        ///// Main Menu
        //////////////////////
        Btn_Menu_Start.onClick.AddListener(delegate {
            if(Time.time - _lastButtonClickTime < _buttonClickCooldown)
            return; 
            _lastButtonClickTime = Time.time;

            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact); 
            LoadLevelsScreen(true);
            GameManager.SavePlayTime();
        });


        Btn_Menu_Shop.onClick.AddListener(delegate 
        { 
            if(Time.time - _lastButtonClickTime < _buttonClickCooldown)
            return; 
            _lastButtonClickTime = Time.time;

            GoToShop(); 
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact); 
        });


        Btn_Menu_About.onClick.AddListener(delegate 
        { 
            if(Time.time - _lastButtonClickTime < _buttonClickCooldown)
            return; 
            _lastButtonClickTime = Time.time;

            TweenAboutSection(true);
            TweenMainMenu(false); 
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact); 
        });

        Btn_About_Back.onClick.AddListener(delegate 
        { 
            if(Time.time - _lastButtonClickTime < _buttonClickCooldown)
            return; 
            _lastButtonClickTime = Time.time;

            TweenAboutSection(false);
            TweenMainMenu(true); 
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact); 
        });


        Btn_LevelFinish_Leaderboard.onClick.AddListener(delegate {  HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact);  });
        Btn_Menu_Settings.onClick.AddListener(delegate {
                if(Time.time - _lastButtonClickTime < _buttonClickCooldown+1)
                return; 
                _lastButtonClickTime = Time.time;

                GameManager.SavePlayTime();
                RefreshTextValuesFromPlayerData();
                 HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact); 
                TweenMainMenu(false);
                TweenGameLogo(false);
                TweenSettingsMenu(true);
                CameraController.Instance.SwitchToSettingsCamera(); 
        });

        // Set Data
        Txt_Menu_Coins.text = "" + PlayerData.COINS;
        Txt_Menu_LvlsFinished.text = PlayerData.TOTAL_LEVELS_FINISHED + "/" + LevelManager.Levels.Length;

        // Paused Screen
        Btn_HUD_PauseGame.onClick.AddListener(delegate {
            if(Time.time - _lastButtonClickTime < _buttonClickCooldown)
            return; 
            _lastButtonClickTime = Time.time;

            PauseGame();
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.Warning);
            GameManager.SavePlayTime(); });

        B_Paused_Resume.onClick.AddListener( delegate 
        {
            if(Time.time - _lastButtonClickTime < _buttonClickCooldown)
            return; 
            _lastButtonClickTime = Time.time;

            ResumeGame();
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact); 
        } );

        B_Paused_Restart.onClick.AddListener(delegate { 
            if(Time.time - _lastButtonClickTime < _buttonClickCooldown)
            return; 
            _lastButtonClickTime = Time.time;

            TweenPauseGame(false);
            GameManager.SavePlayTime();
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact); 
            LevelManager.StartLevel( LevelManager.CurrentLevelID); });

        B_Paused_Menu.onClick.AddListener(delegate {
            if(Time.time - _lastButtonClickTime < _buttonClickCooldown)
            return; 
            _lastButtonClickTime = Time.time;

            StartCoroutine(GoToMenuFromGame());
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact); 
            GameManager.SavePlayTime(); });

        // Level End
        Btn_LevelFinish_Leaderboard.onClick.AddListener( delegate
        {
            if(Time.time - _lastButtonClickTime < _buttonClickCooldown)
            return; 
            _lastButtonClickTime = Time.time;

            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact);
        } );

        Btn_LevelFinish_Restart.onClick.AddListener(delegate
        {
            if(Time.time - _lastButtonClickTime < _buttonClickCooldown)
            return; 
            _lastButtonClickTime = Time.time;

            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact);
            GameManager.SavePlayTime();
            TweenLevelFinishMenu(false);
            LevelManager.StartLevel( LevelManager.CurrentLevelID);
        });

        Btn_LevelFinish_NextLvl.onClick.AddListener(delegate 
        {
            if(Time.time - _lastButtonClickTime < _buttonClickCooldown)
            return; 
            _lastButtonClickTime = Time.time;

            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact); 
            GameManager.SavePlayTime();
            TweenLevelFinishMenu(false);
            StartCoroutine(SwitchScreen_LevelsFromGame());
        });

        Btn_LevelFinish_Back.onClick.AddListener(delegate
        {
            if(Time.time - _lastButtonClickTime < _buttonClickCooldown)
            return; 
            _lastButtonClickTime = Time.time;

            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact);  
            GameManager.SavePlayTime();
            TweenLevelFinishMenu(false);
            StartCoroutine(GoToMenuFromFinishMenu());
        });


        // Levels Section
        B_LevelsMenuBack.onClick.AddListener(delegate 
        {
            if(Time.time - _lastButtonClickTime < _buttonClickCooldown)
            return; 
            _lastButtonClickTime = Time.time;

            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact); 
            TweenLevelsSection(false);
            TweenGameLogo(true);
            GoToMainMenu(); });

        // Shop Section
        Btn_Shop_BackMenu.onClick.AddListener(delegate 
        {
            if(Time.time - _lastButtonClickTime < _buttonClickCooldown+1)
            return; 
            _lastButtonClickTime = Time.time;

            RefreshTextValuesFromPlayerData();
            TweenShopMenu(false);
            TweenMainMenu(true);
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact); 
            GameManager.SavePlayTime();
            var _playerData = SaveSystem.LoadPlayerData();
            var _bikeList = BikeController.Instance.GetAllBikes();
            var _trailList = TrailManager.Instance.GetAllTrails();
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
            if(Time.time - _lastButtonClickTime < _buttonClickCooldown+1)
            return; 
            _lastButtonClickTime = Time.time;

            TweenGameLogo(true);
            TweenSettingsMenu(false);
            TweenMainMenu(true);
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact); 
            var _playerData = SaveSystem.LoadPlayerData();
            _playerData.UpdateLevel();
            GameManager.SavePlayTime();
            CameraController.SwitchToMenuCamera(); 
        });
            
        Settings_Toggle_Mute.onValueChanged.AddListener(OnMuteToggleClick);
        Settings_Toggle_Haptic.onValueChanged.AddListener(OnHapticToggleClick);

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
            GameManager.AddPlayTime(2134); // TESTING
            _data.UpdateLevel();
            Debug.Log("TotalPlayTime: " + _data.TOTAL_PLAYTIME + " current: " + GameManager.CurrentPlayTime);
            GameManager.SavePlayTime();
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
            Debug.Log("before bikes: " + _playerData.UNLOCKED_BIKES.Length + " c: " + _playerData.UNLOCKED_BIKES[0]);
            _playerData.UNLOCKED_BIKES = new int[allBikes.Length];
            _playerData.UNLOCKED_TRAILS = new int[allTrails.Length];
            Debug.Log("unlocked bikes: " + _playerData.UNLOCKED_BIKES);
            Debug.Log("bikes: " + allBikes.Length);
            for (int i = 0; i < allBikes.Length; i++)
                _playerData.UNLOCKED_BIKES[i] = allBikes[i].ID;
            for (int i = 0; i < allTrails.Length; i++)
                _playerData.UNLOCKED_TRAILS[i] = allTrails[i].ID;

            SaveSystem.SavePlayerData(_playerData);
            RefreshTextValuesFromPlayerData(); 
            Debug.Log("after bikes: " + _playerData.UNLOCKED_BIKES.Length); 
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
            var _audioManager = AudioManager.Instance;
            HapticPatterns.PlayEmphasis(0.6f, 1.0f);
            _audioManager.PlayNextTrack(); 
        });
    }

    void FixedUpdate()
    {
        if (GameManager.Instance.gameState == GameState.Menu)
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

    private void OnMuteToggleClick(bool isOn)
    {
        if(Time.time - _lastButtonClickTime < _buttonClickCooldown)
            return;
        _lastButtonClickTime = Time.time;

        PlayerData = SaveSystem.LoadPlayerData();
         HapticPatterns.PlayPreset(HapticPatterns.PresetType.RigidImpact); 
        if (isOn) 
        {
            AudioManager.MainAudioSource.volume = 0;
            AudioManager.SFXAudioSource.volume = 0;
            PlayerData.SETTINGS_mainVolume = 0;
            PlayerData.SETTINGS_sfxVolume = 0;
            Settings_Slider_MainVol.value = 0f;
            Settings_Slider_SFXVol.value = 0f;
            PlayerData.SETTINGS_isMuted = true;
            SaveSystem.SavePlayerData(PlayerData);
            ToggleMuteBG.color = new Color(0,0,0,0.5f);
            Txt_ToggleMuteStatus.text = "Unmute";
        }
        else
        {
            AudioManager.MainAudioSource.volume = 0.85f;
            AudioManager.SFXAudioSource.volume = 0.95f;
            Settings_Slider_MainVol.value = 0.85f;
            Settings_Slider_SFXVol.value = 0.95f;
            PlayerData.SETTINGS_isMuted = false;
            SaveSystem.SavePlayerData(PlayerData);
            ToggleMuteBG.color = new Color(0,0,0,0.2f);
            Txt_ToggleMuteStatus.text = "Mute";
        }

        UpdateMuteToggleImage();
    }

    private void OnHapticToggleClick(bool isOn)
    {
        if(Time.time - _lastButtonClickTime < _buttonClickCooldown)
            return; 
        _lastButtonClickTime = Time.time;

        HapticPatterns.PlayPreset(HapticPatterns.PresetType.RigidImpact); 
        PlayerData = SaveSystem.LoadPlayerData();
        if (!isOn)
        {
            PlayerData.SETTINGS_isHapticEnabled = true;
            HapticController.hapticsEnabled = PlayerData.SETTINGS_isHapticEnabled;
            ToggleHapticBG.color = new Color(0,0,0,0.5f);
        }
        else
        {
            //BikeHapticManager.Instance.HAPTIC_ON = true; // separate haptic for bike
            PlayerData.SETTINGS_isHapticEnabled = false;
            HapticController.hapticsEnabled = PlayerData.SETTINGS_isHapticEnabled;
            ToggleHapticBG.color = new Color(0,0,0,0.2f);
        }
        SaveSystem.SavePlayerData(PlayerData);
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

        if(_playerData.SETTINGS_isMuted)
        {
            OnMuteToggleClick(true);
        }

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
            toggleImage.sprite = Settings_Toggle_Mute.isOn ? UnmuteSprite : MuteSprite;
        }
    }

    public void RefreshTextValuesFromPlayerData()
    {
        PlayerData = SaveSystem.LoadPlayerData();
        var _unlockedBikes = PlayerData.UNLOCKED_BIKES.Length;
        var _unlockedTrails = PlayerData.UNLOCKED_TRAILS.Length;

        // Menu
        Txt_Menu_Coins.text = PlayerData.COINS + "";
        Txt_Menu_LvlsFinished.text = PlayerData.TOTAL_LEVELS_FINISHED + "/" + LevelManager.Levels.Length;

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
        text1 += $"Levels finished: {PlayerData.TOTAL_LEVELS_FINISHED}/{LevelManager.Levels.Length}\n";
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
        if((BikeAnimationInCoroutine == null && BikeAnimationOutCoroutine == null))
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
        var _gameManager = GameManager.Instance;
        TweenMainMenu(true);
        BackgroundParalax.ResetParallax();
        _gameManager.SetGameState(GameState.Menu);
    }

    private void GoToShop()
    {
        var _cameraController =  CameraController.Instance;
        _cameraController.SwitchToShopCamera();
        TweenMainMenu(false);
        TweenShopMenu(true);
    }

    public void PauseGame()
    {
        var _bikeCtrl = BikeController.Instance;
        var _gameManager = GameManager.Instance;
        _bikeCtrl.PauseBike();
        _gameManager.SetGameState(GameState.Paused);
        TweenPauseGame(true);
    }

    public void ResumeGame()
    {
        var _gameManager = GameManager.Instance;
        var _bikeCtrl = BikeController.Instance;
        _gameManager.SetGameState(GameState.Playing);
        _bikeCtrl.ResumeBike();
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
            LeanTween.moveX(Btn_Shop_RightBtn.GetComponent<RectTransform>(), 800f, 0.35f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.moveX(Btn_Shop_LeftBtn.GetComponent<RectTransform>(), -800f, 0.35f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.scale(Btn_Shop_BuyButton.GetComponent<RectTransform>(), new Vector3(0f, 0f, 0f), 0.35f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.moveX(ShopSelectionObject.GetComponent<RectTransform>(), 900f, 0.55f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.moveY(TopOverlayHeader.GetComponent<RectTransform>(), 700f, 0.4f).setEase(LeanTweenType.easeOutExpo).
            setOnComplete(
                delegate()
                {
                    Panel_Shop.SetActive(false);
                }
            );
            LeanTween.moveY(Btn_Shop_BackMenu.GetComponent<RectTransform>(), -400f, 0.35f).setEase(LeanTweenType.easeOutExpo).setDelay(0.2f);
        }
    }

    public void TweenLevelsSection(bool In)
    {
        if (In)
        {
            Panel_Levels.SetActive(true);
            LeanTween.moveX(LevelsBG.GetComponent<RectTransform>(), 0f, 0.85f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.moveX(LevelsSection.GetComponent<RectTransform>(), -20f, 0.75f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.moveY(B_LevelsMenuBack.GetComponent<RectTransform>(), -85f, 0.9f).setEase(LeanTweenType.easeOutExpo).setDelay(0.2f);
        }
        else
        {
            LeanTween.moveX(LevelsBG.GetComponent<RectTransform>(), 850f, 0.85f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.moveX(LevelsSection.GetComponent<RectTransform>(), -700f, 0.75f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.moveY(B_LevelsMenuBack.GetComponent<RectTransform>(), -350f, 0.9f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f).setOnComplete(
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
            Settings_Slider_LevelProgress.value = 0;
            Panel_Settings.SetActive(true);
            LeanTween.moveX(Settings_Stats.GetComponent<RectTransform>(), 0, 0.5f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f).setOnComplete(delegate() {StatsLevelProgressRefresh();});
            LeanTween.moveX(Settings_Main.GetComponent<RectTransform>(), -0, 0.50f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.moveX(temp_PanelDev.GetComponent<RectTransform>(), 0, 0.5f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.moveY(B_SettingsBackMenu.GetComponent<RectTransform>(), -85f, 0.9f).setEase(LeanTweenType.easeOutExpo).setDelay(0f);
        }
        else
        {
            LeanTween.moveX(Settings_Stats.GetComponent<RectTransform>(), 900f, 0.5f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.moveX(Settings_Main.GetComponent<RectTransform>(), -900f, 0.50f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.moveX(temp_PanelDev.GetComponent<RectTransform>(), 900, 0.5f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.moveY(B_SettingsBackMenu.GetComponent<RectTransform>(), -400f, 0.5f).setEase(LeanTweenType.easeOutExpo).setDelay(0.2f).
            setOnComplete(
                delegate()
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

        TweenMainMenu(false);
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
        foreach (var item in LevelManager.Instance.Levels)
        {
            GameObject levelUnitInstance = Instantiate(levelUnitPrefab, LevelsView);
            TMPro.TextMeshProUGUI[] childTexts = levelUnitInstance.GetComponentsInChildren<TMPro.TextMeshProUGUI>();

            LevelEntry levelEntry = levelUnitInstance.GetComponent<LevelEntry>();

            // Using LevelID from the 'item' object.
            levelEntry.SetLevel(item.LevelID);

            levelEntry.Txt_LevelName.text = "Level " + (item.LevelID + 1);

            // Get LevelStats for this level
            var _playerData = SaveSystem.LoadPlayerData();

            // Use item.LevelCategory and item.LevelID instead of converting and using lvlID.

            LevelStats levelStats = _playerData.GetLevelStats(item.LevelCategory, item.LevelID);

            int _trophyCount = 0;

            if(levelStats != null)
                _trophyCount = LevelManager.Levels[item.LevelID].CalculateTrophies(levelStats.Time, levelStats.Faults);

            
            switch (_trophyCount)
            {
                case 3:
                    Debug.Log("3 trophies");
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

            if(levelStats != null)
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
            LeanTween.moveX(Btn_HUD_PauseGame.GetComponent<RectTransform>(), -550f, 0.5f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.alpha(Overlay_Paused.GetComponent<RectTransform>(), 0.5f, 1f);
            LeanTween.moveX(B_Paused_Resume.GetComponent<RectTransform>(), 5f, 0.85f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.moveX(B_Paused_Restart.GetComponent<RectTransform>(), -5f, 0.95f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.moveY(B_Paused_Menu.GetComponent<RectTransform>(), -85f, 0.9f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
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
                LeanTween.moveX(Btn_HUD_PauseGame.GetComponent<RectTransform>(), -370f, 0.8f).setEase(LeanTweenType.easeOutExpo).setDelay(0.6f);
            });
        }
    }

    public void TweenLevelFinishMenu(bool In)
    {
        if (In) 
        {
            Panel_GameOver.SetActive(true);
            LeanTween.alpha(LevelFinishOverlay.GetComponent<RectTransform>(), 0.5f, 0.5f).setDelay(0.5f)
            .setOnComplete(delegate()
            {
                LeanTween.moveY(FinishLevelStatsPanel.GetComponent<RectTransform>(), 90, 0.75f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
                LeanTween.moveX(Btn_LevelFinish_Restart.GetComponent<RectTransform>(), 0f, 0.75f).setEase(LeanTweenType.easeOutExpo).setDelay(1f);
                LeanTween.moveX(Btn_LevelFinish_NextLvl.GetComponent<RectTransform>(), 0f, 0.75f).setEase(LeanTweenType.easeOutExpo).setDelay(1f);
                LeanTween.scale(Btn_LevelFinish_Leaderboard.GetComponent<RectTransform>(), new Vector3(1.45f, 1.45f, 1.45f), 0.75f).setEase(LeanTweenType.easeOutExpo).setDelay(1f);
                LeanTween.moveY(Txt_LevelFinish_XPGained.GetComponent<RectTransform>(), -280f, 0.35f).setEase(LeanTweenType.easeOutExpo).setDelay(1.5f);
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
            delegate()
            {
                LeanTween.moveY(FinishLevelStatsPanel.GetComponent<RectTransform>(), +1500, 0.75f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
                LeanTween.moveX(Btn_LevelFinish_Restart.GetComponent<RectTransform>(), -400f, 0.55f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
                LeanTween.moveX(Btn_LevelFinish_NextLvl.GetComponent<RectTransform>(), 400f, 0.55f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
                LeanTween.moveY(Btn_LevelFinish_Back.GetComponent<RectTransform>(), -400f, 0.75f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
                LeanTween.scale(Btn_LevelFinish_Leaderboard.GetComponent<RectTransform>(), new Vector3(0f, 0f, 0.65f), 0.5f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
                LeanTween.moveY(Txt_LevelFinish_XPGained.GetComponent<RectTransform>(), 400f, 0.75f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f)
                    .setOnComplete(delegate()
                    { Panel_GameOver.SetActive(false); });
            });

        }
    }

    public void TweenGameHUD(bool In)
    {
        if (In)
        {
            Panel_GameHUD.SetActive(true);
            LeanTween.moveX(HUD_TimerBar.GetComponent<RectTransform>(), 0, 0.75f).setEase(LeanTweenType.easeOutExpo).setDelay(1f);
            LeanTween.moveX(HUD_WheelieBar.GetComponent<RectTransform>(), -45.79999f, 0.85f).setEase(LeanTweenType.easeOutExpo).setDelay(1.2f);
            LeanTween.moveX(HUD_FlipsBar.GetComponent<RectTransform>(), -67.5f, 0.95f).setEase(LeanTweenType.easeOutExpo).setDelay(1.3f);
            LeanTween.moveX(HUD_FaultsBar.GetComponent<RectTransform>(), 100f, 0.9f).setEase(LeanTweenType.easeOutExpo).setDelay(1.4f);
            LeanTween.moveX(Btn_HUD_PauseGame.GetComponent<RectTransform>(), -370f, 0.8f).setEase(LeanTweenType.easeOutExpo).setDelay(1.5f);
        }
        else
        {
            LeanTween.moveX(HUD_TimerBar.GetComponent<RectTransform>(), -300f, 0.5f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveX(HUD_WheelieBar.GetComponent<RectTransform>(), -345.8f, 0.5f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveX(HUD_FlipsBar.GetComponent<RectTransform>(), -367.5f, 0.5f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveX(HUD_FaultsBar.GetComponent<RectTransform>(), 300f, 0.5f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveX(Btn_HUD_PauseGame.GetComponent<RectTransform>(), -670f, 0.5f).setEase(LeanTweenType.easeOutExpo)
            .setOnComplete(
            delegate ()
            {
                Panel_GameHUD.SetActive(false);
            });
        }
    }

    public void TweenGameLogo(bool In)
    {
        if(In)
        {
            MENU_GameLogo.SetActive(true);
            LeanTween.moveY(MENU_GameLogo.GetComponent<RectTransform>(), -150f, 0.5f).setEase(LeanTweenType.easeInOutQuad);
            LeanTween.scale(MENU_GameLogo, new Vector2(0.45f, 0.45f), 0.7f)
            .setEaseOutBounce() 
            .setOnComplete(() =>
            {
                LeanTween.scale(MENU_GameLogo, new Vector2(0.42f, 0.42f), 0.5f).setEase(LeanTweenType.easeInOutQuad)
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
            CameraController.Instance.SwitchToMenuCamera();
            LeanTween.alpha(Overlay_Menu.GetComponent<RectTransform>(), 0.5f, 1f);
            LeanTween.moveY(Btn_Menu_Start.GetComponent<RectTransform>(), 57f, 0.8f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveY(Btn_Menu_About.GetComponent<RectTransform>(), -150f, 1f).setEase(LeanTweenType.easeInOutSine).setDelay(0.5f);
            LeanTween.moveY(Btn_Menu_MainLeaderboard.GetComponent<RectTransform>(), -110f, 0.9f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveY(Btn_Menu_Shop.GetComponent<RectTransform>(), -244f, 0.95f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveY(Btn_Menu_Settings.GetComponent<RectTransform>(), -110f, 0.96f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveX(MENU_CoinsBar.GetComponent<RectTransform>(), 0f, 0.95f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveX(MENU_LvlsFinishedBar.GetComponent<RectTransform>(), -23f, 0.95f).setEase(LeanTweenType.easeOutExpo).setDelay(0.2f);
        }
        else
        {
            LeanTween.alpha(Overlay_Menu.GetComponent<RectTransform>(), 0f, 1f);
            LeanTween.moveX(MENU_CoinsBar.GetComponent<RectTransform>(), -550f, 0.95f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveY(Btn_Menu_About.GetComponent<RectTransform>(), -450f, 0.15f).setEase(LeanTweenType.easeInOutExpo);
            LeanTween.moveX(MENU_LvlsFinishedBar.GetComponent<RectTransform>(), -550f, 0.95f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.moveY(Btn_Menu_Shop.GetComponent<RectTransform>(), -1200f, 0.75f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveY(Btn_Menu_Settings.GetComponent<RectTransform>(), -1200f, 0.7f).setEase(LeanTweenType.easeOutExpo).setDelay(0.05f);
            LeanTween.moveY(Btn_Menu_MainLeaderboard.GetComponent<RectTransform>(), -1200f, 0.8f).setEase(LeanTweenType.easeOutExpo).setDelay(0.05f);
            LeanTween.moveY(Btn_Menu_Start.GetComponent<RectTransform>(), -1200f, 0.85f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f).setOnComplete(
                delegate ()
                {
                    Panel_MainMenu.SetActive(false);
                });
        }
    }

    public void TweenAboutSection(bool In)
    {
        if (In)
        {
            Panel_About.SetActive(true);
            TweenGameLogo(false);
            LeanTween.moveX(AboutPanel.GetComponent<RectTransform>(), 0, 0.8f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveY(Btn_About_Back.GetComponent<RectTransform>(), -85f, 2f).setEase(LeanTweenType.easeInOutSine).setDelay(0.5f);
        }
        else
        {
            LeanTween.moveX(AboutPanel.GetComponent<RectTransform>(), 900, 0.8f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveY(Btn_About_Back.GetComponent<RectTransform>(), -450f, 0.25f).setEase(LeanTweenType.easeInOutSine)
            .setOnComplete(delegate()
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
        var _currentLevelID = LevelManager.Instance.CurrentLevelID;
        var _levels = LevelManager.Instance.Levels;
        var _LevelTime = GameManager.LevelTimer;
        var _LevelFaults = BikeController.Instance.faults;
        var _LevelFlips = BikeController.Instance.flipCount;
        var _LevelWheelie = BikeController.Instance.wheeliePoints;
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

        // SAVE DATA

        // Actions based on outcome
        Result result = _playerData.AddLevelStats(_levels[_currentLevelID].LevelCategory, _levels[_currentLevelID].LevelID, CurrentLevelStats);
        switch (result)
        {
            case Result.NoRecord:

                Debug.Log("No existing record");
                break;
            case Result.NewTimeRecord:

                Debug.Log("New time record");
                break;
            case Result.NewWheelieRecord:

                Debug.Log("New wheelie record");
                break;
            case Result.NewFlipsRecord:

                Debug.Log("New flips record");
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
                Debug.Log("3 trophies");
                TrophyAnimation(Trophy1, 0.55f, 2f);
                TrophyAnimation(Trophy2, 0.375f, 2.25f);
                TrophyAnimation(Trophy3, 0.375f, 2.5f);
                break;
            case 2:
                Debug.Log("2 trophies");
                TrophyAnimation(Trophy1, 0.55f, 2f);
                TrophyAnimation(Trophy2, 0.375f, 2.25f);
                break;
            case 1:
                Debug.Log("1 trophy");
                TrophyAnimation(Trophy1, 0.55f, 2f);
                break;
            case 0:
                Debug.Log("0 trophies");
                break;
            default:
                Debug.LogError("Invalid trophy count");
                break;
        }

        SaveSystem.SavePlayerData(_playerData);
        _playerData.TOTAL_FAULTS_ALL_LEVELS = _playerData.GetTotalFaultsOnFinishedLevels();
        SaveSystem.SavePlayerData(_playerData);

        //Level.Category currentLevelCategory = _levels[_currentLevelID].category;
        //LevelStats stats = _playerData.GetLevelStats(currentLevelCategory, _currentLevelID);
        //Debug.Log("Saved Data: " + stats.Time + " " + stats.Faults );

        TweenGameHUD(false);
        TweenLevelFinishMenu(true);

        // Convert the time back to seconds for display and format it as M:SS:MS
        TimeSpan _timeSpan = TimeSpan.FromSeconds(GameManager.Instance.LevelTimer);
        string _formattedTime = string.Format("{0}:{1:00}:{2:00}", _timeSpan.Minutes, _timeSpan.Seconds, _timeSpan.Milliseconds / 10);

        Txt_LevelFinish_LevelTime.text = _formattedTime;
        Txt_LevelFinish_Faults.text = GameManager.Instance.faultCountText.text;
        Txt_LevelFinish_Wheelie.text = GameManager.Instance.wheelieTimeText.text;
        Txt_LevelFinish_Flips.text = GameManager.Instance.flipCountText.text;
    }

    private void TrophyAnimation(GameObject trophy, float targetScale, float delay)
    {
        trophy.transform.localScale = Vector3.zero;
        LeanTween.scale(trophy, Vector3.one * targetScale, 0.5f).setEase(LeanTweenType.easeOutExpo).setDelay(delay)
        .setOnComplete(delegate()
        {
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.RigidImpact);
        }
        );
        
    }

    private IEnumerator SwitchScreen_LevelsFromGame()
    {
        StartCoroutine(PlayTransition());
        yield return new WaitForSeconds(1f);
        TweenLevelsSection(true);
    }

    private IEnumerator AnimateBikeIn(AnimationWheelieType wheelieType)
    {
        float _delay = 2.2f;
        float wheelieDuration = 0.8f + UnityEngine.Random.Range(-0.05f, 0.55f);
        float startRotation = 0; 
        float endRotation = 55; 
        float elapsedTime = 0;
        float maintainRotationTime = 5 + UnityEngine.Random.Range(-0.5f, 1.5f);


        BikeAnimationInCoroutine = StartCoroutine(CameraController.AnimateScreenX(-0.15f, 0.88f, wheelieDuration+maintainRotationTime));

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

        if(BikeAnimationInCoroutine != null)
            StopCoroutine(BikeAnimationInCoroutine);
        
        BikeAnimationInCoroutine = null;
        BikeAnimationOutCoroutine = StartCoroutine(CameraController.AnimateScreenX(0.88f, 0.5f, 2));
        yield return new WaitForSeconds(1f);
        PlayerMenuBikeRb.constraints = RigidbodyConstraints2D.FreezeRotation; // constraint else z rotation is -2~
        PlayerMenuBikeRb.SetRotation(1.25f);
    }

    public IEnumerator PlayStartTransition()
    {
        Debug.Log("Transition start");
        startTransition.SetActive(true);
        yield return new WaitForSeconds(startTransitionDuration);
    }

    public IEnumerator PlayEndTransition()
    {
        ScreenManager.Instance.startTransition.SetActive(false); 
        ScreenManager.Instance.endTransition.SetActive(true);
        yield return new WaitForSeconds(ScreenManager.Instance.GetEndTransitionDuration());
        ScreenManager.Instance.endTransition.SetActive(false);  
        Debug.Log("Transition end");
    }

    private IEnumerator GoToMenuFromFinishMenu()
    {
        StartCoroutine(PlayTransition());
        yield return new WaitForSeconds(0.5f);
        ScreenManager.Instance.PlayerMenuBike.SetActive(true);
        ScreenManager.Instance.MenuPlatformObject.SetActive(true);
        ScreenManager.Instance.PlayerMenuBikeRb.transform.localPosition = new Vector2(0, 0.5f);
        ScreenManager.Instance.MenuPlatformRb.transform.localPosition = new Vector2(-4, 0);
        ScreenManager.Instance.PlayerMenuBikeRb.isKinematic = false;
        GoToMainMenu();
    }

    private IEnumerator GoToMenuFromGame()
    {
        var _gameManager = GameManager.Instance;
        StartCoroutine(PlayTransition());
        // Show Menu Platform & Bike
        ScreenManager.Instance.PlayerMenuBike.SetActive(true);
        ScreenManager.Instance.MenuPlatformObject.SetActive(true);
        ScreenManager.Instance.PlayerMenuBikeRb.transform.localPosition = new Vector2(0, 0.5f);
        ScreenManager.Instance.MenuPlatformRb.transform.localPosition = new Vector2(-4, 0);
        ScreenManager.Instance.PlayerMenuBikeRb.isKinematic = false;
        yield return new WaitForSeconds(0.3f); 
        TweenPauseGame(false);
        TweenGameHUD(false);
        GoToMainMenu();
        _gameManager.ResetLevelStats();
        RefreshTextValuesFromPlayerData();
    }

    public IEnumerator PlayTransition()
    {
        Debug.Log("PlayTransition started");
        StartCoroutine(PlayStartTransition());
        yield return new WaitForSeconds(startTransitionDuration-0.5f);
        StartCoroutine(PlayEndTransition());
        yield return new WaitForSeconds(ScreenManager.Instance.GetEndTransitionDuration());
    }

    public enum AnimationWheelieType
    {
        Static,
        Sine,
        Cosine
    }
}
