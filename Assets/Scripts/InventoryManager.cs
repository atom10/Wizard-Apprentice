using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class InventoryManager : MonoBehaviour
{
    public GameObject inventoryBoxPrefab;
    public GameObject inventoryItemPrefab;
    public AudioClip closing;
    public GameObject journalEntryPrefab;

    GameObject inventoryBox;

    IEnumerator Sound(AudioClip sound){
        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.PlayOneShot(sound);
        yield return null;
    }

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
                StartCoroutine(Sound(closing));
                playerController.CanMove(true);
                Destroy(inventoryBox);
            });
        }

        List<Item_entry> player_inventory = playerController.GetInventoryContainer();
        GameObject item_list = inventoryBox.transform.Find("content_main_container").Find("Item_list").Find("Viewport").Find("Content").gameObject;
        for (int a = 0; a < item_list.transform.childCount; ++a)
        {
            Destroy(item_list.transform.GetChild(a).gameObject);
        }
        foreach (Item_entry item in player_inventory)
        {
            GameObject item_entry = Instantiate(inventoryItemPrefab, item_list.transform);
            item_entry.transform.Find("name").GetComponent<TextMeshProUGUI>().text = item.item.name;
            item_entry.transform.Find("count").GetComponent<TextMeshProUGUI>().text = item.amount.ToString();
            if (item.item.icon != null) item_entry.transform.Find("icon").GetComponent<Image>().sprite = item.item.icon;
            item_entry.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (playerController.UseItem(item.item))
                {
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
                }
            });
            if (item.item.type != Item_types.consumable) { item_entry.GetComponent<Button>().enabled = false; }
        }

        GameObject quest_list = inventoryBox.transform.Find("content_main_container").Find("journal").Find("Viewport").Find("Content").gameObject;
        for (int a = 0; a < quest_list.transform.childCount; ++a)
        {
            Destroy(quest_list.transform.GetChild(a).gameObject);
        }
        List<JournalEntry> journal = PersistanceController.GetInstance().currentSave.journal;
        foreach(JournalEntry element in journal) {
            GameObject new_entry = Instantiate(journalEntryPrefab, quest_list.transform);
            TextMeshProUGUI giverComponent = new_entry.transform.Find("Giver").GetComponent<TextMeshProUGUI>();
            switch (element.fraction)
            {
                case Fraction.wizard:
                    giverComponent.text = "Magnus";
                    break;
                case Fraction.village:
                    giverComponent.text = "Village";
                    break;
                case Fraction.castle:
                    giverComponent.text = "Queen";
                    break;
            }
            new_entry.transform.Find("Header").GetComponent<TextMeshProUGUI>().text = element.header;
            TextMeshProUGUI itemRequiredComponent = new_entry.transform.Find("ItemRequired").GetComponent<TextMeshProUGUI>();
            itemRequiredComponent.text = "";
            itemRequiredComponent.fontSize -= 8;
            for (int a = 0;a < element.itemsRequired.Count; ++a)
            {
                itemRequiredComponent.text += element.amountOfItemsRequired[a].ToString() + " x " + element.itemsRequired[a].name + "\n";
            }
            new_entry.transform.Find("Content").GetComponent<TextMeshProUGUI>().text = element.content;
            new_entry.GetComponent<VerticalLayoutGroup>().CalculateLayoutInputVertical();
            new_entry.GetComponent<VerticalLayoutGroup>().SetLayoutVertical();
        }

        GameObject relationships = inventoryBox.transform.Find("content_main_container/relationships").gameObject;

        relationships.transform.Find("magnus bar/fill").transform.localScale = new Vector3((float)(playerController.relation_magnus / 100f), 1, 1);
        if(playerController.relation_magnus < 0) relationships.transform.Find("magnus bar/fill").GetComponent<Image>().color = Color.red;

        relationships.transform.Find("queen bar/fill").transform.localScale = new Vector3((float)(playerController.relation_queen / 100f), 1, 1);
        if (playerController.relation_queen < 0) relationships.transform.Find("queen bar/fill").GetComponent<Image>().color = Color.red;

        relationships.transform.Find("village bar/fill").transform.localScale = new Vector3((float)(playerController.relation_villagers / 100f), 1, 1);
        if (playerController.relation_villagers < 0) relationships.transform.Find("village bar/fill").GetComponent<Image>().color = Color.red;
    }
}
