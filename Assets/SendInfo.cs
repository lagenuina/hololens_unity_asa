using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using RosMessageTypes.Geometry;
using RosMessageTypes.Scripts;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Audio;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System.Security.Authentication.ExtendedProtection;
using System.Collections.Specialized;
using System.Linq.Expressions;

public class SendInfo : MonoBehaviour
{   [SerializeField] private ROSPublisher publisher;
    [SerializeField] private GameObject spatialAnchor;

    // Start is called before the first frame update
    void Start()
    {
        ROSConnection.GetOrCreateInstance().RegisterPublisher<StringMsg>("/debugging");

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

    // Update is called once per frame
    void Update()
    {
        Vector3 positionTargetASA = ConvertWorldASA(gameObject.transform.position, "ToASA");
        publisher.StringMessage("/debugging", positionTargetASA.ToString());
    }
}
