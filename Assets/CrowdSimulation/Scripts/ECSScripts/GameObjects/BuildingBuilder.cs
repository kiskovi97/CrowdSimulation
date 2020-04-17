using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.GameObjects
{
    public class BuildingBuilder: MonoBehaviour
    {
        public Transform selectedTransform;
        public Renderer selectedRenderer;
        public float round = 2f;

        private void Update()
        {
            if (RayCast.building)
            {
                selectedTransform.gameObject.SetActive(true);
                var count = selectedRenderer.materials.Length;
                var list = new List<Material>();
                for(int i=0; i<count; i++)
                {
                    list.Add(Materails.Instance.okayBuilding);
                }
                selectedRenderer.materials = list.ToArray();
                var point = RayCast.currentPoint;
                selectedTransform.position = new Vector3(((int)Math.Round(point.x / round)) * round, point.y, ((int)Math.Round(point.z / round)) * round);
            } else
            {
                selectedTransform.gameObject.SetActive(false);
            }
        }
    }
}
