using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public GameObject inventoryBoxPrefab;
    public GameObject inventoryItemPrefab;

    GameObject inventoryBox;
    public void OpenInventory(PlayerController playerController)
    {
        playerController.CanMove(false);
        if (inventoryBox != null)
        {
            inventoryBox.SetActive(true);
        }
        else
        {
            inventoryBox = Instantiate(inventoryBoxPrefab);
            inventoryBox.GetComponent<Canvas>().worldCamera = Camera.main;
            inventoryBox.transform.Find("Close").GetComponent<Button>().onClick.AddListener(() =>
            {
                playerController.CanMove(true);
            });
        }

        List<Item_entry> player_inventory = playerController.GetInventoryContainer();
        GameObject item_list = inventoryBox.transform.Find("Item_list").gameObject;
        for (int a = 0; a < item_list.transform.childCount; ++a)
        {
            Destroy(item_list.transform.GetChild(a).gameObject);
        }
        foreach (Item_entry item in player_inventory)
        {
            GameObject item_entry = Instantiate(inventoryItemPrefab, item_list.transform);
            item_entry.transform.Find("name").GetComponent<TextMeshProUGUI>().text = item.item.name;
            item_entry.transform.Find("count").GetComponent<TextMeshProUGUI>().text = item.amount.ToString();
            if (item.item.icon != null) item_entry.transform.Find("icon").GetComponent<SpriteRenderer>().sprite = item.item.icon;
            item_entry.GetComponent<Button>().onClick.AddListener(() =>
            {
                playerController.UseItem(item.item);
                int count = int.Parse(item_entry.transform.Find("count").GetComponent<TextMeshProUGUI>().text);
                count -= 1;
                if (count <= 0)
                {
                    Destroy(item_entry);
                }
                else
                {
                    item_entry.transform.Find("count").GetComponent<TextMeshProUGUI>().text = count.ToString();
                }
            });
            if (item.item.type != Item_types.consumable) { item_entry.GetComponent<Button>().enabled = false; }
        }
    }
}
