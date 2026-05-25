
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

[Serializable]
public class ParameterRelayState : BehaviorTreeBaseState
{
    #region AutoContext
    
public System.String stringValue;
public System.String parameterName;

    public override BTStateObject stateObj 
    {
         get 
        {
            if (_stateObj == null)
            {
                _stateObj = ScriptableObject.CreateInstance<ParameterRelayStateObj>();
                _stateObj.state = state;
                _stateObj.output = output;
                _stateObj.interruptible = interruptible;
                _stateObj.interruptTag = interruptTag;
                
_stateObj.stringValue = stringValue;
_stateObj.parameterName = parameterName;
            }
            return _stateObj;
        }
    }
    private ParameterRelayStateObj _stateObj;
    public override void InitParam(string param)
    {
        base.InitParam(param);
        DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(ParameterRelayStateObj));
        using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(param)))
        {
            _stateObj = ScriptableObject.CreateInstance<ParameterRelayStateObj>();
            var json = new StreamReader(stream).ReadToEnd();
            JsonUtility.FromJsonOverwrite(json, _stateObj);

            output = _stateObj.output;
            interruptible = _stateObj.interruptible;
            interruptTag = _stateObj.interruptTag;
            
stringValue = _stateObj.stringValue;
parameterName = _stateObj.parameterName;
        }
    }
     protected override ESetFieldValueResult SetFieldValue(string fieldName, object value)
    {
        if (StringComparer.Ordinal.Equals(fieldName, default)) return ESetFieldValueResult.Succ;
        
else if (StringComparer.Ordinal.Equals(fieldName, "stringValue") && value is System.String stringValueValue) stringValue = stringValueValue;
else if (StringComparer.Ordinal.Equals(fieldName, "parameterName") && value is System.String parameterNameValue) parameterName = parameterNameValue;
        else if (StringComparer.Ordinal.Equals(fieldName, "pointerEventData") && value is PointerEventData PointerEventDataValue) pointerEventData = PointerEventDataValue;
        else return ESetFieldValueResult.Fail;

        return ESetFieldValueResult.Succ;
    }
    public override void Save()
    {
        if (stateObj == null) return;
        output = _stateObj.output;
        interruptible = _stateObj.interruptible;
        interruptTag = _stateObj.interruptTag;
        
stringValue = _stateObj.stringValue;
parameterName = _stateObj.parameterName;
    }
    #endregion

    public object Value { set { this.value = value; } }

    private object value;
    private object lastValue;
    public override void OnInitFinish()
    {
        base.OnInitFinish();
        for (int i = 0; i < output.Count; i++)
        {
            BTOutputInfo info = output[i];
            runtime.stateDic[info.nodeId].GetMemberValue(info.toPortName,out value);
        }
    }
    public override void OnEnter()
    {
        base.OnEnter();

        bool isCanExecute = value != null && !value.Equals(lastValue) && runtime != null;
        if (isCanExecute) OnExecute();
        else OnExit();
    }
    public override void OnExecute()
    {
        for (int i = 0; i < output.Count; i++)
        {
            BTOutputInfo info = output[i];
            runtime.stateDic[info.nodeId].SetMemberValue(info.toPortName, value);
        }
        OnExit();
    }
}

#region AutoContext_BTStateObject
public class ParameterRelayStateObj : BTStateObject
{
    public EBTState state;
    
public System.String stringValue;
public System.String parameterName;
}
#endregion
