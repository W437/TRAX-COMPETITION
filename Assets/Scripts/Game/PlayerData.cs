using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Level;

[Serializable]
public class PlayerData
{
    public int COINS;
    public int[] UNLOCKED_BIKES;
    public int SELECTED_BIKE_ID;
    public int[] UNLOCKED_TRAILS;
    public int SELECTED_TRAIL_ID;

    [NonSerialized]
    public Dictionary<string, LevelStats> levelStatsDictionary = new Dictionary<string, LevelStats>();
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
    public float[] LEVEL_UP_DIFFICULTIES = new float[10] {1.1f, 1.2f, 1.3f, 1.5f, 1.7f, 2.1f, 2.6f, 2.9f, 3.5f, 4.9f}; // multiplier for each set of 10 levels

    const int MAX_XP = 777777;

    const float PLAYTIME_WEIGHT = 0.3f;
    const float FAULTS_WEIGHT = 0.1f;
    const float FLIPS_WEIGHT = 0.5f;
    const float WHEELIE_WEIGHT = 0.2f;

    const float MIN_XP_SCORE = 511;
    const float MAX_XP_SCORE = 5777;

    // Calculating XP per LVL
    const int BASE_XP = 1000; // per lvl complete
    const int TIME_BONUS_XP = 7;  // awarded per second under time limit (time attack mode)
    const int FLIP_BONUS_XP = 40; // awarded per flip
    const int WHEELIE_BONUS_XP = 35; // awarded per wheelie point
    const int FAULT_PENALTY_XP = 17; // deducted per fault


    // Settings
    public bool SETTINGS_isMuted = false;
    public bool SETTINGS_isHapticEnabled = true;
    public float SETTINGS_mainVolume = 0.85f;
    public float SETTINGS_sfxVolume = 0.55f;

    public void AddXP(int amount)
    {
        TOTAL_XP += amount;
    }


    // LVL SYS UNDER CONSTRUCTION
    public void UpdateLevel()
    {
        int levelGroup;
        float currentGroupDifficulty;
        int baseXP = TOTAL_XP - 1500;
        
        if (baseXP >= 0) 
        {
            levelGroup = baseXP / (MAX_XP / 10);
            currentGroupDifficulty = LEVEL_UP_DIFFICULTIES[levelGroup];
        
            if (levelGroup == 0)
            {
                PLAYER_LEVEL = (int)((baseXP / (MAX_XP * 0.1 * currentGroupDifficulty)) * 10);
            }
            else
            {
                PLAYER_LEVEL = 10 * levelGroup + (int)(((baseXP - (MAX_XP * 0.1 * levelGroup)) / (MAX_XP * 0.1 * currentGroupDifficulty)) * 10);
            }
        }
        else
        {
            PLAYER_LEVEL = 1;
        }

        var _data = SaveSystem.LoadPlayerData();
        _data.PLAYER_LEVEL = PLAYER_LEVEL;
        Debug.Log("Player Level: " + PLAYER_LEVEL);
        SaveSystem.SavePlayerData(_data);
    }


    public int XPForLevel(int level)
    {
        if (level == 1) 
        {
            return 1500;
        }
        else 
        {
            int levelGroup = (level - 1) / 10;
            float currentGroupDifficulty = LEVEL_UP_DIFFICULTIES[levelGroup];
            int baseLevel = level - 1;

            if (levelGroup == 0)
            {
                return 1500 + (int)(baseLevel / 10.0f * (MAX_XP * 0.1 * currentGroupDifficulty));
            }
            else
            {
                return 1500 + (int)((MAX_XP * 0.1 * levelGroup) + ((baseLevel - 10 * levelGroup) / 10.0f * (MAX_XP * 0.1 * currentGroupDifficulty)));
            }
        }
    }


    public int CalculateXpForLevel(LevelStats stats)
    {
        // TODO: different base level xp for level categories.
        int xp = BASE_XP;

        // Award bonus XP for fast completion
        if (stats.Time < 60)  // assuming 60 seconds is the time limit
        {
            xp += (int)(TIME_BONUS_XP * (60 - stats.Time));
        }

        // Award bonus XP for flips and wheelies
        xp += FLIP_BONUS_XP * stats.Flips;
        xp += (int)(WHEELIE_BONUS_XP * stats.Wheelie);

        // Deduct XP for faults
        xp -= FAULT_PENALTY_XP * stats.Faults;

        // Make sure XP awarded is never negative
        return Mathf.Max(xp, 0);
    }

    private float NormalizeStat(float value, float min, float max)
    {
        return (value - min) / (max - min);
    }

    public float GetCurrentXPProgress()
    {
        var _playerData = SaveSystem.LoadPlayerData();
        float currentLevelXP = _playerData.TOTAL_XP - XPForLevel(_playerData.PLAYER_LEVEL);
        float nextLevelXP = XPForLevel(_playerData.PLAYER_LEVEL + 1) - XPForLevel(_playerData.PLAYER_LEVEL);
        
        return currentLevelXP / nextLevelXP; 
    }


