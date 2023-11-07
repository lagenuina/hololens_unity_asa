using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using RosMessageTypes.Geometry;
using Microsoft.MixedReality.Toolkit.UI;

public class ArrowsManager : MonoBehaviour
{
    private int buttonPressed = 0;
    public ROSPublisher publisher;
    ROSConnection ros;
    [SerializeField] private Interactable toggleSwitchBase;

    // Start is called before the first frame update
    void Start()
    {
        // start the ROS connection
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<Int32Msg>("button_pressed");
    }

    void Update()
    {
        publisher.Int32Message("button_pressed", buttonPressed);
    }

    public void AppearArrowsBaseControl(){

        if (toggleSwitchBase.IsToggled){
            gameObject.SetActive(true);

            // Position the objectToActivate in front of you
            gameObject.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 0.5f;
            Vector3 cameraEulerAngles = Camera.main.transform.rotation.eulerAngles;
            Quaternion desiredRotation = Quaternion.Euler(0, cameraEulerAngles.y, 0);
            gameObject.transform.rotation = desiredRotation;   
        }
        else {
            gameObject.SetActive(false);
        }

    }

    public void upPressed()
    {
        // Send 1 if up button is pressed
        buttonPressed = 1;
    }

    public void Released()
    {
        buttonPressed = 0;
    }

    public void downPressed()
    {
        // Send 2 if down button is pressed
        buttonPressed = 2;
    }

    public void rightPressed()
    {
        // Send 3 if right button is pressed
        buttonPressed = 3;
    }

    public void leftPressed()
    {
        // Send 4 if left button is pressed
        buttonPressed = 4;
    }
}
