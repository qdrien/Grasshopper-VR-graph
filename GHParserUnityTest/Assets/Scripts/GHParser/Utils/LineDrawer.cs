using System.Collections.Generic;
using HoloToolkit.Unity;
using UnityEngine;

namespace GHParser.Utils
{
    //TODO:[minor] should probably not be a singleton since we could have multiple cameras
    /// <summary>
    /// Draws lines to outline the group boxes
    /// </summary>
    public class LineDrawer : Singleton<LineDrawer>
    {
        public Material GLMat;

        public List<Vector3> Lines { get; set; }

        private void Awake()
        {
            Lines = new List<Vector3>();
        }

        private void OnPostRender()
        {
            GL.PushMatrix();
            GLMat.SetPass(0);
            GL.Begin(GL.LINES);

            foreach (Vector3 vertex in Lines)
            {
                GL.Vertex(vertex);
            }

            GL.End();
            GL.PopMatrix();
        }
    }
}