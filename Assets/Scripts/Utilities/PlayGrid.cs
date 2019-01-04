using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayGrid
{   
    public static Vector2 ScreenResolution = new Vector2(1280f,720f); // Now 1280 by 720 pixels
    public static Vector2 ScreenUnitResolution = new Vector2(64f,36f); // Number of boxes horizontally and vertically (now 64x36 boxes) --> 48 by 34 (// 14 for menu bar. 1 for border.)

    // Square grid Settings. Playfield has offset from the edges off the screen.
    public static Vector2 PlayFieldSize = new Vector2(48f, 34f);
    private static Vector2 UnitSize; // In pixels
    private static Vector2 OffsetToCentreOfBox; // In pixels
    private static Vector2[,] GridPixelCoordinates = new Vector2[65,37]; // Accompanying array of coordinates in pixels (same indices as GridUnitCoordinates)

    public static void CalculateGrid() {
        UnitSize.x = ScreenResolution.x / ScreenUnitResolution.x; //    20
        UnitSize.y = ScreenResolution.y / ScreenUnitResolution.y; // by 20
        OffsetToCentreOfBox = new Vector2(0f, 0f) + UnitSize / 2;
        //Debug.Log("Screen resolution =  " + ScreenResolution.x + " x " + ScreenResolution.y);
        //Debug.Log("Pixels per Unit box = (" + UnitSize.x + ", " + UnitSize.y + ")");
        //Debug.Log("Offset to centre Unit box = (" + OffsetToCentreOfBox.x + ", " + OffsetToCentreOfBox.y + ")");

        for (int x = 0; x < ScreenUnitResolution.x; x++) { // Loop through width
            for (int y = 0; y < ScreenUnitResolution.y; y++) { // Loop through height
                GridPixelCoordinates[x, y] = new Vector2(x * UnitSize.x, y * UnitSize.y);
                //Debug.Log("Coordinates = (" + x + "," + y + ") = (" + GridPixelCoordinates[x,y].x + "," + GridPixelCoordinates[x,y].y + ")");
            }
        }
    }

    public static Vector2 getGridCoordinates(Vector2 input) {
        float x = input.x;
        float y = input.y;
        if(x < 1 || x > PlayFieldSize.x) {
            //Debug.Log("Coordinate value requested outside of PlayField size (x = " + x + " should fall withing range [1," + PlayFieldSize.x + "])");
        }
        if (y < 1 || y > PlayFieldSize.y) {
            //Debug.Log("Coordinate value requested outside of PlayField size (y = " + y + " should fall withing range [1," + PlayFieldSize.y + "])");
        }

        x = Mathf.Clamp(x, 1, PlayFieldSize.x-1);
        y = Mathf.Clamp(y, 1, PlayFieldSize.y-1);

        Vector2 output = OffsetToCentreOfBox+GridPixelCoordinates[Mathf.RoundToInt(x), Mathf.RoundToInt(y)];
        //output = output / 100;
        //Debug.Log("Coordinates at [" + x + ";" + y + "] = ["+output.x + ";" + output.y + "]");
        return output / 100;
    }

    public static Vector2 getUnitCoordinates(Vector2 input) {
        // Input is middle point coordinate of gameObject
        input = (input*100) - OffsetToCentreOfBox; // correct for offset from middle

        float x = input.x;
        float y = input.y;

        x = Mathf.RoundToInt(x / UnitSize.x);
        y = Mathf.RoundToInt(y / UnitSize.y);

        return new Vector2(x, y);
    }

    public static Vector2 getUnitWorldSize() {
        //Debug.Log("UnitSize = [" + UnitSize.x + ";" + UnitSize.y + "]");
        return UnitSize/100;
    }

    public static bool checkIfOutOfBounds(Vector2 targetUnitCoordinates) {
        if (targetUnitCoordinates.x < 1 || targetUnitCoordinates.x > PlayFieldSize.x - 1) {
            //Debug.Log("Target coordinate lies out of bounds of PlayField");
            return true;
        } else if (targetUnitCoordinates.y < 1 || targetUnitCoordinates.y > PlayFieldSize.y - 1) {
            //Debug.Log("Target coordinate lies out of bounds of PlayField");
            return true;
        }

        return false;
    }
}
