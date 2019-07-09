using System;
using System.Collections.Generic;
using GHParser.Graph;
using GH_IO.Serialization;
using QuickGraph;
using UnityEngine;

namespace GHParser.GHElements
{
    public class PanelComponent : PrimitiveComponent
    {
        public PanelComponent(GH_Chunk objectChunk, Dictionary<Guid, Vertex> vertices, List<Guid[]> edges) :
            base(objectChunk, vertices, edges)
        {
            GH_Chunk container = objectChunk.FindChunk("Container") as GH_Chunk;
            if (container == null)
            {
                Debug.LogError("Container not found.");
                return;
            }

            UserText = container.GetString("UserText");

            vertices.Add(Guid, new Vertex(this));

            int sourceCount = container.GetInt32("SourceCount");
            for (int i = 0; i < sourceCount; i++)
            {
                edges.Add(new[] {container.GetGuid("Source", i), Guid});
            }
        }

        public string UserText { get; set; }

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
            //container.SetDouble("ScrollRatio", ScrollRatio);

            List<Edge> sourceEdges = new List<Edge>(graph.InEdges(vertex));
            for (int i = 0; i < sourceEdges.Count; i++)
            {
                container.SetGuid("Source", i, sourceEdges[i].Source.Chunk.Guid);
            }

            container.SetInt32("SourceCount", sourceEdges.Count);
            container.SetString("UserText", UserText);

            GH_IWriter attributes = container.CreateChunk("Attributes");
            attributes.SetDrawingRectangleF("Bounds", VisualBounds);
            //attributes.SetInt32("MarginLeft", MarginLeft);
            //attributes.SetInt32("MarginRight", MarginRight);
            //attributes.SetInt32("MarginTop", MarginTop);

            GH_IWriter properties = container.CreateChunk("PanelProperties");
            //properties.SetDrawingColor("Colour", Color);
            //properties.SetBoolean("DrawIndices", DrawIndices);
            //properties.SetBoolean("DrawPath", DrawPath);
            //properties.SetBoolean("Multiline", Multiline);
            //properties.SetBoolean("Stream", Stream);
            //properties.SetBoolean("Wrap", Wrap);

            return objectIndex + 1;
        }
    }
}