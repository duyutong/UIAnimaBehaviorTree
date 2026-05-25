using UnityEngine;

public class BTRuntimeComponent : MonoBehaviour
{
    public BTContainer container;
    public BTRuntime runtime;

    private bool isEnable = false;
    public bool IsEnable { get { return isEnable; }private set { isEnable = value; } }
    public void OnEnable()
    {
        InitRuntime();
        runtime?.OnEnable();
    }
   
    public void SetEnable(bool value)
    {
        if (isEnable == value) return;
        isEnable = value;
        if (isEnable) runtime?.OnEnable();
        else runtime?.OnDisable();
    }
    public void InitRuntime()
    {
        runtime ??= new BTRuntime();
        runtime.container = container;
        runtime.gameObject = gameObject;
        runtime.transform = transform;
    }
    public void SetParameter(string paramName,object paramValue) 
    {
        var raleys = runtime?.GetParamRelays(paramName);
        if (raleys == null) return;

        foreach(var raley in raleys) raley.Value = paramValue;
    }

    private void Update()
    {
        runtime?.OnUpdate();
    }
    public void OnDisable()
    {
        runtime?.OnDisable();
    }
    public void OnDestroy()
    {
        runtime?.OnDestroy();
    }
    public void SendMsgToBTRuntime(string triggerTag, EBTState state = EBTState.进入)
    {
        runtime?.OnReceiveMsg(triggerTag, state);
    }
}
