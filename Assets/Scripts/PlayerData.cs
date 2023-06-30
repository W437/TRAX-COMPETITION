using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerData
{
    public int coins;
    public int[] unlockedBikes;
    public int selectedBikeId;
    public int[] unlockedTrails;
    public int selectedTrailId;
    // This field will not be serialized, but will be used in your code
    [NonSerialized]
    public Dictionary<string, LevelStats> levelStatsDictionary = new Dictionary<string, LevelStats>();
    
    // This field will be serialized instead of the dictionary
    public List<LevelDictionaryEntry> serializableLevelStatsList;


    // Stats
    public int experiencePoints;
    public int trophiesCount;
    public float totalRideDistance;
    public float totalFaults; // across all levels at the moment
    public float totalPlayTime;
    public int totalFlips;
    public float totalWheelie;

    // This method allows you to add or update LevelStats for a specific category and level id
    public void AddLevelStats(Level.LevelCategory category, int levelId, LevelStats newStats)
    {
        string key = $"{category}_{levelId}";
        if (levelStatsDictionary.ContainsKey(key))
        {
            LevelStats existingStats = levelStatsDictionary[key];
            
            // For time-based levels (Easy, Medium, Hard), we save if new time is less and faults are less or equal
            if ((category == Level.LevelCategory.Easy || category == Level.LevelCategory.Medium || category == Level.LevelCategory.Hard) &&
                newStats.time <= existingStats.time && newStats.faults <= existingStats.faults)
            {
                levelStatsDictionary[key] = newStats;
            }
            // For Wheelie and Flips levels, we save if new faults are less or new faults are equal but flips/wheelies are more
            else if ((category == Level.LevelCategory.Wheelie || category == Level.LevelCategory.Flips) &&
                (newStats.faults < existingStats.faults || (newStats.faults == existingStats.faults && newStats.flips > existingStats.flips)))
            {
                levelStatsDictionary[key] = newStats;
            }
        }
        else
        {
            levelStatsDictionary.Add(key, newStats);
        } 
    }

    // Convert Dictionary to List for serialization
    public void UpdateSerializableLevelStatsList()
    {
        serializableLevelStatsList = new List<LevelDictionaryEntry>(levelStatsDictionary.Count);
        foreach (var entry in levelStatsDictionary)
        {
            serializableLevelStatsList.Add(new LevelDictionaryEntry { levelKey = entry.Key, levelStats = entry.Value });
        }
    }

    // Convert List back to Dictionary after deserialization
    public void UpdateLevelStatsDictionaryFromList()
    {
        levelStatsDictionary = new Dictionary<string, LevelStats>(serializableLevelStatsList.Count);
        foreach (var entry in serializableLevelStatsList)
        {
            levelStatsDictionary.Add(entry.levelKey, entry.levelStats);
        }
    }

    // This method allows you to get LevelStats for a specific category and level id
    public LevelStats GetLevelStats(Level.LevelCategory category, int levelId)
    {
        string key = $"{category}_{levelId}";
        return levelStatsDictionary.ContainsKey(key) ? levelStatsDictionary[key] : null;
    }

}
