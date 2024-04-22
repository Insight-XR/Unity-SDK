using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Catalogue : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Button closeButton;

    public void CloseCatalogue()
    {
        Time.timeScale = 1;
        gameObject.SetActive(false);
    }

    public void ShowCatalogue()
    {
        Time.timeScale = 0;
        gameObject.SetActive(true);
    }
}
