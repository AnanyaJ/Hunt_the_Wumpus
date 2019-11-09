using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    // constants
    private readonly float[] X_COEFFICIENTS = new float[] { 0, Mathf.Sqrt(3), Mathf.Sqrt(3), 0, -Mathf.Sqrt(3), -Mathf.Sqrt(3) };
    private readonly float[] Z_COEFFICIENTS = new float[] { 1, 1, -1, -1, -1, 1 };
    private readonly float[] Z_INTERCEPTS = new float[] { 5 * Mathf.Sqrt(3), 10 * Mathf.Sqrt(3), 10 * Mathf.Sqrt(3),
                                                            5 * Mathf.Sqrt(3), 10 * Mathf.Sqrt(3), 10 * Mathf.Sqrt(3) };
    private readonly float[] X_TRANSLATIONS = new float[] { 0, -15, -15, 0, 15, 15 };
    private readonly float[] Z_TRANSLATIONS = new float[] { -10 * Mathf.Sqrt(3), -5 * Mathf.Sqrt(3), 5 * Mathf.Sqrt(3),
                                                            10 * Mathf.Sqrt(3), 5 * Mathf.Sqrt(3), -5 * Mathf.Sqrt(3) };
    private readonly float BUFFER = 0.5f;
    private readonly float SPEED = 5;

    // instance variables
    private bool gameInSession;
    private bool isFallingInPit;
    private bool isRisingFromPit;
    private bool arrowBeingShot;
    private bool arrowReachedRoom;

    private GameScene gameScene;

    public GameObject arrow;

    // Start is called before the first frame update
    private void Start()
    {
        gameInSession = true;
        isFallingInPit = false;
        isRisingFromPit = false;
        gameScene = new GameScene();
    }

    // Update is called once per frame
    private void Update()
    {
        if (gameInSession)
        {
            // calculate character movement
            float translation = Input.GetAxisRaw("Vertical") * SPEED * Time.deltaTime;
            float straffe = Input.GetAxisRaw("Horizontal") * SPEED * Time.deltaTime;

            // if not moving, set velocity to 0 (prevents residual movement)
            if (translation == 0 && straffe == 0)
            {
                this.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
            }
            else // update position
            {
                this.GetComponent<Rigidbody>().MovePosition(this.transform.position + this.transform.forward * translation + this.transform.right * straffe);
            }

            int currentRoom = GameControl.GetGameLocations().GetPlayerLocation();
            int[] allAdjacentRooms = GameControl.GetGameLocations().GetCave().GetAllNeighbors(currentRoom);
            List<int> adjacentRooms = GameControl.GetGameLocations().GetCave().GetAdjacentRooms(currentRoom);

            for (int i = 0; i < GameControl.NUM_NEIGHBORS_PER_ROOM; i++)
            {   // if crossing into new room (determined with inequality for lines representing walls)...
                if (X_COEFFICIENTS[i] * this.transform.position.x + Z_COEFFICIENTS[i] * this.transform.position.z > Z_INTERCEPTS[i] + BUFFER)
                {   
                    // translate player to corresponding position in center room (gives illusion of infinite physical space)
                    Vector3 movement = new Vector3(X_TRANSLATIONS[i], 0f, Z_TRANSLATIONS[i]);
                    this.GetComponent<Rigidbody>().MovePosition(this.transform.position + movement);
                    if (adjacentRooms.Contains(allAdjacentRooms[i])) // adjacent room
                    {
                        gameScene.NewRoom(allAdjacentRooms[i]); // change room
                    }
                    break;
                }            
            }

        }
        else if (isFallingInPit) // animation for falling into pit
        {
            this.GetComponent<Rigidbody>().MovePosition(this.transform.position + Vector3.down * SPEED * Time.deltaTime / 2);
        }
        else if (isRisingFromPit) // animation for rising from pit
        {
            this.GetComponent<Rigidbody>().MovePosition(this.transform.position + Vector3.up * SPEED * Time.deltaTime / 2);
        }
        else if (arrowBeingShot) // find which room arrow was shot into
        {
            int currentRoom = GameControl.GetGameLocations().GetPlayerLocation();
            int[] allAdjacentRooms = GameControl.GetGameLocations().GetCave().GetAllNeighbors(currentRoom);
            List<int> adjacentRooms = GameControl.GetGameLocations().GetCave().GetAdjacentRooms(currentRoom);

            for (int i = 0; i < GameControl.NUM_NEIGHBORS_PER_ROOM; i++)
            {   // if arrow crossed into new room (using inequality for lines representing walls)
                if (X_COEFFICIENTS[i] * arrow.transform.position.x + Z_COEFFICIENTS[i] * arrow.transform.position.z > Z_INTERCEPTS[i] + BUFFER && adjacentRooms.Contains(allAdjacentRooms[i]) && !arrowReachedRoom)
                {
                    arrowReachedRoom = true; // arrow shot into adjacent room
                    gameScene.ArrowShot(allAdjacentRooms[i]);
                }
            }
        }
        else // not moving
        {
            this.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
        }
    }

    // mutator methods

    // bool gameInSession: true if player able to move around
    public void SetGameInSession(bool gameInSession)
    {
        this.gameInSession = gameInSession;
    }

    // bool isFallingInPit: true if animation for falling into pit is executing
    public void SetFallingInPit(bool isFallingInPit)
    {
        this.isFallingInPit = isFallingInPit;
    }

    // bool isRisingFromPit: true if animation for rising from pit is executing
    public void SetRisingFromPit(bool isRisingFromPit)
    {
        this.isRisingFromPit = isRisingFromPit;
    }

    // bool arrowWillBeShot: true if arrow is moving through air
    public void ArrowWillBeShot()
    {
        arrowBeingShot = true;
        arrowReachedRoom = false;
    }

    // bool arrowShot: true if arrow has landed on ground
    public bool ArrowShot()
    {
        arrowBeingShot = false;
        return arrowReachedRoom;
    }

    // GameScene gameScene: object handling UI for game
    public void SetGameScene(GameScene gameScene)
    {
        this.gameScene = gameScene;
    }

}
