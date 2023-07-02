using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lofelt.NiceVibrations;
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

    [SerializeField] public TextMeshProUGUI T_Coins, T_UnlockedBikes, T_UnlockedTrails;

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
        bikeButton.onClick.AddListener(delegate
        {
            SwitchToBikeMode();
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact);
        });


        trailButton.onClick.AddListener(delegate
        {
            SwitchToTrailMode();
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact);
        });
        
        
        nextButton.onClick.AddListener(delegate
        {
            NextPrefab();
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.Selection);
        });
            
        nextButton.onClick.AddListener(delegate
        {
            PreviousPrefab();
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.Selection);
        });
        
        buyButton.onClick.AddListener(delegate
        {
            if(isBikeMode)
                BuyID(currentBikeIndex);
            else
                BuyID(currentTrailIndex);
        });

        ScreenManager.Instance.RefreshTextValuesFromPlayerData();
    }

    public void BuyID(int id)
    {
        var _playerData = SaveSystem.LoadPlayerData();
        if(isBikeMode)
        {
            if(BuyBike(currentBikeIndex) == BuyResult.Success)
            {
                HapticPatterns.PlayPreset(HapticPatterns.PresetType.Success);
                SelectBike(currentBikeIndex);
                buyButton.GetComponent<Image>().sprite = buyButton_UnlockedImg;
                BikeStatus.GetComponent<Image>().sprite = UnlockedIcon;
            }
            else if(BuyBike(currentBikeIndex) == BuyResult.InsufficientFunds)
            {
                HapticPatterns.PlayPreset(HapticPatterns.PresetType.Failure);
            }
  
        }
        else
        {
            if(BuyTrail(currentTrailIndex) == BuyResult.Success)
            {
                HapticPatterns.PlayPreset(HapticPatterns.PresetType.Success);
                SelectTrail(currentTrailIndex);
                buyButton.GetComponent<Image>().sprite = buyButton_UnlockedImg;
                TrailStatus.GetComponent<Image>().sprite = UnlockedIcon;
            }
            else if(BuyTrail(currentTrailIndex) == BuyResult.InsufficientFunds)
            {
                HapticPatterns.PlayPreset(HapticPatterns.PresetType.Failure);   
            }
        }

        ScreenManager.Instance.RefreshTextValuesFromPlayerData();
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
        PlayerData _playerData = SaveSystem.LoadPlayerData();
        Bike bikeToBuy = BikeController.Instance.GetAllBikes().FirstOrDefault(b => b.bikeId == bikeId);

        if (bikeToBuy == null)
        {
            Debug.LogError("No bike found with ID: " + bikeId);
            return BuyResult.InvalidID;
        }

        Debug.Log("Shop coins: " + _playerData.COINS);
        if (_playerData.COINS < bikeToBuy.price)
        {
            Debug.Log("Not enough coins to buy this bike.");
            return BuyResult.InsufficientFunds;
        }

        if (_playerData.UNLOCKED_BIKES.Contains(bikeId))
        {
            Debug.Log("You already own this bike.");
            return BuyResult.Owned;
        }

        _playerData.COINS -= bikeToBuy.price;
        _playerData.UNLOCKED_BIKES = _playerData.UNLOCKED_BIKES.Concat(new int[] { bikeId }).ToArray();
        _playerData.AddXP(bikeToBuy.price*5);
         _playerData.UpdateLevel();
        SaveSystem.SavePlayerData(_playerData);

        Debug.Log("Successfully bought bike with ID: " + bikeId);
        return BuyResult.Success;
    }

    public void SelectBike(int bikeId)
    {
        PlayerData playerData = SaveSystem.LoadPlayerData();

        if (!playerData.UNLOCKED_BIKES.Contains(bikeId))
        {
            Debug.Log("You don't own this bike.");
            return;
        }

        playerData.SELECTED_BIKE_ID = bikeId;
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

        PlayerData _playerData = SaveSystem.LoadPlayerData();

        if (_playerData.COINS < trailToBuy.price)
        {
            Debug.Log("Not enough coins to buy this trail.");
            return BuyResult.InsufficientFunds;
        }

        if (_playerData.UNLOCKED_TRAILS.Contains(trailId))
        {
            Debug.Log("You already own this trail.");
            return BuyResult.Owned;
        }

        _playerData.COINS -= trailToBuy.price;
        _playerData.UNLOCKED_TRAILS = _playerData.UNLOCKED_TRAILS.Concat(new int[] { trailId }).ToArray();
        _playerData.AddXP(trailToBuy.price*5);
        SaveSystem.SavePlayerData(_playerData);

        Debug.Log("Successfully bought trail with ID: " + trailId);
        return BuyResult.Success;
    }

    public void SelectTrail(int trailId)
    {
        PlayerData playerData = SaveSystem.LoadPlayerData();

        if (!playerData.UNLOCKED_TRAILS.Contains(trailId))
        {
            Debug.Log("You don't own this trail.");
            return;
        }

        playerData.SELECTED_TRAIL_ID = trailId;
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
        var _playerData = SaveSystem.LoadPlayerData();
        SelectBike(currentBikeIndex);
        SelectTrail(currentTrailIndex);
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
        Transform _currentTrail = null;
        Vector3 _currentTrailLocalPosition = Vector3.zero; // Store local position of the trail
        Quaternion _currentTrailLocalRotation = Quaternion.identity; // Store local rotation of the trail

        if(_currentBike == null)
            Debug.Log("_currentBike null.");

        if (_currentBike != null)
        {
            Transform _currentTrailTransform = _currentBike.transform.Find("Trail");
            if (_currentTrailTransform != null)
            {
                // Detach the trail from the bike
                _currentTrail = _currentTrailTransform;
                _currentTrailLocalPosition = _currentTrail.localPosition;
                _currentTrailLocalRotation = _currentTrail.localRotation;
                _currentTrailTransform.parent = null;
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
        Transform _newBikeTrailTransform = _currentBike.transform.Find("Bike Trail");
        if (_newBikeTrailTransform != null)
        {
            Destroy(_newBikeTrailTransform.gameObject);
        }

        // If a trail was detached from the old bike, attach it to the new one
        if (_currentTrail != null)
        {
            _currentTrail.parent = _currentBike.transform;
            _currentTrail.localPosition = _currentTrailLocalPosition;
            _currentTrail.localRotation = _currentTrailLocalRotation;
        }

        //Debug.Log("Instantiated bike: " + _currentBike.ToString());

        _currentBike.transform.position = oldBikePosition;
        _currentBike.GetComponent<Rigidbody2D>().velocity = oldBikeVelocity;

        ScreenManager.Instance.PlayerMenuBikeRb = _currentBike.GetComponent<Rigidbody2D>();

        StartCoroutine(ApplyImpulseAndFreezeRotation());


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
        CameraController.Instance.settingsCamera.Follow = _currentBike.transform;
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
        bool isUnlocked = _playerData.UNLOCKED_BIKES.Contains(bikeId);
       
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

    IEnumerator ApplyImpulseAndFreezeRotation()
    {
        // Apply the impulse
        ScreenManager.Instance.PlayerMenuBikeRb.AddForce(new Vector2(0, 0.3f), ForceMode2D.Impulse);

        // Freeze the rotation
        ScreenManager.Instance.PlayerMenuBikeRb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Wait for the impulse time
        float impulseTime = 0.5f; // Adjust this value as needed
        yield return new WaitForSeconds(impulseTime);

        // Unfreeze the rotation
        ScreenManager.Instance.PlayerMenuBikeRb.constraints = RigidbodyConstraints2D.None;
    }


    public void DisplayTrailPrefab(GameObject prefab)
    {
        BikeStatus.SetActive(false);
        TrailStatus.SetActive(true);
        var _playerData = SaveSystem.LoadPlayerData();
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

        // Parent the new trail to the current bike
        GameObject _currentBikeTrail = Instantiate(prefab, position, Quaternion.identity, ScreenManager.Instance.PlayerMenuBike.transform);
        _currentBikeTrail.name = "Trail";

        if (_playerData.UNLOCKED_TRAILS != null)
        {
            bool isUnlocked = _playerData.UNLOCKED_TRAILS.Contains(_trailId);

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
