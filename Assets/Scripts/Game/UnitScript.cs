using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("UI Elements")]
    public Canvas healthBarCanvas;
    public TMP_Text hitPointsText;
    public Image healthBar;

    //Ne léphessen egyszerre több egység
    public Queue<int> movementQueue;
    public Queue<int> combatQueue;

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
        y = (int)transform.position.z;

        movementQueue = new Queue<int>();
        combatQueue = new Queue<int>();
        unitMovementState = movementStates.Unselected;
        currentHealthPoints = maxHealthPoints;
        hitPointsText.SetText(currentHealthPoints.ToString());
    }

    public void LateUpdate()
    {
        healthBarCanvas.transform.forward = Camera.main.transform.forward;
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

    public void GetDamage(int damage)
    {
        currentHealthPoints -= damage;
        updateHealthUI();
    }

    public void UnitDie()
    {
       StartCoroutine(CechkIfRoutinesRunning());
    }

    public IEnumerator CechkIfRoutinesRunning()
    {
        while (combatQueue.Count > 0)
        {
            yield return new WaitForEndOfFrame();
        }
        isDead = true;
        hideUnit();
        GameObject tile = gameObject.GetComponent<UnitScript>().tileBeingOccupied;
        tile.GetComponent<ClickableTile>().unitOnTile = null;
        gameObject.GetComponent<UnitScript>().tileBeingOccupied = null;
        //Destroy(gameObject);
    }

    public void updateHealthUI()
    {
        healthBar.fillAmount = (float)currentHealthPoints / maxHealthPoints;
        hitPointsText.SetText(currentHealthPoints.ToString());
    }

    public void hideUnit()
    {
        foreach (Transform child in transform)
            child.gameObject.SetActive(false);
    }
}
