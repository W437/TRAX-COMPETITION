using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameManager;

public class ScreenManager : MonoBehaviour
{
    public static ScreenManager Instance;
    #region Variables

    [Header("Levels Section")]
    public GameObject levelUnitPrefab;
    public GameObject LevelsBG;
    public GameObject LevelsSection;
    public Transform LevelsView;
    public Button B_LevelsMenuBack;
    private List<GameObject> instantiatedLevels = new List<GameObject>();

    public BackgroundParalax backgroundParalax;

    [Header("Game Panels")]
    public GameObject Panel_MainMenu;
    public GameObject Panel_GameHUD;
    public GameObject Panel_GameOver;
    public GameObject Panel_Paused;
    public GameObject Panel_Levels;
    public GameObject Panel_Shop;

    [Header("Game HUD Elements")]
    public Button B_PauseGame;
    public GameObject FaultsBar;
    public GameObject TimerBar;
    public GameObject WheelieBar;
    public GameObject FlipsBar;

    public TextMeshProUGUI wheelieText;
    public TextMeshProUGUI flipText;

    [Header("Main Menu Elements")]
    public Button B_MainLeaderboard;
    public Button B_Start;
    public Button B_Settings;
    public Button B_Shop;
    public GameObject CoinsBar;
    public GameObject GameLogo;
    public GameObject LvlsFinishedBar;
    public GameObject Overlay_Menu;
    public TextMeshProUGUI T_Coins;
    public TextMeshProUGUI T_LvlsFinished;

    public float menuBikeSpeed = 0f;
    public float menuBikeMaxSpeed = 7.5f;
    public float accelerationTimer;

    [SerializeField] private float accelerationTime;

    [Header("Shop Section")]
    public Button B_ShopBackMenu;
    public Button B_RightBtn;
    public Button B_LeftBtn;
    public Button B_Trails;
    public Button B_Bikes;

    [Header("Level End Buttons")]
    public Button B_Leaderboard;
    public Button B_Restart;
    public Button B_NextLvl;
    public Button B_Back;

    [Header("Level End Text")]
    public TextMeshProUGUI T_LevelTime;
    public TextMeshProUGUI T_Faults;
    public TextMeshProUGUI T_Wheelie;
    public TextMeshProUGUI T_Flips;

    [Header("Paused Screen Elements")]
    public Image Overlay_Paused;
    public TextMeshProUGUI T_PausedText;
    public Button B_Paused_Restart;
    public Button B_Paused_Resume;
    public Button B_Paused_Menu;

    [Header("Screen Transitions")]
    public string startTransitionName;
    public string endTransitionName;
    public GameObject startTransition;
    public GameObject endTransition;

    public Animator startTransitionAnimator;
    public Animator endTransitionAnimator;
    const float startTransitionDuration = 1f; // Your start animation duration in seconds
    const float endTransitionDuration = 1f;   // Your end animation duration in seconds 
    #endregion





    public GameObject PlayerMenuBike; // The current displayed prefab
    public Rigidbody2D RB_PlayerMenuBike;
    public Rigidbody2D RB_MenuPlatform;
    public ParticleSystem Ps_PlayerMenuBike;
    public TrailRenderer Trail_PlayerMenuBike;
    public GameObject MenuPlatform;

    public GameObject MenuBikeObjectParent;



    void Awake()
    {
        Instance = this;
        
    }

