using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "isActive", story: "check [target] mini2 active", category: "Conditions", id: "4fda0f56faae8ba84c7d57c803e20247")]
public partial class IsActiveCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> target;
    private BombRoom_Manager m;
    
    public override bool IsTrue()
    {
        m = target?.Value.GetComponent<BombRoom_Manager>();
        if (m.mini2_active)
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
