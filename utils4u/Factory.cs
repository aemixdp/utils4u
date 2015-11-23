using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Factory))]
public class FactoryEditor : Editor
{
    GameObject _prefab;
    string[][] _componentFieldNames;
    SerializedObject[][] _serializedFieldSettings;

    private struct ComponentMetadata
    {
        public Dictionary<string, int> ComponentIndex;
        public Dictionary<string, int>[] FieldIndex;
        public string[] ComponentNames;
        public string[][] FieldNames;
        public FieldInfo[][] FieldInfos;

        public static ComponentMetadata Extract(GameObject gameObject)
        {
            var meta = new ComponentMetadata();
            var componentTypes = gameObject
                .GetComponents<Component>()
                .Select(c => c.GetType())
                .DistinctBy(t => t.Name)
                .ToArray();
            var nonemptyComponentTypes = new List<Type>(componentTypes.Length / 2);
            var componentFieldInfos = new List<FieldInfo[]>(componentTypes.Length / 2);
            foreach (var type in componentTypes)
            {
                var fieldInfos = type
                    .GetFields(BindingFlags.Instance | BindingFlags.Public)
                    .Where(field => Factory.IsSupportedFieldType(field.FieldType))
                    .ToArray();
                if (fieldInfos.Length > 0)
                {
                    nonemptyComponentTypes.Add(type);
                    componentFieldInfos.Add(fieldInfos);
                }
            }
            int len = nonemptyComponentTypes.Count;
            meta.ComponentIndex = new Dictionary<string, int>(len);
            meta.FieldIndex = new Dictionary<string, int>[len];
            meta.ComponentNames = new string[len];
            meta.FieldNames = new string[len][];
            meta.FieldInfos = new FieldInfo[len][];
            for (int i = 0; i < len; ++i)
            {
                var type = nonemptyComponentTypes[i];
                meta.ComponentIndex[type.Name] = i;
                meta.ComponentNames[i] = type.Name;
                var fieldInfos = componentFieldInfos[i];
                meta.FieldInfos[i] = fieldInfos;
                var fieldNames = fieldInfos.Select(fieldInfo => fieldInfo.Name).ToArray();
                meta.FieldNames[i] = fieldNames;
                var fieldIndex = new Dictionary<string, int>(fieldNames.Length);
                for (int j = 0; j < fieldNames.Length; ++j)
                {
                    fieldIndex[fieldNames[j]] = j;
                }
                meta.FieldIndex[i] = fieldIndex;
            }
            return meta;
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var prefabProp = serializedObject.FindProperty("Prefab");
        EditorGUILayout.PropertyField(prefabProp);
        var prefab = (GameObject)prefabProp.objectReferenceValue;
        var componentSettingsProp = serializedObject.FindProperty("ComponentSettings");
        if (prefab == null)
        {
            componentSettingsProp.ClearArray();
            return;
        }
        var meta = ComponentMetadata.Extract(prefab);
        EditorGUILayout.PropertyField(componentSettingsProp.FindPropertyRelative("Array.size"),
            new GUIContent("Components"));
        for (int i = 0; i < componentSettingsProp.arraySize; ++i)
        {
            EditorGUILayout.Space();
            var componentSettingProp = componentSettingsProp.GetArrayElementAtIndex(i);
            var componentNameProp = componentSettingProp.FindPropertyRelative("ComponentName");
            var componentName = componentNameProp.stringValue;
            if (String.IsNullOrEmpty(componentName))
            {
                componentName = meta.ComponentNames[0];
            }
            int newComponentIndex = EditorGUILayout.Popup("Component Name",
                meta.ComponentIndex[componentName], meta.ComponentNames);
            componentNameProp.stringValue = meta.ComponentNames[newComponentIndex];
            var fieldSettingsProp = componentSettingProp.FindPropertyRelative("FieldSettings");
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(fieldSettingsProp.FindPropertyRelative("Array.size"),
                new GUIContent("Fields"));
            for (int j = 0; j < fieldSettingsProp.arraySize; ++j)
            {
                EditorGUILayout.Space();
                var fieldSettingProp = fieldSettingsProp.GetArrayElementAtIndex(j);
                var fieldNameProp = fieldSettingProp.FindPropertyRelative("FieldName");
                var fieldName = fieldNameProp.stringValue;
                if (String.IsNullOrEmpty(fieldName))
                {
                    fieldName = meta.FieldNames[newComponentIndex][0];
                }
                int newFieldIndex = EditorGUILayout.Popup("Field Name",
                    meta.FieldIndex[newComponentIndex][fieldName],
                    meta.FieldNames[newComponentIndex]);
                fieldNameProp.stringValue = meta.FieldNames[newComponentIndex][newFieldIndex];
                var fieldType = meta.FieldInfos[newComponentIndex][newFieldIndex].FieldType;
                FieldSetting(fieldName, fieldType, fieldSettingProp);
            }
            EditorGUI.indentLevel--;
        }
        serializedObject.ApplyModifiedProperties();
    }

