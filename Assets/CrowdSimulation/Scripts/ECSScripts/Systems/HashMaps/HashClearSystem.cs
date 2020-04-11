using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Systems
{
    [UpdateBefore(typeof(EdibleHashMap))]
    class HashClearSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var system = World.GetExistingSystem<EdibleHashMap>();
            if (system != null && !system.ShouldRunSystem())
            {
                EdibleHashMap.quadrantHashMap.Clear();
            }

            var eSystem = World.GetExistingSystem<EntitiesHashMap>();
            if (eSystem != null && !eSystem.ShouldRunSystem())
            {
                EntitiesHashMap.quadrantHashMap.Clear();
            }

            var iSystem = World.GetExistingSystem<InfectionHashMap>();
            if (iSystem != null && !iSystem.ShouldRunSystem())
            {
                InfectionHashMap.quadrantHashMap.Clear();
            }

            var cSystem = World.GetExistingSystem<CollidersHashMap>();
            if (cSystem != null && !cSystem.ShouldRunSystem())
            {
                CollidersHashMap.quadrantHashMap.Clear();
            }
        }
    }
}

