using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class ChestController : MonoBehaviour, Interact
{
    public int id;
    public List<Item> items;
    public List<int> amount;
    public GameObject ChestBoxPrefab;
    public GameObject ChestItemPrefab;

    int selectedChestItemIndex;
    int selectedPlayerItemIndex;
    GameObject selectedChestItem;
    GameObject selectedPlayerItem;
    List<Item_entry> playerItems;

    public void OnDestroy()
    {
        PersistanceController persistanceController = PersistanceController.GetInstance();
        persistanceController.RememberMe(this);
    }

    public void Interact(GameObject player)
    {
        PlayerController playerController = player.GetComponent<PlayerController>();
        Assert.IsNotNull(playerController);
        playerItems = playerController.GetInventoryContainer();
        GameObject chestBox = Instantiate(ChestBoxPrefab);
        chestBox.GetComponent<Canvas>().worldCamera = Camera.main;
        playerController.CanMove(false);
        chestBox.transform.Find("Close").gameObject.GetComponent<Button>().onClick.AddListener(() =>
        {
            Destroy(chestBox);
            playerController.CanMove(true);
        });

        chestBox.transform.Find("moveToInventory").gameObject.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (selectedChestItem != null)
            {
                if (playerItems.Exists((ie) => { return ie.item.name == items[selectedChestItemIndex].name; }))
                {
                    int index = playerItems.FindIndex((Item_entry ie) => { return ie.item == items[selectedChestItemIndex]; });
                    Item_entry copy = playerItems[index];
                    copy.amount = playerItems[index].amount + amount[selectedChestItemIndex];
                    playerItems[index] = copy;
                }
                else
                {
                    Item_entry new_item = new Item_entry(items[selectedChestItemIndex], amount[selectedChestItemIndex]);
                    playerItems.Add(new_item);
                }
                items.RemoveAt(selectedChestItemIndex);
                amount.RemoveAt(selectedChestItemIndex);
                DrawItems(chestBox);
            }
        });

        chestBox.transform.Find("moveToChest").gameObject.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (selectedPlayerItem != null)
            {
                if (items.Contains(playerItems[selectedPlayerItemIndex].item))
                {
                    int index = items.IndexOf(playerItems[selectedPlayerItemIndex].item);
                    amount[index] += playerItems[selectedPlayerItemIndex].amount;
                }
                else
                {
                    items.Add(playerItems[selectedPlayerItemIndex].item);
                    amount.Add(playerItems[selectedPlayerItemIndex].amount);
                }
                playerItems.RemoveAt(selectedPlayerItemIndex);
                DrawItems(chestBox);
            }
        });
        DrawItems(chestBox);
    }

    void DrawItems(GameObject chestBox)
    {
        GameObject chestItemsPanel = chestBox.transform.Find("chestPanel").Find("Viewport").Find("Content").gameObject;
        GameObject playerItemsPanel = chestBox.transform.Find("inventoryPanel").Find("Viewport").Find("Content").gameObject;

        for (int i = 0; i < playerItemsPanel.transform.childCount; ++i)
        {
            Destroy(playerItemsPanel.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < chestItemsPanel.transform.childCount; ++i)
        {
            Destroy(chestItemsPanel.transform.GetChild(i).gameObject);
        }

        //chest
        for (int i = 0; i < items.Count; ++i)
        {
            GameObject instance = Instantiate(ChestItemPrefab, chestBox.transform.Find("chestPanel").Find("Viewport").Find("Content"));
            instance.GetComponent<SimpleValueStorage>().number = i;
            if (items[i].icon != null) instance.transform.Find("icon").GetComponent<Image>().sprite = items[i].icon;
            instance.transform.Find("name").GetComponent<TextMeshProUGUI>().text = items[i].name;
            instance.transform.Find("count").GetComponent<TextMeshProUGUI>().text = amount[i].ToString();

            instance.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (selectedChestItem != null) selectedChestItem.GetComponent<Image>().color = Color.white;
                selectedChestItemIndex = instance.GetComponent<SimpleValueStorage>().number;
                selectedChestItem = instance;
                instance.GetComponent<Image>().color = Color.yellow;
            });
        }

        //Player
        for (int i = 0; i < playerItems.Count; ++i)
        {
            GameObject instance = Instantiate(ChestItemPrefab, chestBox.transform.Find("inventoryPanel").Find("Viewport").Find("Content"));
            instance.GetComponent<SimpleValueStorage>().number = i;
            if (playerItems[i].item.icon != null)
            {
                instance.transform.Find("icon").GetComponent<Image>().sprite = playerItems[i].item.icon;
            }
            instance.transform.Find("name").GetComponent<TextMeshProUGUI>().text = playerItems[i].item.name;
            instance.transform.Find("name").GetComponent<TextMeshProUGUI>().fontSize = instance.transform.Find("name").GetComponent<TextMeshProUGUI>().fontSize - 4;
            instance.transform.Find("count").GetComponent<TextMeshProUGUI>().text = playerItems[i].amount.ToString();

            instance.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (selectedPlayerItem != null) selectedPlayerItem.GetComponent<Image>().color = Color.white;
                selectedPlayerItemIndex = instance.GetComponent<SimpleValueStorage>().number;
                selectedPlayerItem = instance;
                instance.GetComponent<Image>().color = Color.yellow;
            });
        }
    }

    public void SetContent(List<Item> items, List<int> amount)
    {
        this.items = items;
        this.amount = amount;
    }
}
