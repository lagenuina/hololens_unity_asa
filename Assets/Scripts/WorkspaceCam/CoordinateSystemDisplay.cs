using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoordinateSystemDisplay : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        // Draw the X-axis in red
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.right);

        // Draw the Y-axis in green
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.up);

        // Draw the Z-axis in blue
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward);
    }
}

