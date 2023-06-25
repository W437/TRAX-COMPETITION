using System.Linq;
using UnityEngine;

public class ShopSystem : MonoBehaviour
{
    public Bike[] bikes; // Set this in the editor
    private PlayerData playerData;

    private void Start()
    {
        playerData = SaveSystem.LoadPlayerData();
        if (playerData == null)
        {
            playerData = new PlayerData();
            SaveSystem.SavePlayerData(playerData);
        }
    }

    public void BuyBike(int bikeId)
    {
        Bike bikeToBuy = bikes.FirstOrDefault(b => b.id == bikeId);
        if (bikeToBuy == null)
        {
            Debug.LogError("No bike found with ID: " + bikeId);
            return;
        }

        if (playerData.coins < bikeToBuy.price)
        {
            Debug.Log("Not enough coins to buy this bike.");
            return;
        }

        if (playerData.unlockedBikes.Contains(bikeId))
        {
            Debug.Log("You already own this bike.");
            return;
        }

        // Subtract the price from the player's coins and unlock the bike
        playerData.coins -= bikeToBuy.price;
        playerData.unlockedBikes = playerData.unlockedBikes.Concat(new int[] { bikeId }).ToArray();

        // Save the player data
        SaveSystem.SavePlayerData(playerData);

        Debug.Log("Successfully bought bike with ID: " + bikeId);
    }

    public void SelectBike(int bikeId)
    {
        if (!playerData.unlockedBikes.Contains(bikeId))
        {
            Debug.Log("You don't own this bike.");
            return;
        }

        playerData.selectedBikeId = bikeId;
        SaveSystem.SavePlayerData(playerData);
    }
}
