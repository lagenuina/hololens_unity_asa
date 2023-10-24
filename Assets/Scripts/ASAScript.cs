using Microsoft.Azure.SpatialAnchors;
using Microsoft.Azure.SpatialAnchors.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;
using UnityEngine.XR;
using Microsoft.MixedReality.Toolkit.UI;
using RosMessageTypes.Std;
using RosMessageTypes.Geometry;

[RequireComponent(typeof(SpatialAnchorManager))]
public class ASAScript : MonoBehaviour
{
    [SerializeField] private ROSPublisher publisher;

    [SerializeField] private GameObject spatialAnchor, locatedAnchor;

    [SerializeField] 
    [Tooltip("Anchor ID to be located")]
    private string anchorID;

    [SerializeField]
    [Tooltip("Assign DialogSmall_192x96.prefab")]
    private GameObject dialogPrefab;

    [SerializeField] private string topicname;

    /// <summary>
    /// Medium Dialog example prefab to display
    /// </summary>
    public GameObject DialogPrefab
    {
        get => dialogPrefab;
        set => dialogPrefab = value;
    }

    /// <summary>
    /// Main interface to anything Spatial Anchors related
    /// </summary>
    private SpatialAnchorManager _spatialAnchorManager = null;

    /// <summary>
    /// Used to keep track of all GameObjects that represent a found or created anchor
    /// </summary>
    private List<GameObject> _foundOrCreatedAnchorGameObjects = new List<GameObject>();

    /// <summary>
    /// Used to keep track of all the created Anchor IDs
    /// </summary>
    private List<String> _createdAnchorIDs = new List<String>();

    // <Start>
    // Start is called before the first frame update
    void Start()
    {
        ROSConnection.GetOrCreateInstance().RegisterPublisher<StringMsg>(topicname);
        _spatialAnchorManager = GetComponent<SpatialAnchorManager>();
        _spatialAnchorManager.Error += (sender, args) => publisher.StringMessage(topicname, $"ASA - Error: {args.ErrorMessage}");
        _spatialAnchorManager.AnchorLocated += SpatialAnchorManager_AnchorLocated;

        LocateAnchor();
    }

    public void OnClick()
    {
        CreateAnchor();
    }

    public void FindAnchor()
    {
        publisher.StringMessage(topicname, "Locating Anchor...");
        LocateAnchor();
    }

    private async Task CreateAnchor()
    {
        await _spatialAnchorManager.StartSessionAsync();

        //Add and configure ASA components
        CloudNativeAnchor cloudNativeAnchor = spatialAnchor.AddComponent<CloudNativeAnchor>();
        await cloudNativeAnchor.NativeToCloud();
        CloudSpatialAnchor cloudSpatialAnchor = cloudNativeAnchor.CloudAnchor;
        cloudSpatialAnchor.Expiration = DateTimeOffset.Now.AddDays(3);

        //Collect Environment Data
        //while (!_spatialAnchorManager.IsReadyForCreate)
        while (_spatialAnchorManager.SessionStatus.RecommendedForCreateProgress < 1.0f)
        {
            float createProgress = _spatialAnchorManager.SessionStatus.RecommendedForCreateProgress;
        }

        publisher.StringMessage(topicname, "ASA - Saving cloud anchor... ");

        try
        {
            // Now that the cloud spatial anchor has been prepared, we can try the actual save here.
            await _spatialAnchorManager.CreateAnchorAsync(cloudSpatialAnchor);

            bool saveSucceeded = cloudSpatialAnchor != null;
            if (!saveSucceeded)
            {
                publisher.StringMessage(topicname, "ASA - Failed to save, but no exception was thrown.");
                return;
            }

            publisher.StringMessage(topicname, $"ASA - Saved cloud anchor with ID: {cloudSpatialAnchor.Identifier}");
            _foundOrCreatedAnchorGameObjects.Add(spatialAnchor);
            _createdAnchorIDs.Add(cloudSpatialAnchor.Identifier);
            spatialAnchor.GetComponent<MeshRenderer>().material.color = Color.green;

            Dialog.Open(DialogPrefab, DialogButtonType.OK, "Anchor created", $"Anchor stored to cloud with ID: {cloudSpatialAnchor.Identifier}", true);

        }
        catch (Exception exception)
        {
            publisher.StringMessage(topicname, "ASA - Failed to save anchor: " + exception);
            Debug.LogException(exception);
        }

    }

    private async void LocateAnchor()
    {
        await _spatialAnchorManager.StartSessionAsync();

        // Replace with your anchor ID
        _createdAnchorIDs.Add(anchorID);

        // Create watcher to look for the stored anchor IDs
        AnchorLocateCriteria anchorLocateCriteria = new AnchorLocateCriteria();
        anchorLocateCriteria.Identifiers = _createdAnchorIDs.ToArray();
        _spatialAnchorManager.Session.CreateWatcher(anchorLocateCriteria);

        publisher.StringMessage(topicname, $"ASA - Watcher created!");
    }

    /// <summary>
    /// Callback when an anchor is located
    /// </summary>
    /// <param name="sender">Callback sender</param>
    /// <param name="args">Callback AnchorLocatedEventArgs</param>
    private void SpatialAnchorManager_AnchorLocated(object sender, AnchorLocatedEventArgs args)
    {
        publisher.StringMessage(topicname, $"ASA - Anchor recognized as a possible anchor {args.Identifier} {args.Status}");

        if (args.Status == LocateAnchorStatus.Located)
        {
            //Creating and adjusting GameObjects have to run on the main thread. We are using the UnityDispatcher to make sure this happens.
            UnityDispatcher.InvokeOnAppThread(() =>
            {
                // Read out Cloud Anchor values
                CloudSpatialAnchor cloudSpatialAnchor = args.Anchor;

                // Retrieve the position and orientation of the found cloud anchor
                Vector3 anchorPosition = cloudSpatialAnchor.GetPose().position;
                Quaternion anchorOrientation = cloudSpatialAnchor.GetPose().rotation;

                //Create GameObject
                locatedAnchor.SetActive(true);
                locatedAnchor.GetComponent<MeshRenderer>().material.shader = Shader.Find("Legacy Shaders/Diffuse");
                locatedAnchor.GetComponent<MeshRenderer>().material.color = Color.blue;

                // Link to Cloud Anchor
                locatedAnchor.AddComponent<CloudNativeAnchor>().CloudToNative(cloudSpatialAnchor);
                _foundOrCreatedAnchorGameObjects.Add(locatedAnchor);

            });
        }

    }
}