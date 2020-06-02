using Unity.Mathematics;

namespace Assets.CrowdSimulation.Scripts.Utilities
{
    class CollisionCalculator
    {
        public struct Circle
        {
            public float3 position;
            public float radius;
            public float3 velocity;
            public Circle(float3 point, float radius, float3 velocity)
            {
                this.position = point;
                this.radius = radius;
                this.velocity = velocity;
            }
        }
        public static float CalculateCirclesCollisionTime(Circle A, Circle B)
        {
            var Vab = A.velocity - B.velocity;
            var Pab = A.position - B.position;
            if (math.length(Pab) < A.radius + B.radius)
            {
                return -1;
            }

            var a = math.dot(Vab,Vab);
            var b = 2 * math.dot(Pab ,Vab);
            var c = math.dot(Pab,Pab) - (A.radius + B.radius) * (A.radius + B.radius);
            var discrimination = b * b - 4 * a * c;
            if (discrimination <= 0)
            {
                return -1;
            }

            var t0 = (-b - math.sqrt(discrimination)) / (2 * a);
            var t1 = (-b + math.sqrt(discrimination)) / (2 * a);
            if (t0 < 0f) return t1;
            if (t1 < 0f) return t0;
            return math.min(t0, t1);
        }


        public static float3 CalculateCollisionAvoidance(Circle A, Circle B, float time, float maxTime)
        {
            var v = CalculateCollisionVector(A , B, time);
            return CalculateAvoidance(v, A.velocity, time, maxTime);
        }

        public static float3 CalculateCollisionVector(Circle A, Circle B, float time)
        {
            var pointA = A.position + time * A.velocity;
            var pointB = B.position + time * B.velocity;
            var vector = (pointB - pointA) * (A.radius / (A.radius + B.radius));

            return vector;
        }

        public static float3 CalculateAvoidance(float3 vector, float3 velocity, float time, float maxTime)
        {
            var x = vector.z;
            var z = vector.x;
            var dot = vector.x * -velocity.z + vector.z * velocity.x;
            if (dot > 0) z *= -1;
            else x *= -1;
            return new float3(x,0,z) * (maxTime - time) / (maxTime);
        }
    }
}
