using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(EndFrameLocalToParentSystem))]
class ConditionDebuggerSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref ConditionDebugger debugger, ref Parent parent, ref NonUniformScale compositeScale) =>
        {
            if (EntityManager.HasComponent<Condition>(parent.Value))
            {
                var condition = EntityManager.GetComponentData<Condition>(parent.Value);
                //translation.Value.y = (condition.hunger) - localToWorld.Up.y * 0.5f;
                compositeScale.Value.y = condition.hunger * 0.01f;
            }
        });
    }
}

