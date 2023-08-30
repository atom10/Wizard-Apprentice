using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class NpcController : MonoBehaviour, Interact
{
    public int id;
    public string firstname;
    public TextAsset ink_file;
    public string ink_knot_name;
    public Sprite avatar;
    public Fraction fraction = Fraction.none;

    string target_scene = "";
    public void Interact(GameObject player)
    {
        GameObject managers = GameObject.Find("Managers");
        DialogueManager dialogueManager = managers.GetComponent<DialogueManager>();
        dialogueManager.setup(player.GetComponent<PlayerController>(), ink_file, ink_knot_name, firstname, avatar);
        if (!dialogueManager.isTalking()) dialogueManager.Talk();
    }

    public void ChangeScene(string scene)
    {
        target_scene = scene;
        Destroy(gameObject);
    }
    void Start()
    {
        PersistanceController persistanceController = PersistanceController.GetInstance();
        if (!persistanceController.ShouldIBeHere(this))
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        PersistanceController persistanceController = PersistanceController.GetInstance();
        persistanceController.RememberMe(this, target_scene);
    }
    void Update()
    {

    }
}

public enum Fraction
{
    castle,
    village,
    wizard,
    none
}