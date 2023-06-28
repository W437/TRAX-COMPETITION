using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;
    public GameObject displayArea; // The area where prefabs will be instantiated
    public List<GameObject> bikePrefabs;
    public List<GameObject> trailPrefabs;
    public Button bikeButton;
    public Button trailButton;
    public Button nextButton;
    public Button previousButton;

    GameObject MenuBike;


    int currentBikeIndex = 0;
    int currentTrailIndex = 0;

    bool isBikeMode = true; // If true, we're cycling bikes. If false, we're cycling trails

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        bikePrefabs = new List<GameObject>(BikeController.Instance.GetAllBikes().Select(b => b.bikePrefab));
        trailPrefabs = new List<GameObject>(TrailManager.Instance.GetAllTrails().Select(t => t.trailPrefab));

        bikeButton.onClick.AddListener(() => SwitchToBikeMode());
        trailButton.onClick.AddListener(() => SwitchToTrailMode());
        nextButton.onClick.AddListener(() => NextPrefab());
        previousButton.onClick.AddListener(() => PreviousPrefab());

        MenuBike = new GameObject("Player Custom Bike");

    }

    public void BuyBike(int bikeId)
    {
        Bike bikeToBuy = bikePrefabs.FirstOrDefault(b => b.GetComponent<Bike>().bikeId == bikeId).GetComponent<Bike>();
        if (bikeToBuy == null)
            if (bikeToBuy == null)
        {
            Debug.LogError("No bike found with ID: " + bikeId);
            return;
        }

        PlayerData playerData = GameManager.Instance.GetPlayerData();

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
        PlayerData playerData = GameManager.Instance.GetPlayerData();

        if (!playerData.unlockedBikes.Contains(bikeId))
        {
            Debug.Log("You don't own this bike.");
            return;
        }

        playerData.selectedBikeId = bikeId;
        SaveSystem.SavePlayerData(playerData);
    }

    public void BuyTrail(int trailId)
    {
        Trail trailToBuy = trailPrefabs.FirstOrDefault(t => t.GetComponent<Trail>().trailId == trailId).GetComponent<Trail>();
        if (trailToBuy == null)
        {
            Debug.LogError("No trail found with ID: " + trailId);
            return;
        }

        PlayerData playerData = GameManager.Instance.GetPlayerData();

        if (playerData.coins < trailToBuy.price)
        {
            Debug.Log("Not enough coins to buy this trail.");
            return;
        }

        if (playerData.unlockedTrails.Contains(trailId))
        {
            Debug.Log("You already own this trail.");
            return;
        }

        // Subtract the price from the player's coins and unlock the trail
        playerData.coins -= trailToBuy.price;
        playerData.unlockedTrails = playerData.unlockedTrails.Concat(new int[] { trailId }).ToArray();

        // Save the player data
        SaveSystem.SavePlayerData(playerData);

        Debug.Log("Successfully bought trail with ID: " + trailId);
    }

    public void SelectTrail(int trailId)
    {
        PlayerData playerData = GameManager.Instance.GetPlayerData();

        if (!playerData.unlockedTrails.Contains(trailId))
        {
            Debug.Log("You don't own this trail.");
            return;
        }

        playerData.selectedTrailId = trailId;
        SaveSystem.SavePlayerData(playerData);
    }

    void SwitchToBikeMode()
    {
        isBikeMode = true;
        DisplayBikePrefab(bikePrefabs[currentBikeIndex]);
    }

    void SwitchToTrailMode()
    {
        isBikeMode = false;
        DisplayTrailPrefab(trailPrefabs[currentTrailIndex]);
    }

    void NextPrefab()
    {
        if (isBikeMode)
        {
            currentBikeIndex = (currentBikeIndex + 1) % trailPrefabs.Count; // cycle through array
            DisplayBikePrefab(bikePrefabs[currentBikeIndex]);
        }
        else
        {
            currentTrailIndex = (currentTrailIndex + 1) % trailPrefabs.Count;
            DisplayTrailPrefab(trailPrefabs[currentTrailIndex]);
        }
    }

    void PreviousPrefab()
    {
        if (isBikeMode)
        {
            currentBikeIndex--;
            if (currentBikeIndex < 0) currentBikeIndex = trailPrefabs.Count - 1;
            DisplayBikePrefab(bikePrefabs[currentBikeIndex]);
        }
        else
        {
            currentTrailIndex--;
            if (currentTrailIndex < 0) currentTrailIndex = trailPrefabs.Count - 1;
            DisplayTrailPrefab(trailPrefabs[currentTrailIndex]);
        }
    }

    public void DisplayBikePrefab(GameObject prefab)
    {
        var currBike = ScreenManager.Instance.CurrentPlayerBike;
        var currBikeRB = ScreenManager.Instance.RB_CurrentPlayerBike;

        // Check if prefab is null
        if (prefab == null)
        {
            Debug.LogError("Prefab is null in DisplayBikePrefab");
            return;
        }

        // Check if RB_MenuBike is null
        if (currBikeRB == null)
        {
            Debug.LogError("RB_MenuBike is null in DisplayBikePrefab");
            return;
        }

        if (GameManager.Instance.firstLaunch)
        {
            currBike = Instantiate(prefab, new Vector2(0, 2f), Quaternion.identity);
            currBike.transform.SetParent(MenuBike.transform);
        }
        else if (currBike != null)
        {
            Debug.Log("Destroying old bike: " + currBike);
            Destroy(currBike);
        }

        // Instantiate at the old position
        CameraController.Instance.menuCamera.Follow = currBike.transform;
        CameraController.Instance.shopCamera.Follow = currBike.transform;

        Debug.Log("Current bike: " + currBike);

        // Exclude the unnecessary components from the instantiated bike
        BikeComponents bikeComponents = currBike.GetComponent<BikeComponents>();
        if (bikeComponents != null)
        {
            Destroy(bikeComponents);
        }

        BikeParticles playerParticles = currBike.GetComponent<BikeParticles>();
        if (playerParticles != null)
        {
            Destroy(playerParticles);
        }

        currBikeRB = currBike.GetComponent<Rigidbody2D>(); // Update the menu bike reference

        int bikeId = currentBikeIndex; // or get bikeId from prefab
        bool isUnlocked = GameManager.Instance.GetPlayerData().unlockedBikes.Contains(bikeId);
        if (isUnlocked)
        {
            // Highlight the prefab or display other UI elements indicating that the bike is unlocked
        }
        else
        {
            // Highlight the prefab or display other UI elements indicating that the bike can be purchased
        }
    }


    public void DisplayTrailPrefab(GameObject prefab)
    {
        var currBikeRB = ScreenManager.Instance.RB_CurrentPlayerBike;
        if (prefab == null) 
        {
            Debug.LogError("Prefab is null.");
            return;
        }

        if (currBikeRB == null) 
        {
            Debug.LogError("RB_MenuBike is null.");
            return;
        }

        // Access the current trail on the bike
        Transform currentTrailTransform = currBikeRB.transform.Find("Trail");
        if (currentTrailTransform != null)
        {
            // Destroy the current trail
            Destroy(currentTrailTransform.gameObject);
        }

        // Instantiate the new trail as a child of the bike
        ScreenManager.Instance.CurrentPlayerBike = Instantiate(prefab, currBikeRB.transform);

        int trailId = currentTrailIndex; // or get trailId from prefab
        PlayerData playerData = GameManager.Instance.GetPlayerData();
        if (playerData.unlockedTrails != null)
        {
            bool isUnlocked = playerData.unlockedTrails.Contains(trailId);
            if (isUnlocked)
            {
                // Highlight the prefab or display other UI elements indicating that the trail is unlocked
            }
            else
            {
                // Highlight the prefab or display other UI elements indicating that the trail can be purchased
            }
        }
        else
        {
            Debug.LogError("unlockedTrails is null");
        }
    }



    void DisplayPrefab(GameObject prefab)
    {
        var currBike = ScreenManager.Instance.CurrentPlayerBike;
        // Destroy the current display
        if (currBike != null) Destroy(currBike);

        // Instantiate a new prefab
        currBike = Instantiate(prefab, displayArea.transform.position, Quaternion.identity, displayArea.transform);
    }

}
