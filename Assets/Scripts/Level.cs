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
    // add other data as needed

    [System.Serializable]
    public enum Category
    {
        Easy,
        Medium,
        Hard,
        Wheelie,
        Flips
    }

    public Category category;
}
