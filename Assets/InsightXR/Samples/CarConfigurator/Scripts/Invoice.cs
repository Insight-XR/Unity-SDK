using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Invoice : MonoBehaviour
{
    [SerializeField] TMP_Text vehicleFullNameLabel;
    [SerializeField] TMP_Text vehicleSelectionsLabel;

    private void Awake()
    {
        SetFullNameLabel();
        SetSelectionsLabel();
    }

    void SetFullNameLabel()
    {
        vehicleFullNameLabel.text = GameManager.instance.myCarInstance.GetCarFullNameAsString(Basket.instance.GetSelectedCarType()) + "!";
    }

    void SetSelectionsLabel()
    {
        vehicleSelectionsLabel.text = Basket.instance.GetBasketItemsAsFormattedString();
    }
}
