using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour {

    public HealthType healthType;
    private int hp;

    public void ChangeHP(int _hp)
    {
        hp += _hp;
        if(hp<=0)
        {
            Die();
        }
    }
    private void Die()
    {
        gameObject.SetActive(false);
    }
}
