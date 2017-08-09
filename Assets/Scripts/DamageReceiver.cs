using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Collider))]
public class DamageReceiver : MonoBehaviour
{
    [Range(0f, 100f)]
    public float percentageOfReceivingDamage;

    private Health hp;
    private void Start()
    {
        if (transform.parent.GetComponent<Health>())
            hp = transform.parent.GetComponent<Health>();
        else
            transform.parent.gameObject.SetActive(false);
    }
    public void ReceiveDamage(float damage, DamageType damageType)
    {
        hp.ReceiveDamage(damage * percentageOfReceivingDamage, damageType);
    }
}
