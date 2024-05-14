using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Basket : MonoBehaviour
{
    public static Basket instance;

    Dictionary<string, int> basketItems = new Dictionary<string, int>();

    void Awake()
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
    }

    /*********************************************************************
     * Class property getters
     *********************************************************************/
    public string GetBasketItemsAsString()
    {
        int userSelection;
        string basketItemsStr = "Selections:\n\n";

        if (basketItems.TryGetValue("CarType", out userSelection))
            basketItemsStr += GameManager.instance.myCarInstance.GetCarFullNameAsString((CarType) userSelection) + "\n";

        if (basketItems.TryGetValue("TiresetType", out userSelection))
            basketItemsStr += GameManager.instance.myCarInstance.GetTiresetNameAsString((TiresetType) userSelection) + "\n";

        if (basketItems.TryGetValue("FrontType", out userSelection) && userSelection != 0)
            basketItemsStr += GameManager.instance.myCarInstance.GetFrontNameAsString((FrontType) userSelection) + "\n";

        if (basketItems.TryGetValue("WeaponType", out userSelection) && userSelection != 0)
            basketItemsStr += GameManager.instance.myCarInstance.GetWeaponNameAsString((WeaponType) userSelection);

        return basketItemsStr;
    }

    public string GetBasketItemsAsFormattedString()
    {
        int userSelection;
        string basketItemsStr = "<line-height=200%>\n";

        if (basketItems.TryGetValue("TiresetType", out userSelection))
            basketItemsStr += "\u2022<indent=1em>" + GameManager.instance.myCarInstance.GetTiresetNameAsString((TiresetType) userSelection) + "</indent>\n";

        if (basketItems.TryGetValue("FrontType", out userSelection) && userSelection != 0)
            basketItemsStr += "\u2022<indent=1em>" + GameManager.instance.myCarInstance.GetFrontNameAsString((FrontType) userSelection) + "</indent>\n";

        if (basketItems.TryGetValue("WeaponType", out userSelection) && userSelection != 0)
            basketItemsStr += "\u2022<indent=1em>" + GameManager.instance.myCarInstance.GetWeaponNameAsString((WeaponType) userSelection) + "</indent>";

        // Remove tire count
        basketItemsStr = basketItemsStr.Replace("4x ", "");

        return basketItemsStr;
    }

    public CarType GetSelectedCarType()
    {
        int userSelection;

        if (basketItems.TryGetValue("CarType", out userSelection))
            return (CarType) userSelection;
        else
            return 0;
    }

    public void LogBasketItems()
    {
        /*Debug.Log("basketItems contains " + basketItems.Count + " items:");
        foreach (KeyValuePair<string, int> item in basketItems)
        {
            Debug.Log(item.Key + ": " + item.Value);
        }*/
        GetBasketItemsAsString();
    }

    /*********************************************************************
     * Class property setters
     *********************************************************************/
    public void SetBasketDefaults()
    {
        SetBasketNewItem("CarType", 0);        // buggy
        SetBasketNewItem("TiresetType", 0);    // standard tireset (free)
        SetBasketNewItem("FrontType", 0);      // none
        SetBasketNewItem("WeaponType", 0);     // none
    }

    public void ResetBasket()
    {
        SetBasketChangeItem("CarType", 0);        // buggy
        SetBasketChangeItem("TiresetType", 0);    // standard tireset (free)
        SetBasketChangeItem("FrontType", 0);      // none
        SetBasketChangeItem("WeaponType", 0);     // none
    }

    public void SetBasketChangeItem(string configurableType, int configurableValue)
    {
        if (basketItems.ContainsKey(configurableType))
        {
            basketItems[configurableType] = configurableValue;
        }
    }
  
    void SetBasketNewItem(string configurableType, int configurableValue)
    {
        basketItems.Add(configurableType, configurableValue);
    }
}
