using System;
using System.Collections.Generic;
using GHParser.Graph;
using GH_IO.Serialization;
using QuickGraph;
using UnityEngine;

namespace GHParser.GHElements
{
    public class BooleanToggleComponent : PrimitiveComponent
    {
        public BooleanToggleComponent(GH_Chunk objectChunk, Dictionary<Guid, Vertex> vertices, List<Guid[]> edges) :
            base(objectChunk, vertices, edges)
        {
            GH_Chunk container = objectChunk.FindChunk("Container") as GH_Chunk;
            if (container == null)
            {
                Debug.LogError("Container not found.");
                return;
            }

            Value = container.GetBoolean("ToggleValue");

            vertices.Add(Guid, new Vertex(this));
        }

        public bool Value { get; set; }

        public override int Add(
            GH_Chunk definitionObjects, int objectIndex, BidirectionalGraph<Vertex, Edge> graph, Vertex vertex)
        {
            GH_IWriter chunk = definitionObjects.CreateChunk("Object", objectIndex);
            chunk.SetGuid("GUID", TypeGuid);
            chunk.SetString("Name", TypeName);

            GH_IWriter container = chunk.CreateChunk("Container");
            //container.SetString("Description", TypeDescription);
            container.SetGuid("InstanceGuid", Guid);
            container.SetString("Name", TypeName);
            container.SetString("NickName", Nickname);
            //container.SetBoolean("Optional", IsOptional);
            container.SetInt32("SourceCount", 0);
            container.SetBoolean("ToggleValue", Value);

            GH_IWriter attributes = container.CreateChunk("Attributes");
            attributes.SetDrawingRectangleF("Bounds", VisualBounds);

            return objectIndex + 1;
        }
    }
}