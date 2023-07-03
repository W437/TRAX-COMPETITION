using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data")]
public class Level : ScriptableObject
{
    public GameObject LevelPrefab;
    public int LevelID;
    public float Trophy1Time;
    public float Trophy2Time;
    public float Trophy3Time;
    public int LeaderboardID;
    public int MaxFault2Trophies = 5;
    public int MaxFaults1Trophy = 10;
    public Category LevelCategory;

    [System.Serializable]
    public enum Category
    {
        Easy,
        Medium,
        Hard,
        Wheelie,
        Flips
    }

    public int CalculateTrophies(float playerTime, int playerFaults)
    {
        if (playerTime <= Trophy1Time && playerFaults == 0)
        {
            return 3;
        }
        else if (playerTime <= Trophy2Time && playerFaults <= MaxFault2Trophies)
        {
            return 2;
        }
        else if (playerTime <= Trophy3Time && playerFaults <= MaxFaults1Trophy)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

}
