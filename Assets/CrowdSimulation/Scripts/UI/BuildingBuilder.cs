using Assets.CrowdSimulation.Scripts.ECSScripts.GameObjects;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.CrowdSimulation.Scripts.UI
{

    enum BuilderState
    {
        Normal,
        Selection,
        Building,
    }

    public class BuildingBuilder: MonoBehaviour
    {
        public Transform selectedTransform;
        public Renderer selectedRenderer;
        public float round = 2f;
        public float rotateRound = 30f;

        public static BuildingBuilder instance;
        private static BuilderState State;

        public static void OnSecondaryAction()
        {
            switch (State)
            {
                case BuilderState.Building:
                    instance.selectedTransform.Rotate(Vector3.up, instance.rotateRound, Space.World);
                    break;
            }
        }

        public static void OnPrimaryAction()
        {
            switch (State)
            {
                case BuilderState.Building:
                    RayCast.ResetState();
                    ResetState();
                    break;
            }
        }

        public static void OnMouseMove(Vector3 point)
        {
            var count = instance.selectedRenderer.materials.Length;
            var list = new List<Material>();
            for (int i = 0; i < count; i++)
            {
                list.Add(Materails.Instance.okayBuilding);
            }
            instance.selectedRenderer.materials = list.ToArray();
            instance.selectedTransform.position = 
                new Vector3(((int)Math.Round(point.x / instance.round)) * instance.round, point.y, 
                ((int)Math.Round(point.z / instance.round)) * instance.round);

        }

        public void SetBuilding(int id)
        {
            State = BuilderState.Building;
            RayCast.SetToBuilding();
        }

        public void SetSelection()
        {
            State = BuilderState.Selection;
        }

        public static void ResetState()
        {
            State = BuilderState.Normal;
            RayCast.ResetState();
        }

        private void Start()
        {
            instance = this;
        }

        private void Update()
        {
            if (State == BuilderState.Building)
            {
                selectedTransform.gameObject.SetActive(true);
            }
            else
            {
                selectedTransform.gameObject.SetActive(false);
            }
        }
    }
}
