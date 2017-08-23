using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Collider))]
public class DamageReceiver : MonoBehaviour
{
    [Range(0f, 100f)]
    public float percentageOfReceivingDamage;

    private Health hp;
    private AttackControl _ac;
    private MoveControl _mc;
    private void Start()
    {
        if (transform.parent != null)
        {
            if (GetComponentInParent<Health>())
            {
                hp = transform.parent.GetComponent<Health>();
            }
            if (GetComponentInParent<AttackControl>())
            {
                _ac = GetComponentInParent<AttackControl>();
            }
            if (GetComponentInParent<MoveControl>())
            {
                _mc = GetComponentInParent<MoveControl>();
            }
        }
        else
        {
            if (GetComponent<Health>())
                hp = GetComponent<Health>();
            if (GetComponent<AttackControl>())
                _ac = GetComponent<AttackControl>();
            if (GetComponent<MoveControl>())
                _mc = GetComponent<MoveControl>();
        }
        if (!hp && !_ac && !_mc)
            enabled = false;

    }
    public void ReceiveDamage(float damage, DamageType damageType)
    {
        hp.ReceiveDamage(damage * percentageOfReceivingDamage / 100f, damageType);
        if (!_mc.targetDefined)
            _mc.targetDefined = true;
    }
}
