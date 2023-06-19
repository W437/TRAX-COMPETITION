using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Game/LevelData")]
public class LevelData : ScriptableObject
{
    public int levelNumber;
    public float star1Time;
    public float star2Time;
    public float star3Time;
    public int levelLeaderboardID;
    // add other data as needed
}
