using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Systems
{
    [AlwaysSynchronizeSystem]
    [UpdateAfter(typeof(WalkingSystem))]
    class LoggingSystem : ComponentSystem
    {
        private int db = 0;
        private readonly float period = 30;

        private static readonly int maxResult = 4;

        NativeArray<float> Avarage;

        protected override void OnCreate()
        {
            base.OnCreate();
            Avarage = new NativeArray<float>(maxResult, Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            Avarage.Dispose();
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            var deltaTime = Time.DeltaTime;

            NativeArray<float> result = new NativeArray<float>(maxResult, Allocator.TempJob);
            result[0] = 0f;
            result[1] = 0f;
            result[2] = 0f;

            Entities.ForEach((ref Walker walker, ref PathFindingData data, ref Translation tr) =>
            {
                var length = math.length(data.decidedGoal - tr.Value);
                result[0] += 1f;
                result[1] += math.length(walker.direction);
                result[2] += math.max(0f, length - data.radius);
            });
            if (result[0] > 0)
            {
                Avarage[1] += (result[1] / result[0]) / period;
                Avarage[2] += (result[2] / result[0]) / period;
                db++;
                if (db % period == 0)
                {
                    Logger.Log(Avarage[1].ToString("N3"));
                    Logger.Log(Avarage[2].ToString("N3"));
                    for (int i = 0; i < maxResult; i++)
                    {
                        Avarage[i] = 0f;
                    }
                }
            }
            result.Dispose();
        }
    }
}
