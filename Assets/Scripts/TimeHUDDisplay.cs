using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimeHUDDisplay : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        PersistanceController.GetInstance().timeHUD = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
