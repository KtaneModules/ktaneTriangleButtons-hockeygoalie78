using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Module made by hockeygoalie78
/// Module designed by Pruz
/// Based on the orientation of the buttons, press the correct button.
/// </summary>
public class TriangleButtons : MonoBehaviour
{
    public KMAudio bombAudio;
    public GameObject[] triangles;
    public SpriteRenderer[] triangleRenderers;
    public KMSelectable[] buttons;
    private string buttonOrientation; //0 is up, 1 is right, 2 is down, 3 is left
    private int[] buttonIndexes; //0 is up, 1 is right, 2 is down, 3 is left
    private int swapInt;
    private int swapIndex;
    private Dictionary<string, int> solutions; //0 is top-left, 1 is top-right, 2 is bottom-left, 3 is bottom-right
    private KMNeedyModule needyModule;
    private bool moduleActive;
    private Vector3 rotation;

    private readonly string[] buttonLocations = { "Top left", "Top right", "Bottom left", "Bottom right" };
    private readonly string[] arrowDirections = { "up", "right", "down", "left" };

    private static int moduleIdCounter = 1;
    private int moduleId;

    void Start ()
    {
        //Set module ID
        moduleId = moduleIdCounter++;

        //Set up needy module
        needyModule = GetComponent<KMNeedyModule>();
        needyModule.OnNeedyActivation += delegate { RandomizeOrientation(); };
        needyModule.OnNeedyDeactivation += delegate { HideTriangles(); };
        needyModule.OnTimerExpired += delegate { needyModule.HandleStrike(); HideTriangles(false); };

        //Other variables
        moduleActive = false;
        rotation = new Vector3(90, 0, 0);

        //Add solutions to dictionary of solutions
        solutions = new Dictionary<string, int>() {
            { "3102", 0 },
            { "1320", 2 },
            { "0213", 1 },
            { "2103", 2 },
            { "3120", 3 },
            { "2013", 0 },
            { "3210", 1 },
            { "2031", 1 },
            { "1032", 2 },
            { "3021", 3 },
            { "0123", 3 },
            { "2130", 1 },
            { "1203", 0 },
            { "1302", 3 },
            { "0231", 3 },
            { "0321", 2 },
            { "3201", 1 },
            { "3012", 2 },
            { "0312", 0 },
            { "0132", 1 },
            { "1230", 3 },
            { "2301", 0 },
            { "2310", 2 },
            { "1023", 0 }
        };

        //Set up button selections
        for(int c = 0; c < buttons.Length; c++)
        {
            int d = c;
            buttons[c].OnInteract += delegate { ButtonPress(d); return false; };
        }
    }

    /// <summary>
    /// Randomize the orientations of the buttons
    /// </summary>
    private void RandomizeOrientation()
    {
        //Module is active
        moduleActive = true;
        Debug.LogFormat(@"[Triangle Buttons #{0}] Needy module is active.", moduleId);

        //Randomize orientations
        buttonIndexes = new int[4] { 0, 1, 2, 3 };
        for(int c = 0; c < buttonIndexes.Length; c++)
        {
            swapIndex = Random.Range(c, buttonIndexes.Length);
            swapInt = buttonIndexes[swapIndex];
            buttonIndexes[swapIndex] = buttonIndexes[c];
            buttonIndexes[c] = swapInt;
        }
        buttonOrientation = "" + buttonIndexes[0] + buttonIndexes[1] + buttonIndexes[2] + buttonIndexes[3];
        Debug.LogFormat(@"[Triangle Buttons #{0}] Arrow orientation is {1}, {2}, {3}, and {4}.", moduleId, 
            arrowDirections[buttonIndexes[0]], arrowDirections[buttonIndexes[1]], arrowDirections[buttonIndexes[2]], arrowDirections[buttonIndexes[3]]);
        Debug.LogFormat(@"[Triangle Buttons #{0}] Correct button is the {1} button.", moduleId, buttonLocations[solutions[buttonOrientation]].ToLower());

        //Adjust rotation of triangles
        for (int c = 0; c < triangles.Length; c++)
        {
            rotation.y = 90 * buttonIndexes[c];
            triangles[c].transform.localEulerAngles = rotation;
        }

        //Show the triangles
        for(int c = 0; c < triangleRenderers.Length; c++)
        {
            triangleRenderers[c].enabled = true;
        }
    }

    /// <summary>
    /// Hides all of the triangles (for deactivation) and handles a pass or strike
    /// </summary>
    /// <param name="buttonPressedInTime">True if a button was pressed before the timer; false otherwise.</param>
    private void HideTriangles(bool buttonPressedInTime = true)
    {
        for(int c = 0; c < triangleRenderers.Length; c++)
        {
            triangleRenderers[c].enabled = false;
        }
        moduleActive = false;
        if(!buttonPressedInTime)
        {
            Debug.LogFormat(@"[Triangle Buttons #{0}] Timer expired. Strike occurred.", moduleId);
        }
    }

    /// <summary>
    /// Handle the success or failure of the module
    /// </summary>
    /// <param name="index">The index of the button pressed</param>
    /// <param name="strike">Whether or not a strike should be given (true by default)</param>
    private void StrikeHandler(int index, bool strike = true)
    {
        if(strike)
        {
            needyModule.HandleStrike();
            Debug.LogFormat(@"[Triangle Buttons #{0}] {1} button has been pushed and is incorrect. Strike occurred, module deactivated.", moduleId, buttonLocations[index]);
        }
        else
        {
            Debug.LogFormat(@"[Triangle Buttons #{0}] {1} button has been pushed and is correct. Module deactivated.", moduleId, buttonLocations[index]);
        }
        needyModule.HandlePass();
    }

    /// <summary>
    /// Determine whether or not the correct button has been pushed and handle it appropriately
    /// </summary>
    /// <param name="index">The index of the button pushed</param>
    private void ButtonPress(int index)
    {
        //Movement/audio
        buttons[index].AddInteractionPunch(.2f);
        bombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, transform);

        //Handle pass or strike
        if(moduleActive)
        {
            HideTriangles();
            StrikeHandler(index, solutions[buttonOrientation] != index);
        }
    }
}
