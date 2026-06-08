
using System;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
public class Deco_ParameterRelay : DecoratorNode
{
    public override string stateName => "ParameterRelayState";
    public Deco_ParameterRelay() : base() 
    {
        title = "ParameterRelay";
        
        
        Port port_stringValue = CreatePortForNode(this, Direction.Output, typeof(System.String), Port.Capacity.Multi);
        port_stringValue.portName = "stringValue";
        outputContainer.Add(port_stringValue);

    }
}
