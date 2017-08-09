using UnityEngine;
[CreateAssetMenu(fileName = "MovingData", menuName = "Moving", order = 1)]
[System.Serializable]
public class MovingType : ScriptableObject
{
    public float speed = 5f;
    public float timeForIdle = 1f;
    public float changeDirectionPercentage = 5f;
    public float aggressionDistanceInFront = 10f;
    public float closeAggressionDistance = 2f;
}
