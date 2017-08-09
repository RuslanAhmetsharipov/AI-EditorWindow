using System;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class AttackControl : MonoBehaviour
{
    public List<NPCAttackType> attackType = new List<NPCAttackType>();
    [SerializeField]
    public List<HitDealer> hitDealer = new List<HitDealer>();

    private bool[] isAttackReady;
    private float[] timeOfAttack;

    private MoveControl mc;


    private void Start()
    {
        if (attackType.Count == 0 || hitDealer == null || attackType.Count != hitDealer.Count)
            this.gameObject.SetActive(false);
        isAttackReady = new bool[attackType.Count];
        mc = GetComponent<MoveControl>();
        SortByDamage();
        timeOfAttack = new float[attackType.Count];
        for (int i = 0; i < timeOfAttack.Length; i++)
        {
            timeOfAttack[i] = -100f;
        }
    }

    private void SortByDamage()
    {
        for (int i = 0; i < attackType.Count; i++)
        {
            float maxdmg = attackType[i].damage;
            int num = i;
            for (int j = i + 1; j < attackType.Count; j++)
            {
                if (attackType[j].damage > maxdmg)
                {
                    maxdmg = attackType[j].damage;
                    num = j;
                }
            }
            if (num != i)
            {
                NPCAttackType _at = attackType[i];
                attackType[i] = attackType[num];
                attackType[num] = _at;
            }
        }
    }

    public bool IsReady(int attackIndex)
    {
        if (attackType[attackIndex].CoolDownForAttack < (Time.time - timeOfAttack[attackIndex]))
            isAttackReady[attackIndex] = true;
        else
            isAttackReady[attackIndex] = false;
        return isAttackReady[attackIndex];
    }

    public int ChooseAttack()
    {
        for (int i = 0; i < isAttackReady.Length; i++)
        {
            if (IsReady(i))
                return i;
        }
        return -1;
    }

    public bool Attack()
    {
        int attackIndex = ChooseAttack();
        if (attackIndex != -1)
        {
            hitDealer[attackIndex].SetVariables(attackType[attackIndex].damage, attackType[attackIndex].damageType);
            mc.SetAnimatorState("attack");
            isAttackReady[attackIndex] = false;
            timeOfAttack[attackIndex] = Time.time;
            hitDealer[attackIndex].SetVariables(0f, attackType[attackIndex].damageType);
            return true;
        }
        return false;
    }
}
