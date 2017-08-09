using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{

    public HealthType healthType;
    private float hp;
    private float armor;
    private void Start()
    {
        hp = healthType.health;
        armor = healthType.armor;
    }
    public void ReceiveDamage(float _dmg, DamageType damageType)
    {
        switch (damageType)
        {
            case DamageType.HealthAndArmor:
                if (armor > 0)
                {
                    armor -= _dmg;
                    if (armor < 0)
                    {
                        hp -= armor;
                        armor = 0;
                    }
                }
                break;
            case DamageType.OnlyArmor:
                armor -= _dmg;
                if (armor < 0)
                    armor = 0;
                break;
            case DamageType.OnlyHealth:
                hp -= _dmg;
                break;
        }
        if (hp <= 0)
            Die();
    }

    public void GainArmor(float amount)
    {
        armor += amount;
    }

    public void Heal(float amount)
    {
        hp += amount;
        if (hp > healthType.health)
            hp = healthType.health;
    }

    private void Die()
    {
        gameObject.SetActive(false);
    }
}