    void Start()
    {
        // Initiate Player Bike - based on players selected traits.
        
        PlayerData playerData = GameManager.Instance.playerData;

        int selectedBikeId = playerData.selectedBikeId;
        int selectedTrailId = playerData.selectedTrailId;

        //Debug.Log("Selected Bike ID: " + selectedBikeId + " Trail: " + selectedTrailId);

        Bike selectedBikeData = BikeController.Instance.GetBikeById(selectedBikeId);
        Trail selectedTrailData = TrailManager.Instance.GetTrailById(selectedTrailId);

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

        GameObject selectedBike = selectedBikeData.bikePrefab;
        GameObject selectedTrail = selectedTrailData.trailPrefab;

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
        
        
        PlayerMenuBike = ShopManager.Instance.DisplayBikePrefab(selectedBike);
        ShopManager.Instance.DisplayTrailPrefab(selectedTrail);

        RB_PlayerMenuBike = PlayerMenuBike.GetComponent<Rigidbody2D>();

        // ---------- Initial UI pos

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
        #endregion

        // ---------- ON GAME LAUNCH
        //TweenMainMenu(false);
        startPos = new Vector2(0, 0.5f); // Initial pos


        // Main Menu
        B_Start.onClick.AddListener(delegate { LoadLevelsScreen(true); });
        B_Shop.onClick.AddListener(delegate { GoToShop();  });
        B_Leaderboard.onClick.AddListener(delegate {  GameManager.Instance.PrintAllPlayerData();  });

        // Set Data
        T_Coins.text = "" + GameManager.Instance.GetPlayerData().coins;
        T_LvlsFinished.text = "" + "13/45";

        // Paused Screen
        B_PauseGame.onClick.AddListener( PauseGame );

        B_Paused_Resume.onClick.AddListener( ResumeGame );

        B_Paused_Restart.onClick.AddListener(delegate 
        { 
            TweenPauseGame(false);
            LevelManager.Instance.StartLevel(LevelManager.Instance.CurrentLevelID);   
        });

        B_Paused_Menu.onClick.AddListener(delegate 
        {
            StartCoroutine(GoToMenuFromGame());
        });


        // Level End
        B_Leaderboard.onClick.AddListener( OnRestartClicked );
        B_Restart.onClick.AddListener(OnRestartClicked);
        B_NextLvl.onClick.AddListener(delegate { OnBackClicked(Panel_GameOver); });
        B_Back.onClick.AddListener(OnRestartClicked);


        // Levels Section
        B_LevelsMenuBack.onClick.AddListener(delegate 
        {
            TweenLevelsSection(false);
            GoToMainMenu();
        });


        // Shop Section
        B_ShopBackMenu.onClick.AddListener(delegate
        {
            ShopManager.Instance.SelectBike(ShopManager.Instance.currentBikeIndex);
            ShopManager.Instance.SelectTrail(ShopManager.Instance.currentTrailIndex);
            //ShopManager.Instance.ResetDefaultSelection();
            CameraController.Instance.SwitchToMenuCamera();
            GoToMainMenu();
            Panel_Shop.SetActive(false);
        });


    }

    public Vector2 startPos;
    private Vector2 bikePosition;

    IEnumerator GoToMenuFromGame()
    {
        StartCoroutine(PlayTransition());
        yield return new WaitForSeconds(0.3f); // Wait for 0.3 seconds
        TweenPauseGame(false);
        TweenGameHUD(false);
        GoToMainMenu();
        GameManager.Instance.ResetLevelStats();
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if (GameManager.Instance.gameState == GameState.Menu)
        {
            PreviewPlayerBike();
        }
    }

    void PreviewPlayerBike()
    {
        accelerationTimer += Time.fixedDeltaTime;
        // Calculate the current speed based on the elapsed time
        menuBikeSpeed = Mathf.Lerp(0, menuBikeMaxSpeed, accelerationTimer / accelerationTime);
        
        // Apply the speed as a force
        Vector2 force = new Vector2(menuBikeSpeed - RB_PlayerMenuBike.velocity.x, 0);
        RB_PlayerMenuBike.AddForce(force, ForceMode2D.Force);
        RB_PlayerMenuBike.rotation = 0;
        
        // Pin platform to payer bike
        if (RB_PlayerMenuBike != null)
        {
            bikePosition = RB_PlayerMenuBike.position;
            float platformBaseSpeed = 5.0f;  // adjust the value as needed
            float platformSpeed = platformBaseSpeed * menuBikeSpeed;  // platform speed is relative to the bike's speed
            float step = platformSpeed * Time.deltaTime; // calculate distance to move

            Vector2 targetPos = new Vector2(bikePosition.x - 4, RB_MenuPlatform.position.y);
            Vector2 newPlatformPosition = Vector2.MoveTowards(RB_MenuPlatform.position, targetPos, step);

            RB_MenuPlatform.position = newPlatformPosition;
        }
    }


