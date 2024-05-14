using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CarType
{
    Buggy,
    LightCoupe,
    HeavyCoupe,
    Pickup,
    Minibus
}

public enum ConfigurableType
{
    CarType,
    TiresetType,
    FrontType,
    WeaponType
}

public enum TiresetType
{
    Standard,
    Spiked
}

public enum FrontType
{
    None,
    Winch,
    Spiked,
    Shunt
}

public enum WeaponType
{
    None,
    SingleBarrel,
    TwinBarrel
}

[CreateAssetMenu]
public class Car : ScriptableObject
{
    // User-set configurables
    [SerializeField]
    private CarType carType;
    [SerializeField]
    private TiresetType carTiresetType;
    [SerializeField]
    private FrontType carFrontType;
    [SerializeField]
    private WeaponType carWeaponType;
    [SerializeField]
    private string carPrefabStr;

    // User-set totals
    [SerializeField]
    private int basePriceTotal = 0;
    [SerializeField]
    private int tiresetTotal = 0;
    [SerializeField]
    private int frontTotal = 0;
    [SerializeField]
    private int weaponTotal = 0;
    [SerializeField]
    private int totalPrice = 0;


    /*********************************************************************
     * Class property getters
     *********************************************************************/
    public int GetCarBasePriceTotal()
    {
        return basePriceTotal;
    }

    public int GetTiresetTotal()
    {
        return tiresetTotal;
    }

    public int GetFrontTotal()
    {
        return frontTotal;
    }

    public int GetWeaponTotal()
    {
        return weaponTotal;
    }

    public int GetTotalSpend()
    {
        SetTotalSpend();
        return totalPrice;
    }

    public Object GetCarPrefab()
    {
        return Resources.Load(carPrefabStr);
    }

    public CarType GetCarType()
    {
        return carType;
    }

    public FrontType GetFrontType()
    {
        return carFrontType;
    }

    public WeaponType GetWeaponType()
    {
        return carWeaponType;
    }

    public TiresetType GetTiresetType()
    {
        return carTiresetType;
    }

    /*********************************************************************
     * Class property setters
     *********************************************************************/

    public void SetCarBasePriceTotal(CarType carType)
    {
        basePriceTotal = (int) GetCarBasePrice(carType);
    }
    
    public void SetDefaultConfig(CarType theCarType)
    {
        switch (theCarType)
        {
            case CarType.Buggy:
                carType = CarType.Buggy;
                carTiresetType = TiresetType.Standard;
                carFrontType = FrontType.None;
                carWeaponType = WeaponType.None;
                carPrefabStr = "Prefabs/Buggy";
                totalPrice = GetTotalSpend();
                return;

            case CarType.LightCoupe:
                carType = CarType.LightCoupe;
                carTiresetType = TiresetType.Standard;
                carFrontType = FrontType.None;
                carWeaponType = WeaponType.None;
                carPrefabStr = "Prefabs/LightCoupe";
                totalPrice = GetTotalSpend();
                return;

            case CarType.HeavyCoupe:
                carType = CarType.HeavyCoupe;
                carTiresetType = TiresetType.Standard;
                carFrontType = FrontType.None;
                carWeaponType = WeaponType.None;
                carPrefabStr = "Prefabs/HeavyCoupe";
                totalPrice = GetTotalSpend();
                return;

            case CarType.Pickup:
                carType = CarType.Pickup;
                carTiresetType = TiresetType.Standard;
                carFrontType = FrontType.None;
                carWeaponType = WeaponType.None;
                carPrefabStr = "Prefabs/Pickup";
                totalPrice = GetTotalSpend();
                return;

            case CarType.Minibus:
                carType = CarType.Minibus;
                carTiresetType = TiresetType.Standard;
                carFrontType = FrontType.None;
                carWeaponType = WeaponType.None;
                carPrefabStr = "Prefabs/Minibus";
                totalPrice = GetTotalSpend();
                return;

            default:
                return;
        }
    }

    public void SetFrontPriceTotal(FrontType frontType)
    {
        switch (frontType)
        {
            case FrontType.None:
                frontTotal = (int) GetFrontPrice(FrontType.None);
                break;

            case FrontType.Winch:
                frontTotal = (int) GetFrontPrice(FrontType.Winch);
                break;

            case FrontType.Spiked:
                frontTotal = (int) GetFrontPrice(FrontType.Spiked);
                break;

            case FrontType.Shunt:
                frontTotal = (int) GetFrontPrice(FrontType.Shunt);
                break;

            default:
                break;
        }
    }

    public void SetTireset(TiresetType tiresetType)
    {
        carTiresetType = tiresetType;
    }

