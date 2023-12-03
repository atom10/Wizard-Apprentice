using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemOnGround : MonoBehaviour, Interact
{
    [ContextMenu("Generate guid for id")]
    public void GenerateGUID()
    {
        id = System.Guid.NewGuid().ToString();
    }
    public string id = "0";
    public Item item;
    public int amount = 1;
    public bool persistable = true;

    void Start()
    {
        PersistanceController persistanceController = PersistanceController.GetInstance();
        if (!persistanceController.ShouldIBeHere(this))
        {
            Destroy(gameObject);
            return;
        }
        UpdateSprite();
    }

    public void Interact(GameObject player)
    {
        player.GetComponent<PlayerController>().AddItem(item, amount);
        if(persistable) PersistanceController.GetInstance().ForgetMe(this);
        Destroy(this.gameObject);
    }

    public void UpdateSprite()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && item.icon != null) sr.sprite = item.icon;
    }
}
