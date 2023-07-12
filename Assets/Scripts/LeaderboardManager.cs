using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using System;
using System.Linq;

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
    [SerializeField] private GameObject LB_EntryBar;
    [SerializeField] private Transform LB_EntryParent;

    [SerializeField] private Button LB_RefreshBtn;

    [SerializeField] private int MaxPlayerNameLength = 16;

    [SerializeField] private Color32[] LBTop5Colors;


    private List<Sprite> flagSprites;
    public GameObject loadingAnimation;

    // TRAX SYS

    public bool IsLoggedIn { get; private set; } = false;

    public LoginType currentLoginType;
    public string PlayerDisplayName;

    void Awake()
    {
        _instance = this;
        PlayFabLogin();
        DontDestroyOnLoad(gameObject);
    }


    private void Start()
    {
        ScreenManager = ScreenManager.Instance;
        Debug.Log("DisplayName: " + PlayerDisplayName);
       /* LB_RefreshBtn.onClick.AddListener(OnRefreshBtnClick);*/
        LoadFlags();

    }


    // Modified PlayFab login function
    private void PlayFabLogin()
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = currentLoginType == LoginType.DeviceID ? SystemInfo.deviceUniqueIdentifier : PlayerDisplayName,
            CreateAccount = true,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true
            }
        };

        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }


    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Login successful!");
        IsLoggedIn = true;
        string name;
        if (result.InfoResultPayload.PlayerProfile != null)
        {
            name = PlayerDisplayName;
            Debug.Log("name: " + name);
            PlayerDisplayName = PlayerPrefs.GetString("PLAYER_PLAYFAB_NAME", PlayerDisplayName);
            Debug.Log("display name: " + PlayerDisplayName);
        }

/*        if (name == null)
            PlayerNameInputWindow.SetActive(true);*/
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
                Value = Convert.ToInt32(wheelie)
            }
        }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(request, OnLeaderboardUpdate, OnError);
    }

    public void GetPlayerStatsData(string level_key, string playerID, Action<string, float, int, int, int> callback)
    {
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
        Debug.Log("LB Key: " + level_key);
        ResetLeaderboardEntries();
        int rank = 1;
        GetAllPlayerStats(level_key, result =>
        {
            result = result.OrderBy(r => r.faults).ThenBy(r => r.time).ToList();

            foreach (var playerStats in result)
            {
                var lbEntryBarObject = Instantiate(LB_EntryBar, LB_EntryParent);

                LBEntry LBEntry = lbEntryBarObject.GetComponent<LBEntry>();

                string playerName = playerStats.playerName;
                string playerTime = FormatScore(playerStats.time);
                int playerFaults = playerStats.faults;
                int playerFlips = playerStats.flips;
                int playerWheelie = playerStats.wheelie;

                LBEntry.Txt_PlayerName.text = rank + ". " + playerName;
                LBEntry.Txt_Time.text = playerTime;
                LBEntry.Txt_Faults.text = playerFaults + "";
                LBEntry.Txt_Flips.text = playerFlips + "";
                LBEntry.Txt_Wheelie.text = playerWheelie + "";
                rank++;
            }
        });
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

        PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnDisplayNameUpdate, OnError);
    }

    private void OnDisplayNameUpdate(UpdateUserTitleDisplayNameResult result)
    {
        PlayerDisplayName = result.DisplayName;
        PlayerPrefs.SetString("PLAYER_PLAYFAB_NAME", PlayerDisplayName);
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