using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager: MonoBehaviour
{
    public GameObject inventoryBoxPrefab;
    public GameObject inventoryItemPrefab;

    GameObject inventoryBox;
    public void OpenInventory(PlayerController playerController)
    {
        playerController.CanMove(false);
        if (inventoryBox != null) {
            inventoryBox.SetActive(true);
        }
        else
        {
            inventoryBox = Instantiate(inventoryBoxPrefab);
            inventoryBox.transform.Find("Close").GetComponent<Button>().onClick.AddListener(() =>
            {
                playerController.CanMove(true);
            });
        }
    }
}
