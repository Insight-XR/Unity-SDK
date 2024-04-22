using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Init : MonoBehaviour
{
    void Start()
    {
        CleanupObjects();
        StartGame();
    }

    void CleanupObjects()
    {
        GameObject[] oldGameManager = GameObject.FindGameObjectsWithTag("GameManager");
        GameObject[] oldShoppingBasket = GameObject.FindGameObjectsWithTag("ShoppingBasket");

        if (oldGameManager.Length > 0)
            Destroy(oldGameManager[0]);

        if (oldShoppingBasket.Length > 0)
            Destroy(oldShoppingBasket[0]);
    }

    void StartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1, LoadSceneMode.Single);
    }
}
