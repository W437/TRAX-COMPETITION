using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data")]
public class Level : ScriptableObject
{
    public GameObject levelPrefab;
    public int levelID;
    public float star1Time;
    public float star2Time;
    public float star3Time;
    public int leaderboardID;
    public int maxFaultsFor2Stars = 5;
    public int maxFaultsFor1Star = 10;
    public Category category;

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
        if (playerTime <= star1Time && playerFaults == 0)
        {
            return 3;
        }
        else if (playerTime <= star2Time && playerFaults <= maxFaultsFor2Stars)
        {
            return 2;
        }
        else if (playerTime <= star3Time && playerFaults <= maxFaultsFor1Star)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

}
