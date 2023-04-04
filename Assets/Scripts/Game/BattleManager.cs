using Assets.Scripts.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public GameManager GM;

    private bool battleStatus = false;

    //attacker: támadó fél, receiver:védekezõ fél
    public IEnumerator Attack(GameObject unit, GameObject enemy)
    {
        battleStatus = true;

        //Támadás animációk
        while (battleStatus)
        {
            //Damage kiírás animációk
            Battle(unit, enemy);
            yield return new WaitForEndOfFrame();
        }
    }

    public void Battle(GameObject attacker, GameObject receiver)
    {
        battleStatus = true;
        var attackerUnit = attacker.GetComponent<UnitScript>();
        var receiverUnit = receiver.GetComponent<UnitScript>();
        int attackerDmg = attackerUnit.attackDamage;

        receiverUnit.GetDamage(attackerDmg);
        if (checkIfUnitIsDead(receiver))
        {
            //receiver.transform.parent = null;
            
            receiverUnit.UnitDie();
            battleStatus = false;
            GM.cechkIfUnitsRemain(attacker, receiver);
            return;
        }
        battleStatus = false;
    }

    public void VirtualAttack(GameObject unit, GameObject enemy)
    {
        
        var attackerUnit = unit.GetComponent<UnitScript>();
        var receiverUnit = enemy.GetComponent<UnitScript>();
        int attackerDmg = attackerUnit.attackDamage;

        UnitState lastEnemyState = receiverUnit.states.Peek();
        UnitState newEnemyState = new UnitState();
        newEnemyState.healthPoint = Mathf.Max(0, lastEnemyState.healthPoint - attackerDmg);
        receiverUnit.states.Push(newEnemyState);
    }

    public bool checkIfUnitIsDead(GameObject unit)
    {
        if (unit.GetComponent<UnitScript>().currentHealthPoints <= 0)
        {
            return true;
        }
        return false;
    }
}
