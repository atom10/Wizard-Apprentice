using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldSetup : MonoBehaviour
{
    void Awake()
    {
        PersistanceController persistanceController = PersistanceController.GetInstance();
        persistanceController.RecreateAllInstancesFromSave();
    }
}
