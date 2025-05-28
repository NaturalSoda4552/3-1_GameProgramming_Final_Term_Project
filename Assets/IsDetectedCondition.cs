using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "isDetected", story: "Agent Detect [target]", category: "Conditions", id: "2ee5a9e9a43240cf86894cd9ef16a79f")]
public partial class IsDetectedCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> target;
    private Mini2_Detecting m;

    public override bool IsTrue()
    {
        m = target?.Value.GetComponent<Mini2_Detecting>();
        if (m.isDetected)
        {
            return true;
        }
        return false;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
