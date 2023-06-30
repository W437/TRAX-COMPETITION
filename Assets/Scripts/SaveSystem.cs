using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static readonly string filePath = Application.persistentDataPath + "/playerData.json";

    public static void SavePlayerData(PlayerData data)
    {
        data.UpdateSerializableLevelStatsList(); // Convert dictionary to list before saving
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(filePath, json);
    }

    public static PlayerData LoadPlayerData()
    {
        string filePath = Application.persistentDataPath + "/playerData.json";

        if (!File.Exists(filePath))
        {
            PlayerData data = new PlayerData();
            data.coins = 11225;
            data.unlockedBikes = new int[] { 0 };
            data.selectedBikeId = 0;
            data.unlockedTrails = new int[] { 0 };
            data.selectedTrailId = 0;
            data.experiencePoints = 0;
            data.trophiesCount = 0;
            data.totalRideDistance = 0;
            data.totalFaults = 0; 
            data.totalPlayTime = 0;
            data.totalFlips = 0;
            data.totalWheelie = 0;
            data.levelStatsDictionary = new Dictionary<string, LevelStats>();
            data.UpdateSerializableLevelStatsList(); // Convert dictionary to list before saving
            SavePlayerData(data);
            Debug.Log("New data: " + data.ToString() + " at: " + filePath.ToString());

            return data;
        }
        else
        {
            // Read the json from the file
            string json = File.ReadAllText(filePath);

            // Use JsonUtility to convert the json to a PlayerData object
            PlayerData data = JsonUtility.FromJson<PlayerData>(json);
            data.UpdateLevelStatsDictionaryFromList(); // Convert list back to dictionary after loading
            Debug.Log("Loaded savedata: " + data.ToString() + " at: " + filePath.ToString());

            return data;
        }
    }

    public static void ResetSaveFile()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "playerData.json");
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log("Save file deleted.");
        }
    }
}
