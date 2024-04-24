using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DropdownTiresetSelect : MonoBehaviour
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

    public void ShowNextTireset()
    {
        // Pass the dropdown selection value as a TiresetType to gameManager
        gameManager.SetTireset((TiresetType) dropdown.value);
        userSelectionsLabel.text = basket.GetBasketItemsAsString();
    }

    void SetDefaultDropdown()
    {
        dropdown.value = (int) gameManager.myCarInstance.GetTiresetType();
    }
}
