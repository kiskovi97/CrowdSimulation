using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Transforms;

class ConditionDebuggerSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref ConditionDebugger debugger, ref Parent parent, ref Translation translation, ref LocalToWorld localToWorld) =>
        {
            if (EntityManager.HasComponent<Condition>(parent.Value))
            {
                var condition = EntityManager.GetComponentData<Condition>(parent.Value);
                translation.Value.y = (condition.hunger - 0.2f) * localToWorld.Up.y * 2f;
            }
        });
    }
}

