using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCreature : MonoBehaviour {

    //stats
    int carnivore = 0;
    int herbivore = 1;
    int speed = 1;
    float metabolism = 0.2f;            //0.2f means 5 steps for every 1 reduction in satiation
    float chanceOfReproduction = 0.1f;
    float satiationMax = 4;             //maximum amount of nutrition the NPCreature can take in.

    //parameters
    Vector2 UnitPosition;
    Vector2 TargetUnitPosition;
    float direction;
    Vector2 selectedDirectionVector = Vector2.zero;
    public float satiation = 3; // maximum amount of nutrition the NPCreature can take in.

    //action after movement support
    BoxCollider2D boxcol;
    ContactFilter2D contactfilter;

    // References
    private Board board;
    private GameMaster gameMaster;

    // Start is called before the first frame update
    void Start() {
        //connect this NPCreature with the correct movement event listener 
        EventManager.AddListener(EventName.StepNPCMovementEvent, FindPlant);
        EventManager.AddListener(EventName.StepNPCMovementEvent, Move);
        EventManager.AddListener(EventName.StepUpkeepEvent, Upkeep);
        EventManager.AddListener(EventName.StepUpkeepEvent, Dying);

        //gets the collider and sets the contactfilter
        boxcol = GetComponent<BoxCollider2D>();
        contactfilter.useTriggers = true;

        board = GameObject.FindObjectOfType<Board>();
        gameMaster = GameObject.FindObjectOfType<GameMaster>();
    }

    private void FindPlant(int unused) {
        // <ToDo>
        UnitPosition = PlayGrid.getUnitCoordinates((Vector2)gameObject.GetComponent<Transform>().transform.position);
        Vector2 directionVector = Vector2.zero;
        int NutritionalValue = 0;
        for (int direction = 1; direction < 9; direction++) {
            switch (direction) {
                case 1: directionVector = Vector2.right + Vector2.up; break;
                case 2: directionVector = Vector2.up; break; // y++
                case 3: directionVector = Vector2.up + Vector2.left; break;
                case 4: directionVector = Vector2.left; break; // x-- 
                case 5: directionVector = Vector2.left + Vector2.down; break;
                case 6: directionVector = Vector2.down; break; // y--
                case 7: directionVector = Vector2.down + Vector2.right; break;
                case 8: directionVector = Vector2.right; break; // x++
                case 9: directionVector = Vector2.right + Vector2.up; break;
            }

            GameObject observedPlant = SeeIfCollisionWithOtherNPCreature2(directionVector, 2, "Plant");
            if (observedPlant != null) {
                Vector2 plantLocation = UnitPosition + directionVector;
                //Debug.Log("Plant observed at [" + plantLocation.x + "," + plantLocation.y + "]");
                //observed.GetComponent<SpriteRenderer>().material.SetColor("_Color", new Color(0f, 1f, 0f, 1f));

                /*if (observedPlant.GetNutritionalValue() > NutritionalValue) {
                    NutritionalValue = observedPlant.GetNutritionalValue();
                    selectedDirectionVector = directionVector;
                }*/
            }
        }
    }

    private void Move(int unused) {
        StartCoroutine("MoveCoroutine");
    }

    private IEnumerator MoveCoroutine() {
        direction = gameObject.GetComponent<Transform>().eulerAngles.z;
        direction = direction + 45; // compensating for sprite orientation of 45 degrees
        direction = Mathf.RoundToInt(direction / 45);
        // Debug.Log("Direction of NPCreature is: " + direction);

        int movePoints = speed;
        while (movePoints > 0) {

            // Switch case to move the piggie one step in its facing direction
            UnitPosition = PlayGrid.getUnitCoordinates((Vector2)gameObject.GetComponent<Transform>().transform.position);

            Vector2 directionVector = Vector2.zero;
            switch (direction) {
                case 1: directionVector = Vector2.right + Vector2.up; break;
                case 2: directionVector = Vector2.up; break; // y++
                case 3: directionVector = Vector2.up + Vector2.left; break;
                case 4: directionVector = Vector2.left; break; // x-- 
                case 5: directionVector = Vector2.left + Vector2.down; break;
                case 6: directionVector = Vector2.down; break; // y--
                case 7: directionVector = Vector2.down + Vector2.right; break;
                case 8: directionVector = Vector2.right ; break; // x++
                case 9: directionVector = Vector2.right + Vector2.up; break;
            }

            GameObject observed = SeeIfCollisionWithOtherNPCreature2(directionVector, 1, "NPCreature");
            if (observed != null) {// NPCreature in the way
                //Debug.Log("observed = " + observed.tag);
            } else {
                TargetUnitPosition = UnitPosition + directionVector;
                if (!PlayGrid.checkIfOutOfBounds(TargetUnitPosition)) {
                    gameObject.GetComponent<Transform>().position = PlayGrid.getGridCoordinates(TargetUnitPosition); // Update current position
                }else {
                    // flip direction vector
                    gameObject.transform.Rotate(0, 0, 180);
                }
            }

            yield return null;

            // Use Player overlap script to munch away plant if there is any in the vicinity
            //check for a collider overlap
            Collider2D[] encounters = new Collider2D[2];
            boxcol.OverlapCollider(contactfilter, encounters);
            foreach (Collider2D encounter in encounters) {
                if (encounter != null) {
                    if (encounter.gameObject.tag == "Plant" && satiation > 0) {
                        //Debug.Log("Old satiation: " + satiation);
                        satiation += encounter.gameObject.GetComponent<Plant>().GetEaten();
                        satiation = Mathf.Clamp(satiation, 0f, satiationMax);
                        //Debug.Log("New Satiation: " + satiation);
                    } else if (encounter.gameObject.tag == "NPCreature") { // if NPCreatures accidentally step into the same space (which was empty), they will reproduce
                        //Debug.Log("NPCreatures overlapping at [" + PlayGrid.getUnitCoordinates(gameObject.transform.position).x + "," + PlayGrid.getUnitCoordinates(gameObject.transform.position).y + "]");
                        Reproduce();
                    }
                }
            }

            movePoints--;
        }
    }

    private void Upkeep(int unused) {
        satiation = satiation-metabolism;

        if (Random.Range(0f, 1f) < 0.3f) {
            // Randomly determine new orientation to move towards
            gameObject.GetComponent<Transform>().rotation = Quaternion.Euler(0f, 0f, Random.Range(-180f, 180f));
        }
    }

    private void Reproduce() {
        if (Random.Range(0f, 1f) < chanceOfReproduction) {
            int i = Mathf.RoundToInt(Random.Range(1f, PlayGrid.PlayFieldSize.x-1));
            int j = Mathf.RoundToInt(Random.Range(1f, PlayGrid.PlayFieldSize.x-1));

            Vector3 spawnCoordinates = (Vector3)PlayGrid.getGridCoordinates(new Vector2(i, j));
            board.spawnNPCreature(spawnCoordinates);
            Debug.Log("NPCreatures reproduced");
            //Debug.Log("NPCreatures created a child at [" + i + "," + j + "]");
        }
    }

    private void Dying(int unused) {
        if(satiation < 0) { // Die if the NPCreature has not eaten, and his satiation level is negative.
            Destroy(gameObject);
            board.reduceNPCreatureCount();
            Debug.Log("NPCreature died of starvation");
        }
    }

    private GameObject SeeIfCollisionWithOtherNPCreature2(Vector3 RaycastDirection, int unitViewDistance, string tag) {
        float length = PlayGrid.getUnitWorldSize().x * unitViewDistance * Vector3.Magnitude(RaycastDirection);
        RaycastHit2D[] hit = Physics2D.RaycastAll(gameObject.transform.position, RaycastDirection.normalized, length);

        Debug.DrawLine(gameObject.transform.position, gameObject.transform.position + length*RaycastDirection.normalized, Color.blue, gameMaster.getRoundDuration());

        for (int i = 0; i < hit.Length; i++) {
            if (hit[i].collider.gameObject.tag == tag) {
                // Calculate the distance from the NPCreature to the next
                float distance = Vector3.Distance(hit[i].transform.position, transform.position);
                if (distance < 0.00001f) {
                    //Debug.Log("Raycast hits own collider (distance = " + distance + ")");
                } else {
                    //Debug.Log("NPCreature sees " + tag);
                    return hit[i].collider.gameObject;
                }
            } 
        }
        return null;
    }
}
