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
    public string id;
    public Item item;
    public int amount = 1;

    void Start()
    {
        PersistanceController persistanceController = PersistanceController.GetInstance();
        if (!persistanceController.ShouldIBeHere(this))
        {
            Destroy(gameObject);
            return;
        }
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && item.icon != null) sr.sprite = item.icon;
    }

    public void Interact(GameObject player)
    {
        player.GetComponent<PlayerController>().AddItem(item, amount);
        PersistanceController.GetInstance().ForgetMe(this);
        Destroy(this.gameObject);
    }
}
