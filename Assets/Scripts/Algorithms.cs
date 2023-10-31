using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Algorithms : MonoBehaviour
{
    public GameObject nodePrefab;
    
    List<Node> spawnedArtificialNodes = new List<Node>();
    
    List<List<int>> adjencyList = new List<List<int>>();
    List<Node> nodes = new List<Node>();
    private IEnumerator Start()
    {
        yield return null;
        
        BuildGraph();
    }
    
    private void PrintPathFromPacmanToFirstGhost()
    {
        BuildGraph();
    
        int targetNode = nodes.IndexOf(spawnedArtificialNodes[0]);
        int startNode = nodes.IndexOf(spawnedArtificialNodes[1]);
        var path = AStar(targetNode, startNode);
        if (path != null)
        {
            foreach (var node in path)
            {
                Debug.Log(node);
            }
        }

        EditorApplication.isPaused = true;
    }

    public Vector3 GetNextMoveDirectionByAStar(Ghost ghost, Pacman pacman)
    {
        BuildGraph();
        int ghostIdx = -1, pacIdx = -1;
        if (spawnedArtificialNodes.Any(n =>
                Mathf.Approximately(((Vector2)(n.transform.position - ghost.transform.position)).magnitude, 0)))
        {
            ghostIdx = nodes.IndexOf(spawnedArtificialNodes.First(n =>
                Mathf.Approximately(((Vector2)(n.transform.position - ghost.transform.position)).magnitude, 0)));
        }
        
        if (spawnedArtificialNodes.Any(n =>
                Mathf.Approximately(((Vector2)(n.transform.position - pacman.transform.position)).magnitude, 0)))
        {
            pacIdx = nodes.IndexOf(spawnedArtificialNodes.First(n =>
                Mathf.Approximately(((Vector2)(n.transform.position - pacman.transform.position)).magnitude, 0)));
        }

        if (pacIdx != -1 && ghostIdx != -1)
        {
            return GetNextMoveDirectionByAStar(ghostIdx, pacIdx);
        }
        else
        {
            Debug.LogError("Can't find pacman or ghost node");
            return Vector3.zero;
        }
    }
    
    public Vector3 GetNextMoveDirectionByAStar(int startNode, int targetNode)
    {
        var path = AStar(startNode, targetNode);
        if (path != null)
        {
            for (var i = 1; i < path.Count; i++)
            {
                var nextNode = path[i];
                var dir = ((Vector3)Vector3Int.RoundToInt((nodes[nextNode].transform.position - nodes[startNode].transform.position).normalized)).normalized;
                if (dir.sqrMagnitude == 0)
                    continue;
                return dir;
            }
        }

        return Vector3.zero;
    }

    private void UpdateGraph()
    {
        nodes = FindObjectsByType<Node>(FindObjectsSortMode.InstanceID).Where(n => n.isDestroyed == false).ToList();
        nodes = nodes.OrderBy(n => n.transform.position.x).ThenBy(n => n.transform.position.y).ToList();
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying && !EditorApplication.isPaused)
            BuildGraph(true);
        else
            UpdateGraph();
        
        Handles.color = Color.red;
        for (var index = 0; index < nodes.Count; index++)
        {
            var node = nodes[index];
            if (node != null)
                Handles.Label(node.transform.position, index.ToString());
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
            PrintPathFromPacmanToFirstGhost();
    }

    public void BuildGraph(bool fromGizmo = false)
    {
        ResetArtificialNodes();
        SpawnArtificialNodes();

        UpdateGraph();
        adjencyList.Clear();
        
        //build adjency list
        for (int i = 0; i < nodes.Count; i++)
        {
            adjencyList.Add(new List<int>());
            for (int j = 0; j < nodes.Count; j++)
            {
                if (i == j) continue;
                //take only nearest node in each direction
                var vectorDiff = (nodes[j].transform.position - nodes[i].transform.position);
                var dir = vectorDiff.normalized;
                
                if (nodes[i].availableDirections.Any(n => ((Vector3)n).EqualVector(dir)))
                {
                    //check if node nearest in direction
                    int foundNode = -1;
                    foreach (var nIdx in adjencyList[i])
                    {
                        if ((nodes[nIdx].transform.position - nodes[i].transform.position).normalized.EqualVector(dir))
                        {
                            foundNode = nIdx;
                        }
                    }
                    
                    if (foundNode != -1)
                    {
                        if ((nodes[foundNode].transform.position - nodes[i].transform.position).sqrMagnitude > vectorDiff.sqrMagnitude)
                        {
                            adjencyList[i].Remove(foundNode);
                            adjencyList[i].Add(j);
                        }
                    }
                    else
                    {
                        adjencyList[i].Add(j);
                    }
                }
            }
        }
    }

    private void SpawnArtificialNodes()
    {
        var packPos = GameManager.instance.pacman.transform.position;
        packPos.z = 0;
        spawnedArtificialNodes.Add(
            Instantiate(nodePrefab, packPos, Quaternion.identity)
                .GetComponent<Node>());
        foreach (var instanceGhost in GameManager.instance.ghosts)
        {
            var ghostPos = instanceGhost.transform.position;
            packPos.z = 0;
            spawnedArtificialNodes.Add(Instantiate(nodePrefab, ghostPos, Quaternion.identity).GetComponent<Node>());
        }

        foreach (var spawnedArtificialNode in spawnedArtificialNodes)
        {
            spawnedArtificialNode.GetComponent<Collider2D>().enabled = false;
            spawnedArtificialNode.isArtificial = true;
            spawnedArtificialNode.UpdateAvailableDirections();
        }
    }

    private void ResetArtificialNodes()
    {
        foreach (var spawnedArtificialNode in spawnedArtificialNodes)
        {
            spawnedArtificialNode.isDestroyed = true;
            Destroy(spawnedArtificialNode.gameObject);
        }

        spawnedArtificialNodes.Clear();
    }

    public List<int> AStar(int startNode, int targetNode)
    {
        List<int> openList = new List<int>();
        List<int> closedList = new List<int>();
        List<int> cameFrom = new List<int>();
        List<float> gScore = new List<float>();
        List<float> fScore = new List<float>();
        
        for (int i = 0; i < nodes.Count; i++)
        {
            cameFrom.Add(-1);
            gScore.Add(float.MaxValue);
            fScore.Add(float.MaxValue);
        }
        
        openList.Add(startNode);
        gScore[startNode] = 0;
        fScore[startNode] = HeuristicCostEstimate(startNode, targetNode);
        
        while (openList.Count > 0)
        {
            var current = openList.OrderBy(n => fScore[n]).First();
            if (current == targetNode)
            {
                return ReconstructPath(cameFrom, current);
            }

            openList.Remove(current);
            closedList.Add(current);

            foreach (var neighbor in adjencyList[current])
            {
                if (closedList.Contains(neighbor)) continue;
                var tentativeGScore = gScore[current] + (nodes[current].transform.position - nodes[neighbor].transform.position).sqrMagnitude;
                if (!openList.Contains(neighbor)) openList.Add(neighbor);
                else if (tentativeGScore >= gScore[neighbor]) continue;
                
                cameFrom[neighbor] = current;
                gScore[neighbor] = tentativeGScore;
                fScore[neighbor] = gScore[neighbor] + HeuristicCostEstimate(neighbor, targetNode);
            }
        }
        
        return null;
    }

    private List<int> ReconstructPath(List<int> cameFrom, int current)
    {
        List<int> totalPath = new List<int>();
        totalPath.Add(current);
        while (cameFrom[current] != -1)
        {
            current = cameFrom[current];
            totalPath.Add(current);
        }
        totalPath.Reverse();
        
        return totalPath;
    }

    private float HeuristicCostEstimate(int startNode, int targetNode)
    {
        return ((Vector2)(nodes[startNode].transform.position - nodes[targetNode].transform.position)).sqrMagnitude;
    }
}