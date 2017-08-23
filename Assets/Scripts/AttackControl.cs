using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class AttackControl : MonoBehaviour
{
    public List<NPCAttackType> attackType = new List<NPCAttackType>();
    public List<HitDealer> hitDealer = new List<HitDealer>();

    private bool[] isAttackReady;
    private float[] timeOfAttack;

    private MoveControl mc;


    private void Start()
    {
        if (attackType.Count == 0 || attackType.Count != hitDealer.Count)
            this.enabled = false;
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
            if (hitDealer[attackIndex] == null && attackType[attackIndex].attackingObject.GetComponent<HitDealer>() == null)
            {
                Debug.LogError("HitDealer not assigned");
                return false;
            }
            GameObject _fireball;
            if (attackType[attackIndex].isAttackSpawningObject)
            {
                Vector3 initPosition = attackType[attackIndex].startPointOfAttack.position;
                _fireball = Instantiate(attackType[attackIndex].attackingObject, initPosition, Quaternion.identity);
                _fireball.GetComponent<HitDealer>().SetVariables(attackType[attackIndex].damage, attackType[attackIndex].damageType);
                StartCoroutine(MovingAttackObject(_fireball, transform.forward, attackType[attackIndex].speedOfCreatedAttack, attackType[attackIndex].lifetimeOfObject));
            }
            if (hitDealer[attackIndex] != null)
            {
                hitDealer[attackIndex].SetVariables(attackType[attackIndex].damage, attackType[attackIndex].damageType);
            }
            mc.SetAnimatorState("attack");
            isAttackReady[attackIndex] = false;
            timeOfAttack[attackIndex] = Time.time;
            hitDealer[attackIndex].SetVariables(0f, attackType[attackIndex].damageType);
            return true;
        }
        return false;
    }
    private IEnumerator MovingAttackObject(GameObject obj, Vector3 direction, float speed, float lifetime)
    {
        float time = 0f;
        while (time < lifetime)
        {
            obj.transform.position += direction * speed;
            time += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        Destroy(obj);
        yield return null;
    }
}
