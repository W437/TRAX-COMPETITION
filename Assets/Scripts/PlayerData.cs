using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerData
{
    public int COINS;
    public int[] UNLOCKED_BIKES;
    public int SELECTED_BIKE_ID;
    public int[] UNLOCKED_TRAILS;
    public int SELECTED_TRAIL_ID;
    [NonSerialized]
    public Dictionary<string, LevelStats> LEVEL_DICTIONARY = new Dictionary<string, LevelStats>();
    public List<LevelDictionaryEntry> serializableLevelStatsList;

    // Stats
    public int TOTAL_XP; // SAVES ONLY ON LEVEL FINISH
    public int TOTAL_TROPHIES; // DOING
    public float TOTAL_DISTANCE; // DONE
    public float TOTAL_FAULTS; // DONE
    public float TOTAL_PLAYTIME; // DONE 
    public int TOTAL_FLIPS; // DONE
    public int BEST_LEVEL_FLIPS; // ON GAME END
    public int BEST_INTERNAL_FLIPS; // DONE
    public float BEST_SINGLE_WHEELIE;
    public float BEST_LEVEL_WHEELIE; // DONE
    public float TOTAL_WHEELIE; // DONE
    public float TOTAL_FAULTS_ALL_LEVELS;
    public int TOTAL_LEVELS_FINISHED;
    public int PLAYER_LEVEL;

    // XP SYSTEM
    // XP is calculated by stat weights
    const int MAX_XP = 1330007;
    const float PLAYTIME_WEIGHT = 0.3f;
    const float FAULTS_WEIGHT = 0.1f;
    const float FLIPS_WEIGHT = 0.5f;
    const float WHEELIE_WEIGHT = 0.2f;

    const float MIN_XP_SCORE = 511;
    const float MAX_XP_SCORE = 5777;

    // Settings
    public bool SETTINGS_isMuted = false;
    public bool SETTINGS_isHapticEnabled = true;
    public float SETTINGS_mainVolume = 0.85f;
    public float SETTINGS_sfxVolume = 0.55f;

    public void AddXP(int amount)
    {
        TOTAL_XP += amount;
        UpdateLevel();
    }


    public void UpdateLevel()
    {
        // Level 0 - 20 require 30% of XP, 20 - 70 require additional 20% and 70 - 100 require the remaining 50% of XP
        if (TOTAL_XP <= MAX_XP * 0.3)
        {
            // The player is in the first 20 levels (0% - 30% experience)
            PLAYER_LEVEL = (int)((TOTAL_XP / (MAX_XP * 0.3)) * 20);
        }
        else if (TOTAL_XP <= MAX_XP * 0.5)
        {
            // The player is between level 20 and 70 (30% - 50% experience)
            PLAYER_LEVEL = 20 + (int)(((TOTAL_XP - (MAX_XP * 0.3)) / (MAX_XP * 0.2)) * 50);
        }
        else
        {
            // The player is in the last 30 levels (50% - 100% experience)
            PLAYER_LEVEL = 70 + (int)(((TOTAL_XP - (MAX_XP * 0.5)) / (MAX_XP * 0.5)) * 30);
        }

        var _data = SaveSystem.LoadPlayerData();
        _data.PLAYER_LEVEL = PLAYER_LEVEL;
        SaveSystem.SavePlayerData(_data);
    }

    public int XPForLevel(int level)
    {
        if (level <= 20)
        {
            return (int)(level / 20.0f * (MAX_XP * 0.3));
        }
        else if (level <= 70)
        {
            return (int)((MAX_XP * 0.3) + ((level - 20) / 50.0f * (MAX_XP * 0.2)));
        }
        else // 70 <= level <= 100
        {
            return (int)((MAX_XP * 0.5) + ((level - 70) / 30.0f * (MAX_XP * 0.5)));
        }
    }


    public float GetCurrentXPProgress()
    {
        var _playerData = SaveSystem.LoadPlayerData();
        float currentLevelXP = _playerData.TOTAL_XP - XPForLevel(_playerData.PLAYER_LEVEL);
        float nextLevelXP = XPForLevel(_playerData.PLAYER_LEVEL + 1) - XPForLevel(_playerData.PLAYER_LEVEL);
        
        return currentLevelXP / nextLevelXP; 
    }

    // Add or update LevelStats for a specific category and level id (not needed now since all levels have unique ids)
    public Result AddLevelStats(Level.Category category, int levelId, LevelStats newStats)
    {
        var _levelList = LevelManager.Instance.levels;
        string key = $"{category}_{levelId}";
        if (LEVEL_DICTIONARY.ContainsKey(key))
        {
            LevelStats existingStats = LEVEL_DICTIONARY[key];
            int xp = CalculateXpForLevel(newStats);
            TOTAL_XP += xp;


            int newTrophies = _levelList[levelId].CalculateTrophies(newStats.Time, newStats.Faults);
            newStats.Trophies = Math.Max(newTrophies, existingStats.Trophies);

            
            // For time-based levels (Easy, Medium, Hard), prioritizing on faults
            if ((category == Level.Category.Easy || category == Level.Category.Medium || category == Level.Category.Hard))
            {
                if (newStats.Faults < existingStats.Faults) 
                {
                    LEVEL_DICTIONARY[key] = newStats;
                    return Result.NewTimeRecord;
                }
                else if (newStats.Faults == existingStats.Faults && newStats.Time < existingStats.Time)
                {
                    LEVEL_DICTIONARY[key] = newStats;
                    return Result.NewTimeRecord;
                }
            }
            // For Wheelie and Flips levels, we save if new faults are less or new faults are equal but flips/wheelies are more
            else if ((category == Level.Category.Wheelie || category == Level.Category.Flips))
            {
                if (newStats.Flips > existingStats.Flips) 
                {
                    LEVEL_DICTIONARY[key] = newStats;
                    return Result.NewFlipsRecord;
                }
                else if (newStats.Flips == existingStats.Flips && newStats.Faults < existingStats.Faults)
                {
                    LEVEL_DICTIONARY[key] = newStats;
                    return Result.NewFlipsRecord;
                }

                if (newStats.Wheelie > existingStats.Wheelie) 
                {
                    LEVEL_DICTIONARY[key] = newStats;
                    return Result.NewWheelieRecord;
                }
                else if (newStats.Wheelie == existingStats.Wheelie && newStats.Faults < existingStats.Faults)
                {
                    LEVEL_DICTIONARY[key] = newStats;return Result.NewWheelieRecord;

                }
            }
        }
        else
        {
            newStats.Trophies = _levelList[levelId].CalculateTrophies(newStats.Time, newStats.Faults);
            LEVEL_DICTIONARY.Add(key, newStats);
            return Result.NoRecord;
        } 
        return Result.FirstRecord;
    }


    public enum Result
    {
        FirstRecord,
        NoRecord,
        NewTimeRecord,
        NewWheelieRecord,
        NewFlipsRecord
    }

    public int GetPlayerFinishedLevelsTotalFaults()
    {
        int totalFaults = 0;

        foreach (KeyValuePair<string, LevelStats> entry in LEVEL_DICTIONARY)
        {
            LevelStats stats = entry.Value;
        
            if (stats.Time != 0)
            {
                totalFaults += stats.Faults;
            }
        }

        return totalFaults;
    }

    public int CalculateXpForLevel(LevelStats stats)
    {
        // Normalize each stat to a range between 0 and 1
        float normalizedPlayTime = NormalizeStat(TOTAL_PLAYTIME, 0, 2000); // 60s
        float normalizedFaults = NormalizeStat(stats.Faults, 0, 600);
        float normalizedFlips = NormalizeStat(stats.Flips, 0, 1300);
        float normalizedWheeliePoints = NormalizeStat(stats.Wheelie, 0, 850);

        // Calculate the weighted average of the normalized stats
        float average = normalizedPlayTime * PLAYTIME_WEIGHT +
                        normalizedFaults * FAULTS_WEIGHT +
                        normalizedFlips * FLIPS_WEIGHT +
                        normalizedWheeliePoints * WHEELIE_WEIGHT;

        // Map the average to the XP range
        int xp = (int)(MIN_XP_SCORE + average * (MAX_XP_SCORE - MIN_XP_SCORE));

        return xp;
    }

    private float NormalizeStat(float value, float min, float max)
    {
        return (value - min) / (max - min);
    }

    // Convert Dictionary to List for serialization
    public void UpdateSerializableLevelStatsList()
    {
        serializableLevelStatsList = new List<LevelDictionaryEntry>(LEVEL_DICTIONARY.Count);
        foreach (var entry in LEVEL_DICTIONARY)
        {
            serializableLevelStatsList.Add(new LevelDictionaryEntry { levelKey = entry.Key, levelStats = entry.Value });
        }
    }

    // Convert List back to Dictionary after deserialization
    public void UpdatePlayerLevelStatsDictionaryFromList()
    {
        LEVEL_DICTIONARY = new Dictionary<string, LevelStats>(serializableLevelStatsList.Count);
        foreach (var entry in serializableLevelStatsList)
        {
            LEVEL_DICTIONARY.Add(entry.levelKey, entry.levelStats);
        }
    }

    // Get LevelStats for a specific category and level id
    public LevelStats GetPlayerLevelStats(Level.Category category, int levelId)
    {
        string key = $"{category}_{levelId}";
        return LEVEL_DICTIONARY.ContainsKey(key) ? LEVEL_DICTIONARY[key] : null;
    }

}
