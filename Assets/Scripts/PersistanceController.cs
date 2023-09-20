using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
using TextAsset = UnityEngine.TextAsset;

public class PersistanceController
{
    static PersistanceController instance;
    string path = Application.persistentDataPath;
    public SaveFilePacket currentSave = new SaveFilePacket();

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
    public void Save(int id)
    {
        currentSave.id = id;
        currentSave.sceneName = SceneManager.GetActiveScene().name;

        foreach (PlayerController pc in UnityEngine.Object.FindObjectsOfType<PlayerController>()) RememberMe(pc, currentSave.sceneName);
        foreach (NpcController npc in UnityEngine.Object.FindObjectsOfType<NpcController>()) RememberMe(npc, currentSave.sceneName);
        foreach (ChestController cc in UnityEngine.Object.FindObjectsOfType<ChestController>()) RememberMe(cc);

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
        if (currentSave.npcDataPackets.Find((NpcDataPacket ndp) => { return ndp.id == npcController.id && ndp.sceneName != SceneManager.GetActiveScene().name; }) != null)
            return false;
        return true;
    }
    public void RecreateAllInstancesFromSave()
    {
        //NPC
        NpcController[] npcControllers = UnityEngine.Object.FindObjectsByType(typeof(NpcController), FindObjectsSortMode.None) as NpcController[];
        foreach (NpcDataPacket sfcs in currentSave.npcDataPackets.FindAll(
            (NpcDataPacket ndp) =>
            {
                return ndp.sceneName == SceneManager.GetActiveScene().name;
            })
        )
        {
            NpcController matchingNpc = Array.Find(npcControllers, (NpcController nc) => { return nc.id == sfcs.id; });
            if (matchingNpc == null)
            {
                GameObject npc = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/NPCTemplate")) as GameObject;
                matchingNpc = npc.GetComponent<NpcController>();
                matchingNpc.id = sfcs.id;
            }

            matchingNpc.gameObject.transform.GetComponent<SpriteRenderer>().sprite = sfcs.sprite;
            matchingNpc.transform.position = sfcs.position;
            matchingNpc.transform.rotation = sfcs.rotation;
            matchingNpc.transform.localScale = sfcs.scale;
            matchingNpc.ink_file = sfcs.inkFile;
            matchingNpc.ink_knot_name = sfcs.knotName;
            matchingNpc.firstname = sfcs.firstName;
            matchingNpc.avatar = sfcs.avatar;
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
    }

    public void AdvanceTime(int hours)
    {
        int days = hours / 24;
        currentSave.hour += hours % (days * 24);
        currentSave.day += days;
        if(currentSave.hour >= 24)
        {
            currentSave.hour = currentSave.hour - 24;
            currentSave.day++;
        }
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
public class SaveFilePacket
{
    public int id;
    public string sceneName;
    public List<NpcDataPacket> npcDataPackets = new List<NpcDataPacket>();
    public List<ChestDataPacket> chestDataPackets = new List<ChestDataPacket>();
    public PlayerDataPacket playerDataPacket = new PlayerDataPacket();
    public int day = 0;
    public int hour = 0;
}

[Serializable]
public class Metadata
{
    public List<int> active_slots = new List<int>();
}