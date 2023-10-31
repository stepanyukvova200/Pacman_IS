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
        if (node != null && node.isArtificial == false && enabled && !ghost.frightened.enabled)
        {
            ghost.movement.transform.position = node.transform.position;
            if (elapsedStick >= stickCd)
            {
                elapsedStick = 0;
            }
            RecalculateDirection();
        }
    }

}
