using System;
using System.Collections.Generic;
using System.Drawing;
using GHParser.Graph;
using GH_IO.Serialization;
using UnityEngine;

namespace GHParser.GHElements
{
    public abstract class Component : Chunk
    {
        private string _defaultName;

        public Component(GH_Chunk objectChunk, Dictionary<Guid, Vertex> existingVertices, List<Guid[]> edges)
        {
            GH_Chunk container = objectChunk.FindChunk("Container") as GH_Chunk;
            if (container == null)
            {
                Debug.LogError("Container not found.");
                return;
            }

            Nickname = container.GetString("NickName");
            TypeName = objectChunk.GetString("Name");
            TypeGuid = objectChunk.GetGuid("GUID");
            VisualBounds = container.FindChunk("Attributes").GetDrawingRectangleF("Bounds");
            Guid = container.GetGuid("InstanceGuid");
        }

        public Guid TypeGuid { get; set; }

        public string TypeName { get; set; }

        public string DefaultName
        {
            get { return _defaultName; }
            set { _defaultName = string.IsNullOrEmpty(value) ? TypeName : value; }
        }

        public RectangleF VisualBounds { get; set; }

        public override string ToString()
        {
            return "\"" + (string.IsNullOrEmpty(Nickname) ? _defaultName : Nickname) + VisualBounds + "\"";
        }
    }
}