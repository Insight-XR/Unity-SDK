using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DropdownWeaponSelect : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Basket basket;
    [SerializeField] private TMP_Text userSelectionsLabel;
    private Dropdown dropdown;

    void Awake()
    {
        dropdown = GetComponent<Dropdown>();
    }

    void Start()
    {
        SetDefaultDropdown();
    }

    public void ShowNextWeapon()
    {
        // Pass the dropdown selection value as a WeaponType to gameManager
        gameManager.SetWeapon((WeaponType) dropdown.value);
        userSelectionsLabel.text = basket.GetBasketItemsAsString();
    }

    void SetDefaultDropdown()
    {
        dropdown.value = (int) gameManager.myCarInstance.GetWeaponType();
    }
}
