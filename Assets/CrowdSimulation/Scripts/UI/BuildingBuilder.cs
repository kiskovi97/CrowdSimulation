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

        public GameObject[] prefabs;

        public float round = 2f;
        public float rotateRound = 30f;

        public int selectedId = 0; 

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
                    instance.CreateInstance();
                    ResetState();
                    break;
            }
        }

        public void OnCancel()
        {
            ResetState();
        }

        private void CreateInstance()
        {
            Instantiate(prefabs[selectedId], selectedTransform.position, selectedTransform.rotation * Quaternion.Euler(90,0,0));
        }

        public static void OnMouseMove(Vector3 point)
        {
            instance.selectedRenderer.material = Materails.Instance.okayBuilding;
            instance.selectedTransform.position = 
                new Vector3(((int)Math.Round(point.x / instance.round)) * instance.round, point.y, 
                ((int)Math.Round(point.z / instance.round)) * instance.round);

        }

        public void SetBuilding(int id)
        {
            selectedId = id;
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
