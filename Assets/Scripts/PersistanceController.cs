using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
using TextAsset = UnityEngine.TextAsset;
using Unity.Mathematics;
using TMPro;
using System.Linq;

public class PersistanceController
{
    static PersistanceController instance;
    string path = Application.persistentDataPath;
    public SaveFilePacket currentSave = new SaveFilePacket();
    public TextMeshProUGUI timeHUD;

    public static PersistanceController GetInstance()
    {
        if (instance == null)
        {
            instance = new PersistanceController();
            if (instance == null) Debug.Log("Instancja jest nullem wszyscy zginiemy");
        }
        return instance;
    }
    public List<int> GetSaves()
    {
        string metadataFilePath = path + "metadata.bin";
        if (File.Exists(path + "metadata.bin"))
        {
            Metadata returnValue = JsonUtility.FromJson<Metadata>(File.ReadAllText(metadataFilePath));
            return returnValue.active_slots;
        }
        else
        {
            string metadata_content = JsonUtility.ToJson(new Metadata(), true);
            StreamWriter saveFile = new StreamWriter(metadataFilePath);
            saveFile.Write(metadata_content);
            saveFile.Close();
            return new List<int>();
        }
    }
    public void UpdateMetadata(List<int> active_slots)
    {
        string metadataFilePath = path + "metadata.bin";
        Metadata metadata = new Metadata();
        metadata.active_slots = active_slots;
        string metadata_content = JsonUtility.ToJson(metadata, true);
        StreamWriter metadataFile = new StreamWriter(metadataFilePath);
        metadataFile.Write(metadata_content);
        metadataFile.Close();
    }
    public void RememberMe(NpcController controller, string new_scene_name = "")
    {
        NpcDataPacket npc_packet = currentSave.npcDataPackets.Find((NpcDataPacket ndp) => { return ndp.id == controller.id; });
        if (npc_packet == null)
        {
            npc_packet = new NpcDataPacket();
            currentSave.npcDataPackets.Add(npc_packet);
        }
        npc_packet.position = controller.gameObject.transform.position;
        npc_packet.rotation = controller.gameObject.transform.rotation;
        npc_packet.scale = controller.gameObject.transform.localScale;
        npc_packet.sprite = controller.gameObject.GetComponent<SpriteRenderer>().sprite;
        npc_packet.id = controller.id;
        npc_packet.inkFile = controller.ink_file;
        npc_packet.knotName = controller.ink_knot_name;
        npc_packet.firstName = controller.firstname;
        npc_packet.avatar = controller.avatar;
        if (new_scene_name == "") npc_packet.sceneName = controller.gameObject.scene.name;
        else npc_packet.sceneName = new_scene_name;
    }
    public void RememberMe(PlayerController controller, string new_scene_name = "")
    {
        if (currentSave.playerDataPacket == null)
        {
            currentSave.playerDataPacket = new PlayerDataPacket();
        }
        PlayerDataPacket player_packet = currentSave.playerDataPacket;
        player_packet.position = controller.gameObject.transform.position;
        player_packet.rotation = controller.gameObject.transform.rotation;
        player_packet.scale = controller.gameObject.transform.localScale;
        player_packet.sprite = controller.gameObject.GetComponent<SpriteRenderer>().sprite;
        player_packet.charisma = controller.charisma;
        if (new_scene_name == "") currentSave.sceneName = controller.gameObject.scene.name;
        else currentSave.sceneName = new_scene_name;
        player_packet.health = controller.health;
        player_packet.relation_magnus = controller.relation_magnus;
        player_packet.relation_queen = controller.relation_queen;
        player_packet.relation_villagers = controller.relation_villagers;
    }
    public void RememberMe(ItemOnGround controller, string new_scene_name = "")
    {
        if (controller.id.Equals("0")) return;
        ItemOnGroundDataPacket data_packet = currentSave.itemOnGroundDataPackets.Find((ItemOnGroundDataPacket dp) => { return dp.id == controller.id; });
        if (data_packet == null)
        {
            data_packet = new ItemOnGroundDataPacket();
            currentSave.itemOnGroundDataPackets.Add(data_packet);
        }
        data_packet.position = controller.gameObject.transform.position;
        data_packet.id = controller.id;
        data_packet.amount = controller.amount;
        data_packet.item = controller.item;
        if (new_scene_name == "") data_packet.sceneName = controller.gameObject.scene.name;
        else data_packet.sceneName = new_scene_name;
    }
    public void RememberMe(ChestController controller)
    {
        ChestDataPacket chest_packet = currentSave.chestDataPackets.Find((ChestDataPacket cdp) => { return cdp.id == controller.id; });
        if (chest_packet == null)
        {
            chest_packet = new ChestDataPacket();
            currentSave.chestDataPackets.Add(chest_packet);
        }
        chest_packet.id = controller.id;
        chest_packet.items = controller.items;
        chest_packet.amount = controller.amount;
    }
    public void ForgetMe(ItemOnGround controller)
    {
        int index = currentSave.itemOnGroundDataPackets.FindIndex((ItemOnGroundDataPacket dp) => { return dp.id == controller.id; });
        if (index != -1)
        {
            ItemOnGroundDataPacket data_packet = currentSave.itemOnGroundDataPackets[index];
            data_packet.collected = true;
            currentSave.itemOnGroundDataPackets[index] = data_packet;
        } else
        {
            //RememberMe(controller);
            //ForgetMe(controller);
        }
    }
    public void Save(int id)
    {
        currentSave.id = id;
        currentSave.sceneName = SceneManager.GetActiveScene().name;

        foreach (PlayerController pc in UnityEngine.Object.FindObjectsOfType<PlayerController>()) RememberMe(pc, currentSave.sceneName);
        foreach (NpcController npc in UnityEngine.Object.FindObjectsOfType<NpcController>()) RememberMe(npc, currentSave.sceneName);
        foreach (ChestController cc in UnityEngine.Object.FindObjectsOfType<ChestController>()) RememberMe(cc);
        foreach (ItemOnGround iog in UnityEngine.Object.FindObjectsOfType<ItemOnGround>().Where((ItemOnGround filteredElement) => { return filteredElement.persistable; })) RememberMe(iog);

        string saveFilePath = path + "save_" + id.ToString() + ".bin";
        string save_data = JsonUtility.ToJson(currentSave, true);
        StreamWriter saveFile = new StreamWriter(saveFilePath);
        saveFile.Write(save_data);
        saveFile.Close();

        List<int> active_slots = GetSaves();
        if (!active_slots.Contains(id)) active_slots.Add(id);
        UpdateMetadata(active_slots);
    }
    public void Load(int id)
    {
        string saveFilePath = path + "save_" + id.ToString() + ".bin";
        if (File.Exists(saveFilePath))
        {
            currentSave = JsonUtility.FromJson<SaveFilePacket>(File.ReadAllText(saveFilePath));
            SceneManager.LoadScene(currentSave.sceneName);
            RecreateAllInstancesFromSave();
        }
        else Debug.Log("Podany save nie istnieje");
    }
    public bool ShouldIBeHere(NpcController npcController)
    {
        if (
            currentSave.npcDataPackets.FindIndex((NpcDataPacket ndp) => { return ndp.id == npcController.id; }) != -1 &&
            npcController.id != 0
            )
        {
            Debug.Log("Requested removal of default state npc because of saved instance in savefile");
            return false;
        }
        return true;
    }
    public bool ShouldIBeHere(ItemOnGround controller)
    {
        if(
            currentSave.itemOnGroundDataPackets.Find( //Find such item that
                (ItemOnGroundDataPacket dp) => {
                    return dp.id == controller.id && //existed in save AND
                    dp.collected == true; //was collected
                }
            ) == null || //if not found/not collected, return true as "you SHOULD be here"
            controller.id.Equals("0") //Or return true if id is in default state.
            ) {
            return true;
        } else {
            Debug.Log("Requested item removal of " + controller.item.name + " with id of " + controller.id);
            return false;
        }
    }
    public void RecreateAllInstancesFromSave()
    {
        //NPC
        foreach (NpcDataPacket npcSavesFromThisScene in currentSave.npcDataPackets.FindAll(
            (NpcDataPacket ndp) =>
            {
                return ndp.sceneName == SceneManager.GetActiveScene().name;
            })
        )
        {
            Debug.Log("Recreating npc of id " + npcSavesFromThisScene.id);
            GameObject newNpc = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/NPCTemplate")) as GameObject;
            NpcController npcIdentityComponent = newNpc.GetComponent<NpcController>();
            npcIdentityComponent.id = npcSavesFromThisScene.id;

            npcIdentityComponent.gameObject.transform.GetComponent<SpriteRenderer>().sprite = npcSavesFromThisScene.sprite;
            npcIdentityComponent.transform.position = npcSavesFromThisScene.position;
            npcIdentityComponent.transform.rotation = npcSavesFromThisScene.rotation;
            npcIdentityComponent.transform.localScale = npcSavesFromThisScene.scale;
            npcIdentityComponent.ink_file = npcSavesFromThisScene.inkFile;
            npcIdentityComponent.ink_knot_name = npcSavesFromThisScene.knotName;
            npcIdentityComponent.firstname = npcSavesFromThisScene.firstName;
            npcIdentityComponent.avatar = npcSavesFromThisScene.avatar;
        }

        //Player
        PlayerController[] playerControllers = UnityEngine.Object.FindObjectsByType(typeof(PlayerController), FindObjectsSortMode.None) as PlayerController[];
        PlayerController pc;
        if (playerControllers.Length > 0)
            pc = playerControllers[0];
        else
        {
            GameObject player = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/PlayerTemplate")) as GameObject;
            pc = player.GetComponent<PlayerController>();
        }
        pc.health = currentSave.playerDataPacket.health;
        pc.charisma = currentSave.playerDataPacket.charisma;
        pc.relation_magnus = currentSave.playerDataPacket.relation_magnus;
        pc.relation_queen = currentSave.playerDataPacket.relation_queen;
        pc.relation_villagers = currentSave.playerDataPacket.relation_villagers;
        pc.setInventory(currentSave.playerDataPacket.inventory);
        if (currentSave.sceneName == SceneManager.GetActiveScene().name)
        {
            pc.gameObject.transform.position = currentSave.playerDataPacket.position;
            pc.gameObject.transform.localScale = currentSave.playerDataPacket.scale;
            pc.gameObject.transform.rotation = currentSave.playerDataPacket.rotation;
        }

        //Chest
        ChestController[] chestControllers = UnityEngine.Object.FindObjectsByType(typeof(ChestController), FindObjectsSortMode.None) as ChestController[];
        foreach (ChestController cc in chestControllers)
        {
            ChestDataPacket cdp = currentSave.chestDataPackets.Find((ChestDataPacket chestDataPacket) => { return chestDataPacket.id == cc.id; });
            if (cdp != null)
            {
                cc.SetContent(cdp.items, cdp.amount);
            }
        }
        //ItemOnGround
        ItemOnGround[] itemsOnGround = UnityEngine.Object.FindObjectsByType(typeof(ItemOnGround), FindObjectsSortMode.None) as ItemOnGround[];
        foreach (ItemOnGroundDataPacket dp in currentSave.itemOnGroundDataPackets.FindAll(
            (ItemOnGroundDataPacket dp) =>
            {
                return dp.sceneName == SceneManager.GetActiveScene().name;
            })
        )
        {
            ItemOnGround matchingItem = Array.Find(itemsOnGround, (ItemOnGround iog) => { return iog.id == dp.id; });
            if (matchingItem == null)
            {
                GameObject itemOnGroundInstance = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/ItemOnGroundTemplate")) as GameObject;
                matchingItem = itemOnGroundInstance.GetComponent<ItemOnGround>();
                matchingItem.id = dp.id;
            }

            matchingItem.transform.position = dp.position;
            matchingItem.item = dp.item;
            matchingItem.amount = dp.amount;

        }
    }
    public void AdvanceTime(int hours)
    {
        int days = hours / 24;
        if(days > 0)
        {
            currentSave.hour += hours % (days * 24);
        } else
        {
            currentSave.hour += hours;
        }
        
        currentSave.day += days;
        if(currentSave.hour >= 24)
        {
            currentSave.hour = currentSave.hour - 24;
            currentSave.day++;
        }
        if(timeHUD != null)
        {
            string timeName = "";
            switch(currentSave.hour)
            {
                case < 4:
                    timeName = "night";
                    break;
                case < 8:
                    timeName = "early morning";
                    break;
                case < 12:
                    timeName = "morning";
                    break;
                case < 16:
                    timeName = "noon";
                    break;
                case < 20:
                    timeName = "afternoon";
                    break;
                case < 24:
                    timeName = "evening";
                    break;
            }
            timeHUD.text = timeName + ", day " + (currentSave.day + 1);
        }
    }
    public void SetEventFlag(int index, bool flag)
    {
        if (index > 1024) return;
        ref List<ulong> flags = ref currentSave.eventFlags;
        int listIndex = index / 64;
        int elementInChunk = index % 64;
        while (currentSave.eventFlags.Count * 64 < listIndex) currentSave.eventFlags.Add(0);
        ulong temp = flags[listIndex];
        if(flag) temp |= (ulong.MinValue + 1) << elementInChunk;
        else temp &= ~((ulong.MinValue + 1) << elementInChunk);
        flags[listIndex] = temp;
    }
    public bool CheckEventFlag(int index)
    {
        if (index > 1024) return false;
        ref List<ulong> flags = ref currentSave.eventFlags;
        int listIndex = index / 64;
        int elementInChunk = index % 64;
        while (currentSave.eventFlags.Count * 64 < listIndex) currentSave.eventFlags.Add(0);
        ulong temp = flags[listIndex];
        return (temp & ((ulong.MinValue + 1) << elementInChunk)) > 0;

    }
}

