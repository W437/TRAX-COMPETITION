using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using static GameManager;

public class LeaderboardManager : MonoBehaviour
{
    private static LeaderboardManager _instance;
    public static LeaderboardManager Instance
    {
        get
        {
            if (_instance == null)
                Debug.Log("LeaderboardManager is null");
            return _instance;
        }
    }

    private ScreenManager ScreenManager;
    [SerializeField] private VideoPlayer VideoTutorial;
    private float transitionOffset = 0.5f;


    [SerializeField] private GameObject LB_EntryBar;
    [SerializeField] private Transform LB_EntryParent;

    [SerializeField] private Button LB_RefreshBtn;

    [SerializeField] private Color32[] LBTop5Colors;


    private List<Sprite> flagSprites;
    public GameObject loadingAnimation;

    // TRAX SYS

    public bool IsLoggedIn { get; private set; } = false;

    public LoginType currentLoginType;
    public string PlayerDisplayName { get; private set; }


    void Awake()
    {
        _instance = this;


        DontDestroyOnLoad(gameObject);
    }


    private void Start()
    {
        VideoTutorial.loopPointReached += OnVideoEnd;
        VideoTutorial.prepareCompleted += OnVideoPrepared;

        ScreenManager = ScreenManager.Instance;
        PlayerDisplayName = SaveSystem.LoadPlayerData().PLAYER_NAME;
        Debug.Log("DisplayName: " + PlayerDisplayName);
        /* LB_RefreshBtn.onClick.AddListener(OnRefreshBtnClick);*/
        LoadFlags();

    }

    public void OnConfirmButtonPressed()
    {
        string playerName = ScreenManager.Input_PlayerUsername.text;

        if (!string.IsNullOrEmpty(playerName) && playerName.Length >= 3 && playerName.Length <= 14)
        {
            // When player confirms their name, store it and continue with the login
            var _data = SaveSystem.LoadPlayerData();
            _data.PLAYER_NAME = playerName;
            SaveSystem.SavePlayerData(_data);

            PlayerDisplayName = playerName;
            PlayFabLogin();
            ScreenManager.TweenWelcomePanel(false);
            PlayVideo();
        }

        else
        {
            Debug.Log("Player name cannot be empty. Please enter a valid name.");
        }
    }


    private void OnVideoPrepared(VideoPlayer source)
    {
        StartCoroutine(StartTransition());
    }

    IEnumerator StartTransition()
    {
        // Wait for the video duration minus the transition offset
        yield return new WaitForSeconds((float)VideoTutorial.clip.length - transitionOffset);

        StartCoroutine(ScreenManager.PlayTransition());
    }

    private void OnVideoEnd(VideoPlayer source)
    {
        VideoTutorial.gameObject.SetActive(false);
        ScreenManager.Panel_PlayerProfile.SetActive(false);
        GameManager.Instance.SetGameState(GameState.Menu);
    }

    public void PlayVideo()
    {
        if (VideoTutorial != null)
        {
            VideoTutorial.gameObject.SetActive(true);
            VideoTutorial.Play();
        }
    }

    public void StopVideo()
    {
        if (VideoTutorial != null)
        {
            VideoTutorial.Stop();
        }
    }

