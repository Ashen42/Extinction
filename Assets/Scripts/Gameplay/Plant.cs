using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : MonoBehaviour
{
    //parameters
    int plantValue;
    enum plantStage { Seed, Bush, Tree};
    plantStage stage;
    int nutritrionalValue;

    //components
    SpriteRenderer spriteRenderer;

    //sprites for each stage
    [SerializeField]
    Sprite spriteSeed, spriteBush, spriteTree;

    //property
    public int PlantStage
    {
        get {  return (int)stage;        }
    }

    // Start is called before the first frame update
    void Start()
    {
        EventManager.AddListener(EventName.StepUpkeepEvent, Growth);
        spriteRenderer = GetComponent<SpriteRenderer>();
        plantValue = Random.Range(0, 11);
        UpdateStage();
    }
    

    private void Growth(int unused)
    {
        if (plantValue < 10) plantValue++;
        UpdateStage();
    }

    private void UpdateStage()
    {
        if (plantValue <= 5)
        {
            stage = plantStage.Seed;
            nutritrionalValue = 0;
            spriteRenderer.sprite = spriteSeed;
        }
        if (plantValue >= 6 && plantValue <= 9)
        {
            stage = plantStage.Bush;
            nutritrionalValue = 1;
            spriteRenderer.sprite = spriteBush;
        }
        if (plantValue >= 10)
        {
            stage = plantStage.Tree;
            nutritrionalValue = 4;
            spriteRenderer.sprite = spriteTree;
        }
    }

    public int GetEaten()
    {
        plantValue = 0;
        int tempNutrition = nutritrionalValue;
        UpdateStage();
        return tempNutrition;
    }

    public int GetNutritionalValue() { return nutritrionalValue; }
}
