using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Physics;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Entities;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using System.Linq;
using Assets.CrowdSimulation.Scripts.Utilities;
using System.Drawing;
using Assets.CrowdSimulation.Scripts.ECSScripts.Systems;

public class PhysicsShapeConverter : MonoBehaviour, IConvertGameObjectToEntity
{
    Unity.Physics.Authoring.PhysicsShapeAuthoring physicsShape;

    private static float offset = 1.4f;

    public static Graph graph = new Graph();
    public static bool Changed = false;

    // Start is called before the first frame update
    void Awake()
    {
        physicsShape = gameObject.GetComponent<Unity.Physics.Authoring.PhysicsShapeAuthoring>();
    }

    private void GePlane()
    {
        physicsShape.GetPlaneProperties(out float3 pCenter, out float2 pSize, out quaternion pOrientation);
        var orientation = math.mul(pOrientation, transform.rotation);
        var scale = (float3)transform.lossyScale * new float3(pSize.x, 0, pSize.y);
        var center = pCenter + (float3)transform.position;
        center.y = 0;
        AddPoints(center, orientation, scale);
    }

    private void GetSphere()
    {
        var props = physicsShape.GetSphereProperties(out quaternion outOrientation);
        var orientation = math.mul(outOrientation, transform.rotation);
        var scale = transform.lossyScale * props.Radius;
        var center = props.Center + (float3)transform.position;
        center.y = 0;
        AddPoints(center, orientation, scale);
    }

    private void GetBox()
    {
        var props = physicsShape.GetBoxProperties();
        var orientation = math.mul(props.Orientation, transform.rotation);
        var scale = transform.lossyScale * props.Size;
        var center = props.Center + (float3)transform.position;
        center.y = 0;
        AddPoints(center, orientation, scale);
    }

    private void AddPoints(float3 center, quaternion orientation, float3 scale)
    {

        var A = center + math.mul(orientation, scale * new float3(1, 0, 1) * 0.5f);
        var B = center + math.mul(orientation, scale * new float3(-1, 0, 1) * 0.5f);
        var C = center + math.mul(orientation, scale * new float3(-1, 0, -1) * 0.5f);
        var D = center + math.mul(orientation, scale * new float3(1, 0, -1) * 0.5f);

        var AOffset = center + math.mul(orientation, scale * new float3(1, 0, 1) * 0.5f + new float3(offset, 0, offset));
        var BOffset = center + math.mul(orientation, scale * new float3(-1, 0, 1) * 0.5f + new float3(-offset, 0, offset));
        var COffset = center + math.mul(orientation, scale * new float3(-1, 0, -1) * 0.5f + new float3(-offset, 0, -offset));
        var DOffset = center + math.mul(orientation, scale * new float3(1, 0, -1) * 0.5f + new float3(offset, 0, -offset));

        var added = new List<float3>
        {
            A, B, C, D
        };
        var addedWOffset = new List<float3>
        {
            AOffset, BOffset, COffset, DOffset
        };

        graph.AddPoints(added, addedWOffset);
        Changed = true;
        GraphSystem.UpdateGraph();
    }

    class Comperer : IComparer<float3>
    {
        float3 from;
        public Comperer(float3 from)
        {
            this.from = from;
        }

        public int Compare(float3 x, float3 y)
        {
            return (int)(math.length(x - from) - math.length(y - from));
        }
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        switch (physicsShape.ShapeType)
        {
            case Unity.Physics.Authoring.ShapeType.Box:
                GetBox();
                break;
            case Unity.Physics.Authoring.ShapeType.Capsule:
                break;
            case Unity.Physics.Authoring.ShapeType.ConvexHull:
                break;
            case Unity.Physics.Authoring.ShapeType.Cylinder:
                break;
            case Unity.Physics.Authoring.ShapeType.Mesh:
                break;
            case Unity.Physics.Authoring.ShapeType.Plane:
                GePlane();
                break;
            case Unity.Physics.Authoring.ShapeType.Sphere:
                GetSphere();
                break;
        }

        dstManager.AddComponentData(entity, new PathCollidable()
        {
        });
    }
}
