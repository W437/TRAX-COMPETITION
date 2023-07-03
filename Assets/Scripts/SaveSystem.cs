using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static readonly string filePath = Application.persistentDataPath + "/trx_data.json";

    public static void SavePlayerData(PlayerData data)
    {
        data.UpdateSerializableLevelStatsList(); // Convert dictionary to list before saving
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(filePath, json);
    }

    public static PlayerData LoadPlayerData()
    {
        if (!File.Exists(filePath))
        {
            PlayerData data = new PlayerData();
            data.COINS = 150;
            data.UNLOCKED_BIKES = new int[] { 0 };
            data.SELECTED_BIKE_ID = 0;
            data.UNLOCKED_TRAILS = new int[] { 0 };
            data.SELECTED_TRAIL_ID = 0;
            data.TOTAL_XP = 100;
            data.TOTAL_TROPHIES = 0;
            data.TOTAL_DISTANCE = 0;
            data.TOTAL_FAULTS = 0; 
            data.TOTAL_PLAYTIME = 65;
            data.TOTAL_FLIPS = 0;
            data.BEST_LEVEL_FLIPS = 0;
            data.BEST_INTERNAL_FLIPS = 0;
            data.BEST_LEVEL_WHEELIE = 0;
            data.BEST_SINGLE_WHEELIE = 0;
            data.TOTAL_FAULTS_ALL_LEVELS = 0;
            data.TOTAL_WHEELIE = 0;
            data.TOTAL_LEVELS_FINISHED = 0;
            data.PLAYER_LEVEL = 1;
            data.LEVEL_DICTIONARY = new Dictionary<string, LevelStats>();
            data.UpdateSerializableLevelStatsList(); // Convert dictionary to list before saving
            // Initialize settings
            data.SETTINGS_isMuted = false;
            data.SETTINGS_isHapticEnabled = true;
            data.SETTINGS_mainVolume = 0.85f; 
            data.SETTINGS_sfxVolume = 0.55f;
            SavePlayerData(data);
            Debug.Log("New data: " + data.ToString() + " at: " + filePath.ToString());
            return data;
        }
        else
        {
            string json = File.ReadAllText(filePath);
            // JsonUtility to convert the json to a PlayerData object
            PlayerData data = JsonUtility.FromJson<PlayerData>(json);
            data.UpdatePlayerLevelStatsDictionaryFromList(); // Convert list back to dictionary after loading
            //Debug.Log("Loaded savedata: " + data.ToString() + " at: " + filePath.ToString());

            return data;
        }
    }

    public static void ResetSaveFile()
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log("Save file deleted.");
        }
    }

}
