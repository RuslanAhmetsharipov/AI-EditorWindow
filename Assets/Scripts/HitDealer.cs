using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitDealer : MonoBehaviour
{
    [HideInInspector]
    public int damage = 0;
    private void OnCollisionEnter(Collision collision)
    {
        DamageReceiver dr = collision.transform.GetComponent<DamageReceiver>();
        if (dr != null)
            dr.ReceiveDamage(damage);
    }
}
