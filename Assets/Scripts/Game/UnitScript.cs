using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitScript : MonoBehaviour
{
    public int team;
    public int x;
    public int y;

    public float visualMovementSpeed = .15f;

    public GameObject tileBeingOccupied;

    public string UnitName;
    public int moveSpeed = 2;
    public int attackRange = 1;
    public int attackDamage = 1;
    public int maxHealthPoints = 5;
    public int currentHealthPoints;
    public Sprite unitSprite;

    public bool isDead = false;
    public bool isTurn = false;
    public List<Node> path = null;

    public Queue<int> movementQueue;

    public TileMap map;

    public enum movementStates
    {
        Unselected,
        Selected,
        Moved
    }

    public movementStates unitMovementState;

    private void Awake()
    {
        x = (int)transform.position.x;
        y = (int)transform.position.y;

        movementQueue = new Queue<int>();
        unitMovementState = movementStates.Unselected;
        currentHealthPoints = maxHealthPoints;
    }

    public movementStates GetMovementState(int i)
    {
        if (i == 0)
        {
            return movementStates.Unselected;
        }
        if(i == 1)
        {
            return movementStates.Selected;
        }
        if(i == 2)
        {
            return movementStates.Moved;
        }
        return movementStates.Unselected;
    }

    public void SetMovementState(int i)
    {
        if (i == 0)
        {
            unitMovementState = movementStates.Unselected;
        }
        if (i == 1)
        {
           unitMovementState =  movementStates.Selected;
        }
        if (i == 2)
        {
            unitMovementState = movementStates.Moved;
        }
    }

    public void MoveToNextTile()
    {
        Debug.Log(path.Count);
        if(path.Count == 0)
        {
            return;
        }
        else
        {
            StartCoroutine(MoveOverSeconds(transform.gameObject, path[path.Count - 1]));
        }
    }

    public IEnumerator MoveOverSeconds(GameObject objectToMove, Node endNode)
    {
        movementQueue.Enqueue(1);
        path.RemoveAt(0);
        while (path.Count != 0)
        {
            Vector3 endPos = map.tileCoordinateToWorldCoordinate(path[0].x, path[0].y);
            objectToMove.transform.position = Vector3.Lerp(transform.position, endPos, visualMovementSpeed);
            if((transform.position - endPos).sqrMagnitude < 0.001)
            {
                path.RemoveAt(0);
            }
            yield return new WaitForEndOfFrame();
        }
        visualMovementSpeed = 0.15f;
        transform.position = map.tileCoordinateToWorldCoordinate(endNode.x, endNode.y);

        x = endNode.x;
        y = endNode.y;
        tileBeingOccupied.GetComponent<ClickableTile>().unitOnTile = null;
        tileBeingOccupied = map.tilesOnMap[x, y];
        movementQueue.Dequeue();
    }
}