    public void ShowSelectedBikeAndTrail()
    {
        // Load player data and get the selected bike and trail IDs
        PlayerData playerData = GameManager.Instance.GetPlayerData();
        int selectedBikeId = playerData.selectedBikeId;
        int selectedTrailId = playerData.selectedTrailId;

        // Find and display the selected bike and trail
        GameObject selectedBike = BikeController.Instance.GetBikeById(selectedBikeId).bikePrefab;
        GameObject selectedTrail = TrailManager.Instance.GetTrailById(selectedTrailId).trailPrefab;
        ShopManager.Instance.DisplayBikePrefab(selectedBike);
        ShopManager.Instance.DisplayTrailPrefab(selectedTrail);
    }

    public void PlayShopTransition()
    {
        StartCoroutine(PlayTransition());
    }

    public void TweenLevelsSection(bool In)
    {
        if (In)
        {
            LeanTween.moveX(LevelsBG.GetComponent<RectTransform>(), 0f, 0.85f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.moveX(LevelsSection.GetComponent<RectTransform>(), -20f, 0.75f).setEase(LeanTweenType.easeOutExpo).setDelay(0.1f);
            LeanTween.moveY(B_LevelsMenuBack.GetComponent<RectTransform>(), -100f, 0.9f).setEase(LeanTweenType.easeOutExpo).setDelay(0.2f);
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


    public IEnumerator PlayStartTransition()
    {
        startTransition.SetActive(true);
        yield return new WaitForSeconds(startTransitionDuration);
    }

    public IEnumerator PlayEndTransition()
    {
        ScreenManager.Instance.startTransition.SetActive(false);  // Deactivate the Start animation
        ScreenManager.Instance.endTransition.SetActive(true);
        yield return new WaitForSeconds(ScreenManager.Instance.GetEndTransitionDuration());
        ScreenManager.Instance.endTransition.SetActive(false);   // Deactivate the End animation
    }

    public IEnumerator PlayTransition()
    {
        Debug.Log("PlayTransition started");
        StartCoroutine(PlayStartTransition());
        yield return new WaitForSeconds(startTransitionDuration-0.5f);
        StartCoroutine(PlayEndTransition());
        yield return new WaitForSeconds(ScreenManager.Instance.GetEndTransitionDuration());
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
        //StopSimulation();
        TweenMainMenu(true);
        backgroundParalax.ResetParallax();
        GameManager.Instance.SetGameState(GameState.Menu);
    }

    private void GoToShop()
    {
        CameraController.Instance.SwitchToShopCamera();
        TweenMainMenu(false);
        //TweenShop();
        Panel_Shop.SetActive(true);

        //SimulateMovement();
    }

    public void PauseGame()
    {
        BikeController.Instance.PauseBike();
        GameManager.Instance.SetGameState(GameState.Paused);
        TweenPauseGame(true);
    }

    public void ResumeGame()
    {
        GameManager.Instance.SetGameState(GameState.Playing);
        TweenPauseGame(false);
    }

    public void LoadLevelsScreen(bool In)
    {
        if (!Panel_Levels.activeSelf)
        {
            Panel_Levels.SetActive(true);
        }

        TweenMainMenu(false);
        TweenLevelsSection(true);

        // Destroy existing levels
        foreach (GameObject instantiatedLevel in instantiatedLevels)
        {
            Destroy(instantiatedLevel);
        }

        // Clear the list
        instantiatedLevels.Clear();

        int i = 0;
        foreach (var item in LevelManager.Instance.levels)
        {
            GameObject levelUnitInstance = Instantiate(levelUnitPrefab, LevelsView);
            TMPro.TextMeshProUGUI[] childTexts = levelUnitInstance.GetComponentsInChildren<TMPro.TextMeshProUGUI>();

            LevelEntry levelEntry = levelUnitInstance.GetComponent<LevelEntry>();

            levelEntry.SetLevel(i);

            levelEntry.T_LevelName.text = "Level " + (i+1);

            // Get LevelStats for this level
            PlayerData _playerData = GameManager.Instance.GetPlayerData();
            LevelStats levelStats = _playerData.GetLevelStats(item.category, i);
            
            if(levelStats != null)
            {
                // Time in milliseconds is converted to TimeSpan and then formatted to a string
                TimeSpan timeSpan = TimeSpan.FromMilliseconds(levelStats.time);
                levelEntry.T_Timer.text = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds / 10);

                // Display faults
                levelEntry.T_Faults.text = levelStats.faults.ToString();
            }
            else
            {
                // Display default values or a message indicating that there are no stats available for this level
                levelEntry.T_Timer.text = "N/A";
                levelEntry.T_Faults.text = "N/A";
            }

            // Add this level to our list
            instantiatedLevels.Add(levelUnitInstance);

            i++;
        }
    }


    public void RemoveLevelsPanel()
    {
        Panel_Levels.SetActive(false);
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
            LeanTween.moveX(TimerBar.GetComponent<RectTransform>(), -300f, 0.5f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveX(WheelieBar.GetComponent<RectTransform>(), -345.8f, 0.5f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveX(FlipsBar.GetComponent<RectTransform>(), -367.5f, 0.5f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveX(FaultsBar.GetComponent<RectTransform>(), 300f, 0.5f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveX(B_PauseGame.GetComponent<RectTransform>(), -670f, 0.5f).setEase(LeanTweenType.easeOutExpo).setOnComplete(
                delegate ()
                {
                    Panel_GameHUD.SetActive(false);
                });
        }
    }

    // void SimulateMovement()
    // {
    //     // Set the emission rates
    //     var exhaustEmission = Ps_PlayerMenuBike.emission;
    //     exhaustEmission.rateOverTime = 50; // Set this value to what looks best in your game

    //     Trail_PlayerMenuBike.time = 0.25f;
    // }

    // void StopSimulation()
    // {
    //     // Reset the emission rates
    //     var exhaustEmission = Ps_PlayerMenuBike.emission;
    //     exhaustEmission.rateOverTime = 0;

    //     Trail_PlayerMenuBike.time = 0;
    // }

    public void TweenMainMenu(bool In)
    {
        if (In)
        {
            Panel_MainMenu.SetActive(true);
            MenuPlatform.SetActive(true);
            //MenuBikeObjectParent.SetActive(true);
            CameraController.Instance.SwitchToMenuCamera();

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
            LeanTween.moveX(CoinsBar.GetComponent<RectTransform>(), 0f, 0.95f).setEase(LeanTweenType.easeOutExpo);
            LeanTween.moveX(LvlsFinishedBar.GetComponent<RectTransform>(), -23f, 0.95f).setEase(LeanTweenType.easeOutExpo).setDelay(0.2f);
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

    void OnRestartClicked()
    {
        SceneManager.LoadScene(0);
    }

    void OnBackClicked(GameObject currentPanel)
    {
        currentPanel.SetActive(false);
        TweenMainMenu(true);
    }

    public void OnLevelEnd()
    {
        // Assume GameManager.Instance.levelTime is in seconds. Convert to milliseconds for storage.
        float levelTimeMilliseconds = GameManager.Instance.LevelTimer * 1000;

        LevelStats levelStats = new LevelStats 
        { 
            time = levelTimeMilliseconds, 
            faults = BikeController.Instance.faults, 
            flips = BikeController.Instance.flipCount, 
            wheelie = BikeController.Instance.wheeliePoints 
        };
        var _currentLevelID = LevelManager.Instance.CurrentLevelID;
        var _levels = LevelManager.Instance.levels;
        
        PlayerData _playerData = GameManager.Instance.GetPlayerData();
        _playerData.AddLevelStats(_levels[_currentLevelID].category, _levels[_currentLevelID].levelID, levelStats);

        SaveSystem.SavePlayerData(_playerData);

        Level.LevelCategory currentLevelCategory = _levels[_currentLevelID].category;
        LevelStats stats = _playerData.GetLevelStats(currentLevelCategory, _currentLevelID);
        Debug.Log("Saved Data: " + stats.time + " " + stats.faults );

        Panel_GameHUD.SetActive(false);
        Panel_GameOver.SetActive(true);

        // Convert the time back to seconds for display and format it as M:SS:MS
        TimeSpan _timeSpan = TimeSpan.FromSeconds(GameManager.Instance.LevelTimer);
        string _formattedTime = string.Format("{0}:{1:00}:{2:00}", _timeSpan.Minutes, _timeSpan.Seconds, _timeSpan.Milliseconds / 10);

        T_LevelTime.text = _formattedTime;
        T_Faults.text = GameManager.Instance.faultCountText.text;
        T_Wheelie.text = GameManager.Instance.wheelieTimeText.text;
        T_Flips.text = GameManager.Instance.flipCountText.text;
    }




}
