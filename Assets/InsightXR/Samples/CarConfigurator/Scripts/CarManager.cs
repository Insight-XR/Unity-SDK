using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarManager : MonoBehaviour
{
    private void OnEnable()
    {
        if (FindObjectOfType<InsightXRAPI>().IsRecording())
        {
            foreach (var car in FindObjectsOfType<CarComponent>())
            {
                car.enabled = false;
            }
        }
    }
}