    /*    public void PrintLevelDictionaryData()
        {
            Debug.Log("Print Dictionary Data: ");
            foreach (KeyValuePair<string, LevelStats> item in LEVEL_DICTIONARY)
            {
                Debug.Log($"Key: {item.Key} Value: {item.Value.Time}");
            }
        }


        // Add or update LevelStats for a specific category and level id
        public Result AddLevelStats(Level.Category category, int levelId, LevelStats newStats)
        {
            var _levelList = LevelManager.Instance.Levels;
            var _playerData = SaveSystem.LoadPlayerData();
            string key = $"{category}_{levelId}";
            Debug.Log($"Added stats for: {category}_{levelId}");
            Debug.Log($"Stats: Time {newStats.Time} Faults: {newStats.Faults}");



            if (LEVEL_DICTIONARY.ContainsKey(key))
            {
                LevelStats existingStats = LEVEL_DICTIONARY[key];
                Debug.Log($"Existing stats Found: Time {existingStats.Time} Faults: {existingStats.Faults}");

                // Update trophies if necessary
                //int newTrophies = _levelList[levelId].CalculateTrophies(newStats.Time, newStats.Faults);
                //newStats.Trophies = Math.Max(newTrophies, existingStats.Trophies);

                // Compare and update other stats as needed
                if (category == Level.Category.Easy || category == Level.Category.Medium || category == Level.Category.Hard)
                {
                    // Time-based levels (Easy, Medium, Hard)
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
                else if (category == Level.Category.Wheelie || category == Level.Category.Flips)
                {
                    // Wheelie and Flips levels
                    if (newStats.Flips > existingStats.Flips || (newStats.Flips == existingStats.Flips && newStats.Faults < existingStats.Faults))
                    {
                        LEVEL_DICTIONARY[key] = newStats;
                        return Result.NewFlipsRecord;
                    }
                    else if (newStats.Wheelie > existingStats.Wheelie || (newStats.Wheelie == existingStats.Wheelie && newStats.Faults < existingStats.Faults))
                    {
                        LEVEL_DICTIONARY[key] = newStats;
                        return Result.NewWheelieRecord;
                    }
                }

            }
            else if(!LEVEL_DICTIONARY.ContainsKey(key))
            {
                // New level stats
                LEVEL_DICTIONARY.Add(key, newStats);
                Debug.Log($"No stats found. Adding: {newStats.Time} for key: {key}");
                Debug.Log($"Saved: {LEVEL_DICTIONARY.ContainsKey(key)} \n Value: {LEVEL_DICTIONARY[key].Time}");

                return Result.FirstRecord;
            }
            return Result.NoRecord;
        }*/



    // Add or update LevelStats for a specific category and level id (not needed now since all levels have unique ids)
    public Result AddLevelStats(Level.Category category, int levelId, LevelStats newStats)
    {
        var _levelList = LevelManager.Instance.Levels;
        string key = $"{category}_{levelId}";
        if (levelStatsDictionary.ContainsKey(key))
        {
            LevelStats existingStats = levelStatsDictionary[key];
            int xp = CalculateXpForLevel(newStats);
            TOTAL_XP += xp;


            int newTrophies = _levelList[levelId].CalculateTrophies(newStats.Time, newStats.Faults);
            newStats.Trophies = Math.Max(newTrophies, existingStats.Trophies);


            // For time-based levels (Easy, Medium, Hard), prioritizing on faults
            if ((category == Level.Category.Easy || category == Level.Category.Medium || category == Level.Category.Hard))
            {
                if (newStats.Faults < existingStats.Faults)
                {
                    levelStatsDictionary[key] = newStats;
                    return Result.NewTimeRecord;
                }
                else if (newStats.Faults == existingStats.Faults && newStats.Time < existingStats.Time)
                {
                    levelStatsDictionary[key] = newStats;
                    return Result.NewTimeRecord;
                }
            }
            // For Wheelie and Flips levels, we save if new faults are less or new faults are equal but flips/wheelies are more
            else if ((category == Level.Category.Wheelie || category == Level.Category.Flips))
            {
                if (newStats.Flips > existingStats.Flips)
                {
                    levelStatsDictionary[key] = newStats;
                    return Result.NewFlipsRecord;
                }
                else if (newStats.Flips == existingStats.Flips && newStats.Faults < existingStats.Faults)
                {
                    levelStatsDictionary[key] = newStats;
                    return Result.NewFlipsRecord;
                }

                if (newStats.Wheelie > existingStats.Wheelie)
                {
                    levelStatsDictionary[key] = newStats;
                    return Result.NewWheelieRecord;
                }
                else if (newStats.Wheelie == existingStats.Wheelie && newStats.Faults < existingStats.Faults)
                {
                    levelStatsDictionary[key] = newStats; return Result.NewWheelieRecord;

                }
            }
        }
        else
        {
            newStats.Trophies = _levelList[levelId].CalculateTrophies(newStats.Time, newStats.Faults);
            levelStatsDictionary.Add(key, newStats);
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

    public int GetTotalFaultsOnFinishedLevels()
    {
        int totalFaults = 0;

        foreach (KeyValuePair<string, LevelStats> entry in levelStatsDictionary)
        {
            LevelStats stats = entry.Value;

            if (stats.Time != 0)
            {
                totalFaults += stats.Faults;
            }
        }

        return totalFaults;
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

    // Get LevelStats for a specific category and level id
    public LevelStats GetLevelStats(Level.Category category, int levelId)
    {
        string key = $"{category}_{levelId}";
        return levelStatsDictionary.ContainsKey(key) ? levelStatsDictionary[key] : null;
    }

}
