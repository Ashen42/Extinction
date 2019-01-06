using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCreature : MonoBehaviour {

    //stats
    int carnivore = 0;
    int herbivore = 1;
    int speed = 1;

    //parameters
    Vector2 UnitPosition;
    Vector2 TargetUnitPosition;
    float direction;
    public float satiation = 3;         // maximum amount of nutrition the NPCreature can take in
    float metabolism = 0.5f;            // 0.2f means 5 steps for every 1 reduction in satiation
    float chanceOfReproduction = 0.5f; // .07f works if reproduce only on same square.
    float satiationMax = 6;             // maximum amount of nutrition the NPCreature can take in.
    int ageMax = 52 * 5;                // maximum age the NPCreatre can become. Every step is a week. NPCreatues maximum age = 52 wks = 1 yr.
    int childrenMax = 3;

    // Utility
    bool adult = false;
    int age = 0;
    int children = 0;
    bool dead = false;
    List<GameObject> NPCreaturesInTheAfterlife = new List<GameObject>();

    //action after movement support
    BoxCollider2D boxcol;
    ContactFilter2D contactfilter;

    // References
    private Board board;
    private GameMaster gameMaster;

    // Start is called before the first frame update
    void Start() {
        //connect this NPCreature with the correct movement event listener 
        EventManager.AddListener(EventName.StepNPCMovementEvent, Intelligence);
        EventManager.AddListener(EventName.StepNPCMovementEvent, Move);
        EventManager.AddListener(EventName.StepUpkeepEvent, Upkeep);
        EventManager.AddListener(EventName.StepUpkeepEvent, Dying);

        //gets the collider and sets the contactfilter
        boxcol = GetComponent<BoxCollider2D>();
        contactfilter.useTriggers = true;

        // Get gameobjects
        board = GameObject.FindObjectOfType<Board>();
        gameMaster = GameObject.FindObjectOfType<GameMaster>();

        // Determine if instantiated NPCreature is adult or child, and set size accordingly
        if (!checkAdulthood()) {
            transform.localScale = new Vector2(0.7f, 0.7f); // updated in the CheckAdult function
        }
    }

    private void Intelligence(int unused) {
        if (dead) { return; }

    }

    private void Move(int unused) {
        if (dead) {return;}
        StartCoroutine("MoveCoroutine");
    }

    private IEnumerator MoveCoroutine() {
        // Randomise the distance walked (if NPCreature is not hungry)
        int movePoints;
        if (Hungry() || Horny()) {
            movePoints = speed;
        } else {
            movePoints = Mathf.RoundToInt(Random.Range(0f, speed));
        }
        
        // Start walking
        while (movePoints > 0) {
            UnitPosition = PlayGrid.getUnitCoordinates((Vector2)gameObject.GetComponent<Transform>().transform.position);
            Vector2 directionVector = Vector2.zero;

            // Check if the NPCreature is hungry. If yes, determine in which direction there is a plant with the most nutrition
            if (Hungry()) {
                directionVector = FindPlant();      //Debug.Log("Highest nutriotional plant at Vector [" + directionVector.x + "," + directionVector.y + "]");
            } else if (Horny()) {
                directionVector = findMate();
            }

            if (directionVector == Vector2.zero) {  //If the directionVector has remained zero (i.e., plant or mate not found) 
                directionVector = chooseRandomWalkingDirection();
            }

            // Take a step forward (checking for collision with other NPCreatures)
            GameObject NPCreatureObstacle = checkforNPColission(directionVector);
            if (NPCreatureObstacle == null) {
                stepForward(directionVector);
            } else if (Horny() && NPCreatureObstacle.GetComponent<NPCreature>().Horny()){
                // collision between two horny NPCreatures.
                //Debug.Log("Horny NPCreatures collided");
                Reproduce();
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
                        if (encounter.gameObject.GetComponent<NPCreature>().Horny()) {
                            Reproduce();
                        }
                    }
                }
            }

            movePoints--;
        }
    }

    private Vector2 chooseRandomWalkingDirection() {
        // Choose a random direction to walk at if the NPCreature is not walking towards vegitation
        Vector2 directionVector = Vector2.zero;
        direction = gameObject.GetComponent<Transform>().eulerAngles.z;
        direction = direction + 45; // compensating for sprite orientation of 45 degrees
        direction = Mathf.RoundToInt(direction / 45);
        // Debug.Log("Direction of NPCreature is: " + direction);

        // Switch case to move the piggie one step in its facing direction
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
        return directionVector;
    }

    private GameObject checkforNPColission(Vector2 directionVector) {
        // Check for accidental collision with other NPCreature
        GameObject observed = SeeIfCollisionWithOtherNPCreature2(directionVector, 1, "NPCreature");
        return observed;
    }

    private void stepForward(Vector2 directionVector) {
        TargetUnitPosition = UnitPosition + directionVector;
        if (!PlayGrid.checkIfOutOfBounds(TargetUnitPosition)) {
            // Take a step forward (update current NPCreature position)
            gameObject.GetComponent<Transform>().position = PlayGrid.getGridCoordinates(TargetUnitPosition);
        } else {
            // flip direction vector
            gameObject.transform.Rotate(0, 0, 180);
        }
    }

    private void Upkeep(int unused) {
        if (dead) { return; }

        satiation = satiation-metabolism;
        age++;
        checkAdulthood();

        // Randomly determine new orientation to move towards
        if (Random.Range(0f, 1f) < 0.3f) {
            gameObject.GetComponent<Transform>().rotation = Quaternion.Euler(0f, 0f, Random.Range(-180f, 180f));
        }
    }

    private bool Hungry() {
        if (satiation < metabolism * 5) {
            return true;
        } else {
            return false;
        }
    }

    private Vector2 FindPlant() {
        Vector2 directionVector = Vector2.zero;
        Vector2 selectedDirectionVector = Vector2.zero;
        int NutritionalValue = 0;
        for (int direction = 1; direction <= 8; direction++) {
            switch (direction) {
                case 1: directionVector = Vector2.right + Vector2.up; break;
                case 2: directionVector = Vector2.up; break; // y++
                case 3: directionVector = Vector2.up + Vector2.left; break;
                case 4: directionVector = Vector2.left; break; // x-- 
                case 5: directionVector = Vector2.left + Vector2.down; break;
                case 6: directionVector = Vector2.down; break; // y--
                case 7: directionVector = Vector2.down + Vector2.right; break;
                case 8: directionVector = Vector2.right; break; // x++
            }

            GameObject observedPlant = SeeIfCollisionWithOtherNPCreature2(directionVector, 2, "Plant");
            if (observedPlant != null) {
                Vector2 plantLocation = UnitPosition + directionVector;
                //Debug.Log("Plant observed at [" + plantLocation.x + "," + plantLocation.y + "]");
                //observed.GetComponent<SpriteRenderer>().material.SetColor("_Color", new Color(0f, 1f, 0f, 1f));
                if (observedPlant.GetComponent<Plant>().GetNutritionalValue() > NutritionalValue) {
                    // if the observed plant has highest nutritional value seen thus far, take this as direction
                    NutritionalValue = observedPlant.GetComponent<Plant>().GetNutritionalValue();
                    selectedDirectionVector = directionVector;
                } else if (observedPlant.GetComponent<Plant>().GetNutritionalValue() == NutritionalValue && Random.Range(0f, 1f) < 0.5f) {
                    // if observed plant has similar nutritional value, randomize if we choose this route to go.
                    selectedDirectionVector = directionVector;
                }
            }
        }
        return selectedDirectionVector;
    }

    private bool checkAdulthood() {
        // Check adulthood & adjust NPCreatures to adult size (distinguishing between child and adults)
        if (age > ageMax / 3) {
            if (!adult) { // if not an adult yet, set size to full
                transform.localScale = new Vector2(1f, 1f);
            }
            adult = true;
        } else {
            adult = false;
        }
        return adult;
    }

    private Vector2 findMate() {
        if (!adult) { return Vector2.zero; }

        Vector2 directionVector = Vector2.zero;
        Vector2 selectedDirectionVector = Vector2.zero;
        for (int direction = 1; direction <= 8; direction++) {
            switch (direction) {
                case 1: directionVector = Vector2.right + Vector2.up; break;
                case 2: directionVector = Vector2.up; break; // y++
                case 3: directionVector = Vector2.up + Vector2.left; break;
                case 4: directionVector = Vector2.left; break; // x-- 
                case 5: directionVector = Vector2.left + Vector2.down; break;
                case 6: directionVector = Vector2.down; break; // y--
                case 7: directionVector = Vector2.down + Vector2.right; break;
                case 8: directionVector = Vector2.right; break; // x++
            }

            GameObject observedMate = SeeIfCollisionWithOtherNPCreature2(directionVector, 3, "NPCreature");
            if (observedMate != null) {
                Vector2 mateLocation = UnitPosition + directionVector;
                if (observedMate.GetComponent<NPCreature>().Horny()) {
                    // if the observed mate is horny, go in that direction
                    selectedDirectionVector = directionVector;
                }
            }
        }
        return selectedDirectionVector;
    }

    private void Reproduce() {
        int i = Mathf.RoundToInt(Random.Range(1f, PlayGrid.PlayFieldSize.x-1));
        int j = Mathf.RoundToInt(Random.Range(1f, PlayGrid.PlayFieldSize.x-1));
        Vector3 spawnCoordinates = (Vector3)PlayGrid.getGridCoordinates(new Vector2(i, j));
        board.spawnNPCreature(spawnCoordinates,0);
        Debug.Log("NPCreatures reproduced");
        //Debug.Log("NPCreatures created a child at [" + i + "," + j + "]");

        children++;
    }

    private void Dying(int unused) {
        if (dead) { return; }

        if (satiation < 0) { // Die if the NPCreature has not eaten, and his satiation level is negative.
            Destroy(gameObject);
            //recycleLife(gameObject);    
            board.reduceNPCreatureCount();
            Debug.Log("NPCreature died of starvation");
        } else if (age > ageMax) {
            Destroy(gameObject);
            //recycleLife(gameObject);
            board.reduceNPCreatureCount();
            Debug.Log("NPCreature died of old age");
        }
        /*
        if (children == 1) {Debug.Log("NPCreature left behind " + children + " child");
        } else {Debug.Log("NPCreature left behind " + children + " children");
        }*/
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

    private void recycleLife(GameObject NPCreature) {
        // input NPCreature is destined to die. To save on computing time (destroying the gameObject for creating it later again), put it on ice outside the screen
        dead = true;
        age = 1;
        children = 0;
        satiation = satiationMax;
        
        NPCreature.transform.position = PlayGrid.getOutsideOfScreenCoordinates();
        NPCreaturesInTheAfterlife.Add(NPCreature);
    }

    // --- Public function ---
    public void setAge(int inputAge) {
        age = inputAge;
        checkAdulthood();
    }
    public void setRandomAge() {
        age = Mathf.RoundToInt(Random.Range(0f, ageMax * 0.8f));
        checkAdulthood();
    }

    public bool Horny() {
        if (checkAdulthood() && Random.Range(0f, 1f) < chanceOfReproduction && children < childrenMax) {
            return true;
        }
        return false;
    }
}
