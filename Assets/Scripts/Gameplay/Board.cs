using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour {
    [SerializeField]
    GameObject plant, NPCreature, playerCreature;

    Vector2 worldSize;

    //environment parameters
    float plantDensity = .25f;              // 0.25f
    float NPCreatureDensity = .05f;         // 0.05f
    int maximumNumberOfNPCreatures = 100;   // 100
    int maximumVegitation = 500;            // 500

    // Environment counter
    private int CountNPCreatures = 0;
    private int CountVegitation = 0;

    // Start is called before the first frame update
    void Start() {
        PlayGrid.CalculateGrid();
        worldSize = PlayGrid.PlayFieldSize;
        GenerateEnvironment();
    }

    private void GenerateEnvironment() {
        //go over all the tiles on the board
        for (int i = 0; i < (int)worldSize.x; i++) {
            for (int j = 0; j < (int)worldSize.y; j++) {
                // Spawn plant
                Vector3 spawnCoordinates = (Vector3)PlayGrid.getGridCoordinates(new Vector2(i, j));
                //determine if a plant should be instantiated
                if (Random.Range(0f, 1f) < plantDensity) {
                    spawnVegitation(spawnCoordinates);
                    //Instantiate(plant, spawnCoordinates, Quaternion.identity);
                } else if (Random.Range(0f, 1f) < NPCreatureDensity) {
                    spawnNPCreature(spawnCoordinates,-1); 
                    //Instantiate(NPCreature, spawnCoordinates, Quaternion.Euler(0f, 0f, Random.Range(-180f, 180f)));
                }
            }
        }

        //spawn the player creatures
        GameObject player1 = Instantiate(playerCreature, (Vector3)PlayGrid.getGridCoordinates(new Vector2(30, 10)), Quaternion.identity);
        player1.GetComponent<PlayCreature>().Birth(1, new Vector2(30, 10), false);
    }

    public void spawnVegitation(Vector3 spawnCoordinates) {
        if (CountVegitation >= maximumVegitation) {
            //Debug.Log("Maximum Vegitation spawned.");
            return;
        }

        // Spawn vegitation
        Instantiate(plant, spawnCoordinates, Quaternion.identity);
        CountVegitation++;
    }

    public void spawnNPCreature(Vector3 spawnCoordinates,int age){
        if (CountNPCreatures >= maximumNumberOfNPCreatures) {
            //Debug.Log("Maximum number of NPCreatures have spawned.");
            return;
        }

        // Spawn with random orientation and predetermined or random age
        GameObject creature = Instantiate(NPCreature, spawnCoordinates, Quaternion.Euler(0f,0f,Random.Range(-180f,180f)));
        if (age == -1) {
            creature.GetComponent<NPCreature>().setRandomAge();
        } else {
            creature.GetComponent<NPCreature>().setAge(age);
        }
        CountNPCreatures++;
    }

    public void reduceNPCreatureCount() {CountNPCreatures--;}
    public int getNPCreatureCount() {return CountNPCreatures;}
}

