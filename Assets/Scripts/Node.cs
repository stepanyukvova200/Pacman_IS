using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Node : MonoBehaviour
{
    public LayerMask obstacleLayer;
    public List<Vector2> availableDirections { get; private set; }

    public bool isArtificial = false;
    [HideInInspector]
    public bool isDestroyed = false;
    
    private void Start()
    {
        UpdateAvailableDirections();
    }

    public void UpdateAvailableDirections()
    {
        availableDirections = new List<Vector2>();

        CheckAvailableDirection(Vector2.up);
        CheckAvailableDirection(Vector2.down);
        CheckAvailableDirection(Vector2.left);
        CheckAvailableDirection(Vector2.right);
    }

    private void OnDrawGizmos()
    {
        foreach (var dir in availableDirections)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)dir);
        }
    }

    private void CheckAvailableDirection(Vector2 direction)
    {
        RaycastHit2D[] hit = Physics2D.BoxCastAll(transform.position, Vector2.one * 0.5f, 0f, direction, 1f, obstacleLayer);

        // If no collider is hit then there is no obstacle in that direction
        if (hit.Length == 0 || hit.Any(h => h.transform.GetComponent<Node>())) {
            availableDirections.Add(direction);
        }
    }

}
