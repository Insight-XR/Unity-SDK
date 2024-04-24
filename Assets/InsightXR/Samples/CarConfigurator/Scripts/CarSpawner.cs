using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    [Header("Car Models")]
    public CarComponent Buggy;
    public CarComponent HCoupe;
    public CarComponent LCoupe;
    public CarComponent MiniBus;
    public CarComponent Pickup;

    private CarComponent SelectedCar;

    [Header("References")] 
    public Transform SpawnPoint;
    
    private List<CarComponent> vehicles;
    
    // Start is called before the first frame update

    private void Awake()
    {
        vehicles = new List<CarComponent>();
        
        vehicles.Add(Buggy);
        vehicles.Add(HCoupe);
        vehicles.Add(LCoupe);
        vehicles.Add(MiniBus);
        vehicles.Add(Pickup);

        foreach (var car in vehicles)
        {
            car.HideCar();
        }
        
        if (FindObjectOfType<InsightXRAPI>().InReplayMode())
        {
            GameObject.Find("Canvas").SetActive(false);
            this.enabled = false;
        }
    }

    void Start()
    {
        // vehicles[Random.Range(0,4)].ShowCar(SpawnPoint.position);
        vehicles[0].ShowCar(SpawnPoint.position);
        SelectedCar = vehicles[0];
    }

    public void CarSelection(Int32 Selection)
    {

        SelectedCar.HideCar();
        SelectedCar = vehicles[Selection];
        SelectedCar.ShowCar(SpawnPoint.position);
    }

    public void SetWeapon(Int32 Sel)
    {
        SelectedCar.SetWeapon(Sel);
    }

    public void SetWheels(Int32 Sel)
    {
        SelectedCar.SetWheel(Sel);
    }

    public void SetFront(Int32 Sel)
    {
        SelectedCar.SetFront(Sel);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
