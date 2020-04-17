using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas
{
    [GenerateAuthoringComponent]
    struct SpawnerPrefabContainer : IComponentData
    {
        public Entity prefab;
    }
}
