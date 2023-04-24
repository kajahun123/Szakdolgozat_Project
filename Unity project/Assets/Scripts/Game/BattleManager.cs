using Assets.Scripts.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public GameManager GM;

    public bool battleStatus = false;

    public float attackTime = 1.0f;

    private float attackTimer = 0.0f;

    public IEnumerator Attack(GameObject unit, GameObject enemy, Action callBack)
    {
        battleStatus = true;
        attackTimer = 0.0f;
        yield return unit.GetComponent<UnitScript>().Rotate(enemy.transform.position);
        //támadás animáció indítás
        unit.GetComponent<UnitScript>().animator.SetTrigger("Attack");
        while (attackTimer <= attackTime)
        {
            attackTimer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        unit.GetComponent<UnitScript>().animator.SetTrigger("EndAttack");
        battleStatus = false;
        Battle(unit, enemy);
        callBack();
    }

    public void Battle(GameObject attacker, GameObject receiver)
    {
        var attackerUnit = attacker.GetComponent<UnitScript>();
        var receiverUnit = receiver.GetComponent<UnitScript>();
        int attackerDmg = attackerUnit.attackDamage;

        receiverUnit.GetDamage(attackerDmg);
        if (checkIfUnitIsDead(receiver))
        {
            receiverUnit.UnitDie();
            GM.cechkIfUnitsRemain(attacker, receiver);
        }
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
