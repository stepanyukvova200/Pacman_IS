using System;
using UnityEngine;

public class GhostChase : GhostBehavior
{
    private float stickCd = 1;
    private float elapsedStick = 1;
    private void OnDisable()
    {
        ghost.scatter.Enable();
    }

    private void OnEnable()
    {
        base.Enable();
        RecalculateDirection();
    }

    private void FixedUpdate()
    {
        elapsedStick += Time.fixedDeltaTime;
    }

    private void RecalculateDirection()
    {
        if (enabled)
        {
            Vector2 direction = GameManager.instance.algorithms.GetNextMoveDirectionByAStar(ghost, GameManager.instance.pacman);

            if (GameManager.instance.ghosts.IndexOf(ghost) == 0)
                Debug.Log($"Ghost {ghost.name} is going to {direction}");
            
            ghost.movement.SetDirection(direction, true);
            ghost.movement.Move();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Node node = other.GetComponent<Node>();
        //
        // // Do nothing while the ghost is frightened
        if (node != null && node.isArtificial == false && enabled && !ghost.frightened.enabled)
        {
            ghost.movement.transform.position = node.transform.position;
            if (elapsedStick >= stickCd)
            {
                elapsedStick = 0;
            }
            RecalculateDirection();
        }
        // {
        //     Vector2 direction = GameManager.instance.algorithms.GetNextMoveDirectionByAStar(ghost, GameManager.instance.pacman);
        //     // float minDistance = float.MaxValue;
        //     //
        //     // // Find the available direction that moves closet to pacman
        //     // foreach (Vector2 availableDirection in node.availableDirections)
        //     // {
        //     //     // If the distance in this direction is less than the current
        //     //     // min distance then this direction becomes the new closest
        //     //     Vector3 newPosition = transform.position + new Vector3(availableDirection.x, availableDirection.y);
        //     //     float distance = (ghost.target.position - newPosition).sqrMagnitude;
        //     //
        //     //     if (distance < minDistance)
        //     //     {
        //     //         direction = availableDirection;
        //     //         minDistance = distance;
        //     //     }
        //     // }
        //
        //     ghost.movement.SetDirection(direction);
        // }
    }

}