[Serializable]
public class NpcDataPacket
{
    public string sceneName;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
    public Sprite sprite;
    public int id;
    public TextAsset inkFile;
    public string knotName;
    public string firstName;
    public Sprite avatar;
}

[Serializable]
public class PlayerDataPacket
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
    public Sprite sprite;
    public List<Item_entry> inventory = new List<Item_entry>();
    public float health = 100;
    public int charisma = 10;
    public int relation_magnus = 0;
    public int relation_queen = 0;
    public int relation_villagers = 0;
}

[Serializable]
public class ChestDataPacket
{
    public int id;
    public List<Item> items = new List<Item>();
    public List<int> amount = new List<int>();
}

[Serializable]
public class ItemOnGroundDataPacket
{
    public string id;
    public Item item;
    public int amount;
    public Vector3 position;
    public string sceneName;
    public bool collected = false;
}

[Serializable]
public class SaveFilePacket
{
    public int id;
    public string sceneName;
    public List<NpcDataPacket> npcDataPackets = new List<NpcDataPacket>();
    public List<ChestDataPacket> chestDataPackets = new List<ChestDataPacket>();
    public PlayerDataPacket playerDataPacket = new PlayerDataPacket();
    public int day = 0;
    public int hour = 8;
    public List<ulong> eventFlags = new List<ulong>();
    public List<ItemOnGroundDataPacket> itemOnGroundDataPackets = new List<ItemOnGroundDataPacket>();
    public List<JournalEntry> journal = new List<JournalEntry>();
    public List<GatherQuest> gatherQuests = new List<GatherQuest>();
}

[Serializable]
public class Metadata
{
    public List<int> active_slots = new List<int>();
}