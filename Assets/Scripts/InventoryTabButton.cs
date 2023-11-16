using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryTabButton : MonoBehaviour
{
    public GameObject allTabsContainer;
    public GameObject ourContainer;
    public AudioClip changeSound;

    IEnumerator Sound(AudioClip sound){
        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.PlayOneShot(sound);
        yield return null;
    }

    public void ChangeTab()
    {
        StartCoroutine(Sound(changeSound));
        for (int i = 0; i<allTabsContainer.transform.childCount; ++i)
        {
            allTabsContainer.transform.GetChild(i).gameObject.SetActive(false);
        }
        ourContainer.SetActive(true);
    }
}