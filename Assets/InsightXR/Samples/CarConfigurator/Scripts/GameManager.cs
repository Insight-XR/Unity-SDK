using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public Car myCarInstance;
    private Basket basket;
    private GameObject spawnPoint;
    private GameObject myCarPrefab;
    private Dictionary<int, string> shoppingList = new Dictionary<int, string>();
    private TMP_Text currentSelectionText;
    private TMP_Text currentPriceText;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        basket = GameObject.FindGameObjectWithTag("ShoppingBasket").GetComponent<Basket>();
        spawnPoint = GameObject.Find("SpawnPoint");
        currentSelectionText = GameObject.Find("LabelCurrentSelection").GetComponent<TMP_Text>();
        currentPriceText = GameObject.Find("LabelRunningTotal").GetComponent<TMP_Text>();

        basket.SetBasketDefaults();
        NewCarInstance(CarType.Buggy); // Set Buggy first
    }

    // Handler for changing the car called by dropdown menu
    public void ChangeCar(CarType carToShow)
    {
        NewCarInstance(carToShow);
    }

    public void NewCarInstance(CarType carType)
    {
        myCarPrefab = GameObject.FindGameObjectWithTag("Car");
        if (myCarPrefab != null)
        {
            DestroyImmediate(myCarPrefab.gameObject);
            myCarPrefab = null;
        }

        // Create an instance of the vehicle
        myCarInstance = ScriptableObject.CreateInstance<Car>();
        myCarInstance.SetDefaultConfig(carType);

        // Create an instance of it's prefab
        Instantiate(myCarInstance.GetCarPrefab(), spawnPoint.transform.position, spawnPoint.transform.rotation);
        ShowTiresetPrefab(TiresetType.Standard);

        // Set basic totals
        myCarInstance.SetCarBasePriceTotal(carType);
        myCarInstance.SetTiresetPriceTotal(TiresetType.Standard); // FREE

        // Reset then update shopping basket entries
        basket.ResetBasket();
        basket.SetBasketChangeItem("CarType", (int) carType);
        basket.SetBasketChangeItem("TiresetType", (int) TiresetType.Standard);
#if UNITY_EDITOR
        basket.LogBasketItems();
#endif

        // Update labels with new totals
        UpdateRunningTotalLabel();
    }

    public void HideFrontPrefabs()
    {
        GameObject newCarPrefab = GameObject.FindGameObjectWithTag("Car");
        Transform[] transforms = newCarPrefab.GetComponentsInChildren<Transform>();

        foreach (Transform transform in transforms)
        {
            if (transform.name == "Fronts")
            {
                foreach (Transform front in transform)
                {
                    front.gameObject.SetActive(false);
                }
            }
        }
    }

    public void SetFront(FrontType frontToShow)
    {
        HideFrontPrefabs();
        ShowFrontPrefab(frontToShow);                                   // show prefab
        myCarInstance.SetFrontPriceTotal(frontToShow);                  // set pricing
        basket.SetBasketChangeItem("FrontType", (int) frontToShow);     // add to basket
#if UNITY_EDITOR
        basket.LogBasketItems();
#endif
        UpdateRunningTotalLabel();
    }

    public void ShowFrontPrefab(FrontType frontToShow)
    {
        GameObject newCarPrefab = GameObject.FindGameObjectWithTag("Car");
        Transform[] transforms = newCarPrefab.GetComponentsInChildren<Transform>();

        foreach (Transform transform in transforms)
        {
            if (transform.name == "Fronts")
            {
                foreach (Transform front in transform)
                {
                    if (front.gameObject.name == "Winch" && frontToShow == FrontType.Winch)
                    {
                        front.gameObject.SetActive(true);
                        break;
                    }
                    else if (front.gameObject.name == "Spiked" && frontToShow == FrontType.Spiked)
                    {
                        front.gameObject.SetActive(true);
                        break;
                    }
                    else if (front.gameObject.name == "Shunt" && frontToShow == FrontType.Shunt)
                    {
                        front.gameObject.SetActive(true);
                        break;
                    }
                    else
                    {
                        front.gameObject.SetActive(false);
                    }
                }
                break;
            }
        }
    }

    public void HideTiresetPrefabs()
    {
        GameObject newCarPrefab = GameObject.FindGameObjectWithTag("Car");
        Transform[] transforms = newCarPrefab.GetComponentsInChildren<Transform>();

        foreach (Transform transform in transforms)
        {
            if (transform.name == "Tiresets")
            {
                foreach (Transform tireSet in transform)
                {
                    tireSet.gameObject.SetActive(false);
                }
            }
        }
    }

    public void SetTireset(TiresetType tiresetToShow)
    {
        HideTiresetPrefabs();
        ShowTiresetPrefab(tiresetToShow);
        myCarInstance.SetTiresetPriceTotal(tiresetToShow);
        basket.SetBasketChangeItem("TiresetType", (int) tiresetToShow);
        UpdateRunningTotalLabel();
        ShowTiresetPrefab(tiresetToShow);
#if UNITY_EDITOR
        basket.LogBasketItems();
#endif
    }

    public void ShowTiresetPrefab(TiresetType tiresetToShow)
    {
        GameObject newCarPrefab = GameObject.FindGameObjectWithTag("Car");
        Transform[] transforms = newCarPrefab.GetComponentsInChildren<Transform>();

        foreach (Transform transform in transforms)
        {
            if (transform.name == "Tiresets")
            {
                foreach (Transform tireSet in transform)
                {
                    if (tireSet.gameObject.name == "Standard" && tiresetToShow == TiresetType.Standard)
                    {
                        tireSet.gameObject.SetActive(true);
                        break;
                    }
                    else if (tireSet.gameObject.name == "Spiked" && tiresetToShow == TiresetType.Spiked)
                    {
                        tireSet.gameObject.SetActive(true);
                        break;
                    }
                    else
                    {
                        tireSet.gameObject.SetActive(false);
                    }
                }
                break;
            }
        }
    }

    public void HideWeaponPrefabs()
    {
        GameObject newCarPrefab = GameObject.FindGameObjectWithTag("Car");
        Transform[] transforms = newCarPrefab.GetComponentsInChildren<Transform>();

        foreach (Transform transform in transforms)
        {
            if (transform.name == "Weapons")
            {
                foreach (Transform weapon in transform)
                {
                    weapon.gameObject.SetActive(false);
                }
            }
        }
    }

    public void SetWeapon(WeaponType weaponToShow)
    {
        HideWeaponPrefabs();
        ShowWeaponPrefab(weaponToShow);
        myCarInstance.SetWeaponPriceTotal(weaponToShow);
        basket.SetBasketChangeItem("WeaponType", (int) weaponToShow);
#if UNITY_EDITOR
        basket.LogBasketItems();
#endif
        UpdateRunningTotalLabel();
    }

    public void ShowWeaponPrefab(WeaponType weaponToShow)
    {
        GameObject newCarPrefab = GameObject.FindGameObjectWithTag("Car");
        Transform[] transforms = newCarPrefab.GetComponentsInChildren<Transform>();

        foreach (Transform transform in transforms)
        {
            if (transform.name == "Weapons")
            {
                foreach (Transform weapon in transform)
                {
                    if (weapon.gameObject.name == "CustomWeaponSingle" && weaponToShow == WeaponType.SingleBarrel)
                    {
                        weapon.gameObject.SetActive(true);
                        break;
                    }
                    else if (weapon.gameObject.name == "CustomWeaponTwin" && weaponToShow == WeaponType.TwinBarrel)
                    {
                        weapon.gameObject.SetActive(true);
                        break;
                    }
                    else
                    {
                        weapon.gameObject.SetActive(false);
                    }
                }
                break;
            }
        }
    }

    public void SayHello()
    {
        Debug.Log("Hello");
    }

    void UpdateRunningTotalLabel()
    {
        currentPriceText.text = "Running total: £" + myCarInstance.GetTotalSpend().ToString();
    }
}
