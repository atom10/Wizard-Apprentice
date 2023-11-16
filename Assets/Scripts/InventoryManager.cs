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
        List<string> journal = PersistanceController.GetInstance().currentSave.journal;
        foreach(string element in journal) {
            GameObject new_entry = Instantiate(new GameObject(), quest_list.transform);
            TextMeshProUGUI text = new_entry.AddComponent<TextMeshProUGUI>();
            text.text = element;
        }
    }
}
