using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardStats : MonoBehaviour
{
    private Text NPCreatureCountText;
    private Board board;

    // Start is called before the first frame update
    void Start()
    {
        NPCreatureCountText = GameObject.Find("NPCpopulationCount").GetComponent<Text>();
        board = GameObject.FindObjectOfType<Board>();
    }

    // Update is called once per frame
    void Update()
    {
        NPCreatureCountText.text = "NPCreature population = " + board.getNPCreatureCount().ToString();
    }
}
