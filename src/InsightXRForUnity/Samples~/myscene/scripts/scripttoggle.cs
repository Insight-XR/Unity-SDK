using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scripttoggle : MonoBehaviour
{
    private raycast rayscript;
    public GameObject heatmap;
    // Start is called before the first frame update
    void Start()
    {
        heatmap.SetActive(false);
        rayscript = gameObject.GetComponent<raycast>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (rayscript.enabled==false)
            {
                heatmap.SetActive(true);
                rayscript.enabled = true;

            }
            else
            {
                heatmap.SetActive(false);
                rayscript.enabled = false;
            }
        }
    }
}
