using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using RosMessageTypes.Geometry;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using System.Collections.Specialized;

public class CameraHandler : MonoBehaviour
{       
    [SerializeField] private ROSPublisher publisher;
    [SerializeField] private GameObject spatialAnchor, targetObject, cameraObject;
    private Vector3 positionTargetWorld;

    // Start is called before the first frame update
    void Start()
    {
        ROSConnection.GetOrCreateInstance().Subscribe<PoseMsg>("/my_gen3/target_hologram", TargetUpdate);
        ROSConnection.GetOrCreateInstance().RegisterPublisher<PoseMsg>("/workspace_cam");

    }
    
    private Vector3 ConvertWorldASA(Vector3 positionVector, string toFrame)
    {
        Vector3 newPositionVector = Vector3.zero; // Initialize the vector

        switch (toFrame)
        {
            case "ToASA":
                newPositionVector = spatialAnchor.transform.InverseTransformDirection(positionVector - spatialAnchor.transform.position);
                break;
            case "ToWorld":
                newPositionVector = spatialAnchor.transform.TransformDirection(positionVector) + spatialAnchor.transform.position;
                break;
        }

        return newPositionVector;
    }

    void TargetUpdate(PoseMsg targetPose)
    {
        Vector3 positionTargetASA = new Vector3((float)targetPose.position.x, (float)targetPose.position.y, -(float)targetPose.position.z);
        positionTargetWorld = ConvertWorldASA(positionTargetASA, "ToWorld");
        Quaternion orientationTargetWorld = new Quaternion((float)targetPose.orientation.x, -(float)targetPose.orientation.y, -(float)targetPose.orientation.z, (float)targetPose.orientation.w);
        
        targetObject.transform.position = positionTargetWorld;
        targetObject.transform.rotation = orientationTargetWorld;
    }

    // Update is called once per frame
    void Update()
    {
        //Vector3 cameraPosition = ConvertWorldASA(cameraObject.transform.position, "ToASA");
        
        //Quaternion cameraOrientation = cameraObject.transform.rotation * spatialAnchor.transform.rotation;

        // Calculate the relative rotation
        //Quaternion cameraOrientation = Quaternion.Inverse(cameraObject.transform.rotation) * spatialAnchor.transform.rotation;

        //publisher.Pose("/workspace_cam", cameraPosition, cameraOrientation);
    }
}
