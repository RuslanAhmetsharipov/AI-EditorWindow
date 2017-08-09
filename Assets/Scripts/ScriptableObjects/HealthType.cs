using UnityEngine;

[CreateAssetMenu(fileName = "HealthType", menuName = "HealthType", order = 1)]
[System.Serializable]
public class HealthType : ScriptableObject
{

    public int health = 1;
    public int armor = 0;

}