    void FieldSetting(string name, Type type, SerializedProperty prop)
    {
        var settingType = Factory.SettingTypeFor(type);
        prop.FindPropertyRelative("SettingType").enumValueIndex = (int)settingType;
        var minValueProp = prop.FindPropertyRelative("NumericMinValue");
        var maxValueProp = prop.FindPropertyRelative("NumericMaxValue");
        switch (settingType)
        {
            case Factory.SettingType.IntSetting:
                minValueProp.floatValue =
                    EditorGUILayout.IntField(new GUIContent("Min Value"), (int)minValueProp.floatValue);
                maxValueProp.floatValue =
                    EditorGUILayout.IntField(new GUIContent("Max Value"), (int)maxValueProp.floatValue);
                break;
            case Factory.SettingType.FloatSetting:
                minValueProp.floatValue =
                    EditorGUILayout.FloatField(new GUIContent("Min Value"), minValueProp.floatValue);
                maxValueProp.floatValue =
                    EditorGUILayout.FloatField(new GUIContent("Max Value"), maxValueProp.floatValue);
                break;
            case Factory.SettingType.StringSetting:
                var stringValuesProp = prop.FindPropertyRelative("RandomStringValues");
                EditorGUILayout.PropertyField(stringValuesProp.FindPropertyRelative("Array.size"),
                    new GUIContent("Variants"));
                for (int i = 0; i < stringValuesProp.arraySize; ++i)
                {
                    EditorGUILayout.PropertyField(
                        stringValuesProp.GetArrayElementAtIndex(i));
                }
                break;
            case Factory.SettingType.ObjectSetting:
                var objectValuesProp = prop.FindPropertyRelative("RandomObjectValues");
                EditorGUILayout.PropertyField(objectValuesProp.FindPropertyRelative("Array.size"),
                    new GUIContent("Variants"));
                for (int i = 0; i < objectValuesProp.arraySize; ++i)
                {
                    EditorGUILayout.PropertyField(
                        objectValuesProp.GetArrayElementAtIndex(i));
                }
                break;
        }
    }
}
#endif

public class Factory : MonoBehaviour
{
    [Serializable]
    public enum SettingType
    {
        IntSetting,
        FloatSetting,
        StringSetting,
        ObjectSetting
    }

    public static SettingType SettingTypeFor(Type type)
    {
        if (type == typeof(int))
            return SettingType.IntSetting;
        else if (type == typeof(float))
            return SettingType.FloatSetting;
        else if (type == typeof(string))
            return SettingType.StringSetting;
        else if (type.IsSubclassOf(typeof(UnityEngine.Object)))
            return SettingType.ObjectSetting;
        else
            throw new Exception("Unsupported type!");
    }

    public static bool IsSupportedFieldType(Type type)
    {
        return type == typeof(int) || type == typeof(float)
            || type == typeof(string)
            || type.IsSubclassOf(typeof(UnityEngine.Object));
    }

    [Serializable]
    public struct FieldSetting
    {
        public string FieldName;
        public SettingType SettingType;
        public float NumericMinValue;
        public float NumericMaxValue;
        public string[] RandomStringValues;
        public UnityEngine.Object[] RandomObjectValues;
    }

    [Serializable]
    public struct ComponentSetting
    {
        public string ComponentName;
        public FieldSetting[] FieldSettings;
    }

    private struct CachedFieldSetting
    {
        public FieldInfo FieldInfo;
        public FieldSetting FieldSetting;

        public void Apply(object target)
        {
            switch (FieldSetting.SettingType)
            {
                case SettingType.IntSetting:
                    FieldInfo.SetValue(target,
                        UnityEngine.Random.Range(
                            (int)FieldSetting.NumericMinValue,
                            (int)FieldSetting.NumericMaxValue));
                    break;
                case SettingType.FloatSetting:
                    FieldInfo.SetValue(target,
                        UnityEngine.Random.Range(
                            FieldSetting.NumericMinValue,
                            FieldSetting.NumericMaxValue));
                    break;
                case SettingType.StringSetting:
                    FieldInfo.SetValue(target,
                        FieldSetting.RandomStringValues[
                            UnityEngine.Random.Range(0,
                                FieldSetting.RandomStringValues.Length)
                        ]);
                    break;
                case SettingType.ObjectSetting:
                    FieldInfo.SetValue(target,
                        FieldSetting.RandomObjectValues[
                            UnityEngine.Random.Range(0,
                                FieldSetting.RandomObjectValues.Length)
                        ]);
                    break;
            }
        }
    }

    public GameObject Prefab;
    public ComponentSetting[] ComponentSettings;

    Dictionary<string, Type> _componentTypes;
    Dictionary<Type, List<CachedFieldSetting[]>> _fieldSettingsCache;

    public GameObject Instantiate()
    {
        var instance = GameObject.Instantiate(Prefab);
        if (_fieldSettingsCache == null)
        {
            PopulateCache(instance);
        }
        foreach (var kvp in _fieldSettingsCache)
        {
            var type = kvp.Key;
            var components = instance.GetComponents(type);
            for (int i = 0; i < components.Length; ++i)
            {
                var settingsChain = kvp.Value[i];
                foreach (var cachedFieldSetting in settingsChain)
                {
                    cachedFieldSetting.Apply(components[i]);
                }
            }
        }
        return instance;
    }

    void PopulateCache(GameObject instance)
    {
        var components = instance.GetComponents<Component>();
        _componentTypes = new Dictionary<string, Type>();
        foreach (var component in components)
        {
            var type = component.GetType();
            _componentTypes[type.Name] = type;
        }
        _fieldSettingsCache = new Dictionary<Type, List<CachedFieldSetting[]>>();
        foreach (var componentSetting in ComponentSettings)
        {
            var type = _componentTypes[componentSetting.ComponentName];
            var cachedFieldSettings = componentSetting.FieldSettings
                .Select(fieldSetting =>
                    new CachedFieldSetting
                    {
                        FieldInfo = type.GetField(fieldSetting.FieldName),
                        FieldSetting = fieldSetting
                    })
                .ToArray();
            List<CachedFieldSetting[]> chain;
            if (_fieldSettingsCache.TryGetValue(type, out chain))
            {
                chain.Add(cachedFieldSettings);
            }
            else
            {
                chain = new List<CachedFieldSetting[]>();
                chain.Add(cachedFieldSettings);
                _fieldSettingsCache[type] = chain;
            }
        }
    }
}
