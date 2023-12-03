using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[CreateAssetMenu]
public class JournalEntry : ScriptableObject
{
    public Fraction fraction;
    public string header;
    public string content;
    public List<Item> itemsRequired;
    public List<int> amountOfItemsRequired;
}