using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class ParameterData
{
    public string type;
    public string value;
}
[Serializable]
public class PersistentData
{
    public UnityEngine.Object target;   // 目标 GameObject
    public string assemblyTypeName;     // 类名
    public string methodName;           // 方法名
    public ParameterData parameter;

    public BTTargetObject btTargetObject;
    public void SerializeBTTargetObject()
    {
        if (btTargetObject == null) btTargetObject = new BTTargetObject();
        btTargetObject.target = target;

        bool isPrefabStage = IsPrefabStage();
        btTargetObject.pathType = isPrefabStage ? EFindObjPathType.LocalPath : EFindObjPathType.ScenePath;

        btTargetObject.SerializeSelf();
        target = null;
    }
    private bool IsPrefabStage()
    {
        var stage = PrefabStageUtility.GetCurrentPrefabStage();
        return stage != null; // 如果不为空，说明当前是 Prefab 编辑窗口
    }
}

[Serializable]
public class BTTargetEvent
{
    [SerializeField]
    [HideInInspector]
    public PersistentData[] persistentDatas;

    [SerializeField]
    public UnityEvent targetEvent;

    [NonSerialized]
    public BTRuntime runtime;
    public void SetTargetEvent()
    {
        if (targetEvent != null && targetEvent.GetPersistentEventCount() > 0) return;
        if (persistentDatas == null) return;

        targetEvent ??= new UnityEvent();
        targetEvent.RemoveAllListeners();
        for (int i = 0; i < persistentDatas.Length; i++)
        {
            int index = i;
            PersistentData persistentData = persistentDatas[index];
            BTTargetObject btTargetObject = persistentData.btTargetObject;
            if (btTargetObject == null) continue;

            btTargetObject.runtime = runtime;
            btTargetObject.SetObejctByPath();
            persistentData.target = btTargetObject.target;
            IntegrateEventInfo(persistentData);
        }
    }
    public void SerializeSelf()
    {
        if (targetEvent != null && targetEvent.GetPersistentEventCount() > 0)
        {
            int persistentEventCount = targetEvent.GetPersistentEventCount();
            persistentDatas = new PersistentData[persistentEventCount];
            for (int i = 0; i < persistentEventCount; i++)
            {
                int index = i;
                PersistentData persistentData = ExtractEventInfo(index);
                persistentData.SerializeBTTargetObject();
                persistentDatas.SetValue(persistentData, index);
            }
        }
        targetEvent?.RemoveAllListeners();
        targetEvent = null;
    }
    private void IntegrateEventInfo(PersistentData data)
    {
        if (data?.target == null) return;

        GameObject go = data.target.GameObject();
        if (go == null) return;

        object target = typeof(GameObject).FullName == data.assemblyTypeName
            ? go
            : go.GetComponents<Component>()
                .FirstOrDefault(x => x?.GetType().FullName == data.assemblyTypeName);

        if (target == null) return;

        MethodInfo method = target.GetType().GetMethod(
            data.methodName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (method == null || method.ReturnType != typeof(void)) return;

        var ps = method.GetParameters();

        if (ps.Length == 0)
        {
            var action = Delegate.CreateDelegate(typeof(UnityAction), target, method, false) as UnityAction;
            if (action == null) return;

#if UNITY_EDITOR
            UnityEventTools.AddPersistentListener(targetEvent, action);
#else
            targetEvent.AddListener(action);
#endif
            return;
        }

        if (ps.Length != 1 || data.parameter == null) return;

        Type type = ps[0].ParameterType;
        object value = ConvertValue(type, data.parameter.value);

        if (value == null && type.IsValueType) return;

#if UNITY_EDITOR
        AddPersistentListener(targetEvent, target, method, type, value);
#else
        targetEvent.AddListener(() => method.Invoke(target, new[] { value }));
#endif
    }
#if UNITY_EDITOR
    private void AddPersistentListener(UnityEvent evt, object target, MethodInfo method, Type type, object value)
    {
        if (type == typeof(int))
            UnityEventTools.AddIntPersistentListener(evt, (UnityAction<int>)Delegate.CreateDelegate(typeof(UnityAction<int>), target, method), (int)value);
        else if (type == typeof(float))
            UnityEventTools.AddFloatPersistentListener(evt, (UnityAction<float>)Delegate.CreateDelegate(typeof(UnityAction<float>), target, method), (float)value);
        else if (type == typeof(string))
            UnityEventTools.AddStringPersistentListener(evt, (UnityAction<string>)Delegate.CreateDelegate(typeof(UnityAction<string>), target, method), (string)value);
        else if (type == typeof(bool))
            UnityEventTools.AddBoolPersistentListener(evt, (UnityAction<bool>)Delegate.CreateDelegate(typeof(UnityAction<bool>), target, method), (bool)value);
        else if (typeof(UnityEngine.Object).IsAssignableFrom(type))
        {
            var actionType = typeof(UnityAction<>).MakeGenericType(type);
            var action = Delegate.CreateDelegate(actionType, target, method);

            var addMethod = typeof(UnityEventTools)
                .GetMethod("AddObjectPersistentListener")
                ?.MakeGenericMethod(type);

            addMethod?.Invoke(null, new[] { evt, action, value });
        }
    }
#endif
    private object ConvertValue(Type type, string value)
    {
        if (string.IsNullOrEmpty(value)) return type.IsValueType ? Activator.CreateInstance(type) : null;
        if (type == typeof(int)) return int.Parse(value);
        if (type == typeof(float)) return float.Parse(value);
        if (type == typeof(string)) return value;
        if (type == typeof(bool)) return bool.Parse(value);
        if (type.IsEnum) return Enum.Parse(type, value);
        if (type == typeof(GameObject)) return GameObject.Find(value);
        if (type == typeof(Transform)) return GameObject.Find(value)?.transform;
        if (typeof(UnityEngine.Object).IsAssignableFrom(type)) return GameObject.Find(value)?.GetComponent(type);
        return null;
    }
    private PersistentData ExtractEventInfo(int index)
    {
        if (targetEvent == null) return null;
        if (index < 0 || index >= targetEvent.GetPersistentEventCount()) return null;

        var target = targetEvent.GetPersistentTarget(index);
        if (target == null) return null;

        string assemblyTypeName = target.GetType().FullName;
        var methodName = targetEvent.GetPersistentMethodName(index);

        var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
        if (method == null) return null;

        var parameterInfos = method.GetParameters();

        return new PersistentData
        {
            target = target,
            methodName = methodName,
            assemblyTypeName = assemblyTypeName,

            parameter = GetPersistentArgument(index)
        };
    }

    private ParameterData GetPersistentArgument(int index)
    {
        // 获取 UnityEventBase 内部的 m_PersistentCalls
        var baseType = typeof(UnityEventBase); // UnityEventBase 是 UnityEvent 的基类
        var callsField = baseType.GetField("m_PersistentCalls", BindingFlags.NonPublic | BindingFlags.Instance);
        var persistentCalls = callsField.GetValue(targetEvent);

        // 获取 PersistentCallGroup 中的 m_Calls 列表
        var callListType = persistentCalls.GetType();
        var callsFieldInGroup = callListType.GetField("m_Calls", BindingFlags.NonPublic | BindingFlags.Instance);

        // 使用反射获取 List<PersistentCall>
        if (callsFieldInGroup.GetValue(persistentCalls) is not IList calls || index >= calls.Count) return null;

        var call = calls[index];
        var callType = call.GetType();
        var fieldInfo = callType.GetField("m_Arguments", BindingFlags.NonPublic | BindingFlags.Instance);

        var argumentCache = fieldInfo.GetValue(call);
        Type type = argumentCache.GetType();

        ParameterData result = new();

        var intField = type.GetField("m_IntArgument", BindingFlags.Instance | BindingFlags.NonPublic);
        if (intField != null)
        {
            int value = (int)intField.GetValue(argumentCache);
            result.type = typeof(int).AssemblyQualifiedName;
            result.value = value.ToString();
        }

        var floatField = type.GetField("m_FloatArgument", BindingFlags.Instance | BindingFlags.NonPublic);
        if (floatField != null)
        {
            float value = (float)floatField.GetValue(argumentCache);
            result.type = typeof(float).AssemblyQualifiedName;
            result.value = value.ToString();
        }


        var stringField = type.GetField("m_StringArgument", BindingFlags.Instance | BindingFlags.NonPublic);
        if (stringField != null)
        {
            string value = (string)stringField.GetValue(argumentCache);
            result.type = typeof(string).AssemblyQualifiedName;
            result.value = value;
        }

        var boolField = type.GetField("m_BoolArgument", BindingFlags.Instance | BindingFlags.NonPublic);
        if (boolField != null)
        {
            bool value = (bool)boolField.GetValue(argumentCache);
            result.type = typeof(bool).AssemblyQualifiedName;
            result.value = value.ToString();
        }

        var objectField = type.GetField("m_ObjectArgument", BindingFlags.Instance | BindingFlags.NonPublic);
        if (objectField != null)
        {
            UnityEngine.Object value = objectField.GetValue(argumentCache) as UnityEngine.Object;
            result.type = value != null ? value.GetType().FullName : typeof(UnityEngine.Object).FullName;
            result.value = value != null ? GetScenePath(value) : null;
        }

        return result;
    }
    private string GetScenePath(UnityEngine.Object obj)
    {
        GameObject gameObject = null;
        if (obj is GameObject) gameObject = obj as GameObject;
        else if (obj is Component) gameObject = (obj as Component).gameObject;

        if (gameObject == null) return string.Empty;

        string path = gameObject.name;
        Transform current = gameObject.transform;

        while (current.parent != null)
        {
            current = current.parent;
            path = current.name + "/" + path;
        }

        return path;
    }
}
