using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapController : MonoBehaviour, IPointerClickHandler
{
    public DestinationName destination;

    public void OnPointerClick(PointerEventData eventData)
    {
        string sceneName = "";
        switch (destination)
        {
            case DestinationName.tower:
                sceneName = "WizardTower";
                break;
            case DestinationName.forest:
                sceneName = "Forest";
                break;
            case DestinationName.castle:
                sceneName = "Castle";
                break;
            case DestinationName.village:
                sceneName = "Village";
                break;
            default:
                break;
        }
        SceneManager.LoadScene(sceneName);
    }
}

public enum DestinationName
{
    tower,
    forest,
    village,
    castle
}