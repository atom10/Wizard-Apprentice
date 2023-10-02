using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemOnGround : MonoBehaviour, Interact
{
    public Item item;
    public int amount = 1;

    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && item.icon != null) sr.sprite = item.icon;
    }

    public void Interact(GameObject player)
    {
        player.GetComponent<PlayerController>().AddItem(item, 1);
        Destroy(this.gameObject);
    }
}
