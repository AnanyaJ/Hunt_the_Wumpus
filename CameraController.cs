using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    // constants
    private readonly float SENSITIVITY = 50.0f;
    private readonly float SMOOTHING = 0.1f;
    private readonly float SPEED = 7;

    private Vector2 md;
    private Vector2 mouseLook;
    private Vector2 smoothV;
    private Image compass;
    private bool gameInSession;
    private bool isShowingBats;

    public GameObject character;
    public GameObject arrow;

    // Start is called before the first frame update
    private void Start()
    {
        md = new Vector2();
        mouseLook = new Vector2();
        smoothV = new Vector2();
        gameInSession = true;
        isShowingBats = false;
    }

    // Update is called once per frame
    private void Update()
    {
        // if dragging mouse, update mouse location
        if (Input.GetMouseButton(0) && !Input.GetMouseButtonDown(0))
        {
            md = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
            md = Vector2.Scale(md, new Vector2(SENSITIVITY * SMOOTHING, SENSITIVITY * SMOOTHING));
            smoothV.x = Mathf.Lerp(smoothV.x, md.x, 1f / SMOOTHING);
            smoothV.y = Mathf.Lerp(smoothV.y, md.y, 1f / SMOOTHING);
            mouseLook += smoothV;
        }

        if (gameInSession) // rotate screen
        {
            this.transform.localRotation = Quaternion.AngleAxis(-mouseLook.y, Vector3.right);
            character.transform.localRotation = Quaternion.AngleAxis(mouseLook.x, Vector3.up);
            arrow.transform.localRotation = Quaternion.AngleAxis(mouseLook.x, character.transform.up);
            arrow.transform.localRotation = Quaternion.AngleAxis(-mouseLook.y, character.transform.right);
            compass.transform.localRotation = Quaternion.AngleAxis(mouseLook.x, compass.transform.forward);
        }
        else if (isShowingBats) // bats animation
        {
            this.transform.Rotate(Vector3.down * SPEED * Time.deltaTime);
            character.transform.Rotate(Vector3.down * SPEED * Time.deltaTime);
            arrow.transform.Rotate(Vector3.down * SPEED * Time.deltaTime); 
            compass.transform.Rotate(Vector3.back * SPEED * Time.deltaTime);
        }
        else // reset angular velocity if not rotating (set rotation to 0)
        {
            character.GetComponent<Rigidbody>().angularVelocity = new Vector3(0, 0, 0);
            arrow.GetComponent<Rigidbody>().angularVelocity = new Vector3(0, 0, 0);
        }
    }

    // mutator methods

    // bool gameInSession: whether or not screen should rotate upon dragging of mouse
    public void SetGameInSession(bool gameInSession)
    {
        this.gameInSession = gameInSession;
    }

    // bool isShowingBats: whether or not bats animation is taking place
    public void SetIsShowingBats(bool isShowingBats)
    {
        this.isShowingBats = isShowingBats;
    }

    // Image compass: image for compass
    public void SetCompass(Image compass)
    {
        this.compass = compass;
    }
}
