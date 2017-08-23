using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Animations;

[System.Serializable]
public class AttackTypeEditor : EditorWindow
{

    public AttackControl attackType;
    public MoveControl movingType;
    public Health healthType;
    public Animator animator;

    private bool showAttackingObjectMenu = false;
    private bool showAnimatorStates = false;
    private AnimatorController animatorController;
    private AnimatorControllerParameter[] animatorParameters;

    private int listCount = 0;
    private int listCountTemp = 0;
    public Vector2 scrollPos = Vector2.zero;
    [MenuItem("NPCEditors/MonsterEditor")]
    static void Init()
    {
        GetWindow(typeof(AttackTypeEditor));
    }

    private void OnGUI()
    {
        scrollPos = GUI.BeginScrollView(new Rect(0, 0, position.width, position.height), scrollPos, new Rect(0, 0, 300, 690));
        if (Selection.activeGameObject != null)
        {

            if (Selection.activeGameObject.GetComponent<AttackControl>() != null && attackType == null)
            {
                attackType = Selection.activeGameObject.GetComponent<AttackControl>();

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
            }
            if (Selection.activeGameObject.GetComponent<MoveControl>() != null && movingType == null)
            {
                movingType = Selection.activeGameObject.GetComponent<MoveControl>();

            }
            if (Selection.activeGameObject.GetComponent<Health>() != null && healthType == null)
            {
                healthType = Selection.activeGameObject.GetComponent<Health>();
            }
            if (Selection.activeGameObject.GetComponent<Animator>() != null && animator == null)
            {
                animator = Selection.activeGameObject.GetComponent<Animator>();
                int paramsCount = animator.parameterCount;
                animatorParameters = new AnimatorControllerParameter[paramsCount];
                for (int i = 0; i < paramsCount; i++)
                {
                    animatorParameters[i] = animator.GetParameter(i);
                }
                movingType.animator = animator;
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
                        attackType.attackType[i].isRanged = EditorGUILayout.Toggle("Is this attack ranged", attackType.attackType[i].isRanged);
                        attackType.attackType[i].CoolDownForAttack = EditorGUILayout.FloatField("CoolDownForAttack", attackType.attackType[i].CoolDownForAttack);
                        attackType.attackType[i].damageType = (DamageType)EditorGUILayout.EnumPopup("Damage Type", attackType.attackType[i].damageType);
                        attackType.attackType[i].isAttackSpawningObject = EditorGUILayout.Toggle("Is attack spawning object", attackType.attackType[i].isAttackSpawningObject);
                        if (attackType.attackType[i].isAttackSpawningObject)
                        {
                            EditorGUI.indentLevel++;
                            attackType.attackType[i].attackingObject = EditorGUILayout.ObjectField("Created object", attackType.attackType[i].attackingObject, typeof(GameObject), true) as GameObject;
                            showAttackingObjectMenu = EditorGUILayout.Foldout(showAttackingObjectMenu, "Show more info");
                            if (showAttackingObjectMenu && attackType.attackType[i].attackingObject.GetComponent<AttackingObject>() != null)
                            {
                                EditorGUI.indentLevel++;
                                AttackingObject _attackingObject = attackType.attackType[i].attackingObject.GetComponent<AttackingObject>();
                                _attackingObject.animationClip =
                                    EditorGUILayout.ObjectField("Animation when hit", _attackingObject.animationClip, typeof(AnimationClip), false) as AnimationClip;
                                _attackingObject.audioClip =
                                    EditorGUILayout.ObjectField("Audio clip when hit", _attackingObject.audioClip, typeof(AudioClip), false) as AudioClip;
                                EditorGUI.indentLevel--;
                            }
                            attackType.attackType[i].startPointOfAttack = EditorGUILayout.ObjectField("Created object base position", attackType.attackType[i].startPointOfAttack, typeof(Transform), true) as Transform;
                            attackType.attackType[i].speedOfCreatedAttack = EditorGUILayout.FloatField("Speed of created object", attackType.attackType[i].speedOfCreatedAttack);
                            attackType.attackType[i].lifetimeOfObject = EditorGUILayout.FloatField("LifeTime of object", attackType.attackType[i].lifetimeOfObject);
                            if (attackType.attackType[i].attackingObject != null && attackType.attackType[i].attackingObject.GetComponent<HitDealer>() != null)
                                attackType.hitDealer[i] = attackType.attackType[i].attackingObject.GetComponent<HitDealer>();
                            EditorGUI.indentLevel--;
                        }
                        else
                            attackType.hitDealer[i] = (HitDealer)EditorGUILayout.ObjectField("Hit dealer of attack: ", attackType.hitDealer[i], typeof(HitDealer), true);
                        attackType.attackType[i] = EditorGUILayout.ObjectField(attackType.attackType[i], typeof(NPCAttackType), true) as NPCAttackType;
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("Save"))
                        {
                            string path = AssetDatabase.GetAssetPath(attackType.attackType[i]);
                            if (AssetDatabase.Contains(attackType.attackType[i]) && Path.GetFileName(path) == attackType.attackType[i].name)
                                EditorUtility.SetDirty(attackType.attackType[i]);
                            if (path != "")
                            {
                                if (EditorUtility.DisplayDialog("Creating asset error", "Asset that you want to create already exist. Would you like to rename it?", "Yes", "No"))
                                    ScriptableObjectUtility.OverwriteAsset(attackType.attackType[i], attackType.attackType[i].name);
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
                movingType.npcType = (NPCType)EditorGUILayout.EnumPopup("NPC type", movingType.npcType);
                if (movingType.npcType != NPCType.standOnePlaceNPC)
                    movingType.movementMode = (Mode)EditorGUILayout.EnumPopup("Movement mode", movingType.movementMode);
                EditorGUI.indentLevel++;
                if (movingType.movementMode == Mode.RandomMovement)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Change decision percentage");
                    movingType.movingType.changeDirectionPercentage = EditorGUILayout.Slider(movingType.movingType.changeDirectionPercentage, 0f, 100f);
                    GUILayout.EndHorizontal();
                }
                else
                {
                    listCount = movingType.wayPoints.Count;
                    GUILayout.BeginHorizontal();
                    listCountTemp = EditorGUILayout.IntField("Count of waypoints", listCountTemp);
                    if (GUILayout.Button("Apply"))
                    {
                        listCount = listCountTemp;
                    }
                    GUILayout.EndHorizontal();
                    for (int i = 0; i < movingType.wayPoints.Count; i++)
                    {
                        movingType.wayPoints[i] = EditorGUILayout.ObjectField(movingType.wayPoints[i], typeof(Transform), true) as Transform;
                    }
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Add waypoint"))
                    {
                        listCount++;
                        Repaint();
                    }
                    if (GUILayout.Button("Remove waypoint"))
                    {
                        listCount--;
                        Repaint();
                    }
                    while (listCount > movingType.wayPoints.Count)
                    {
                        movingType.wayPoints.Add(null);
                        listCountTemp = listCount;
                    }
                    while (listCount < movingType.wayPoints.Count)
                    {
                        movingType.wayPoints.RemoveAt(movingType.wayPoints.Count - 1);
                        listCountTemp = listCount;
                    }
                    GUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
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
                        movingType.movementMode = Mode.RandomMovement;
                    }
                }
                else
                {
                    movingType.movingType = CreateInstance<MovingType>();
                    movingType.movementMode = Mode.RandomMovement;
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
        GUI.EndScrollView();
    }
    private void OnSelectionChange()
    {
        Repaint();
        attackType = null;
        movingType = null;
        healthType = null;
        animator = null;
        showAttackingObjectMenu = false;
    }
}
