using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu]
public class Item : ScriptableObject, Interact
{
    public string item_name;
    public Sprite icon;
    public Item_types type;
    public string description;

    public List<what_is_affected> effects;
    public List<float> effect_amount;
    public uint effect_duration;

    public void Interact(GameObject player)
    {
        throw new System.NotImplementedException();
    }
}

public enum Item_types
{
    werable,
    consumable,
    quest,
    material,
    throwable
}

public enum what_is_affected
{
    health,
    speed,
    poison,
    shield
}

[Serializable]
public struct Item_entry
{
    public Item item;
    public int amount;

    public Item_entry(Item item, int amount)
    {
        this.item = item;
        this.amount = amount;
    }
}