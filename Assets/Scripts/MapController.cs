using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapController : MonoBehaviour, IPointerClickHandler
{
    public DestinationButton destination;

    public void OnPointerClick(PointerEventData eventData)
    {
        string sceneName = "";
        switch (destination)
        {
            case DestinationButton.tower:
                sceneName = "WizardTower";
                break;
            case DestinationButton.forest:
                sceneName = "Forest";
                break;
            case DestinationButton.castle:
                sceneName = "Castle";
                break;
            case DestinationButton.village:
                sceneName = "Village";
                break;
            default:
                break;
        }
        SceneManager.LoadScene(sceneName);
    }
}

public enum DestinationButton
{
    tower,
    forest,
    village,
    castle
}