    public void SetTiresetPriceTotal(TiresetType tiresetType)
    {
        switch (tiresetType)
        {
            case TiresetType.Standard:
                tiresetTotal = (int) GetTiresetPrice(TiresetType.Standard);
                break;

            case TiresetType.Spiked:
                tiresetTotal = (int) GetTiresetPrice(TiresetType.Spiked);
                break;

            default:
                break;
        }
    }

    public void SetTotalSpend()
    {
        totalPrice = basePriceTotal + tiresetTotal + frontTotal + weaponTotal;
    }

    public void SetWeaponPriceTotal(WeaponType weaponType)
    {
        switch (weaponType)
        {
            case WeaponType.None:
                weaponTotal = (int) GetWeaponPrice(WeaponType.None);
                break;

            case WeaponType.SingleBarrel:
                weaponTotal = (int) GetWeaponPrice(WeaponType.SingleBarrel);
                break;

            case WeaponType.TwinBarrel:
                weaponTotal = (int) GetWeaponPrice(WeaponType.TwinBarrel);
                break;

            default:
                break;
        }
    }

    /********************************************************************
     * Part name lookups
     ********************************************************************/
    public string GetCarFullNameAsString(CarType carType)
    {
        switch (carType)
        {
            case CarType.Buggy:
                return "Bugs Buggy";

            case CarType.LightCoupe:
                return "Light Coupe";

            case CarType.HeavyCoupe:
                return "Heavy Coupe";

            case CarType.Pickup:
                return "Pickup Truck";

            case CarType.Minibus:
                return "Minibus";

            default:
                return "";
        }
    }

    public string GetTiresetNameAsString(TiresetType tiresetType)
    {
        switch (tiresetType)
        {
            case TiresetType.Standard:
                return "4x Standard Tires";

            case TiresetType.Spiked:
                return "4x Spiked Tires";

            default:
                return "";
        }
    }

    public string GetFrontNameAsString(FrontType frontType)
    {
        switch (frontType)
        {
            case FrontType.None:
                return "None";

            case FrontType.Winch:
                return "Winch";

            case FrontType.Spiked:
                return "Spiked Front";

            case FrontType.Shunt:
                return "Front Shunt";

            default:
                return "";
        }
    }

    public string GetWeaponNameAsString(WeaponType weaponType)
    {
        switch (weaponType)
        {
            case WeaponType.None:
                return "None";

            case WeaponType.SingleBarrel:
                return "Single Barrel Shooter";

            case WeaponType.TwinBarrel:
                return "Twin Barrel Shooter";

            default:
                return "";
        }
    }


    /********************************************************************
     * Pricing lookups
     ********************************************************************/
    public int GetCarBasePrice(CarType carType)
    {
        switch (carType)
        {
            case CarType.Buggy:
                return 10000;

            case CarType.LightCoupe:
                return 20000;

            case CarType.HeavyCoupe:
                return 50000;

            case CarType.Pickup:
                return 75000;

            case CarType.Minibus:
                return 85000;

            default:
                return 0;
        }
    }

    public int GetTiresetPrice(TiresetType tiresetType)
    {
        switch (tiresetType)
        {
            case TiresetType.Standard:
                return 0; // FREE

            case TiresetType.Spiked:
                return (750 * 4);

            default:
                return 0;
        }
    }

    public int GetFrontPrice(FrontType frontType)
    {
        switch (frontType)
        {
            case FrontType.None:
                return 0;

            case FrontType.Winch:
                return 500;

            case FrontType.Spiked:
                return 2500;

            case FrontType.Shunt:
                return 3500;

            default:
                return 0;
        }
    }

    public int GetWeaponPrice(WeaponType weaponType)
    {
        switch (weaponType)
        {
            case WeaponType.None:
                return 0;

            case WeaponType.SingleBarrel:
                return 500;

            case WeaponType.TwinBarrel:
                return 800;

            default:
                return 0;
        }
    }

    // Returns tuple of both full name representation and price
    (string, int) GetConfigurableNameAndPrice(ConfigurableType configurableType, int userSelection)
    {
        string configurableName = "";
        int configurablePrice = 0;

        switch (configurableType)
        {
            case ConfigurableType.CarType:
                configurableName = GetCarFullNameAsString((CarType) userSelection);
                configurablePrice = GetCarBasePrice((CarType) userSelection);
                break;

            case ConfigurableType.TiresetType:
                configurableName = GetTiresetNameAsString((TiresetType) userSelection);
                configurablePrice = GetTiresetPrice((TiresetType) userSelection);
                break;

            case ConfigurableType.FrontType:
                configurableName = GetFrontNameAsString((FrontType) userSelection);
                configurablePrice = GetFrontPrice((FrontType) userSelection);
                break;

            case ConfigurableType.WeaponType:
                configurableName = GetWeaponNameAsString((WeaponType) userSelection);
                configurablePrice = GetWeaponPrice((WeaponType) userSelection);
                break;

            default:
                break;
        }

        return (configurableName, configurablePrice);
    }
}