using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CardData))]
public class CardDataEditor : Editor
{
    private SerializedProperty effectsProp;
    private List<Type> effectTypes;
    private string[] effectTypeNames;
    private int addIndex = 0;

    private void OnEnable()
    {
        effectsProp = serializedObject.FindProperty("Effects");
        // 找出所有實作 ICardEffect 的類型
        effectTypes = GetAllEffectTypes();
        effectTypeNames = effectTypes.Select(t => t.FullName).ToArray();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 繪製其他欄位（Name, Cost, Description 等）
        DrawDefaultInspectorExcept("Effects");

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Effects", EditorStyles.boldLabel);

        if (effectsProp == null)
        {
            EditorGUILayout.HelpBox("Effects property not found.", MessageType.Warning);
            return;
        }

        // 列出現有的效果
        for (int i = 0; i < effectsProp.arraySize; i++)
        {
            var elem = effectsProp.GetArrayElementAtIndex(i);
            EditorGUILayout.BeginVertical(GUI.skin.box);

            // 顯示目前的型別名稱
            Type currentType = elem.managedReferenceValue?.GetType();
            string typeLabel = currentType != null ? currentType.FullName : "Null";
            EditorGUILayout.LabelField($"[{i}] {typeLabel}", EditorStyles.boldLabel);

            // 顯示欄位內容
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(elem, new GUIContent(""), true);
            EditorGUI.indentLevel--;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Remove"))
            {
                effectsProp.DeleteArrayElementAtIndex(i);
                break; // 修改集合，退出 loop
            }

            // 換型別的下拉選單
            if (effectTypes.Count > 0)
            {
                int currentIdx = currentType != null ? effectTypes.IndexOf(effectTypes.FirstOrDefault(t => t == currentType)) : -1;
                int sel = EditorGUILayout.Popup(currentIdx, effectTypeNames);
                if (sel >= 0 && sel < effectTypes.Count && effectTypes[sel] != currentType)
                {
                    // 以新的型別建立實例並指派
                    object instance = Activator.CreateInstance(effectTypes[sel]);
                    elem.managedReferenceValue = instance;
                    serializedObject.ApplyModifiedProperties();
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (effectTypes.Count == 0)
        {
            EditorGUILayout.HelpBox("No ICardEffect implementations found in the project.", MessageType.Info);
        }
        else
        {
            addIndex = EditorGUILayout.Popup(addIndex, effectTypeNames);
            if (GUILayout.Button("Add Effect"))
            {
                Type t = effectTypes[Mathf.Clamp(addIndex, 0, effectTypes.Count - 1)];
                effectsProp.arraySize++;
                var newElem = effectsProp.GetArrayElementAtIndex(effectsProp.arraySize - 1);
                newElem.managedReferenceValue = Activator.CreateInstance(t);
            }
        }
        EditorGUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }

    private List<Type> GetAllEffectTypes()
    {
        var list = new List<Type>();
        // 搜尋所有載入的 assemblies
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var asm in assemblies)
        {
            Type[] types = null;
            try { types = asm.GetTypes(); } catch { continue; }
            foreach (var t in types)
            {
                if (t == null) continue;
                if (t.IsAbstract) continue;
                if (typeof(ICardEffect).IsAssignableFrom(t))
                {
                    // 只加入可以被序列化的類別
                    list.Add(t);
                }
            }
        }
        // 以名稱排序
        list.Sort((a, b) => String.Compare(a.FullName, b.FullName, StringComparison.Ordinal));
        return list;
    }

    private void DrawDefaultInspectorExcept(string propertyNameToSkip)
    {
        SerializedProperty prop = serializedObject.GetIterator();
        bool enterChildren = true;
        while (prop.NextVisible(enterChildren))
        {
            enterChildren = false;
            if (prop.name == propertyNameToSkip) continue;
            EditorGUILayout.PropertyField(prop, true);
        }
    }
}
