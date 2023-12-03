using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugMenuManager : MonoBehaviour
{
    public GameObject DebugMenuPrefab;
    GameObject debugMenuInstance;

    public GatherQuest SampleQuest;
    public JournalEntry SampleJournalEntry;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            if (debugMenuInstance == null)
            {
                debugMenuInstance = Instantiate(DebugMenuPrefab);
                debugMenuInstance.GetComponent<Canvas>().worldCamera = Camera.main;
                debugMenuInstance.transform.Find("B1").GetComponent<Button>().onClick.AddListener(() => {
                    PersistanceController persistanceController = PersistanceController.GetInstance();
                    if (SampleQuest != null)
                    {
                        if (!persistanceController.currentSave.gatherQuests.Contains(SampleQuest)) persistanceController.currentSave.gatherQuests.Add(SampleQuest);
                        ForestQuestManager forestQuestManager = GameObject.Find("Managers").GetComponent<ForestQuestManager>();
                        forestQuestManager.GenerateQuestItems();
                    } else
                    {
                        Debug.Log("Sample quest not set");
                    }
                });

                debugMenuInstance.transform.Find("B2").GetComponent<Button>().onClick.AddListener(() => {
                    PersistanceController persistanceController = PersistanceController.GetInstance();
                    if (SampleJournalEntry != null)
                    {
                        persistanceController.currentSave.journal.Add(SampleJournalEntry);
                    }
                    else
                    {
                        Debug.Log("Sample journal entry not set");
                    }
                });
            }
            debugMenuInstance.SetActive(!debugMenuInstance.activeSelf);
        }
    }
}
