using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using System.Text;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;

public class GazeTracker : MonoBehaviour
{
    private Vector3 gazeOrigin, gazeDirection;
    private StringBuilder csvContent;
    private string filePath;

    void Start()
    {
        csvContent = new StringBuilder();
        filePath = Path.Combine(UnityEngine.Application.persistentDataPath, "GazeData.csv");

        // Add CSV header
        csvContent.AppendLine("Timestamp, GazeOriginX, GazeOriginY, GazeOriginZ, GazeDirectionX, GazeDirectionY, GazeDirectionZ");
    }

    void LogGazeDirectionOrigin()
    {
        gazeDirection = CoreServices.InputSystem.GazeProvider.GazeDirection;
        gazeOrigin = CoreServices.InputSystem.GazeProvider.GazeOrigin;

        // Append data to CSV content
        string dataLine = string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}",
            Time.time, gazeOrigin.x, gazeOrigin.y, gazeOrigin.z, gazeDirection.x, gazeDirection.y, gazeDirection.z);
        csvContent.AppendLine(dataLine);
    }

    void Update()
    {
        LogGazeDirectionOrigin();
    }

    private void OnDestroy()
    {
        SaveCSV();
    }

    private void SaveCSV()
    {
        File.WriteAllText(filePath, csvContent.ToString());
        //Debug.Log("CSV saved to: " + filePath);
    }
}