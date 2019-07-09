using System;
using System.Collections.Generic;
using GHParser.Graph;
using GHParser.Utils;
using GH_IO.Serialization;
using QuickGraph;
using UnityEngine;
using Color = System.Drawing.Color;

namespace GHParser.GHElements
{
    public class Group : Chunk
    {
        public Group(GH_Chunk objectChunk)
        {
            GH_Chunk container = objectChunk.FindChunk("Container") as GH_Chunk;
            if (container == null)
            {
                Debug.LogError("Container not found.");
                return;
            }

            Nickname = container.GetString("NickName");
            Guid = container.GetGuid("InstanceGuid");


            Color = container.GetDrawingColor("Colour");

            Constituents = new List<Guid>();
            int idCount = container.GetInt32("ID_Count");
            for (int i = 0; i < idCount; i++)
            {
                Constituents.Add(container.GetGuid("ID", i));
            }
        }

        public Color Color { get; set; }

        public List<Guid> Constituents { get; set; }

        public override string ToString()
        {
            return "\"Group:" + Guid.ToString().Split('-')[0] + "\"";
        }

        public override int Add(
            GH_Chunk definitionObjects, int objectIndex, BidirectionalGraph<Vertex, Edge> graph, Vertex vertex)
        {
            GH_IWriter groupChunk = definitionObjects.CreateChunk("Object", objectIndex);
            groupChunk.SetGuid("GUID", GHConstants.Group);
            groupChunk.SetString("Name", "Group");
            GH_IWriter container = groupChunk.CreateChunk("Container");
            container.SetInt32("Border", 1);
            container.SetDrawingColor("Colour",
                Color.FromArgb(Color.A, Color.R, Color.G, Color.B));
            //container.SetString("Description", "A group of Grasshopper objects");
            for (int i = 0; i < Constituents.Count; i++)
            {
                container.SetGuid("ID", i, Constituents[i]);
            }

            container.SetInt32("ID_Count", Constituents.Count);
            container.SetGuid("InstanceGuid", Guid);
            container.SetString("Name", "Group");
            container.SetString("NickName", Nickname);
            container.CreateChunk("Attributes");

            return objectIndex + 1;
        }

        public override bool Remove(Vertex vertex, BidirectionalGraph<Vertex, Edge> graph, List<Group> groups)
        {
            return groups.Remove(this);
            ;
        }
    }
}