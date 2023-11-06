using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryTabButton : MonoBehaviour
{
    public GameObject allTabsContainer;
    public GameObject ourContainer;

    public void ChangeTab()
    {
        for (int i = 0; i<allTabsContainer.transform.childCount; ++i)
        {
            allTabsContainer.transform.GetChild(i).gameObject.SetActive(false);
        }
        ourContainer.SetActive(true);
    }
}