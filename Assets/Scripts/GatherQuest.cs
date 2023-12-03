using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu]
public class GatherQuest : ScriptableObject
{
    public List<Item> item;
    public List<int> amountToSpawn;
    public List<int> startRadius;
    public List<int> endRadius;
    public List<SpawnStrategy> spawnStrategy;
    public bool persistent; //Not respawnable on level re-enter
    public DestinationName destinationName = DestinationName.forest;
}

public enum SpawnStrategy
{
    square,
    circle
}

