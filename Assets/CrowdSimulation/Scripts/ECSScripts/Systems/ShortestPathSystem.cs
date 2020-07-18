using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Systems
{
    [AlwaysSynchronizeSystem]
    class ShortestPathSystem : ComponentSystem
    {
        public static readonly float minDistance = 10f;
        public static NativeList<float3> goalPoints;

        protected override void OnCreate()
        {
            goalPoints = new NativeList<float3>(Allocator.Persistent);
            base.OnCreate();
        }

        protected override void OnDestroy()
        {
            goalPoints.Dispose();
            base.OnDestroy();
        }


        public static void AddGoalPoint(float3 goalPoint)
        {
            if (goalPoints.Length > 0)
            {
                var min = ClosestGoalPoint(goalPoint);
                if (math.length(goalPoints[min] - goalPoint) < minDistance) return;
            }

            goalPoints.Add(goalPoint);

            DijsktraSystem.AddGoalPoint(goalPoint);
            AStarMatrixSystem.AddGoalPoint(goalPoint);

            DebugProxy.Log(goalPoint + " / " + goalPoints.Length);
        }

        public static int ClosestGoalPoint(float3 point)
        {
            var min = 0;
            for (int i = 1; i < goalPoints.Length; i++)
            {
                if (math.lengthsq(goalPoints[min] - point) > math.lengthsq(goalPoints[i] - point))
                {
                    min = i;
                }
            }
            return min;
        }

        protected override void OnUpdate()
        {
        }
    }
}
