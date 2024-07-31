using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AsteroidHit : MonoBehaviour
{
    [SerializeField] private GameObject asteroidExplosion;
    [SerializeField] private GameController gameController;
    [SerializeField] private GameObject popupCanvas;

    private void Awake()
    {
        gameController = FindObjectOfType<GameController>();
    }

    public void AsteroidDestroyed()
    {
        Instantiate(asteroidExplosion, transform.position, transform.rotation);

        if(GameController.currentGameStatus == GameController.GameState.Playing)
        {
            // Calculate the score for hitting this object

            float distanceFromPlayer = Vector3.Distance(transform.position, Vector3.zero);
            int bonusPoints = (int)distanceFromPlayer;

            int asteroidScore = 10 * bonusPoints;

            //set text for popup - Instantiate popup
            popupCanvas.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = asteroidScore.ToString();
            GameObject asteroidPopup = Instantiate(popupCanvas, transform.position, Quaternion.identity);

            //adjust the scale of the popUp
            asteroidPopup.transform.localScale = new Vector3((transform.localScale.x * distanceFromPlayer / 10), (transform.localScale.y * distanceFromPlayer / 10), (transform.localScale.z * distanceFromPlayer / 10));

            //pass score to game manager
            gameController.UpdatePlayerScore(asteroidScore);
        }
        Destroy(this.gameObject); 
    }
}
