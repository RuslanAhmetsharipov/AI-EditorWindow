using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageReceiver : MonoBehaviour {
    private Health hp;
    private void Start()
    {
        if (transform.parent.GetComponent<Health>())
            hp = transform.parent.GetComponent<Health>();
        else
            transform.parent.gameObject.SetActive(false);
    }
    public void ReceiveDamage(int damage)
    {
        hp.ChangeHP(-1*damage);
    }
}
