using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.GameObjects
{
    public class Building: MonoBehaviour
    {
        public Transform goal;
        public float round = 2f;
        private void Start()
        {
            
        }

        private void Update()
        {
            if (RayCast.building)
            {
                var point = RayCast.currentPoint;
                goal.position = new Vector3(((int)Math.Round(point.x / round)) * round, point.y, ((int)Math.Round(point.z / round)) * round);
            }
        }
    }
}
