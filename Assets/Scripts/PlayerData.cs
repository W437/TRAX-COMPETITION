using System;

[Serializable]
public class PlayerData
{
    public int coins;
    public int[] unlockedBikes;
    public int selectedBikeId;
    public int[] unlockedTrails;
    public int selectedTrailId;

    // Stats
    public int experiencePoints;
    public int trophiesCount;
    public float totalRideDistance;
    public float totalFaults; // across all levels at the moment
    public float totalPlayTime;
    public int totalFlips;
    public float totalWheelie;
}
