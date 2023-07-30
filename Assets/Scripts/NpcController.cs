using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

public class NpcController : MonoBehaviour, Interact
{
    public string firstname;
    public TextAsset ink_file;
    public string ink_knot_name;
    public Sprite icon;

    public void Interact(GameObject player)
    {
        GameObject managers = GameObject.Find("Managers");
        DialogueManager dialogueManager = managers.GetComponent<DialogueManager>();

        dialogueManager.setup(player.GetComponent<PlayerController>(), ink_file, ink_knot_name, firstname, icon);
        if (!dialogueManager.isTalking()) dialogueManager.Talk();
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
