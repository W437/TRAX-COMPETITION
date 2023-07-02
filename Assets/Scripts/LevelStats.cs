[System.Serializable]
public class LevelStats
{
   public float Time { get; set; }
    public int Faults { get; set; }
    public int Flips { get; set; }
    public float Wheelie { get; set; }
    public int LevelID { get; set; }
    public LevelStats Stats { get; set; }
    public int Trophies { get; set; }

}