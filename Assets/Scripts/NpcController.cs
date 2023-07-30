using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcController : MonoBehaviour, Interact
{
    public string firstname;
    public TextAsset ink_file;
    public string ink_knot_name;
    public Sprite icon;

    public void Interact()
    {
        //Debug.Log("Hello my name is " + firstname);
        //var dialogue_box = GameObject.Find("dialogue_box");
        //dialogue_box.SetActive(true);
        
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
