using Lofelt.NiceVibrations;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;
    public static ScreenManager ScreenManager;
    public Button Btn_SwitchBike;
    public Button Btn_SwitchTrail;
    public Button Btn_Next;
    public Button Btn_Prev;
    public Button Btn_Buy;

    public TextMeshProUGUI T_Coins, T_UnlockedBikes, T_UnlockedTrails;

    [SerializeField] private Sprite buyButton_UnlockedImg, buyButton_LockedImg;

    public GameObject BikeStatus, TrailStatus;
    public Sprite LockedIcon, UnlockedIcon;

    public TextMeshProUGUI SelectedShopItemPrice, SelectedShopItemID;
    bool isBikeMode = true; // If true, we're cycling bikes. If false, we're cycling trails
    public bool isBikeSwitched = false;

    public int CurrentBikeIndex { get; private set; } = 0;
    public int CurrentTrailIndex { get; private set; } = 0;

    private float _lastButtonClickTime = 0f;
    private float _buttonClickCooldown = 0.25f;



    private void Awake()
    {
        Instance = this;
        PlayerData _data = SaveSystem.LoadPlayerData();
        CurrentBikeIndex = _data.SELECTED_BIKE_ID;
        CurrentTrailIndex = _data.SELECTED_TRAIL_ID;
        SaveSystem.SavePlayerData(_data);
    }

    void Start()
    {
        Btn_SwitchBike.onClick.AddListener(delegate
        {
            if (Time.time - _lastButtonClickTime < _buttonClickCooldown)
                return;
            _lastButtonClickTime = Time.time;

            SwitchToBikeMode();
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact);
        });


        Btn_SwitchTrail.onClick.AddListener(delegate
        {
            if (Time.time - _lastButtonClickTime < _buttonClickCooldown)
                return;
            _lastButtonClickTime = Time.time;

            SwitchToTrailMode();
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact);
        });


        Btn_Next.onClick.AddListener(delegate
        {
            if (Time.time - _lastButtonClickTime < _buttonClickCooldown)
                return;
            _lastButtonClickTime = Time.time;

            NextPrefab();
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.Selection);
        });

        Btn_Prev.onClick.AddListener(delegate
        {
            if (Time.time - _lastButtonClickTime < _buttonClickCooldown)
                return;
            _lastButtonClickTime = Time.time;

            PreviousPrefab();
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.Selection);
        });

        Btn_Buy.onClick.AddListener(delegate
        {
            if (Time.time - _lastButtonClickTime / 2 < _buttonClickCooldown)
                return;
            _lastButtonClickTime = Time.time;

            if (isBikeMode)
                BuyID(CurrentBikeIndex);
            else
                BuyID(CurrentTrailIndex);
        });

        ScreenManager.Instance.RefreshTextValuesFromPlayerData();
    }

    public void BuyID(int id)
    {
        var _playerData = SaveSystem.LoadPlayerData();
        if (isBikeMode)
        {
            if (BuyBike(CurrentBikeIndex) == BuyResult.Success)
            {
                HapticPatterns.PlayPreset(HapticPatterns.PresetType.Success);
                SelectBike(CurrentBikeIndex);
                Btn_Buy.GetComponent<Image>().sprite = buyButton_UnlockedImg;
                BikeStatus.GetComponent<Image>().sprite = UnlockedIcon;
            }
            else if (BuyBike(CurrentBikeIndex) == BuyResult.InsufficientFunds)
            {
                HapticPatterns.PlayPreset(HapticPatterns.PresetType.Failure);
            }
        }

        else
        {
            if (BuyTrail(CurrentTrailIndex) == BuyResult.Success)
            {
                HapticPatterns.PlayPreset(HapticPatterns.PresetType.Success);
                SelectTrail(CurrentTrailIndex);
                Btn_Buy.GetComponent<Image>().sprite = buyButton_UnlockedImg;
                TrailStatus.GetComponent<Image>().sprite = UnlockedIcon;
            }
            else if (BuyTrail(CurrentTrailIndex) == BuyResult.InsufficientFunds)
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

    public int GetCurrentBikeIndex()
    {
        return CurrentBikeIndex;
    }

    public BuyResult BuyBike(int bikeId)
    {
        PlayerData _playerData = SaveSystem.LoadPlayerData();
        Bike bikeToBuy = BikeController.Instance.GetAllBikes().FirstOrDefault(b => b.ID == bikeId);

        if (bikeToBuy == null)
        {
            Debug.LogError("No bike found with ID: " + bikeId);
            return BuyResult.InvalidID;
        }

        Debug.Log("Shop coins: " + _playerData.COINS);
        if (_playerData.COINS < bikeToBuy.PRICE)
        {
            Debug.Log("Not enough coins to buy this bike.");
            return BuyResult.InsufficientFunds;
        }

        if (_playerData.UNLOCKED_BIKES.Contains(bikeId))
        {
            Debug.Log("You already own this bike.");
            return BuyResult.Owned;
        }

        // tween coins
        int startValue = _playerData.COINS;
        _playerData.COINS -= bikeToBuy.PRICE;
        int endValue = _playerData.COINS;

        LeanTween.value(gameObject, startValue, endValue, 2f).setEaseInOutSine().setOnUpdate((float value) =>
        {
            ScreenManager.Instance.Txt_Shop_Coins.text = "" + Mathf.RoundToInt(value);
        });

        _playerData.UNLOCKED_BIKES = _playerData.UNLOCKED_BIKES.Concat(new int[] { bikeId }).ToArray();
        _playerData.SELECTED_BIKE_ID = bikeId;
        _playerData.AddXP(bikeToBuy.PRICE * 5);
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
        Trail trailToBuy = TrailManager.Instance.GetAllTrails().FirstOrDefault(t => t.ID == trailId);
        if (trailToBuy == null)
        {
            Debug.LogError("No trail found with ID: " + trailId);
            return BuyResult.InvalidID;
        }

        PlayerData _playerData = SaveSystem.LoadPlayerData();

        if (_playerData.COINS < trailToBuy.PRICE)
        {
            Debug.Log("Not enough coins to buy this trail.");
            return BuyResult.InsufficientFunds;
        }

        if (_playerData.UNLOCKED_TRAILS.Contains(trailId))
        {
            Debug.Log("You already own this trail.");
            return BuyResult.Owned;
        }

        // tween coins
        int startValue = _playerData.COINS;
        _playerData.COINS -= trailToBuy.PRICE;
        int endValue = _playerData.COINS;

        LeanTween.value(gameObject, startValue, endValue, 2f).setEaseInOutSine().setOnUpdate((float value) =>
        {
            ScreenManager.Instance.Txt_Shop_Coins.text = "" + Mathf.RoundToInt(value);
        });

        _playerData.UNLOCKED_TRAILS = _playerData.UNLOCKED_TRAILS.Concat(new int[] { trailId }).ToArray();
        _playerData.AddXP(trailToBuy.PRICE * 5);
        _playerData.SELECTED_TRAIL_ID = trailId;
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

    private void SwitchToBikeMode()
    {
        isBikeMode = true;
        float rgb = 255;
        Btn_SwitchBike.GetComponent<Image>().color = new Color(255 / rgb, 186 / rgb, 0, 0.85f);
        Btn_SwitchTrail.GetComponent<Image>().color = new Color(0, 0, 0, 0.4f);
        DisplayBikePrefab(BikeController.Instance.GetAllBikes()[CurrentBikeIndex].BikePrefab);
    }

    private void SwitchToTrailMode()
    {
        isBikeMode = false;
        float rgb = 255;
        Btn_SwitchBike.GetComponent<Image>().color = new Color(0, 0, 0, 0.4f);
        Btn_SwitchTrail.GetComponent<Image>().color = new Color(255 / rgb, 186 / rgb, 0, 0.85f);
        DisplayTrailPrefab(TrailManager.Instance.GetAllTrails()[CurrentTrailIndex].TrailPrefab);
    }

    private void NextPrefab()
    {
        PlayerData playerData = SaveSystem.LoadPlayerData();

        if (isBikeMode)
        {
            CurrentBikeIndex = (CurrentBikeIndex + 1) % BikeController.Instance.GetAllBikes().Length; // cycle through array
            Debug.Log("Current bike index: " + CurrentBikeIndex);
            Debug.Log("Bike List Length: " + BikeController.Instance.GetAllBikes().Length);
            DisplayBikePrefab(BikeController.Instance.GetAllBikes()[CurrentBikeIndex].BikePrefab);

            if (playerData.UNLOCKED_BIKES.Contains(CurrentBikeIndex))
            {
                playerData.SELECTED_BIKE_ID = CurrentBikeIndex;
            }
        }
        else
        {
            CurrentTrailIndex = (CurrentTrailIndex + 1) % TrailManager.Instance.GetAllTrails().Length;
            DisplayTrailPrefab(TrailManager.Instance.GetAllTrails()[CurrentTrailIndex].TrailPrefab);
            if (playerData.UNLOCKED_TRAILS.Contains(CurrentTrailIndex))
            {
                playerData.SELECTED_TRAIL_ID = CurrentTrailIndex;
            }
        }

        SaveSystem.SavePlayerData(playerData);
    }

    private void PreviousPrefab()
    {
        PlayerData playerData = SaveSystem.LoadPlayerData();

        if (isBikeMode)
        {
            CurrentBikeIndex--;
            if (CurrentBikeIndex < 0) CurrentBikeIndex = BikeController.Instance.GetAllBikes().Length - 1;
            DisplayBikePrefab(BikeController.Instance.GetAllBikes()[CurrentBikeIndex].BikePrefab);

            if (playerData.UNLOCKED_BIKES.Contains(CurrentBikeIndex))
            {
                playerData.SELECTED_BIKE_ID = CurrentBikeIndex;
            }
        }
        else
        {
            CurrentTrailIndex--;
            if (CurrentTrailIndex < 0) CurrentTrailIndex = TrailManager.Instance.GetAllTrails().Length - 1;
            DisplayTrailPrefab(TrailManager.Instance.GetAllTrails()[CurrentTrailIndex].TrailPrefab);

            if (playerData.UNLOCKED_TRAILS.Contains(CurrentTrailIndex))
            {
                playerData.SELECTED_TRAIL_ID = CurrentTrailIndex;
            }
        }

        SaveSystem.SavePlayerData(playerData);
    }

    public GameObject DisplayBikePrefab(GameObject prefab)
    {
        isBikeSwitched = true;
        TrailStatus.SetActive(false);
        BikeStatus.SetActive(true);
        var _playerData = SaveSystem.LoadPlayerData();
        SelectBike(this.CurrentBikeIndex);
        SelectTrail(CurrentTrailIndex);

        if (prefab == null)
        {
            Debug.LogError("Prefab is null");
            return null;
        }

        // Check if exists
        GameObject _currentBike = ScreenManager.Instance.PlayerMenuBike;

        Vector2 oldBikePosition = (_currentBike != null) ? _currentBike.transform.position : Vector2.zero;
        Vector2 oldBikeVelocity = (_currentBike != null) ? _currentBike.GetComponent<Rigidbody2D>().velocity : Vector2.zero;

        // Store the current trail so it can be reattached to the new bike
        Transform _currentTrail = null;
        Transform _trailPosition = null;
        Vector3 _currentTrailLocalPosition = Vector3.zero;
        Quaternion _currentTrailLocalRotation = Quaternion.identity;

        if (_currentBike == null)
            Debug.Log("_currentBike null.");

        if (_currentBike != null)
        {
            _trailPosition = _currentBike.transform.Find("Bike Trail");
            Transform _currentTrailTransform = _currentBike.transform.Find("Trail");
            if (_currentTrailTransform != null)
            {
                // Detach the trail from the bike
                _currentTrail = _currentTrailTransform;
                _currentTrailLocalPosition = _trailPosition.localPosition;
                _currentTrailLocalRotation = _trailPosition.localRotation;
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


        // If a trail was detached from the old bike, attach it to the new one
        if (_currentTrail != null)
        {
            _currentTrail.parent = _currentBike.transform;
            _currentTrail.localPosition = _trailPosition.localPosition;
            _currentTrail.localRotation = _trailPosition.localRotation;
            Destroy(_trailPosition.gameObject);
        }

        //Debug.Log("Instantiated bike: " + _currentBike.ToString());

        _currentBike.transform.position = oldBikePosition;
        _currentBike.GetComponent<Rigidbody2D>().velocity = oldBikeVelocity;

        ScreenManager.Instance.PlayerMenuBikeRb = _currentBike.GetComponent<Rigidbody2D>();

        ApplyImpulseAndFreezeRotation(); // Pops bike when switched


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
        CameraController.Instance.MenuCamera.Follow = _currentBike.transform;
        CameraController.Instance.SettingsCamera.Follow = _currentBike.transform;
        CameraController.Instance.ShopCamera.Follow = _currentBike.transform;

        //Debug.Log("Current bike: " + _currentBike);

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

        bool isUnlocked = _playerData.UNLOCKED_BIKES.Contains(CurrentBikeIndex);

        // Debug.Log("BikeID: " + bikeId + " Is Unlocked?:" + isUnlocked);
        // Debug.Log("Number of Bikes: " + BikeController.Instance.GetAllBikes().Length);

        Bike bikeToBuy = BikeController.Instance.GetAllBikes().FirstOrDefault(t => t.ID == CurrentBikeIndex);

        // Debug.Log("BikeToBuy: " + bikeToBuy + " Info?:" + bikeToBuy.price);

        if (isUnlocked)
        {
            SelectedShopItemPrice.text = (bikeToBuy != null ? bikeToBuy.PRICE : "0") + "";
            Btn_Buy.GetComponent<Image>().sprite = buyButton_UnlockedImg;

            BikeStatus.GetComponent<Image>().sprite = UnlockedIcon;
        }
        else
        {
            SelectedShopItemPrice.text = (bikeToBuy != null ? bikeToBuy.PRICE : "0") + "";
            Btn_Buy.GetComponent<Image>().sprite = buyButton_LockedImg;

            BikeStatus.GetComponent<Image>().sprite = LockedIcon;
        }

        SelectedShopItemID.text = "Bike ID: " + (bikeToBuy != null ? bikeToBuy.ID : "null") + "";

        return _currentBike;
    }

    private void ApplyImpulseAndFreezeRotation()
    {
        ScreenManager.Instance.PlayerMenuBikeRb.AddForce(new Vector2(0, 0.3f), ForceMode2D.Impulse);
        ScreenManager.Instance.PlayerMenuBikeRb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public GameObject DisplayTrailPrefab(GameObject prefab)
    {
        BikeStatus.SetActive(false);
        TrailStatus.SetActive(true);
        var _playerData = SaveSystem.LoadPlayerData();
        var _currentBike = ScreenManager.Instance.PlayerMenuBike;
        Transform _trailPosition = (_currentBike != null) ? _currentBike.transform.Find("Bike Trail") : null;

        if (prefab == null)
        {
            Debug.LogError("Prefab is null.");
            return null;
        }

        if (_currentBike == null)
        {
            Debug.LogError("Current player bike is null.");
            return null;
        }

        // Corrected the name to "Trail"
        Transform currentTrailTransform = _currentBike.transform.Find("Trail");
        Vector3 position = _trailPosition.transform.position;

        if (currentTrailTransform != null)
        {
            position = currentTrailTransform.position;
            Destroy(currentTrailTransform.gameObject);
            //Destroy(_trailPosition.gameObject);
        }

        // Instantiate the new trail at the position of the "Trail" gameobject
        Trail _trailToBuy = TrailManager.Instance.GetAllTrails().FirstOrDefault(t => t.ID == CurrentTrailIndex);

        // Parent the new trail to the current bike
        GameObject _currentBikeTrail = Instantiate(prefab, position, Quaternion.identity, ScreenManager.Instance.PlayerMenuBike.transform);
        _currentBikeTrail.name = "Trail";

        if (_playerData.UNLOCKED_TRAILS != null)
        {
            bool isUnlocked = _playerData.UNLOCKED_TRAILS.Contains(CurrentTrailIndex);

            if (isUnlocked && !isBikeMode)
            {
                SelectedShopItemPrice.text = _trailToBuy.PRICE + "";
                TrailStatus.GetComponent<Image>().sprite = UnlockedIcon;
                Btn_Buy.GetComponent<Image>().sprite = buyButton_UnlockedImg;
            }
            else
            {
                if (!isBikeMode)
                {
                    SelectedShopItemPrice.text = _trailToBuy.PRICE + "";
                    TrailStatus.GetComponent<Image>().sprite = LockedIcon;
                    Btn_Buy.GetComponent<Image>().sprite = buyButton_LockedImg;
                }
            }

            if (!isBikeMode)
                SelectedShopItemID.text = "Trail ID: " + (_trailToBuy != null ? _trailToBuy.ID : "null") + "";
        }
        else
            Debug.LogError("unlockedTrails is null");

        return _currentBikeTrail;
    }
}
