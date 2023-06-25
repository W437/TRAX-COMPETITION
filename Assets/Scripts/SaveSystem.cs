using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static readonly string filePath = Application.persistentDataPath + "/playerData.json";

    public static void SavePlayerData(PlayerData data)
    {
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(filePath, json);
    }

    public static PlayerData LoadPlayerData()
    {
        string filePath = Application.persistentDataPath + "/playerData.json";

        if (!File.Exists(filePath))
        {
            PlayerData data = new PlayerData();
            data.coins = 125;
            data.unlockedBikes = new int[] { 0 };
            data.selectedBikeId = 0;

            SavePlayerData(data);

            return data;
        }
        else
        {
            // Read the json from the file
            string json = File.ReadAllText(filePath);

            // Use JsonUtility to convert the json to a PlayerData object
            PlayerData data = JsonUtility.FromJson<PlayerData>(json);

            return data;
        }
    }


}

