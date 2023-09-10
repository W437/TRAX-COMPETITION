using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static readonly string __filePath = Application.persistentDataPath + "/trxdata.json";
    private static readonly object lockObject = new();  // Lock object (thread-safe)

    public static void SavePlayerData(PlayerData data)
    {
        lock (lockObject)
        {
            data.UpdateSerializableLevelStatsList(); // Convert dictionary to list before saving
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(__filePath, json);
        }
    }

    public static PlayerData LoadPlayerData()
    {
        lock (lockObject)
        {
            if (!File.Exists(__filePath))
            {
                PlayerData data = new()
                {
                    PLAYER_NAME = null,
                    COINS = 24147,
                    UNLOCKED_BIKES = new int[] { 0 },
                    SELECTED_BIKE_ID = 0,
                    UNLOCKED_TRAILS = new int[] { 0 },
                    SELECTED_TRAIL_ID = 0,
                    TOTAL_XP = 100,
                    TOTAL_TROPHIES = 0,
                    TOTAL_DISTANCE = 0,
                    TOTAL_FAULTS = 0,
                    TOTAL_PLAYTIME = 65,
                    TOTAL_FLIPS = 0,
                    BEST_LEVEL_FLIPS = 0,
                    BEST_INTERNAL_FLIPS = 0,
                    BEST_LEVEL_WHEELIE = 0,
                    BEST_SINGLE_WHEELIE = 0,
                    TOTAL_FAULTS_ALL_LEVELS = 0,
                    TOTAL_WHEELIE = 0,
                    TOTAL_LEVELS_FINISHED = 0,
                    PLAYER_LEVEL = 1,
                    levelStatsDictionary = new Dictionary<string, LevelStats>()
                };
                data.UpdateSerializableLevelStatsList(); // Convert dictionary to list before saving
                // Initialize settings
                data.SETTINGS_isMuted = false;
                data.SETTINGS_isHapticEnabled = true;
                data.SETTINGS_mainVolume = 0.45f;
                data.SETTINGS_sfxVolume = 0.45f;
                SavePlayerData(data);
                Debug.Log("New data: " + data.ToString() + " at: " + __filePath.ToString());
                return data;
            }
            else
            {
                string json = File.ReadAllText(__filePath);
                // Use JsonUtility to convert the json to a PlayerData object
                PlayerData data = JsonUtility.FromJson<PlayerData>(json);
                data.UpdateLevelStatsDictionaryFromList(); // Convert list back to dictionary after loading
                //Debug.Log("Loaded savedata: " + data.ToString() + " at: " + __filePath.ToString());
                return data;
            }
        }
    }

    public static void ResetSaveFile()
    {
        lock (lockObject)
        {
            if (File.Exists(__filePath))
            {
                File.Delete(__filePath);
                Debug.Log("Save file deleted.");
            }
        }
    }

}
