using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldSetup : MonoBehaviour
{
    public GameObject HUDPrefab;

    void Start()
    {
        if (GameObject.Find("HUD") == null && HUDPrefab != null)
        {
            GameObject hUDInstance = Instantiate(HUDPrefab);
            hUDInstance.GetComponent<Canvas>().worldCamera = Camera.main;
        }

        PersistanceController persistanceController = PersistanceController.GetInstance();
        persistanceController.RecreateAllInstancesFromSave();
    }
}
