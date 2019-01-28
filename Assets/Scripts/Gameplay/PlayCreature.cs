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
    int carnivore;
    int herbivore;
    int speed = 2;
    int metabolism = 1;
    int viewingDistance = 4;

    //parameters
    Vector2 UnitPosition;
    Vector2 TargetUnitPosition;
    float direction;

    Vector2 position;
    Vector2 worldspacePos;
    public float satiation = 10;              // start nutrition the PlayCreature 
    int satiationMax = 20;                    // maximum amount of nutrition the PlayCreature can take in.
    //Vector4 extent = new Vector4(0, 0, 0, 0);

    //action after movement support
    ContactFilter2D contactfilter;
    //Collider2D[] encounters = new Collider2D[2];

    // References
    private Board board;
    private GameMaster gameMaster;


    // Start is called before the first frame update
    void Start()
    {
        //get the world size
        worldSize = PlayGrid.PlayFieldSize;

        //gets the collider and sets the contactfilter
        boxcol = GetComponent<BoxCollider2D>();
        contactfilter.useTriggers = true;

        // Get gameobjects
        board = GameObject.FindObjectOfType<Board>();
        gameMaster = GameObject.FindObjectOfType<GameMaster>();

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
        satiation = satiation - metabolism;
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
            Vector2 directionVector = Vector2.zero;
            int NutritionalValue = 0;
            UnitPosition = PlayGrid.getUnitCoordinates((Vector2)gameObject.GetComponent<Transform>().transform.position);

            // If (omnivore && hungry) --> find highest satiation value between observed plants and 
            if (Hungry()) {
                if (herbivore > 0) { // If (herbivore && hungry) -> find plant
                    FindPlant(out int NutritionalValue_observed, out Vector2 selectedDirectionVector_observed);
                    if (NutritionalValue_observed * herbivore > NutritionalValue) {
                        NutritionalValue = NutritionalValue_observed * herbivore;
                        directionVector = selectedDirectionVector_observed;
                    }
                }
                if (carnivore > 0) { // If (carnivore && hungry) --> find meat
                    FindMeat(out int NutritionalValue_observed, out Vector2 selectedDirectionVector_observed);
                    if (NutritionalValue_observed * carnivore > NutritionalValue) {
                        NutritionalValue = NutritionalValue_observed * carnivore;
                        directionVector = selectedDirectionVector_observed;
                    }
                }
            }

            if (directionVector == Vector2.zero) {  //If the directionVector has remained zero (i.e., plant or mate not found) 
                directionVector = chooseRandomWalkingDirection();
            }

            // Take a step forward (checking for collision with other PlayCreatures)
            GameObject PlayCreatureObstacle = checkforPlayCreatureColission(directionVector);
            if (PlayCreatureObstacle == null) {
                stepForward(directionVector);
            }

            yield return null;

            //check for a collider overlap
            Collider2D[] encounters = new Collider2D[2];
            boxcol.OverlapCollider(contactfilter, encounters);
            foreach (Collider2D encounter in encounters)
            {
                if (encounter != null){
                    if (encounter.gameObject.tag == "Plant" && herbivore > 0) {
                        satiation += encounter.gameObject.GetComponent<Plant>().GetEaten()*herbivore;
                    } else if (encounter.gameObject.tag == "NPCreature" && carnivore > 0) {
                        satiation += encounter.gameObject.GetComponent<NPCreature>().GetEaten()*carnivore;
                    }
                    satiation = Mathf.Clamp(satiation, 0f, satiationMax);
                }
            }
            movePoints--;
        }
    }

    // -------- Movement Utility Functions --------
    #region MovementUtilityFunctions

    private bool Hungry() {
        if (satiation < satiationMax * 0.5) {
            return true;
        } else {
            return false;
        }
    }

    private void FindPlant(out int NutritionalValue, out Vector2 selectedDirectionVector) {
        Vector2 directionVector = Vector2.zero;
        selectedDirectionVector = Vector2.zero;
        NutritionalValue = 0;
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

            GameObject observedPlant = SeeIfCollisionWithOtherObject(directionVector, viewingDistance, "Plant");
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

        //if (NutritionalValue>0) Debug.Log("Plant observed with NutrionalValue " + NutritionalValue);
    }

    private void FindMeat(out int NutritionalValue, out Vector2 selectedDirectionVector) {
        Vector2 directionVector = Vector2.zero;
        selectedDirectionVector = Vector2.zero;
        NutritionalValue = 0;
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

            GameObject observedNPCreature = SeeIfCollisionWithOtherObject(directionVector, viewingDistance, "NPCreature");
            if (observedNPCreature != null) {
                Vector2 NPCreatureLocation = UnitPosition + directionVector;
                //Debug.Log("Plant observed at [" + plantLocation.x + "," + plantLocation.y + "]");
                //observed.GetComponent<SpriteRenderer>().material.SetColor("_Color", new Color(0f, 1f, 0f, 1f));
                if (observedNPCreature.GetComponent<NPCreature>().GetNutritionalValue() > NutritionalValue) {
                    // if the observed plant has highest nutritional value seen thus far, take this as direction
                    NutritionalValue = observedNPCreature.GetComponent<NPCreature>().GetNutritionalValue();
                    selectedDirectionVector = directionVector;
                } else if (observedNPCreature.GetComponent<NPCreature>().GetNutritionalValue() == NutritionalValue && Random.Range(0f, 1f) < 0.5f) {
                    // if observed plant has similar nutritional value, randomize if we choose this route to go.
                    selectedDirectionVector = directionVector;
                }
            }
        }

        // if (NutritionalValue > 0) Debug.Log("Meat observed with NutrionalValue " + NutritionalValue);
    }

    private GameObject SeeIfCollisionWithOtherObject(Vector3 RaycastDirection, int unitViewDistance, string tag) {
        float length = PlayGrid.getUnitWorldSize().x * unitViewDistance * Vector3.Magnitude(RaycastDirection);
        RaycastHit2D[] hit = Physics2D.RaycastAll(gameObject.transform.position, RaycastDirection.normalized, length);

        //Debug.DrawLine(gameObject.transform.position, gameObject.transform.position + length * RaycastDirection.normalized, Color.blue, gameMaster.getRoundDuration());

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

    private GameObject checkforPlayCreatureColission(Vector2 directionVector) {
        // Check for accidental collision with other NPCreature
        GameObject observed = SeeIfCollisionWithOtherObject(directionVector, 1, "PlayCreature");
        return observed;
    }

    private void stepForward(Vector2 directionVector) {
        TargetUnitPosition = UnitPosition + directionVector;
        if (!PlayGrid.checkIfOutOfBounds(TargetUnitPosition)) {
            // Take a step forward (update current NPCreature position)
            gameObject.GetComponent<Transform>().position = PlayGrid.getGridCoordinates(TargetUnitPosition);
        } else { // This is neccesary when hitting the play area boundary
            // flip direction vector
            gameObject.transform.Rotate(0, 0, 180);
        }
    }

    private Vector2 chooseRandomWalkingDirection() {

        // Randomly determine new orientation to move towards
        if (Random.Range(0f, 1f) < 0.3f) {
            // Choose random orientation
            //gameObject.GetComponent<Transform>().rotation = Quaternion.Euler(0f, 0f, Random.Range(-180f, 180f));

            // Alternative: choose rotation with maximum -45 to 45 degrees change
            float zRotation = gameObject.GetComponent<Transform>().eulerAngles.z;
            int sign = Random.value < .5 ? 1 : -1;
            zRotation += Random.Range(10f, 45f) * sign;
            gameObject.GetComponent<Transform>().eulerAngles = new Vector3(0, 0, zRotation);
        }

        // Choose a random direction to walk at if the NPCreature is not walking towards food (vegitation or meat)
        Vector2 directionVector = Vector2.zero;
        direction = gameObject.GetComponent<Transform>().eulerAngles.z;
        direction = Mathf.RoundToInt(direction / 45);
        // Debug.Log("Direction of PlayCreature is: " + direction);

        // Switch case to move the piggie one step in its facing direction
        switch (direction) {
            case 8: directionVector = Vector2.right + Vector2.up; break;
            case 0: directionVector = Vector2.up; break; // y++
            case 1: directionVector = Vector2.up + Vector2.left; break;
            case 2: directionVector = Vector2.left; break; // x-- 
            case 3: directionVector = Vector2.left + Vector2.down; break;
            case 4: directionVector = Vector2.down; break; // y--
            case 5: directionVector = Vector2.down + Vector2.right; break;
            case 6: directionVector = Vector2.right; break; // x++
            case 7: directionVector = Vector2.right + Vector2.up; break;
        }
        return directionVector;
    }

    #endregion
    // --------------------------------------------

    private void Evolve() {

    }
}
