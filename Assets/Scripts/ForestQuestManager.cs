using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class ForestQuestManager : MonoBehaviour
{
    public GameObject ItemOnGroundPrefab;

    void Start()
    {
        if(SceneManager.GetActiveScene().name.Equals("Forest")) GenerateQuestItems();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateQuestItems()
    {
        Assert.IsNotNull(ItemOnGroundPrefab);
        List<GatherQuest> gatherQuests = PersistanceController.GetInstance().currentSave.gatherQuests;
        foreach (GatherQuest gatherQuest in gatherQuests)
        {
            for (int i = 0; i < gatherQuest.amountToSpawn.Count; i++)
            {
                for (int j = 0; j < gatherQuest.amountToSpawn[i]; j++)
                {
                    float angle = Mathf.PI / 2 * Random.Range((float)0, (float)1);
                    int side = Random.Range(0, 4);
                    float distance = Random.Range((float)gatherQuest.startRadius[i], (float)gatherQuest.endRadius[i]);
                    if (gatherQuest.spawnStrategy[i] == SpawnStrategy.square)
                    {
                        distance += (distance * Mathf.Sqrt(2) - distance) * angle; //It's not linear but I failed math so deal with it
                    }
                    float y = Mathf.Sin(angle) * distance;
                    float x = y / Mathf.Tan(angle);
                    switch (side)
                    {
                        case 1:
                            x *= -1;
                            break;
                        case 2:
                            x *= -1;
                            y *= -1;
                            break;
                        case 3:
                            y *= -1;
                            break;
                        default: break;
                    }
                    GameObject spawnedItem = Instantiate(ItemOnGroundPrefab);
                    spawnedItem.GetComponent<Transform>().position = new Vector3(Mathf.Floor(x)+(float)0.5, Mathf.Floor(y) + (float)0.5, 0);
                    ItemOnGround itemOnGround = spawnedItem.GetComponent<ItemOnGround>();
                    itemOnGround.item = gatherQuest.item[i];
                    itemOnGround.UpdateSprite();
                    itemOnGround.persistable = gatherQuest.persistent;
                    if (gatherQuest.persistent) itemOnGround.GenerateGUID();
                }
            }
            if (gatherQuest.persistent)
            {
                PersistanceController.GetInstance().currentSave.gatherQuests.Remove(gatherQuest);
            }
        }
    }
}
