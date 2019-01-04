using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayCreature : MonoBehaviour
{
    //components
    BoxCollider2D boxcol;

    //world and player info
    Vector2 worldSize;
    int player;

    //stats
    int carnivore = 0;
    int herbivore = 0;
    int speed = 2;
    int metabolism = 1;

    //parameters
    Vector2 position;
    Vector2 worldspacePos;
    int satiation = 5;
    Vector4 extent = new Vector4(0, 0, 0, 0);

    //action after movement support
    ContactFilter2D contactfilter;
    //Collider2D[] encounters = new Collider2D[2];




    // Start is called before the first frame update
    void Start()
    {
        //get the world size
        worldSize = PlayGrid.PlayFieldSize;

        //gets the collider and sets the contactfilter
        boxcol = GetComponent<BoxCollider2D>();
        contactfilter.useTriggers = true;

        //connect this creature with the correct movement event listener 
        switch (player)
        {
            case 1:
                EventManager.AddListener(EventName.StepP1MovementEvent, Move);
                break;
            case 2:
                EventManager.AddListener(EventName.StepP2MovementEvent, Move);
                break;
            case 3:
                EventManager.AddListener(EventName.StepP3MovementEvent, Move);
                break;
            case 4:
                EventManager.AddListener(EventName.StepP4MovementEvent, Move);
                break;
        }
        //also connect the upkeep event
        EventManager.AddListener(EventName.StepUpkeepEvent, Upkeep);

    }

    public void Birth(int playerNumber, Vector2 startPosition, bool herbivorous)
    {
        player = playerNumber;
        position = startPosition;
        if (herbivorous)
        {
            herbivore = 1;
            GetComponent<SpriteRenderer>().material.SetColor("_Color", new Color(0f, 1f, 0f, 1f));
        }
        if (!herbivorous)
        {
            carnivore = 1;
            GetComponent<SpriteRenderer>().material.SetColor("_Color", new Color(1f, 0f, 0f, 1f));
        }
    }

    private void Upkeep(int unused) {
        satiation--;
    }

    private void Reproduce() {

    }

    private void Move(int unused) {
        StartCoroutine("MoveCoroutine");
    }

    private IEnumerator MoveCoroutine() { 
        int movePoints = speed;
        while (movePoints > 0)
        {
            //make a single step in the specified direction
            if (position.x > 0)
            {
                position.x--;
                worldspacePos = PlayGrid.getGridCoordinates(new Vector2(position.x, position.y));
                gameObject.GetComponent<Transform>().position = new Vector3(worldspacePos.x, worldspacePos.y, 0f);
            }
            yield return null;
            //check for a collider overlap
            Collider2D[] encounters = new Collider2D[2];
            boxcol.OverlapCollider(contactfilter, encounters);
            foreach (Collider2D encounter in encounters)
            {
                if (encounter != null)
                {
                    if (encounter.gameObject.tag == "Plant")
                    {
                        //Debug.Log("Old satiation: " + satiation);
                        satiation += encounter.gameObject.GetComponent<Plant>().GetEaten();
                        //Debug.Log("New Satiation: " + satiation);
                    }
                }
            }
            movePoints--;
        }
    }

    private void Evolve() {

    }
}
