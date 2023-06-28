using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;
    public GameObject displayArea; // The area where prefabs will be instantiated
    public Button bikeButton;
    public Button trailButton;
    public Button nextButton;
    public Button previousButton;
    public Button buyButton;
    public TextMeshProUGUI selectedPrice;
    public bool isBikeSwitched = false;

    private GameObject currBike;

    int currentBikeIndex = 0;
    int currentTrailIndex = 0;

    bool isBikeMode = true; // If true, we're cycling bikes. If false, we're cycling trails

    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
    }

    void Start()
    {
        bikeButton.onClick.AddListener(() => SwitchToBikeMode());
        trailButton.onClick.AddListener(() => SwitchToTrailMode());
        nextButton.onClick.AddListener(() => NextPrefab());
        previousButton.onClick.AddListener(() => PreviousPrefab());
        buyButton.onClick.AddListener(() => PreviousPrefab());
    }

    public void BuyBike(int bikeId)
    {
        Bike bikeToBuy = BikeController.Instance.GetAllBikes().FirstOrDefault(b => b.bikeId == bikeId);

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
        Trail trailToBuy = TrailManager.Instance.GetAllTrails().FirstOrDefault(t => t.trailId == trailId);
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
        bikeButton.GetComponent<Image>().color = new Color(0,0,0,0.8f);
        trailButton.GetComponent<Image>().color = new Color(0,0,0,0.4f);
        DisplayBikePrefab(BikeController.Instance.GetAllBikes()[currentBikeIndex].bikePrefab);
    }

    void SwitchToTrailMode()
    {
        isBikeMode = false;
        bikeButton.GetComponent<Image>().color = new Color(0,0,0,0.4f);
        trailButton.GetComponent<Image>().color = new Color(0,0,0,0.8f);
        DisplayTrailPrefab(TrailManager.Instance.GetAllTrails()[currentTrailIndex].trailPrefab);
    }

    void NextPrefab()
    {
        if (isBikeMode)
        {
            currentBikeIndex = (currentBikeIndex + 1) % BikeController.Instance.GetAllBikes().Length; // cycle through array
            DisplayBikePrefab(BikeController.Instance.GetAllBikes()[currentBikeIndex].bikePrefab);
        }
        else
        {
            currentTrailIndex = (currentTrailIndex + 1) % TrailManager.Instance.GetAllTrails().Length;
            DisplayTrailPrefab(TrailManager.Instance.GetAllTrails()[currentTrailIndex].trailPrefab);
        }
    }

    void PreviousPrefab()
    {
        if (isBikeMode)
        {
            currentBikeIndex--;
            if (currentBikeIndex < 0) currentBikeIndex = BikeController.Instance.GetAllBikes().Length - 1;
            DisplayBikePrefab(BikeController.Instance.GetAllBikes()[currentBikeIndex].bikePrefab);
        }
        else
        {
            currentTrailIndex--;
            if (currentTrailIndex < 0) currentTrailIndex = TrailManager.Instance.GetAllTrails().Length - 1;
            DisplayTrailPrefab(TrailManager.Instance.GetAllTrails()[currentTrailIndex].trailPrefab);
        }
    }



    public GameObject DisplayBikePrefab(GameObject prefab)
    {

        if (prefab == null)
        {
            Debug.LogError("Prefab is null");
            return null;
        }


        isBikeSwitched = true;

        // Check if exists
        GameObject currBike = ScreenManager.Instance.PlayerMenuBike;

        Vector2 oldBikePosition = (currBike != null) ? currBike.transform.position : Vector2.zero;
        Vector2 oldBikeVelocity = (currBike != null) ? currBike.GetComponent<Rigidbody2D>().velocity : Vector2.zero;


        // Store the current trail so it can be reattached to the new bike
        Transform currentTrail = null;
        Vector3 currentTrailLocalPosition = Vector3.zero; // Store local position of the trail
        Quaternion currentTrailLocalRotation = Quaternion.identity; // Store local rotation of the trail

        if(currBike == null)
            Debug.Log("Currbike Inistial Null.");

        if (currBike != null)
        {
            Transform currentTrailTransform = currBike.transform.Find("Bike Trail");
            if (currentTrailTransform != null)
            {
                // Detach the trail from the bike
                currentTrail = currentTrailTransform;
                currentTrailLocalPosition = currentTrail.localPosition;
                currentTrailLocalRotation = currentTrail.localRotation;
                currentTrailTransform.parent = null;
            }

            oldBikePosition = currBike.transform.position;
            oldBikeVelocity = currBike.GetComponent<Rigidbody2D>().velocity;
            Debug.Log("Destroying old bike: " + currBike);
            Destroy(currBike);
        }

        currBike = Instantiate(prefab, new Vector2(0, 0.7f), Quaternion.identity);
    if (currBike == null)
    {
        Debug.LogError("Failed to instantiate bike");
        return null;
    }
        // Remove the default trail from the new bike
        Transform newBikeTrailTransform = currBike.transform.Find("Bike Trail");
        if (newBikeTrailTransform != null)
        {
            Destroy(newBikeTrailTransform.gameObject);
        }

        // If a trail was detached from the old bike, attach it to the new one
        if (currentTrail != null)
        {
            currentTrail.parent = currBike.transform;
            currentTrail.localPosition = currentTrailLocalPosition;
            currentTrail.localRotation = currentTrailLocalRotation;
        }

        Debug.Log("Instantiated new bike: " + currBike.ToString());

        currBike.transform.position = oldBikePosition;
        currBike.GetComponent<Rigidbody2D>().velocity = oldBikeVelocity;

        ScreenManager.Instance.RB_PlayerMenuBike = currBike.GetComponent<Rigidbody2D>();

        ScreenManager.Instance.RB_PlayerMenuBike.AddForce(new Vector2(0, 0.3f), ForceMode2D.Impulse);
        ScreenManager.Instance.RB_PlayerMenuBike.constraints = RigidbodyConstraints2D.FreezeRotation; // Freeze rotation

        // Update the PlayerMenuBike reference in ScreenManager script
        ScreenManager.Instance.PlayerMenuBike = currBike;

        var bikeParent = ScreenManager.Instance.MenuBikeObjectParent;

        currBike.transform.SetParent(bikeParent.transform);
        Debug.Log("Set to Parent: " + bikeParent);

        // Check if prefab is null
        if (prefab == null)
        {
            Debug.LogError("Prefab is null in DisplayBikePrefab");
            return null;
        }

        // Camera Follow
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
        int bikeId = currentBikeIndex; // or get bikeId from prefab
        bool isUnlocked = GameManager.Instance.GetPlayerData().unlockedBikes.Contains(bikeId);
        Debug.Log("BikeID: " + bikeId + " Is Unlocked?:" + isUnlocked);
        Debug.Log("Current Bike ID: " + bikeId);
        Debug.Log("Number of Bikes: " + BikeController.Instance.GetAllBikes().Length);
        foreach(Bike bike in BikeController.Instance.GetAllBikes())
{
    Debug.Log("Bike in bikePrefabs, ID: " + bike.bikeId);
}

        Bike bikeToBuy = BikeController.Instance.GetAllBikes().FirstOrDefault(t => t.bikeId == bikeId);
        foreach(Bike bike in BikeController.Instance.GetAllBikes())
        {
            Debug.Log("Bike ID: " + bike.bikeId + ", Bike Price: " + bike.price);
        }
        Debug.Log("BikeToBuy: " + bikeToBuy + " Info?:" + bikeToBuy.price);
        if (isUnlocked)
        {
             selectedPrice.text = (bikeToBuy != null ? bikeToBuy.price : "0") + "\nAlready owned.";
        }
        else
        {
            selectedPrice.text = (bikeToBuy != null ? bikeToBuy.price : "0") + "";
        }

        return currBike;
    }


    public void DisplayTrailPrefab(GameObject prefab)
    {
        var currBike = ScreenManager.Instance.PlayerMenuBike;

        if (prefab == null) 
        {
            Debug.LogError("Prefab is null.");
            return;
        }

        if (currBike == null) 
        {
            Debug.LogError("PlayerMenuBike is null.");
            return;
        }

        // Access the current trail on the bike
        Transform currentTrailTransform = currBike.transform.Find("Bike Trail");
        if (currentTrailTransform != null)
        {
            // Destroy the current trail
            Destroy(currentTrailTransform.gameObject);
        }

        // Instantiate the new trail at the position of the "Bike Trail" gameobject
        GameObject newTrail = Instantiate(prefab, currentTrailTransform.position, Quaternion.identity, currBike.transform);
        newTrail.name = "Bike Trail";  // Name the new gameobject as "Bike Trail"
      

        int trailId = currentTrailIndex; // or get trailId from prefab
        Trail trailToBuy = TrailManager.Instance.GetAllTrails().FirstOrDefault(t => t.trailId == trailId);
        PlayerData playerData = GameManager.Instance.GetPlayerData();
        if (playerData.unlockedTrails != null)
        {
            bool isUnlocked = playerData.unlockedTrails.Contains(trailId);
            if (isUnlocked)
            {
                selectedPrice.text = trailToBuy.price + "\nAlready owned.";
            }
            else
            {
               selectedPrice.text = trailToBuy.price + "\nAlready owned.";
            }
        }
        else
        {
            Debug.LogError("unlockedTrails is null");
        }
        
    }



    void DisplayPrefab(GameObject prefab)
    {
        var currBike = ScreenManager.Instance.PlayerMenuBike;
        // Destroy the current display
        if (currBike != null) Destroy(currBike);

        // Instantiate a new prefab
        currBike = Instantiate(prefab, displayArea.transform.position, Quaternion.identity, displayArea.transform);
    }

}
