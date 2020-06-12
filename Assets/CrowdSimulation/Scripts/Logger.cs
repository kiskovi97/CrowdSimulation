using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.CrowdSimulation
{
    public class Logger : MonoBehaviour
    {
        static StreamWriter sw;
        private readonly static Queue<string> cLines = new Queue<string>();
        private static readonly object lockObj = new object();

        private static string[] Types =
        {
            "velocity", "distance", "collided", "near others", "near all", "rotation"
        };

        public static void Log(string line)
        {
            lock (lockObj)
            {
                cLines.Enqueue(line);
            }
        }

        private void Awake()
        {
            var sceneName = SceneManager.GetActiveScene().name;
            sw = new StreamWriter(new FileStream("c:/tmp/Log_" + sceneName + "_"  + DateTime.Now.Ticks + ".csv", FileMode.Create));
            Application.targetFrameRate = 100;
        }

        private void Start()
        {
            sw.AutoFlush = true;
            EditorApplication.quitting += OnApplicationExit;
            EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
            //DensityGrid, Forces

            StringBuilder builder = new StringBuilder();
            for (int i=0; i< Types.Length; i++)
            {
                var type = Types[i];
                builder.Append(CollisionAvoidanceMethod.DensityGrid + " avarage " + type + ";");
                builder.Append(CollisionAvoidanceMethod.Forces + " avarage " + type + ";");
                builder.Append(CollisionAvoidanceMethod.FutureAvoidance + " avarage " + type + ";");
                builder.Append(CollisionAvoidanceMethod.Probability + " avarage " + type + ";");
                builder.Append(CollisionAvoidanceMethod.No + " avarage " + type + ";");
            }

            sw.WriteLine("Time;"+ builder.ToString());
        }

        private void EditorApplication_playModeStateChanged(PlayModeStateChange obj)
        {
            if (obj.Equals(PlayModeStateChange.ExitingPlayMode))
            {

                OnApplicationExit();
            }
        }

        void Update()
        {
            if (sw == null) return;
            var hasData = cLines.Count > 0;
            if (hasData)
            {
                TimeSpan time = TimeSpan.FromSeconds(Time.time);
                sw?.Write(time.ToString() + ";");
                while (cLines.Count > 0)
                {
                    sw?.Write(cLines.Dequeue() + ";");
                }
                sw?.WriteLine();
            }
        }


        void OnApplicationExit()
        {
            sw?.Close();
            sw = null;
        }
    }
}
