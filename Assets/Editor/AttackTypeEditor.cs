using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class AttackTypeEditor : EditorWindow
{

    public AttackControl attackType;
    public MoveControl movingType;
    public Health healthType;

    [MenuItem("NPCEditors/MonsterEditor")]
    static void Init()
    {
        GetWindow(typeof(AttackTypeEditor));
    }

    private void OnGUI()
    {
        if (Selection.activeGameObject != null)
        {

            if (Selection.activeGameObject.GetComponent<AttackControl>() != null && attackType == null)
            {
                attackType = Selection.activeGameObject.GetComponent<AttackControl>();
            }
            if (attackType.attackType == null || attackType.attackType.Count != attackType.hitDealer.Count)
            {
                if (attackType.attackType != null && attackType.attackType.Count > attackType.hitDealer.Count)
                {
                    int i = attackType.attackType.Count - 1;
                    while (attackType.attackType.Count != attackType.hitDealer.Count) { attackType.attackType.RemoveAt(i); i--; }
                }
                else
                {
                    int i = attackType.hitDealer.Count - 1;
                    while (attackType.attackType.Count != attackType.hitDealer.Count) { attackType.hitDealer.RemoveAt(i); i--; }
                }
            }
            if (Selection.activeGameObject.GetComponent<MoveControl>() != null && movingType == null)
            {
                movingType = Selection.activeGameObject.GetComponent<MoveControl>();

            }
            if (Selection.activeGameObject.GetComponent<Health>() != null && healthType == null)
            {
                healthType = Selection.activeGameObject.GetComponent<Health>();
            }
            if (attackType != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Attack type editor", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                for (int i = 0; i < attackType.attackType.Count;)
                {
                    if (attackType.attackType[i] != null)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Name of asset");
                        attackType.attackType[i].name = GUILayout.TextField(attackType.attackType[i].name);
                        GUILayout.EndHorizontal();
                        attackType.attackType[i].damage = EditorGUILayout.FloatField("Damage", attackType.attackType[i].damage);
                        attackType.attackType[i].range = EditorGUILayout.FloatField("Range of attacks", attackType.attackType[i].range);
                        attackType.attackType[i].CoolDownForAttack = EditorGUILayout.FloatField("CoolDownForAttack", attackType.attackType[i].CoolDownForAttack);
                        attackType.attackType[i].damageType =(DamageType)EditorGUILayout.EnumPopup(attackType.attackType[i].damageType);
                        attackType.hitDealer[i] = (HitDealer)EditorGUILayout.ObjectField("Hit dealer of attack: ", attackType.hitDealer[i], typeof(HitDealer), true);
                        attackType.attackType[i] = EditorGUILayout.ObjectField(attackType.attackType[i], typeof(NPCAttackType), true) as NPCAttackType;
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("Save"))
                        {
                            string path = AssetDatabase.GetAssetPath(attackType.attackType[i]);
                            if (AssetDatabase.Contains(attackType.attackType[i])&&Path.GetFileName(path)==attackType.attackType[i].name)
                                EditorUtility.SetDirty(attackType.attackType[i]);
                            if (path != "")
                            {
                                if (EditorUtility.DisplayDialog("Creating asset error", "Asset that you want to create already exist. Would you like to rename it?", "Yes", "No"))
                                    ScriptableObjectUtility.OverwriteAsset<NPCAttackType>(attackType.attackType[i], attackType.attackType[i].name);
                            }
                            else
                            {
                                ScriptableObjectUtility.CreateAsset(attackType.attackType[i], attackType.attackType[i].name, "attack");
                            }
                        }
                        if (GUILayout.Button("Remove"))
                        {
                            attackType.attackType.RemoveAt(i);
                            Repaint();
                        }
                        else
                        {
                            i++;
                        }
                        GUILayout.EndHorizontal();
                    }
                    else
                    {
                        attackType.attackType.RemoveAt(i);
                    }
                }
                GUILayout.Space(15);
            }
            if (GUILayout.Button("Add skill"))
            {
                if (attackType == null)
                {
                    attackType = Selection.activeGameObject.AddComponent<AttackControl>();
                }
                attackType.attackType.Add(CreateInstance<NPCAttackType>());
                attackType.hitDealer.Add(new HitDealer());
            }
            GUILayout.Space(35);
            if (movingType != null && movingType.movingType != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Moving type editor", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Name of asset");
                movingType.movingType.name = GUILayout.TextField(movingType.movingType.name);
                GUILayout.EndHorizontal();
                movingType.movingType.speed = EditorGUILayout.FloatField("Moving Speed", movingType.movingType.speed);
                movingType.movingType.timeForIdle = EditorGUILayout.FloatField("Time For Idle", movingType.movingType.timeForIdle);
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Distance For Agression In Front");
                movingType.movingType.aggressionDistanceInFront = EditorGUILayout.FloatField(movingType.movingType.aggressionDistanceInFront);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Radius for aggresion in anyway");
                movingType.movingType.closeAggressionDistance = EditorGUILayout.FloatField(movingType.movingType.closeAggressionDistance);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Change decision percentage");
                movingType.movingType.changeDirectionPercentage = EditorGUILayout.Slider(movingType.movingType.changeDirectionPercentage, 0f, 100f);
                GUILayout.EndHorizontal();
                movingType.movingType = EditorGUILayout.ObjectField(movingType.movingType, typeof(MovingType), true) as MovingType;
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Save"))
                {
                    string path = AssetDatabase.GetAssetPath(movingType.movingType);
                    if (path != "")
                    {
                        if (EditorUtility.DisplayDialog("Creating asset error", "Asset that you want to create already exist. Would you like to rename it?", "Yes", "No"))
                            ScriptableObjectUtility.OverwriteAsset(movingType.movingType, movingType.movingType.name);
                    }
                    else
                    {
                        if (movingType.name != "")
                            ScriptableObjectUtility.CreateAsset(movingType.movingType, movingType.movingType.name, "moving");
                        else
                            EditorUtility.DisplayDialog("Wrong name", "Name of asset is missing", "OK");
                    }
                }
                if (GUILayout.Button("Remove"))
                {
                    movingType = null;
                    DestroyImmediate(Selection.activeGameObject.GetComponent<MoveControl>());
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                if (movingType == null)
                {
                    if (GUILayout.Button("Add AI moving"))
                    {
                        movingType = Selection.activeGameObject.AddComponent<MoveControl>();
                        movingType.movingType = CreateInstance<MovingType>();
                        movingType.movementMode = MoveControl.Mode.RandomPointInFOW;
                    }
                }
                else
                {
                    movingType.movingType = CreateInstance<MovingType>();
                    movingType.movementMode = MoveControl.Mode.RandomPointInFOW;
                }
            }
            GUILayout.Space(35);
            if (healthType != null && healthType.healthType != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Health type editor", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Name of asset");
                healthType.healthType.name = GUILayout.TextField(healthType.healthType.name);
                GUILayout.EndHorizontal();
                healthType.healthType.health = EditorGUILayout.IntField("Health", healthType.healthType.health);
                healthType.healthType.armor = EditorGUILayout.IntField("Armor", healthType.healthType.armor);
                healthType.healthType = EditorGUILayout.ObjectField(healthType.healthType, typeof(HealthType), true) as HealthType;
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Save"))
                {
                    string path = AssetDatabase.GetAssetPath(healthType);
                    if (path != "")
                    {
                        if (EditorUtility.DisplayDialog("Creating asset error", "Asset that you want to create already exist. Would you like to rename it?", "Yes", "No"))
                            ScriptableObjectUtility.OverwriteAsset(healthType.healthType, healthType.healthType.name);
                    }
                    else
                    {
                        if (healthType.name != "")
                            ScriptableObjectUtility.CreateAsset(healthType.healthType, healthType.healthType.name, "health");
                        else
                            EditorUtility.DisplayDialog("Wrong name", "Name of asset is missing", "OK");
                    }
                }
                if (GUILayout.Button("Remove"))
                {
                    healthType = null;
                    DestroyImmediate(Selection.activeGameObject.GetComponent<Health>());
                    //Repaint();
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                if (GUILayout.Button("Add health to object"))
                {
                    if (healthType == null)
                    {
                        healthType = Selection.activeGameObject.AddComponent<Health>();
                        healthType.healthType = CreateInstance<HealthType>();
                        //Repaint();
                    }
                    else
                    {
                        healthType.healthType = CreateInstance<HealthType>();
                        Debug.Log(healthType.healthType.GetType());
                        //Repaint();
                    }
                }
            }
        }
    }
    private void OnSelectionChange()
    {
        Repaint();
        attackType = null;
        movingType = null;
        healthType = null;
    }
}
