using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DropdownCarSelect : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Basket basket;
    [SerializeField] private Dropdown dropdownCarSelect;
    [SerializeField] private Dropdown dropdownTiresetSelect;
    [SerializeField] private Dropdown dropdownFrontSelect;
    [SerializeField] private Dropdown dropdownWeaponSelect;
    [SerializeField] private TMP_Text userSelectionsLabel;
    
    void Start()
    {
        SetDefaultDropdown();
        SetDefaultSelectionsLabel();
    }

    void ResetDropdowns()
    {
        dropdownTiresetSelect.value = 0;
        dropdownFrontSelect.value = 0;
        dropdownWeaponSelect.value = 0;
    }

    void SetDefaultDropdown()
    {
        dropdownCarSelect.value = (int)gameManager.myCarInstance.GetCarType();
    }

    void SetDefaultSelectionsLabel()
    {
        userSelectionsLabel.text = basket.GetBasketItemsAsString();
    }

    public void ShowNextCar()
    {
        ResetDropdowns();

        // Pass the dropdown selection value as a CarType to gameManager
        gameManager.ChangeCar((CarType) dropdownCarSelect.value);

        // Change selection label
        userSelectionsLabel.text = userSelectionsLabel.text = basket.GetBasketItemsAsString();
    }
}
