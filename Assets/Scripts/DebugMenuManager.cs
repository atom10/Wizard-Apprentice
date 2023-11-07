using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugMenuManager : MonoBehaviour
{
    public GameObject DebugMenuPrefab;
    GameObject debugMenuInstance;

    public GatherQuest SampleQuest;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Dollar))
        {
            if (debugMenuInstance == null)
            {
                debugMenuInstance = Instantiate(DebugMenuPrefab);
                debugMenuInstance.GetComponent<Canvas>().worldCamera = Camera.main;
                debugMenuInstance.transform.Find("B1").GetComponent<Button>().onClick.AddListener(() => {
                    PersistanceController persistanceController = PersistanceController.GetInstance();
                    if (!persistanceController.currentSave.gatherQuests.Contains(SampleQuest)) persistanceController.currentSave.gatherQuests.Add(SampleQuest);
                    ForestQuestManager forestQuestManager = debugMenuInstance.GetComponent<ForestQuestManager>();
                    if (forestQuestManager == null) { debugMenuInstance.AddComponent<ForestQuestManager>(); }
                    forestQuestManager.GenerateQuestItems();
                    
                });
            }
            debugMenuInstance.SetActive(!debugMenuInstance.activeSelf);
        }
    }
}
