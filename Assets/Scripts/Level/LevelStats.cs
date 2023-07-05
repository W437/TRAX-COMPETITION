using static Level;

[System.Serializable]
public class LevelStats
{
    // properties are NOT serialized.
    public float Time;
    public int Faults;
    public int Flips;
    public float Wheelie;
    public int LevelID;
    public int Trophies;
}
