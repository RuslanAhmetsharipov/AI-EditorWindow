using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitDealer : MonoBehaviour
{
    public float damage = 0f;
    private DamageType damageType = DamageType.HealthAndArmor;
    public void SetVariables(float _damage, DamageType _damageType)
    {
        damage = _damage;
        damageType = _damageType;
    }
    private void OnCollisionEnter(Collision collision)
    {
        DamageReceiver dr = collision.transform.GetComponent<DamageReceiver>();
        if (dr != null)
            dr.ReceiveDamage(damage, damageType);
    }
}
