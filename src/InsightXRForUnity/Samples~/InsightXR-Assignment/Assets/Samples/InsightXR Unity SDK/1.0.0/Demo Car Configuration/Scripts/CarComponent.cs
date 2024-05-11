using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarComponent : MonoBehaviour
{

    public GameObject StandardWheel;
    public GameObject SpikeWheel;

    public GameObject FrontWinch;
    public GameObject FrontSpiked;
    public GameObject FrontShunt;

    public GameObject SingleWeapon;
    public GameObject DoubleWeapon;
    
    public void HideCar()
    {
        gameObject.GetComponent<Rigidbody>().isKinematic = true;
        transform.position = new Vector3(1000, 1000, 1000);
    }

    public void ShowCar(Vector3 position)
    {
        GetComponent<Rigidbody>().isKinematic = false;
        SetFront(0);
        SetWeapon(0);
        SetWheel(0);
        
        transform.position = position;
        gameObject.GetComponent<Rigidbody>().isKinematic = false;
    }

    public void SetWeapon(int ID)
    {
        switch (ID)
        {
            case 0:
                SingleWeapon.transform.localPosition = new Vector3(-1000, -1000, -1000);
                DoubleWeapon.transform.localPosition = new Vector3(-1000, -1000, -1000);
                break;
            case 1:
                SingleWeapon.transform.localPosition = new Vector3(0, 0, 0);
                DoubleWeapon.transform.localPosition = new Vector3(-1000, -1000, -1000);
                break;
            case 2:
                SingleWeapon.transform.localPosition = new Vector3(-1000, -1000, -1000);
                DoubleWeapon.transform.localPosition = new Vector3(0,0,0);
                break;
        }
    }

    public void SetWheel(int ID)
    {
        switch (ID)
        {
            case 0:
                StandardWheel.transform.localPosition = new Vector3(0,0,0);
                SpikeWheel.transform.localPosition = new Vector3(-1000, -1000, -1000);
                break;
            
            case 1:
                StandardWheel.transform.localPosition = new Vector3(-1000, -1000, -1000);
                SpikeWheel.transform.localPosition = new Vector3(0,0,0);
                break;
        }
    }

    public void SetFront(int ID)
    {
        switch (ID)
        {
            case 0:
                FrontShunt.transform.localPosition = new Vector3(-1000, -1000, -1000);
                FrontSpiked.transform.localPosition = new Vector3(-1000, -1000, -1000);
                FrontWinch.transform.localPosition = new Vector3(-1000, -1000, -1000);
                break;
            
            case 1:
                FrontShunt.transform.localPosition = new Vector3(0,0,0);
                FrontSpiked.transform.localPosition = new Vector3(-1000, -1000, -1000);
                FrontWinch.transform.localPosition = new Vector3(-1000, -1000, -1000);
                break;
            
            case 2:
                FrontShunt.transform.localPosition = new Vector3(-1000, -1000, -1000);
                FrontSpiked.transform.localPosition = new Vector3(0,0,0);
                FrontWinch.transform.localPosition = new Vector3(-1000, -1000, -1000);
                break;
            
            case 3:
                FrontShunt.transform.localPosition = new Vector3(-1000, -1000, -1000);
                FrontSpiked.transform.localPosition = new Vector3(-1000, -1000, -1000);
                FrontWinch.transform.localPosition = new Vector3(0,0,0);
                break;
            
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.name == "default")
        {
            Invoke("OFF", 1f);
        }
        
    }

    void OFF()
    {
        GetComponent<Rigidbody>().isKinematic = true;
    }
}
