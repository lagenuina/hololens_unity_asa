using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;

public class WorkspaceCameraManager : MonoBehaviour
{
    [SerializeField] private ROSPublisher publisher;

    // Start is called before the first frame update
    void Start()
    {
        // ROSConnection.GetOrCreateInstance().RegisterPublisher<PoseMsg>("/hologram_camera_feedback/pose");
    }

    // private Vector3 ConvertWorldASA(Vector3 positionVector, string ToFrame)
    // {
    //     Vector3 newPositionVector = Vector3.zero; // Initialize the vector

    //     if (ToFrame == "ToASA")
    //     {
    //         positionVector -= spatialAnchor.transform.position;
    //         newPositionVector = spatialAnchor.transform.InverseTransformDirection(positionVector);
    //     }
    //     else if (ToFrame == "ToWorld")
    //     {
    //         Vector3 vectorWorldFrame = spatialAnchor.transform.TransformDirection(positionVector);
    //         newPositionVector = vectorWorldFrame + spatialAnchor.transform.position;
    //     }

    //     return newPositionVector;
    // }

    // Update is called once per frame
    void Update()
    {
        // desiredCameraASA = ConvertWorldASA(gameObject.transform.position, "ToASA");
        // publisher.Pose("/hologram_camera_feedback/pose", desiredCameraASA, new Quaternion(0, 0, 0, 1));

    }
}
