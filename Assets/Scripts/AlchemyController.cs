using Ink.Parsed;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class AlchemyController : MonoBehaviour
{
    public GameObject alchemyTablePrefab;
    public GameObject recipePrefab;
    public GameObject ingedientPrefab;

    Color defaultRecipeBackgroundColor;
    GameObject selectedRecipe;

    GameObject alchemyTable;
    Recipe[] recipes = new Recipe[0];

    public void OpenAlchemyTable(PlayerController playerController)
    {
        Debug.Log("Opening alchemy table");
        playerController.CanMove(false);
        defaultRecipeBackgroundColor = recipePrefab.GetComponent<Image>().color;
        alchemyTable = Instantiate( alchemyTablePrefab );
        alchemyTable.transform.Find("Close").gameObject.GetComponent<Button>().onClick.AddListener(() => {
            playerController.CanMove(true);
            Destroy(alchemyTable);
        });
        recipes = Resources.LoadAll<Recipe>("Recipes");
        GameObject recepturesPanel = alchemyTable.transform.Find("RecepturesPanel").Find("Viewport").Find("Content").gameObject;
        GameObject ingredientsPanel = alchemyTable.transform.Find("IngedientsPanel").Find("Viewport").Find("Content").gameObject;
        foreach (Recipe recipe in recipes)
        {
            GameObject recipeButton = Instantiate(recipePrefab, recepturesPanel.transform);
            recipeButton.transform.Find("label").GetComponent<TextMeshProUGUI>().text = recipe.result.name;
            recipeButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                if(selectedRecipe != null)
                    selectedRecipe.GetComponent<Image>().color = defaultRecipeBackgroundColor;
                selectedRecipe = recipeButton;
                selectedRecipe.GetComponent<Image>().color = Color.yellow;
                Button make = alchemyTable.transform.Find("RecepturesPanel").Find("Make").GetComponent<Button>();
                GameObject resultBox = alchemyTable.transform.Find("ResultItem").gameObject;
                resultBox.transform.Find("Image").GetComponent<Image>().sprite = recipe.result.icon;
                resultBox.transform.Find("CurrentlyHaving").GetComponent<TextMeshProUGUI>().text = "Current: " +
                    playerController.GetInventoryContainer().Find((Item_entry ie) => { return ie.item.name.Equals(recipe.result.name); }).amount;
                make.onClick.RemoveAllListeners();
                make.onClick.AddListener(() =>
                {
                    playerController.AddItem(recipe.result, 1);
                    for(int a = 0; a< recipe.ingredients.Count; a++)
                    {
                        playerController.RemoveItem(recipe.ingredients[a], recipe.amount[a]);
                    }
                });

                for (int a = 0; a < ingredientsPanel.transform.childCount; ++a)
                {
                    Debug.Log("Destroy child cast it into fire!");
                    Destroy(ingredientsPanel.transform.GetChild(a).gameObject);
                }
                for(int a = 0; a< recipe.ingredients.Count; ++a)
                {
                    GameObject ingredientItem = Instantiate(ingedientPrefab, ingredientsPanel.transform);
                    ingredientItem.transform.Find("Image").GetComponent<Image>().sprite = recipe.ingredients[a].icon;
                    ingredientItem.transform.Find("label").GetComponent<TextMeshProUGUI>().text = recipe.ingredients[a].name + " x" + recipe.amount[a].ToString();
                    if(playerController.GetInventoryContainer().FindAll((Item_entry itemInInventory) => {
                        if (
                        itemInInventory.item.name.Equals(recipe.ingredients[a].name) &&
                        itemInInventory.amount >= recipe.amount[a]
                        ) return true;
                        else return false;
                    }).Count == 0)
                    {
                        ingredientItem.transform.Find("label").GetComponent<TextMeshProUGUI>().color = Color.gray;
                        make.enabled = false;
                    }
                }
            });
        }
    }
}
