using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;
    public Button bikeButton;
    public Button trailButton;
    public Button nextButton;
    public Button previousButton;
    public Button buyButton;

    [SerializeField] TextMeshProUGUI T_Coins, T_UnlockedBikes, T_UnlockedTrails;

    public Sprite buyButton_UnlockedImg, buyButton_LockedImg;

    public GameObject BikeStatus, TrailStatus;
    public Sprite LockedIcon, UnlockedIcon;

    public TextMeshProUGUI selectedPrice, selectedID;
    public bool isBikeSwitched = false;

    private GameObject currBike;

    public int currentBikeIndex { get; private set; } = 0;
    public int currentTrailIndex { get; private set; } = 0;

    public int GetCurrentBikeIndex()
    {
        return currentBikeIndex;
    }

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
        
        buyButton.onClick.AddListener(delegate
        {
            if(isBikeMode)
                BuyID(currentBikeIndex);
            else
                BuyID(currentTrailIndex);
        });

        T_Coins.text = GameManager.Instance.GetPlayerData().coins + "";
        T_UnlockedBikes.text = GameManager.Instance.GetPlayerData().unlockedBikes.Length 
        + "/" + BikeController.Instance.GetAllBikes().Length + "";

        T_UnlockedTrails.text = GameManager.Instance.GetPlayerData().unlockedTrails.Length 
        + "/" + TrailManager.Instance.GetAllTrails().Length + "";
    }

    public void ResetDefaultSelection()
    {
        Debug.Log("BikeList length: " + GameManager.Instance.BikeList.Length);
Debug.Log("TrailList length: " + GameManager.Instance.TrailList.Length);

        PlayerData _playerData = GameManager.Instance.GetPlayerData();
        Debug.Log("Player Data: " + _playerData.selectedBikeId + " Trail: " + _playerData.selectedTrailId);

        if (_playerData.selectedBikeId >= 0 && _playerData.selectedBikeId < GameManager.Instance.BikeList.Length)
        {
            DisplayBikePrefab(GameManager.Instance.BikeList[_playerData.selectedBikeId].bikePrefab);
        }
        else
        {
            Debug.LogError("Bike index is out of range.");
        }

        if (_playerData.selectedTrailId >= 0 && _playerData.selectedTrailId < GameManager.Instance.TrailList.Length)
        {
            DisplayTrailPrefab(GameManager.Instance.TrailList[_playerData.selectedTrailId].trailPrefab);
        }
        else
        {
            Debug.LogError("Trail index is out of range.");
        }
    }


    public void BuyID(int id)
    {
        if(isBikeMode)
        {
            if(BuyBike(currentBikeIndex) == BuyResult.Success)
            {
                SelectBike(currentBikeIndex);
                buyButton.GetComponent<Image>().sprite = buyButton_UnlockedImg;
                BikeStatus.GetComponent<Image>().sprite = UnlockedIcon;
            }
            else if(BuyBike(currentBikeIndex) == BuyResult.InsufficientFunds)
            {

            }
  
        }
        else
        {
            if(BuyTrail(currentTrailIndex) == BuyResult.Success)
            {
                SelectTrail(currentTrailIndex);
                buyButton.GetComponent<Image>().sprite = buyButton_UnlockedImg;
                TrailStatus.GetComponent<Image>().sprite = UnlockedIcon;
            }
            else if(BuyTrail(currentTrailIndex) == BuyResult.InsufficientFunds)
            {

            }
        }

                T_Coins.text = GameManager.Instance.GetPlayerData().coins + "";
        T_UnlockedBikes.text = GameManager.Instance.GetPlayerData().unlockedBikes.Length 
        + "/" + BikeController.Instance.GetAllBikes().Length + "";

        T_UnlockedTrails.text = GameManager.Instance.GetPlayerData().unlockedTrails.Length 
        + "/" + TrailManager.Instance.GetAllTrails().Length + "";
    }


    public enum BuyResult
    {
        Success,
        InsufficientFunds,
        InvalidID,
        Owned
    }

    public BuyResult BuyBike(int bikeId)
    {
        Bike bikeToBuy = BikeController.Instance.GetAllBikes().FirstOrDefault(b => b.bikeId == bikeId);

        if (bikeToBuy == null)
        {
            Debug.LogError("No bike found with ID: " + bikeId);
            return BuyResult.InvalidID;
        }

        PlayerData _playerData = GameManager.Instance.GetPlayerData();

        if (_playerData.coins < bikeToBuy.price)
        {
            Debug.Log("Not enough coins to buy this bike.");
            return BuyResult.InsufficientFunds;
        }

        if (_playerData.unlockedBikes.Contains(bikeId))
        {
            Debug.Log("You already own this bike.");
            return BuyResult.Owned;
        }

        _playerData.coins -= bikeToBuy.price;
        _playerData.unlockedBikes = _playerData.unlockedBikes.Concat(new int[] { bikeId }).ToArray();

        SaveSystem.SavePlayerData(_playerData);

        Debug.Log("Successfully bought bike with ID: " + bikeId);
        return BuyResult.Success;
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

    public BuyResult BuyTrail(int trailId)
    {
        Trail trailToBuy = TrailManager.Instance.GetAllTrails().FirstOrDefault(t => t.trailId == trailId);
        if (trailToBuy == null)
        {
            Debug.LogError("No trail found with ID: " + trailId);
            return BuyResult.InvalidID;
        }

        PlayerData playerData = GameManager.Instance.GetPlayerData();

        if (playerData.coins < trailToBuy.price)
        {
            Debug.Log("Not enough coins to buy this trail.");
            return BuyResult.InsufficientFunds;
        }

        if (playerData.unlockedTrails.Contains(trailId))
        {
            Debug.Log("You already own this trail.");
            return BuyResult.Owned;
        }

        playerData.coins -= trailToBuy.price;
        playerData.unlockedTrails = playerData.unlockedTrails.Concat(new int[] { trailId }).ToArray();

        SaveSystem.SavePlayerData(playerData);

        Debug.Log("Successfully bought trail with ID: " + trailId);
        return BuyResult.Success;
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
        Debug.Log("SwitchToTrailMode called");
        Debug.Log("TrailManager.Instance: " + (TrailManager.Instance == null ? "null" : "exists"));
        Debug.Log("GetAllTrails: " + (TrailManager.Instance.GetAllTrails() == null ? "null" : "exists"));
        Debug.Log("GetAllTrails[currentTrailIndex]: " + (TrailManager.Instance.GetAllTrails()[currentTrailIndex] == null ? "null" : "exists"));
        Debug.Log("trailPrefab: " + (TrailManager.Instance.GetAllTrails()[currentTrailIndex].trailPrefab == null ? "null" : "exists"));
        
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
        TrailStatus.SetActive(false);
        BikeStatus.SetActive(true);

        if (prefab == null)
        {
            Debug.LogError("Prefab is null");
            return null;
        }


        isBikeSwitched = true;

        // Check if exists
        GameObject _currentBike = ScreenManager.Instance.PlayerMenuBike;

        Vector2 oldBikePosition = (_currentBike != null) ? _currentBike.transform.position : Vector2.zero;
        Vector2 oldBikeVelocity = (_currentBike != null) ? _currentBike.GetComponent<Rigidbody2D>().velocity : Vector2.zero;

        // Store the current trail so it can be reattached to the new bike
        Transform currentTrail = null;
        Vector3 currentTrailLocalPosition = Vector3.zero; // Store local position of the trail
        Quaternion currentTrailLocalRotation = Quaternion.identity; // Store local rotation of the trail

        if(_currentBike == null)
            Debug.Log("_currentBike null.");

        if (_currentBike != null)
        {
            Transform currentTrailTransform = _currentBike.transform.Find("Bike Trail");
            if (currentTrailTransform != null)
            {
                // Detach the trail from the bike
                currentTrail = currentTrailTransform;
                currentTrailLocalPosition = currentTrail.localPosition;
                currentTrailLocalRotation = currentTrail.localRotation;
                currentTrailTransform.parent = null;
            }

            oldBikePosition = _currentBike.transform.position;
            oldBikeVelocity = _currentBike.GetComponent<Rigidbody2D>().velocity;
            Debug.Log("Destroying old bike: " + _currentBike);
            Destroy(_currentBike);
        }

        _currentBike = Instantiate(prefab, new Vector2(0, 0.7f), Quaternion.identity);
        if (_currentBike == null)
        {
            Debug.LogError("Failed to instantiate bike");
            return null;
        }
        // Remove the default trail from the new bike
        Transform newBikeTrailTransform = _currentBike.transform.Find("Bike Trail");
        if (newBikeTrailTransform != null)
        {
            Destroy(newBikeTrailTransform.gameObject);
        }

        // If a trail was detached from the old bike, attach it to the new one
        if (currentTrail != null)
        {
            currentTrail.parent = _currentBike.transform;
            currentTrail.localPosition = currentTrailLocalPosition;
            currentTrail.localRotation = currentTrailLocalRotation;
        }

        //Debug.Log("Instantiated bike: " + _currentBike.ToString());

        _currentBike.transform.position = oldBikePosition;
        _currentBike.GetComponent<Rigidbody2D>().velocity = oldBikeVelocity;

        ScreenManager.Instance.RB_PlayerMenuBike = _currentBike.GetComponent<Rigidbody2D>();

        ScreenManager.Instance.RB_PlayerMenuBike.AddForce(new Vector2(0, 0.3f), ForceMode2D.Impulse);
        ScreenManager.Instance.RB_PlayerMenuBike.constraints = RigidbodyConstraints2D.FreezeRotation; // Freeze rotation

        // Update the PlayerMenuBike reference in ScreenManager script
        ScreenManager.Instance.PlayerMenuBike = _currentBike;

        var bikeParent = ScreenManager.Instance.MenuBikeObjectParent;

        _currentBike.transform.SetParent(bikeParent.transform);
        //Debug.Log("Set to Parent: " + bikeParent);

        if (prefab == null)
        {
            Debug.LogError("Prefab is null in DisplayBikePrefab");
            return null;
        }

        // Camera Follow
        CameraController.Instance.menuCamera.Follow = _currentBike.transform;
        CameraController.Instance.shopCamera.Follow = _currentBike.transform;

        Debug.Log("Current bike: " + _currentBike);

        // Exclude unnecessary components from the instantiated bike (for Menu Only)
        BikeComponents bikeComponents = _currentBike.GetComponent<BikeComponents>();
        
        if (bikeComponents != null)
        {
            Destroy(bikeComponents);
        }

        BikeParticles playerParticles = _currentBike.GetComponent<BikeParticles>();

        if (playerParticles != null)
        {
            Destroy(playerParticles);
        }

        int bikeId = currentBikeIndex;
        bool isUnlocked = GameManager.Instance.GetPlayerData().unlockedBikes.Contains(bikeId);
       
        // Debug.Log("BikeID: " + bikeId + " Is Unlocked?:" + isUnlocked);
        // Debug.Log("Number of Bikes: " + BikeController.Instance.GetAllBikes().Length);

        Bike bikeToBuy = BikeController.Instance.GetAllBikes().FirstOrDefault(t => t.bikeId == bikeId);

        // Debug.Log("BikeToBuy: " + bikeToBuy + " Info?:" + bikeToBuy.price);

        if (isUnlocked)
        {
            selectedPrice.text = (bikeToBuy != null ? bikeToBuy.price : "0") + "";
            buyButton.GetComponent<Image>().sprite = buyButton_UnlockedImg;

            BikeStatus.GetComponent<Image>().sprite = UnlockedIcon;
        }
        else
        {
            selectedPrice.text = (bikeToBuy != null ? bikeToBuy.price : "0") + "";
            buyButton.GetComponent<Image>().sprite = buyButton_LockedImg;

            BikeStatus.GetComponent<Image>().sprite = LockedIcon;
        }

        selectedID.text = "Bike ID: " + (bikeToBuy != null ? bikeToBuy.bikeId : "null") + "";

        return _currentBike;
    }

    public void DisplayTrailPrefab(GameObject prefab)
    {
        BikeStatus.SetActive(false);
        TrailStatus.SetActive(true);

        var _currentBike = ScreenManager.Instance.PlayerMenuBike;

        if (prefab == null) 
        {
            Debug.LogError("Prefab is null.");
            return;
        }

        if (_currentBike == null) 
        {
            Debug.LogError("Current player bike is null.");
            return;
        }

        // Corrected the name to "Trail"
        Transform currentTrailTransform = _currentBike.transform.Find("Trail");
        Vector3 position = _currentBike.transform.position;

        if (currentTrailTransform != null)
        {
            position = currentTrailTransform.position;
            Destroy(currentTrailTransform.gameObject);
        }

        // Instantiate the new trail at the position of the "Bike Trail" gameobject
        int _trailId = currentTrailIndex;
        Trail _trailToBuy = TrailManager.Instance.GetAllTrails().FirstOrDefault(t => t.trailId == _trailId);

        GameObject _currentBikeTrail = Instantiate(prefab, position, Quaternion.identity, _currentBike.transform);
        _currentBikeTrail.name = "Trail";  // Name the new gameobject as "Trail"

        PlayerData _playerData = GameManager.Instance.GetPlayerData();

        if (_playerData.unlockedTrails != null)
        {
            bool isUnlocked = _playerData.unlockedTrails.Contains(_trailId);

            if (isUnlocked && !isBikeMode)
            {
                selectedPrice.text = _trailToBuy.price + "";
                TrailStatus.GetComponent<Image>().sprite = UnlockedIcon;
                buyButton.GetComponent<Image>().sprite = buyButton_UnlockedImg;
            }
            else
            {
                if (!isBikeMode)
                {
                    selectedPrice.text = _trailToBuy.price + "";
                    TrailStatus.GetComponent<Image>().sprite = LockedIcon;
                    buyButton.GetComponent<Image>().sprite = buyButton_LockedImg;
                }
            }

            if (!isBikeMode)
                selectedID.text = "Trail ID: " + (_trailToBuy != null ? _trailToBuy.trailId : "null") + "";
        }
        else
            Debug.LogError("unlockedTrails is null");
    }


}
