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
    PlayerData _playerData;
    BikeController _bikeController;
    TrailManager _trailManager;
    ShopManager _shopManager;
    CameraController _cameraController;
    LevelManager _levelManager;
    AudioManager _audioManager;
    GameManager _gameManager;

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

    public BackgroundParalax backgroundParalax;
    public LevelStats CurrentLevelStats;

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
    public Button B_PauseGame;
    public GameObject FaultsBar;
    public GameObject TimerBar;
    public GameObject WheelieBar;
    public GameObject FlipsBar;
    public TextMeshProUGUI wheelieText;
    public TextMeshProUGUI flipText;

    /////////////////////////////////////////////////////////////////////////////////////
    /////// MAIN MENU SCREEN
    /////////////////////////////////////////////////////////////////////////////////////

    [Header("Main Menu Elements")]
    public Button B_MainLeaderboard;
    public Button B_Start;
    public Button B_Settings;
    public Button B_Shop;
    public Button B_About;
    public GameObject CoinsBar;
    public GameObject GameLogo;
    public GameObject LvlsFinishedBar;
    public GameObject Overlay_Menu;
    public TextMeshProUGUI T_Coins;
    public TextMeshProUGUI T_LvlsFinished;
    public GameObject AboutPanel;
    public Button B_AboutBack;

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
    public GameObject PanelStats;
    public GameObject PanelSettings;
    public GameObject temp_PanelDev;
    public TextMeshProUGUI TextStats_1, TextStats_2;
    public TextMeshProUGUI T_PlayerLevel, T_PlayerXP;
    public Toggle ToggleMute, ToggleHaptic;
    public TextMeshProUGUI T_ToggleMute;
    public Sprite MuteSprite, UnmuteSprite;
    public Image ToggleMuteImage, ToggleMuteBG, ToggleHapticBG;
    public Slider SliderMainVol, SliderSFXVol, SliderLevelProgress;
    // Dev tools
    public Button btn_ResetCoins, btn_AddCoins, btn_ResetSavedata, btn_UnlockAll, btn_AddXP, btn_ShuffleSoundtrack;
    public Button B_SettingsBackMenu;

    /////////////////////////////////////////////////////////////////////////////////////
    /////// SHOP SCREEN MENU
    /////////////////////////////////////////////////////////////////////////////////////

    [Header("Shop Section")]
    public Button B_ShopBackMenu;
    public Button B_RightBtn;
    public Button B_LeftBtn;
    public Button B_Trails;
    public Button B_Bikes;
    public Button B_BuyButton;
    public GameObject ShopSelectionObject;
    public GameObject TopMenuHeader;
    public GameObject TopOverlayHeader;
    public TextMeshProUGUI T_ShopCoins, T_ShopUnlockedBikes, T_ShopUnlockedTrails;
    public GameObject PrefabPrice;

    /////////////////////////////////////////////////////////////////////////////////////
    /////// LEVEL ENDING SCREEN
    /////////////////////////////////////////////////////////////////////////////////////

    [Header("Level Finish Menu")]
    public Button B_Leaderboard;
    public Button B_Restart;
    public Button B_NextLvl;
    public Button B_Back;
    public GameObject FinishLevelStatsPanel;
    public GameObject Trophy1, Trophy2, Trophy3;
    public Image LevelFinishOverlay;

    [Header("Level End Text")]
    public TextMeshProUGUI T_LevelTime;
    public TextMeshProUGUI T_Faults;
    public TextMeshProUGUI T_Wheelie;
    public TextMeshProUGUI T_Flips;
    public TextMeshProUGUI T_XPGained;

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
        SaveSystem.ResetSaveFile();
        // Init Managers
        _playerData = SaveSystem.LoadPlayerData();
        _bikeController = BikeController.Instance;
        _trailManager = TrailManager.Instance;
        _shopManager = ShopManager.Instance;
        _cameraController = CameraController.Instance;
        _levelManager = LevelManager.Instance;
        _audioManager = AudioManager.Instance;
        _gameManager = GameManager.Instance;

        /////////////////////////////////////////////////////////////////////////////////////
        /////// INITIATE MENU PLAYER BIKE & TRAIL
        /////////////////////////////////////////////////////////////////////////////////////

        int selectedBikeId = _playerData.SELECTED_BIKE_ID;
        int selectedTrailId = _playerData.SELECTED_TRAIL_ID;
        Bike selectedBikeData = _bikeController.GetBikeById(selectedBikeId);
        Trail selectedTrailData = _trailManager.GetTrailById(selectedTrailId);
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
        GameObject selectedBike = selectedBikeData.bikePrefab;
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
        PlayerMenuBike = _shopManager.DisplayBikePrefab(selectedBike);
        PlayerMenuBikeRb = PlayerMenuBike.GetComponent<Rigidbody2D>();
        _shopManager.DisplayTrailPrefab(selectedTrail);

        _cameraController.menuCamera.Follow = PlayerMenuBike.transform;
        _cameraController.shopCamera.Follow = PlayerMenuBike.transform;
        _cameraController.settingsCamera.Follow = PlayerMenuBike.transform;

        /////////////////////////////////////////////////////////////////////////////////////
        /////// INITIATE SETTINGS DATA
        /////////////////////////////////////////////////////////////////////////////////////

        ToggleMute.isOn = _playerData.SETTINGS_isMuted;

        // Subscribes the method to the toggle's onValueChanged event
        ToggleMute.onValueChanged.AddListener(OnMuteToggleClick);

        if(_playerData.SETTINGS_isMuted)
        {
            _audioManager.MainAudioSource.volume = 0;
            _audioManager.SFXAudioSource.volume = 0;
            ToggleMuteBG.color = new Color(0,0,0,0.5f);
            T_ToggleMute.text = "Unmute";
        }
        else
        {
            _audioManager.MainAudioSource.volume = _playerData.SETTINGS_mainVolume;
            _audioManager.SFXAudioSource.volume = _playerData.SETTINGS_sfxVolume;
            ToggleMuteBG.color = new Color(0,0,0,0.2f);
            T_ToggleMute.text = "Mute";
        }


        UpdateMuteToggleImage();

        OnHapticToggleClick(_playerData.SETTINGS_isHapticEnabled);
        SliderMainVol.value = _playerData.SETTINGS_isMuted ? 0 : _playerData.SETTINGS_mainVolume;
        SliderSFXVol.value = _playerData.SETTINGS_isMuted ? 0 : _playerData.SETTINGS_sfxVolume;

        Debug.Log("MainAudioSource volume: " + _audioManager.MainAudioSource.volume);
        Debug.Log("SFXAudioSource volume: " + _audioManager.SFXAudioSource.volume);
    
        SliderLevelProgress.interactable = false;
        // Stats Data
        RefreshTextValuesFromPlayerData();
        UpdateXPBar();

        /////////////////////////////////////////////////////////////////////////////////////
        /////// INITIAL UI POSITIONS
        /////////////////////////////////////////////////////////////////////////////////////

        #region UI Initial Position
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

        obj = B_About.transform.localPosition;
        B_About.transform.localPosition =
            new Vector2(obj.x, obj.y-400);

        obj = B_Shop.transform.localPosition;
        B_Shop.transform.localPosition =
            new Vector2(obj.x, obj.y - 900f);

        obj = CoinsBar.transform.position;
        CoinsBar.transform.position =
            new Vector2(obj.x - 250f, obj.y);

        obj = LvlsFinishedBar.transform.position;
        LvlsFinishedBar.transform.position =
            new Vector2(obj.x - 220f, obj.y);

        obj = GameLogo.transform.localPosition;
        GameLogo.transform.localPosition =
            new Vector2(obj.x, obj.y+450);

        obj = B_AboutBack.transform.localPosition;
        B_AboutBack.transform.localPosition =
            new Vector2(obj.x, obj.y-500);

        obj = AboutPanel.transform.localPosition;
        AboutPanel.transform.localPosition =
            new Vector2(obj.x+900, obj.y);

        // Game HUD
        obj = B_PauseGame.transform.localPosition;
        B_PauseGame.transform.localPosition =
            new Vector2(obj.x - 300f, obj.y);

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
        obj = B_ShopBackMenu.transform.localPosition;
        B_ShopBackMenu.transform.localPosition =
            new Vector2(0, obj.y-400f); 

        obj = B_RightBtn.transform.localPosition;
        B_RightBtn.transform.localPosition =
            new Vector2(obj.x+400, obj.y); 

        obj = B_LeftBtn.transform.localPosition;
        B_LeftBtn.transform.localPosition =
            new Vector2(obj.x-400, obj.y); 

        obj = B_BuyButton.transform.localScale;
        B_BuyButton.transform.localScale =
            new Vector2(0, 0); 
            
        obj = TopOverlayHeader.transform.localPosition;
        TopOverlayHeader.transform.localPosition =
            new Vector2(0, obj.y+500f); 

        obj = ShopSelectionObject.transform.localPosition;
        ShopSelectionObject.transform.localPosition =
            new Vector2(obj.x-850, obj.y); 


        // Settings Section
        obj = PanelStats.transform.localPosition;
        PanelStats.transform.localPosition =
            new Vector2(obj.x+900, obj.y); 

        obj = PanelSettings.transform.localPosition;
        PanelSettings.transform.localPosition =
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

        obj = B_Leaderboard.transform.localScale;
        B_Leaderboard.transform.localScale =
            new Vector3(0,0,0); 

        obj = B_Restart.transform.localPosition;
        B_Restart.transform.localPosition =
            new Vector2(obj.x-400, obj.y); 

        obj = B_NextLvl.transform.localPosition;
        B_NextLvl.transform.localPosition =
            new Vector2(obj.x+400, obj.y); 

        obj = B_Back.transform.localPosition;
        B_Back.transform.localPosition =
            new Vector2(obj.x, obj.y-350); 

        obj = T_XPGained.transform.localPosition;
        T_XPGained.transform.localPosition =
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
        B_Start.onClick.AddListener(delegate {
            if(Time.time - _lastButtonClickTime < _buttonClickCooldown)
            return; 
            _lastButtonClickTime = Time.time;

            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact); 
            LoadLevelsScreen(true);
            _gameManager.SavePlayTime();
        });


        B_Shop.onClick.AddListener(delegate 
        { 
            if(Time.time - _lastButtonClickTime < _buttonClickCooldown)
            return; 
            _lastButtonClickTime = Time.time;

            GoToShop(); 
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact); 
        });


        B_About.onClick.AddListener(delegate 
        { 
            if(Time.time - _lastButtonClickTime < _buttonClickCooldown+2)
            return; 
            _lastButtonClickTime = Time.time;

            TweenAboutSection(true);
            TweenMainMenu(false); 
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact); 
        });

        B_AboutBack.onClick.AddListener(delegate 
        { 
            if(Time.time - _lastButtonClickTime < _buttonClickCooldown+2)
            return; 
            _lastButtonClickTime = Time.time;

            TweenAboutSection(false);
            TweenMainMenu(true); 
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact); 
        });


        B_Leaderboard.onClick.AddListener(delegate {  HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact);  });
        B_Settings.onClick.AddListener(delegate {
                if(Time.time - _lastButtonClickTime < _buttonClickCooldown+1)
                return; 
                _lastButtonClickTime = Time.time;

                _gameManager.SavePlayTime();
                RefreshTextValuesFromPlayerData();
                 HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact); 
                TweenMainMenu(false);
                TweenGameLogo(false);
                TweenSettingsMenu(true);
                CameraController.Instance.SwitchToSettingsCamera(); });

        // Set Data
        T_Coins.text = "" + _playerData.COINS;
        T_LvlsFinished.text = _playerData.TOTAL_LEVELS_FINISHED + "/" + _levelManager.Levels.Length;

        // Paused Screen
        B_PauseGame.onClick.AddListener(delegate {
            if(Time.time - _lastButtonClickTime < _buttonClickCooldown)
            return; 
            _lastButtonClickTime = Time.time;

            PauseGame();
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.Warning);
            _gameManager.SavePlayTime(); });

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
            _gameManager.SavePlayTime();
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact); 
            _levelManager.StartLevel( _levelManager.CurrentLevelID); });

        B_Paused_Menu.onClick.AddListener(delegate {
            if(Time.time - _lastButtonClickTime < _buttonClickCooldown)
            return; 
            _lastButtonClickTime = Time.time;

            StartCoroutine(GoToMenuFromGame());
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact); 
            _gameManager.SavePlayTime(); });

        // Level End
        B_Leaderboard.onClick.AddListener( delegate
        {
            if(Time.time - _lastButtonClickTime < _buttonClickCooldown)
            return; 
            _lastButtonClickTime = Time.time;

            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact);
        } );

        B_Restart.onClick.AddListener(delegate
        {
            if(Time.time - _lastButtonClickTime < _buttonClickCooldown)
            return; 
            _lastButtonClickTime = Time.time;

            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact);
            _gameManager.SavePlayTime();
            TweenLevelFinishMenu(false);
            _levelManager.StartLevel( _levelManager.CurrentLevelID);
        });

        B_NextLvl.onClick.AddListener(delegate 
        {
            if(Time.time - _lastButtonClickTime < _buttonClickCooldown)
            return; 
            _lastButtonClickTime = Time.time;

            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact); 
            _gameManager.SavePlayTime();
            TweenLevelFinishMenu(false);
            StartCoroutine(SwitchScreen_LevelsFromGame());
        });

        B_Back.onClick.AddListener(delegate
        {
            if(Time.time - _lastButtonClickTime < _buttonClickCooldown)
            return; 
            _lastButtonClickTime = Time.time;

            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact);  
            _gameManager.SavePlayTime();
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
        B_ShopBackMenu.onClick.AddListener(delegate 
        {
            if(Time.time - _lastButtonClickTime < _buttonClickCooldown+1)
            return; 
            _lastButtonClickTime = Time.time;

            RefreshTextValuesFromPlayerData();
            TweenShopMenu(false);
            TweenMainMenu(true);
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact); 
            _gameManager.SavePlayTime();
            var _playerData = SaveSystem.LoadPlayerData();
            var _bikeList = BikeController.Instance.GetAllBikes();
            var _trailList = TrailManager.Instance.GetAllTrails();
            _shopManager.SelectBike(_shopManager.CurrentBikeIndex);
            _shopManager.SelectTrail(_shopManager.CurrentTrailIndex);
            PlayerMenuBike = _shopManager.DisplayBikePrefab(_bikeList[_playerData.SELECTED_BIKE_ID].bikePrefab);
            PlayerMenuBikeRb = PlayerMenuBike.GetComponent<Rigidbody2D>();
            _shopManager.DisplayTrailPrefab(_trailList[_playerData.SELECTED_TRAIL_ID].TrailPrefab);
            _cameraController.SwitchToMenuCamera(); 
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
            _gameManager.SavePlayTime();
            RefreshTextValuesFromPlayerData();
            _cameraController.SwitchToMenuCamera(); 
        });
            
        ToggleMute.onValueChanged.AddListener(OnMuteToggleClick);
        ToggleHaptic.onValueChanged.AddListener(OnHapticToggleClick);

        SliderMainVol.onValueChanged.AddListener(delegate { OnSliderMainVolumeValueChanged(SliderMainVol.value); });
        SliderSFXVol.onValueChanged.AddListener(delegate { OnSliderSFXVolumeValueChanged(SliderSFXVol.value); });

        // Dev tools buttons
        btn_AddCoins.onClick.AddListener(delegate 
        {
            var _data = SaveSystem.LoadPlayerData();
            HapticPatterns.PlayEmphasis(0.6f, 1.0f);
            Debug.Log("Coins before: " + _data.COINS);
            _data.COINS += 1231;
            Debug.Log("Coins after: " + _data.COINS);
            SaveSystem.SavePlayerData(_data);
            RefreshTextValuesFromPlayerData();
        });

        btn_ResetCoins.onClick.AddListener(delegate 
        {
            var _data = SaveSystem.LoadPlayerData();
            HapticPatterns.PlayEmphasis(0.6f, 1.0f);
            _gameManager.AddPlayTime(2134); // TESTING
            _data.UpdateLevel();
            Debug.Log("TotalPlayTime: " + _data.TOTAL_PLAYTIME + " current: " + _gameManager.CurrentPlayTime);
            _gameManager.SavePlayTime();
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
            Bike[] allBikes = _bikeController.GetAllBikes();
            Trail[] allTrails = _trailManager.GetAllTrails();
            HapticPatterns.PlayEmphasis(0.6f, 1.0f);
            Debug.Log("before bikes: " + _playerData.UNLOCKED_BIKES.Length + " c: " + _playerData.UNLOCKED_BIKES[0]);
            _playerData.UNLOCKED_BIKES = new int[allBikes.Length];
            _playerData.UNLOCKED_TRAILS = new int[allTrails.Length];
            Debug.Log("unlocked bikes: " + _playerData.UNLOCKED_BIKES);
            Debug.Log("bikes: " + allBikes.Length);
            for (int i = 0; i < allBikes.Length; i++)
                _playerData.UNLOCKED_BIKES[i] = allBikes[i].bikeId;
            for (int i = 0; i < allTrails.Length; i++)
                _playerData.UNLOCKED_TRAILS[i] = allTrails[i].TrailID;

            SaveSystem.SavePlayerData(_playerData);
            RefreshTextValuesFromPlayerData(); 
            Debug.Log("after bikes: " + _playerData.UNLOCKED_BIKES.Length); 
        });

        btn_AddXP.onClick.AddListener(delegate 
        {
            var _playerData = SaveSystem.LoadPlayerData();
            _playerData.TOTAL_XP += 86167;
            SaveSystem.SavePlayerData(_playerData);
            HapticPatterns.PlayEmphasis(0.6f, 1.0f);
            StatsLevelProgressRefresh();
            UpdateXPBar();
        });

        btn_ShuffleSoundtrack.onClick.AddListener(delegate 
        {
            var _audioManager = AudioManager.Instance;
            HapticPatterns.PlayEmphasis(0.6f, 1.0f);
            _audioManager.PlayNextTrack(); 
        });
    }

    public void StatsLevelProgressRefresh()
    {
        var _data = SaveSystem.LoadPlayerData();
        SliderLevelProgress.maxValue = 1; 
        SliderLevelProgress.value = 0;
        LeanTween.value(gameObject, 0, _data.GetCurrentXPProgress(), 1f).setEaseInOutSine().setOnUpdate((float value) => 
        {
            SliderLevelProgress.value = value;
        }).setOnComplete(() =>
        {
            UpdateXPBar();
        });

        SaveSystem.SavePlayerData(_data);
    }

    private void UpdateSlider(float value)
    {
        SliderLevelProgress.value = value;
    }

    public void UpdateXPBar()
    {
        var _playerData = SaveSystem.LoadPlayerData();
        float currentLevelXP = _playerData.TOTAL_XP - _playerData.XPForLevel(_playerData.PLAYER_LEVEL);
        float nextLevelXP = _playerData.XPForLevel(_playerData.PLAYER_LEVEL + 1) - _playerData.XPForLevel(_playerData.PLAYER_LEVEL);

        SliderLevelProgress.maxValue = 1; 
        SliderLevelProgress.value = currentLevelXP / nextLevelXP;

        T_PlayerLevel.text = "Level: " + _playerData.PLAYER_LEVEL.ToString() + "/100";
        T_PlayerXP.text = _playerData.TOTAL_XP.ToString() + " XP";

        SaveSystem.SavePlayerData(_playerData);
    }

    void OnMuteToggleClick(bool isOn)
    {
        if(Time.time - _lastButtonClickTime < _buttonClickCooldown)
            return; 
        _lastButtonClickTime = Time.time;

        _playerData = SaveSystem.LoadPlayerData();
         HapticPatterns.PlayPreset(HapticPatterns.PresetType.RigidImpact); 
        if (isOn) 
        {
            _audioManager.MainAudioSource.volume = 0;
            _audioManager.SFXAudioSource.volume = 0;
            _playerData.SETTINGS_mainVolume = 0;
            _playerData.SETTINGS_sfxVolume = 0;
            SliderMainVol.value = 0f;
            SliderSFXVol.value = 0f;
            _playerData.SETTINGS_isMuted = true;
            SaveSystem.SavePlayerData(_playerData);
            ToggleMuteBG.color = new Color(0,0,0,0.5f);
            T_ToggleMute.text = "Unmute";
        }
        else
        {
            _audioManager.MainAudioSource.volume = 0.85f;
            _audioManager.SFXAudioSource.volume = 0.95f;
            SliderMainVol.value = 0.85f;
            SliderSFXVol.value = 0.95f;
            _playerData.SETTINGS_isMuted = false;
            SaveSystem.SavePlayerData(_playerData);
            ToggleMuteBG.color = new Color(0,0,0,0.2f);
            T_ToggleMute.text = "Mute";
        }

        UpdateMuteToggleImage();
    }


    void OnHapticToggleClick(bool isOn)
    {
        if(Time.time - _lastButtonClickTime < _buttonClickCooldown)
            return; 
        _lastButtonClickTime = Time.time;

        HapticPatterns.PlayPreset(HapticPatterns.PresetType.RigidImpact); 
        _playerData = SaveSystem.LoadPlayerData();
        if (isOn)
        {
            _playerData.SETTINGS_isHapticEnabled = false;
            HapticController.hapticsEnabled = false;
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.RigidImpact); 
            ToggleHapticBG.color = new Color(0,0,0,0.2f);
        }
        else
        {
            //BikeHapticManager.Instance.HAPTIC_ON = true;
            _playerData.SETTINGS_isHapticEnabled = true;
            HapticController.hapticsEnabled = true;
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.RigidImpact); 
            ToggleHapticBG.color = new Color(0,0,0,0.5f);
        }
        SaveSystem.SavePlayerData(_playerData);
    }

    private void OnSliderSFXVolumeValueChanged(float value)
    {
        var _playerData = SaveSystem.LoadPlayerData();
        _audioManager.SFXAudioSource.volume = value;
        _playerData.SETTINGS_sfxVolume = value;
        HapticPatterns.PlayConstant(value, 0.1f, 0.05f); 
        SaveSystem.SavePlayerData(_playerData);
    }

    private void OnSliderMainVolumeValueChanged(float value)
    {
        var _playerData = SaveSystem.LoadPlayerData();
        _audioManager.MainAudioSource.volume = value;
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

    void UpdateMuteToggleImage()
    {
        Image toggleImage = ToggleMuteImage;
        if (toggleImage != null)
        {
            Debug.Log("Image: " + ToggleMute.isOn);
            toggleImage.sprite = ToggleMute.isOn ? UnmuteSprite : MuteSprite;
        }
    }


    public void RefreshTextValuesFromPlayerData()
    {
        var _playerData = SaveSystem.LoadPlayerData();
        var _bikeController = BikeController.Instance;
        var _trailManager = TrailManager.Instance;
        var _levelManager = LevelManager.Instance;
        var _shopManager = ShopManager.Instance;
        var _unlockedBikes = _playerData.UNLOCKED_BIKES.Length;
        var _unlockedTrails = _playerData.UNLOCKED_TRAILS.Length;

        // Menu
        T_Coins.text = _playerData.COINS + "";
        T_LvlsFinished.text = _playerData.TOTAL_LEVELS_FINISHED + "/" + _levelManager.Levels.Length;

        // Shop
        _shopManager.T_Coins.text = _playerData.COINS + "";
        _shopManager.T_UnlockedBikes.text = _unlockedBikes 
        + "/" + _bikeController.GetAllBikes().Length + "";
        _shopManager.T_UnlockedTrails.text = _unlockedTrails 
        + "/" + _trailManager.GetAllTrails().Length + "";

        // Settings (Stats)
        // stats1
        string text1 = $"Bikes owned: {_playerData.UNLOCKED_BIKES.Length}/{_bikeController.GetAllBikes().Length}\n";
        text1 += $"Trails owned: {_playerData.UNLOCKED_TRAILS.Length}/{_trailManager.GetAllTrails().Length}\n";
        text1 += $"Levels finished: {_playerData.TOTAL_LEVELS_FINISHED}/{_levelManager.Levels.Length}\n";
        text1 += $"Level Faults: {_playerData.TOTAL_FAULTS_ALL_LEVELS}\n";
        text1 += $"Distance: {_playerData.TOTAL_DISTANCE.ToString("F2")}km\n";
        text1 += $"Playtime: {((int)(_playerData.TOTAL_PLAYTIME / 3600))}h {((int)(_playerData.TOTAL_PLAYTIME % 3600 / 60))}min\n";

        TextStats_1.text = text1;

        // stats2
        string text2 = $"Most Flips: {_playerData.BEST_INTERNAL_FLIPS}\n";
        text2 += $"Total Flips: {_playerData.TOTAL_FLIPS}\n";
        text2 += $"Most Wheelie: {_playerData.BEST_SINGLE_WHEELIE.ToString("F2")}\n";
        text2 += $"Total Faults: {_playerData.TOTAL_FAULTS}\n";
        text2 += $"Best Wheelie: {_playerData.BEST_LEVEL_WHEELIE.ToString("F2")}\n";
        text2 += $"Total Wheelie: {_playerData.TOTAL_WHEELIE.ToString("F2")}\n";
        TextStats_2.text = text2;


        // Level and XP
        T_PlayerLevel.text = "Level: " + _playerData.PLAYER_LEVEL + "/100";
        T_PlayerXP.text = _playerData.TOTAL_XP + " XP";


    }

    IEnumerator GoToMenuFromFinishMenu()
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

    IEnumerator GoToMenuFromGame()
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


    void FixedUpdate()
    {
        if (GameManager.Instance.gameState == GameState.Menu)
        {
            if (!hasAnimatedIn)
            {
                int randomInt = UnityEngine.Random.Range(1, 3);
                WheelieType randomType = (WheelieType)randomInt;
                StartCoroutine(AnimateBikeIn(randomType));
                hasAnimatedIn = true;
            }
            else
            {
                PreviewPlayerBike();
            }
        }
    }
    public enum WheelieType
    {
        Static,
        Sine,
        Cosine
    }

    IEnumerator AnimateBikeIn(WheelieType wheelieType)
    {
        float _delay = 2.2f;
        float wheelieDuration = 0.8f + UnityEngine.Random.Range(-0.05f, 0.55f);
        float startRotation = 0; 
        float endRotation = 55; 
        float elapsedTime = 0;
        float maintainRotationTime = 5 + UnityEngine.Random.Range(-0.5f, 1.5f);


        BikeAnimationInCoroutine = StartCoroutine(_cameraController.AnimateScreenX(-0.15f, 0.88f, wheelieDuration+maintainRotationTime));

        yield return new WaitForSeconds(_delay);
        Debug.Log("Animating IN");

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
                case WheelieType.Static:
                    rotation = endRotation;
                    break;
                case WheelieType.Sine:
                    // Natural wheelie effect
                    rotation = endRotation + Mathf.Sin(elapsedTime * 2 * Mathf.PI) * 5 + UnityEngine.Random.Range(-1.5f, 1.8f); 
                    break;
                case WheelieType.Cosine:
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
        BikeAnimationOutCoroutine = StartCoroutine(_cameraController.AnimateScreenX(0.88f, 0.5f, 2));
        yield return new WaitForSeconds(1f);
        PlayerMenuBikeRb.constraints = RigidbodyConstraints2D.FreezeRotation; // constraint else z rotation is -2~
        PlayerMenuBikeRb.SetRotation(1.25f);
    }




    void PreviewPlayerBike()
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

    public void TweenShopMenu(bool In)
    {
        if (In)
        {
            Panel_Shop.SetActive(true);
            LeanTween.moveX(B_RightBtn.GetComponent<RectTransform>(), 0, 0.75f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.moveX(B_LeftBtn.GetComponent<RectTransform>(), 0, 0.75f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.scale(B_BuyButton.GetComponent<RectTransform>(), new Vector3(1.40f, 1.40f, 1.40f), 0.75f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.moveX(ShopSelectionObject.GetComponent<RectTransform>(), 0, 0.55f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.moveY(TopOverlayHeader.GetComponent<RectTransform>(), 56f, 0.4f).setEase(LeanTweenType.easeOutExpo).setDelay(0.2f);
            LeanTween.moveY(B_ShopBackMenu.GetComponent<RectTransform>(), -85f, 0.9f).setEase(LeanTweenType.easeOutExpo).setDelay(0.2f);
        }
        else
        {
            LeanTween.moveX(B_RightBtn.GetComponent<RectTransform>(), 800f, 0.35f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.moveX(B_LeftBtn.GetComponent<RectTransform>(), -800f, 0.35f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.scale(B_BuyButton.GetComponent<RectTransform>(), new Vector3(0f, 0f, 0f), 0.35f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.moveX(ShopSelectionObject.GetComponent<RectTransform>(), 900f, 0.55f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.moveY(TopOverlayHeader.GetComponent<RectTransform>(), 700f, 0.4f).setEase(LeanTweenType.easeOutExpo).
            setOnComplete(
                delegate()
                {
                    Panel_Shop.SetActive(false);
                }
            );
            LeanTween.moveY(B_ShopBackMenu.GetComponent<RectTransform>(), -400f, 0.35f).setEase(LeanTweenType.easeOutExpo).setDelay(0.2f);
        }
    }

    public void TweenSettingsMenu(bool In)
    {
        if (In)
        {
            SliderLevelProgress.value = 0;
            Panel_Settings.SetActive(true);
            LeanTween.moveX(PanelStats.GetComponent<RectTransform>(), 0, 0.5f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f).setOnComplete(StatsLevelProgressRefresh);
            LeanTween.moveX(PanelSettings.GetComponent<RectTransform>(), -0, 0.50f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.moveX(temp_PanelDev.GetComponent<RectTransform>(), 0, 0.5f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.moveY(B_SettingsBackMenu.GetComponent<RectTransform>(), -85f, 0.9f).setEase(LeanTweenType.easeOutExpo).setDelay(0f);
        }
        else
        {
            LeanTween.moveX(PanelStats.GetComponent<RectTransform>(), 900f, 0.5f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.moveX(PanelSettings.GetComponent<RectTransform>(), -900f, 0.50f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
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

    public IEnumerator PlayStartTransition()
    {
        Debug.Log("Transition start");
        startTransition.SetActive(true);
        yield return new WaitForSeconds(startTransitionDuration);
    }

    public IEnumerator PlayEndTransition()
    {
        HapticPatterns.PlayConstant(0.4f, 0.4f, ScreenManager.Instance.GetEndTransitionDuration() );
        ScreenManager.Instance.startTransition.SetActive(false); 
        ScreenManager.Instance.endTransition.SetActive(true);
        yield return new WaitForSeconds(ScreenManager.Instance.GetEndTransitionDuration());
        ScreenManager.Instance.endTransition.SetActive(false);  
        Debug.Log("Transition end");
    }

    public IEnumerator PlayTransition()
    {
        Debug.Log("PlayTransition started");
        StartCoroutine(PlayStartTransition());
        yield return new WaitForSeconds(startTransitionDuration-0.5f);
        StartCoroutine(PlayEndTransition());
        yield return new WaitForSeconds(ScreenManager.Instance.GetEndTransitionDuration());
        HapticPatterns.PlayConstant(0.4f, 0.4f, ScreenManager.Instance.GetEndTransitionDuration() );
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
        backgroundParalax.ResetParallax();
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

        int i = 0;
        foreach (var item in LevelManager.Instance.Levels)
        {
            GameObject levelUnitInstance = Instantiate(levelUnitPrefab, LevelsView);
            TMPro.TextMeshProUGUI[] childTexts = levelUnitInstance.GetComponentsInChildren<TMPro.TextMeshProUGUI>();


            LevelEntry levelEntry = levelUnitInstance.GetComponent<LevelEntry>();

            levelEntry.SetLevel(i);

            levelEntry.Txt_LevelName.text = "Level " + (i+1);

            // Get LevelStats for this level
            var _playerData = SaveSystem.LoadPlayerData();
            LevelStats levelStats = _playerData.GetPlayerLevelStats(item.LevelCategory, i);
            int _trophyCount;
            if(levelStats != null)
            {
                _trophyCount = _levelManager.Levels[item.LevelID].CalculateTrophies(levelStats.Time, levelStats.Faults);  
            }
            else{
                _trophyCount = 0;
            }
            
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
                    Debug.Log("0 trophies");
                    break;
                default:
                    Debug.LogError("Invalid trophy count");
                    break;
            }

            if(levelStats != null)
            {
                // Time in milliseconds is converted to TimeSpan and then formatted to a string
                TimeSpan timeSpan = TimeSpan.FromMilliseconds(levelStats.Time);
                levelEntry.Txt_Timer.text = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds / 10);

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

            i++;
        }
    }

    public void TweenPauseGame(bool In)
    {
        if (In)
        {
            Panel_Paused.SetActive(true);
            LeanTween.moveX(B_PauseGame.GetComponent<RectTransform>(), -550f, 0.5f).setEase(LeanTweenType.easeOutExpo);
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
                LeanTween.moveX(B_PauseGame.GetComponent<RectTransform>(), -370f, 0.8f).setEase(LeanTweenType.easeOutExpo).setDelay(0.6f);
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
                LeanTween.moveX(B_Restart.GetComponent<RectTransform>(), 0f, 0.75f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
                LeanTween.moveX(B_NextLvl.GetComponent<RectTransform>(), 0f, 0.75f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
                LeanTween.scale(B_Leaderboard.GetComponent<RectTransform>(), new Vector3(1.45f, 1.45f, 1.45f), 0.75f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
                LeanTween.moveY(T_XPGained.GetComponent<RectTransform>(), -280f, 0.35f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
                LeanTween.value(T_XPGained.gameObject, 0, _playerData.CalculateXpForLevel(CurrentLevelStats), 3f).setEase(LeanTweenType.easeOutExpo).setDelay(0.3f).setOnUpdate((float value) =>
                {
                    T_XPGained.text = Mathf.RoundToInt(value).ToString() + " XP GAINED!";
                });
                LeanTween.moveY(B_Back.GetComponent<RectTransform>(), -85f, 0.75f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f); 
            });
        }
        else
        {
            LeanTween.alpha(LevelFinishOverlay.GetComponent<RectTransform>(), 0f, 0.3f)
            .setOnComplete(
            delegate()
            {
                LeanTween.moveY(FinishLevelStatsPanel.GetComponent<RectTransform>(), +1500, 0.75f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
                LeanTween.moveX(B_Restart.GetComponent<RectTransform>(), -400f, 0.55f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
                LeanTween.moveX(B_NextLvl.GetComponent<RectTransform>(), 400f, 0.55f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
                LeanTween.moveY(B_Back.GetComponent<RectTransform>(), -400f, 0.75f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
                LeanTween.scale(B_Leaderboard.GetComponent<RectTransform>(), new Vector3(0f, 0f, 0.65f), 0.5f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
                LeanTween.moveY(T_XPGained.GetComponent<RectTransform>(), 400f, 0.75f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f)
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
            LeanTween.moveX(TimerBar.GetComponent<RectTransform>(), 0, 0.75f).setEase(LeanTweenType.easeOutExpo).setDelay(0.2f);
            LeanTween.moveX(WheelieBar.GetComponent<RectTransform>(), -45.79999f, 0.85f).setEase(LeanTweenType.easeOutExpo).setDelay(0.3f);
            LeanTween.moveX(FlipsBar.GetComponent<RectTransform>(), -67.5f, 0.95f).setEase(LeanTweenType.easeOutExpo).setDelay(0.4f);
            LeanTween.moveX(FaultsBar.GetComponent<RectTransform>(), 100f, 0.9f).setEase(LeanTweenType.easeOutExpo).setDelay(0.4f);
            LeanTween.moveX(B_PauseGame.GetComponent<RectTransform>(), -370f, 0.8f).setEase(LeanTweenType.easeOutExpo).setDelay(0.6f);
        }
        else
        {
            LeanTween.moveX(TimerBar.GetComponent<RectTransform>(), -300f, 0.5f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveX(WheelieBar.GetComponent<RectTransform>(), -345.8f, 0.5f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveX(FlipsBar.GetComponent<RectTransform>(), -367.5f, 0.5f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveX(FaultsBar.GetComponent<RectTransform>(), 300f, 0.5f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveX(B_PauseGame.GetComponent<RectTransform>(), -670f, 0.5f).setEase(LeanTweenType.easeOutExpo)
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
            GameLogo.SetActive(true);
            LeanTween.moveY(GameLogo.GetComponent<RectTransform>(), -150f, 0.5f).setEase(LeanTweenType.easeInOutQuad);
            LeanTween.scale(GameLogo, new Vector2(0.45f, 0.45f), 0.7f)
            .setEaseOutBounce() 
            .setOnComplete(() =>
            {
                LeanTween.scale(GameLogo, new Vector2(0.42f, 0.42f), 0.5f).setEase(LeanTweenType.easeInOutQuad)
                    .setDelay(0.3f);
            });
        }
        else
        {
            LeanTween.moveY(GameLogo.GetComponent<RectTransform>(), 450f, 0.8f).setEase(LeanTweenType.easeOutExpo)
            .setOnComplete(() =>
            {
                GameLogo.SetActive(false);
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
            LeanTween.moveY(B_Start.GetComponent<RectTransform>(), 57f, 0.8f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveY(B_About.GetComponent<RectTransform>(), -150f, 1f).setEase(LeanTweenType.easeInOutSine).setDelay(0.5f);
            LeanTween.moveY(B_MainLeaderboard.GetComponent<RectTransform>(), -110f, 0.9f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveY(B_Shop.GetComponent<RectTransform>(), -244f, 0.95f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveY(B_Settings.GetComponent<RectTransform>(), -110f, 0.96f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveX(CoinsBar.GetComponent<RectTransform>(), 0f, 0.95f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveX(LvlsFinishedBar.GetComponent<RectTransform>(), -23f, 0.95f).setEase(LeanTweenType.easeOutExpo).setDelay(0.2f);
        }
        else
        {
            LeanTween.alpha(Overlay_Menu.GetComponent<RectTransform>(), 0f, 1f);
            LeanTween.moveX(CoinsBar.GetComponent<RectTransform>(), -550f, 0.95f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveY(B_About.GetComponent<RectTransform>(), -450f, 0.15f).setEase(LeanTweenType.easeInOutExpo);
            LeanTween.moveX(LvlsFinishedBar.GetComponent<RectTransform>(), -550f, 0.95f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.moveY(B_Shop.GetComponent<RectTransform>(), -1200f, 0.75f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveY(B_Settings.GetComponent<RectTransform>(), -1200f, 0.7f).setEase(LeanTweenType.easeOutExpo).setDelay(0.05f);
            LeanTween.moveY(B_MainLeaderboard.GetComponent<RectTransform>(), -1200f, 0.8f).setEase(LeanTweenType.easeOutExpo).setDelay(0.05f);
            LeanTween.moveY(B_Start.GetComponent<RectTransform>(), -1200f, 0.85f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f).setOnComplete(
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
            LeanTween.moveY(B_AboutBack.GetComponent<RectTransform>(), -85f, 2f).setEase(LeanTweenType.easeInOutSine).setDelay(0.5f);
        }
        else
        {
            LeanTween.moveX(AboutPanel.GetComponent<RectTransform>(), 900, 0.8f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveY(B_AboutBack.GetComponent<RectTransform>(), -450f, 0.25f).setEase(LeanTweenType.easeInOutSine)
            .setOnComplete(delegate()
            {
                Panel_About.SetActive(false);
                TweenGameLogo(true);
            });
        }
    }

    void OnRestartClicked()
    {
        SceneManager.LoadScene(0);
    }

    void OnBackClicked(GameObject currentPanel)
    {
        currentPanel.SetActive(false);
        TweenMainMenu(true);
    }


    IEnumerator SwitchScreen_LevelsFromGame()
    {
        StartCoroutine(PlayTransition());
        yield return new WaitForSeconds(1f);
        TweenLevelsSection(true);
    }

    public void OnLevelFinish()
    {   
        _gameManager.SetGameState(GameState.Finished);
        // TODO: Stop player in a nicer way. Smooth out velocity to 0.
        _bikeController.bikeRearWheelJoint.useMotor = false;
        
        HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact);
        var _playerData = SaveSystem.LoadPlayerData(); 
        var _currentLevelID = LevelManager.Instance.CurrentLevelID;
        var _levels = LevelManager.Instance.Levels;
        var _LevelTime = _gameManager.LevelTimer;
        var _LevelFaults = BikeController.Instance.faults;
        var _LevelFlips = BikeController.Instance.flipCount; 
        var _LevelWheelie = BikeController.Instance.wheeliePoints;
        var _trophyCount = 3; //_levels[_currentLevelID].CalculateTrophies(_LevelTime, _LevelFaults);       

        _playerData.UpdateLevel();

        CurrentLevelStats = new LevelStats 
        { 
            Time = _gameManager.LevelTimer, 
            Faults = BikeController.Instance.faults, 
            Flips = BikeController.Instance.flipCount, 
            Wheelie = BikeController.Instance.wheeliePoints,
            Trophies = _levels[_currentLevelID].CalculateTrophies(_LevelTime, _LevelFaults)
            // ADD OTHER STATS
        };

        // SAVE DATA

        // Actions based on outcome
        Result result = _playerData.AddLevelStats(_levels[_currentLevelID].LevelCategory, _levels[_currentLevelID].LevelID, CurrentLevelStats);
        switch (result)
        {
            case Result.NoRecord:
                // No existing record for the level
                Debug.Log("No existing record");
                break;
            case Result.NewTimeRecord:
                // New time record
                Debug.Log("New time record");
                break;
            case Result.NewWheelieRecord:
                // New wheelie record
                Debug.Log("New wheelie record");
                break;
            case Result.NewFlipsRecord:
                // New flips record
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
        _playerData.TOTAL_FAULTS_ALL_LEVELS = _playerData.GetPlayerFinishedLevelsTotalFaults();
        SaveSystem.SavePlayerData(_playerData);

        //Level.Category currentLevelCategory = _levels[_currentLevelID].category;
        //LevelStats stats = _playerData.GetLevelStats(currentLevelCategory, _currentLevelID);
        //Debug.Log("Saved Data: " + stats.Time + " " + stats.Faults );

        TweenGameHUD(false);
        TweenLevelFinishMenu(true);

        // Convert the time back to seconds for display and format it as M:SS:MS
        TimeSpan _timeSpan = TimeSpan.FromSeconds(GameManager.Instance.LevelTimer);
        string _formattedTime = string.Format("{0}:{1:00}:{2:00}", _timeSpan.Minutes, _timeSpan.Seconds, _timeSpan.Milliseconds / 10);

        T_LevelTime.text = _formattedTime;
        T_Faults.text = GameManager.Instance.faultCountText.text;
        T_Wheelie.text = GameManager.Instance.wheelieTimeText.text;
        T_Flips.text = GameManager.Instance.flipCountText.text;
    }

    private void TrophyAnimation(GameObject trophy, float targetScale, float delay)
    {
        trophy.transform.localScale = Vector3.zero;
        LeanTween.scale(trophy, Vector3.one * targetScale, 0.5f).setEase(LeanTweenType.easeOutExpo).setDelay(delay+1)
        .setOnComplete(delegate()
        {
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.LightImpact);
        }
        );
        
    }


}
