using UnityEngine;

[CreateAssetMenu(fileName = "NPCAttackType", menuName = "NPCAttackType", order = 1)]
[System.Serializable]
public class NPCAttackType : ScriptableObject
{
    public string id = string.Empty;
    public int damage = 5;
    public float CoolDownForAttack = 1f;
    public float range = 1f;

    private void Start()
    {
        this.hideFlags = HideFlags.DontUnloadUnusedAsset;
    }
}