    // Modified PlayFab login function
    public void PlayFabLogin()
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier, // Always use the device ID as the Custom ID
            CreateAccount = false, // Don't create a new account by default
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true
            }
        };

        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, error =>
        {
            if (error.Error == PlayFabErrorCode.AccountNotFound)
            {
                // The account doesn't exist, so try to create it
                request.CreateAccount = true;
                PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
            }
            else
            {
                // Some other error occurred
                OnLoginFailure(error);
            }
        });
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Login successful!");
        IsLoggedIn = true;
        if (result.InfoResultPayload.PlayerProfile != null)
        {
            var request = new UpdateUserTitleDisplayNameRequest { DisplayName = PlayerDisplayName };
            PlayerDisplayName = PlayerDisplayName;
            UpdateDisplayName(PlayerDisplayName);
            Debug.Log("Disp name: " + PlayerDisplayName);
            PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnDisplayNameUpdate, OnError);
        }

    }

    private void OnLoginFailure(PlayFabError error)
    {
        IsLoggedIn = false;
        Debug.LogError("Login failed: " + error.GenerateErrorReport());
    }

    private void OnError(PlayFabError error)
    {
        Debug.Log("Error while logging/creating account.");
        Debug.Log(error.GenerateErrorReport());
    }

    public void SendAllStats(string level_key, float time, int faults, int flips, float wheelie)
    {
        // Send all player level stats to separate leaderboards, each leaderboard (statisticName) represents Time, Faults, Flips, Wheelie for a level key (f.e: Easy_2)
        // Time is stored in MS and then converted to desired format

        int wheelieInt = (int)(wheelie * 100);

        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName = $"Time_{level_key}",
                    Value = Convert.ToInt32(time)
                },
                new StatisticUpdate
                {
                    StatisticName = $"Faults_{level_key}",
                    Value = faults
                },
                new StatisticUpdate
                {
                    StatisticName = $"Flips_{level_key}",
                    Value = flips
                },
                new StatisticUpdate
                {
                    StatisticName = $"Wheelie_{level_key}",
                    Value = wheelieInt
                }
            }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(request, OnLeaderboardUpdate, OnError);
    }

    public void GetPlayerStatsData(string level_key, string playerID, Action<string, float, int, int, int> callback)
    {
        // Workaround for async/await
        int requestsRemaining = 4;  // Number of statistics to retrieve
        float time = 0;
        int faults = 0, flips = 0, wheelie = 0;

        Action<int> processResult = result =>
        {
            requestsRemaining--;
            if (requestsRemaining <= 0)
                callback(playerID, time, faults, flips, wheelie);
        };

        GetPlayerLeaderboardData(playerID, $"Time_{level_key}", result => { time = result; processResult(result); });
        GetPlayerLeaderboardData(playerID, $"Faults_{level_key}", result => { faults = result; processResult(result); });
        GetPlayerLeaderboardData(playerID, $"Flips_{level_key}", result => { flips = result; processResult(result); });
        GetPlayerLeaderboardData(playerID, $"Wheelie_{level_key}", result => { wheelie = result; processResult(result); });
    }

    public void GetAllPlayerStats(string level_key, Action<List<(string playerID, string playerName, float time, int faults, int flips, int wheelie)>> callback)
    {
        GetLeaderboardData($"Faults_{level_key}", result =>
        {
            List<(string playerID, string playerName, float time, int faults, int flips, int wheelie)> playerStats = new List<(string playerID, string playerName, float time, int faults, int flips, int wheelie)>();
            int remainingPlayerStats = result.Count;

            foreach (var player in result)
            {
                GetPlayerStatsData(level_key, player.PlayFabId, (playerID, time, faults, flips, wheelie) =>
                {
                    playerStats.Add((playerID, player.DisplayName, time, faults, flips, wheelie));

                    remainingPlayerStats--;
                    if (remainingPlayerStats <= 0)
                    {
                        callback(playerStats);
                    }
                });
            }
        });
    }

    private void OnLeaderboardUpdate(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("Sucessfull leaderboard sent.");
    }

    public void UpdateLeaderboardUI(string level_key)
    {
        //Debug.Log("LB Key: " + level_key);
        ResetLeaderboardEntries();
        int rank = 1;
        GetAllPlayerStats(level_key, result =>
        {
            // Prioritize less faults and best time
            result = result.OrderBy(r => r.faults).ThenBy(r => r.time).ToList();

            foreach (var playerStats in result)
            {
                var lbEntryBarObject = Instantiate(LB_EntryBar, LB_EntryParent);

                LBEntry LBEntry = lbEntryBarObject.GetComponent<LBEntry>();

                string playerName = playerStats.playerName;
                string playerTime = FormatScore(playerStats.time);
                int playerFaults = playerStats.faults;
                int playerFlips = playerStats.flips;
                float playerWheelie = playerStats.wheelie / 100f;

                if (playerName == PlayerDisplayName)
                {
                    // highlight the local player's record
                    LBEntry.Txt_PlayerName.text = $"<size=30><color=#ff9910>{rank}.</color></size> <color=#ff9910>{playerName}</color>";
                }
                else
                {
                    LBEntry.Txt_PlayerName.text = $"<size=30><color=#ff9910>{rank}.</color></size> {playerName}";
                }

                LBEntry.Txt_Time.text = playerTime;
                LBEntry.Txt_Faults.text = playerFaults + "";
                LBEntry.Txt_Flips.text = playerFlips + "";
                LBEntry.Txt_Wheelie.text = playerWheelie.ToString("0.00");
                rank++;

                GetPlayerProfile(playerStats.playerID, countryCode =>
                {

                    Sprite flagSprite = flagSprites.Find(s => s.name == countryCode + "@2x");

                    if (flagSprite != null)
                    {
                        var flagObject = lbEntryBarObject.transform.Find("Country/FlagImg");
                        var prefabFlagImg = LBEntry.flagIcon;
                        prefabFlagImg.texture = flagSprite.texture;
                    }

                    else
                    {
                        Debug.LogError($"Failed to find flag sprite for country code: {countryCode}");
                    }
                    LBEntry.Txt_CountryCode.text = countryCode;
                });
            }
        });
    }

    private void GetPlayerProfile(string playFabId, Action<string> callback)
    {
        PlayFabClientAPI.GetPlayerProfile(new GetPlayerProfileRequest()
        {
            PlayFabId = playFabId,
            ProfileConstraints = new PlayerProfileViewConstraints()
            {
                ShowLocations = true
            }
        },
        result =>
        {
            var countryCode = result.PlayerProfile.Locations[result.PlayerProfile.Locations.Count - 1].CountryCode.ToString();
            callback(countryCode);
        },
        error => Debug.LogError(error.GenerateErrorReport()));
    }

    public void GetPlayerLeaderboardData(string playerId, string lbName, Action<int> callback)
    {
        var request = new GetLeaderboardAroundPlayerRequest
        {
            StatisticName = lbName,
            PlayFabId = playerId,
            MaxResultsCount = 1
        };

        PlayFabClientAPI.GetLeaderboardAroundPlayer(request, result =>
        {
            var playerData = result.Leaderboard.FirstOrDefault();
            if (playerData != null)
            {
                int playerScore = (int)playerData.StatValue;
                callback(playerScore);
            }
            else
            {
                Debug.Log("Player not found in leaderboard.");
            }
        }, error =>
        {
            Debug.LogError("Failed to get player leaderboard data: " + error.GenerateErrorReport());
        });
    }

    public void GetLeaderboardData(string statisticName, Action<List<PlayerLeaderboardEntry>> callback)
    {
        var request = new GetLeaderboardRequest
        {
            StatisticName = statisticName,
            StartPosition = 0,
            MaxResultsCount = 100
        };

        PlayFabClientAPI.GetLeaderboard(request, result =>
        {
            callback(result.Leaderboard);
        }, error =>
        {
            Debug.LogError("Failed to retrieve leaderboard: " + error.GenerateErrorReport());
        });
    }

    public void ResetLeaderboardEntries()
    {
        // Destroy all instantiated LB_EntryBar objects
        foreach (Transform child in LB_EntryParent.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void UpdateDisplayName(string newName)
    {
        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = newName
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnDisplayNameUpdate, error =>
        {
            if (error.Error == PlayFabErrorCode.NameNotAvailable)
            {
                // The display name is already in use
                Debug.LogError("The name " + newName + " is already in use. Please choose a different name.");
                ScreenManager.Txt_UsernamePlaceholder.text = "Username is taken.";
            }
            else
            {
                OnError(error);
            }
        });
    }


    private void OnDisplayNameUpdate(UpdateUserTitleDisplayNameResult result)
    {
        PlayerDisplayName = result.DisplayName;
        var _data = SaveSystem.LoadPlayerData();
        _data.PLAYER_NAME = PlayerDisplayName;
        SaveSystem.SavePlayerData(_data);
        Debug.Log("changed display: " + PlayerDisplayName);
    }

    private string FormatScore(float score)
    {
        TimeSpan time = TimeSpan.FromMilliseconds(score);
        int twoDigitMilliseconds = (int)Math.Round(time.Milliseconds / 10.0);
        return string.Format("{0}:{1:D2}:{2:D2}", time.Minutes, time.Seconds, twoDigitMilliseconds);
    }

    private void LoadFlags()
    {
        flagSprites = new List<Sprite>();

        // Load all of the flag sprites in the "Flags" folder
        Sprite[] sprites = Resources.LoadAll<Sprite>(@"Flags" + System.IO.Path.AltDirectorySeparatorChar);

        // Add the flag sprites to the list
        flagSprites.AddRange(sprites);
    }


    public void UpdateLeaderboardFromOfflinePlay()
    {
        Debug.Log("Offline data");
        var playerData = SaveSystem.LoadPlayerData();
        if (playerData != null)
        {
            foreach (var levelStatEntry in playerData.levelStatsDictionary)
            {
                LevelStats levelStats = levelStatEntry.Value;
                int timeInMS = GameManager.Instance.ConvertSecondsToMilliseconds(levelStats.Time);
                SendAllStats(levelStatEntry.Key, timeInMS, levelStats.Faults, levelStats.Flips, levelStats.Wheelie);
                Debug.Log("Offline data: " + levelStatEntry.Value);
            }
        }
    }


    public void SwitchLoginType(LoginType newLoginType, string newPlayerName = "")
    {
        currentLoginType = newLoginType;
        PlayerDisplayName = newPlayerName;
        PlayFabLogin();
    }

    public enum LoginType
    {
        DeviceID,
        PlayerName
    }

}