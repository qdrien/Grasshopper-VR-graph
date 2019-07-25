using System;
using System.Collections.Generic;
using System.Drawing;
using GHParser.Graph;
using GH_IO.Serialization;
using QuickGraph;
using UnityEngine;

namespace GHParser.GHElements
{
    public class IoComponent : Component
    {
        public IoComponent(GH_Chunk objectChunk,
            Dictionary<Guid, Vertex> existingVertices, List<Guid[]> edges) : base(objectChunk, existingVertices, edges)
        {
            GH_Chunk container = objectChunk.FindChunk("Container") as GH_Chunk;
            if (container == null)
            {
                Debug.LogError("Container not found.");
                return;
            }

            existingVertices.Add(Guid, new Vertex(this));

            foreach (GH_IChunk iChunk in container.Chunks)
            {
                GH_Chunk chunk = iChunk as GH_Chunk;
                if (chunk.Name.Equals("param_input"))
                {
                    InputPort inputPort;
                    if (chunk.ChunkExists("PersistentData"))
                    {
                        inputPort = new InputPort(chunk.GetGuid("InstanceGuid"), chunk.GetString("NickName"),
                            chunk.GetString("Name"), chunk.FindChunk("Attributes").GetDrawingRectangleF("Bounds"),
                            chunk.FindChunk("PersistentData") as GH_Chunk);
                    }
                    else
                    {
                        inputPort = new InputPort(chunk.GetGuid("InstanceGuid"), chunk.GetString("NickName"),
                            chunk.GetString("Name"), chunk.FindChunk("Attributes").GetDrawingRectangleF("Bounds"));
                    }

                    existingVertices.Add(inputPort.Guid, new Vertex(inputPort));
                    edges.Add(new[] {inputPort.Guid, Guid});

                    int sourceCount = chunk.GetInt32("SourceCount");
                    for (int i = 0; i < sourceCount; i++)
                    {
                        edges.Add(new[] {chunk.GetGuid("Source", i), inputPort.Guid});
                    }
                }
                else if (chunk.Name.Equals("param_output"))
                {
                    OutputPort outputPort = new OutputPort(chunk.GetGuid("InstanceGuid"), chunk.GetString("NickName"),
                        chunk.GetString("Name"), chunk.FindChunk("Attributes").GetDrawingRectangleF("Bounds"));
                    existingVertices.Add(outputPort.Guid, new Vertex(outputPort));
                    edges.Add(new[] {Guid, outputPort.Guid});
                }
            }
        }

        public IoComponent(string defaultName, Guid typeGuid, string typeName, RectangleF visualBounds, Guid instanceGuid, string nickname) : base(defaultName, typeGuid, typeName, visualBounds, instanceGuid, nickname)
        {
            
        }

        /*public override string ToString()
        {
            return "\"C:" + Nickname + "@" + Guid.ToString().Split('-')[0] + "\"";
        }*/

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

            GH_IWriter attributes = container.CreateChunk("Attributes");
            attributes.SetDrawingRectangleF("Bounds", VisualBounds);
            //attributes.SetDrawingPointF("Pivot", VisualPivot);

            //For each input port
            int count = 0;
            foreach (Edge inEdge in graph.InEdges(vertex))
            {
                Vertex inputVertex = inEdge.Source;
                InputPort inputPort = inputVertex.Chunk as InputPort;
                GH_IWriter inputChunk = container.CreateChunk("param_input", count);
                //inputChunk.SetString("Description", inputPort.Description);
                inputChunk.SetGuid("InstanceGuid", inputPort.Guid);
                inputChunk.SetString("Name", inputPort.DefaultName); //or DefaultNameToGive?
                inputChunk.SetString("NickName", inputPort.Nickname);
                //inputChunk.SetBoolean("Optional", inputPort.IsOptional);

                //For each source connected to this input port
                List<Edge> sourceEdges = new List<Edge>(graph.InEdges(inputVertex));
                for (int i = 0; i < sourceEdges.Count; i++)
                {
                    inputChunk.SetGuid("Source", i, sourceEdges[i].Source.Chunk.Guid);
                }

                inputChunk.SetInt32("SourceCount", sourceEdges.Count);

                GH_IWriter inputAttributes = inputChunk.CreateChunk("Attributes");
                inputAttributes.SetDrawingRectangleF("Bounds", inputPort.VisualBounds);
                //inputAttributes.SetDrawingPointF("Pivot", inputPort.VisualPivot);

                if (inputPort.PersistentData != null)
                {
                    GH_Chunk persistentDataCopy = inputChunk.CreateChunk("PersistentData") as GH_Chunk;
                    CopyChildren(inputPort.PersistentData, persistentDataCopy);
                }

                count++;
            }

            count = 0;
            foreach (Edge outEdge in graph.OutEdges(vertex))
            {
                Vertex outputVertex = outEdge.Target;
                OutputPort outputPort = outputVertex.Chunk as OutputPort;
                GH_IWriter outputChunk = container.CreateChunk("param_output", count);
                //outputChunk.SetString("Description", outputPort.Description);
                outputChunk.SetGuid("InstanceGuid", outputPort.Guid);
                outputChunk.SetString("Name", outputPort.DefaultName); //or DefaultNameToGive?
                outputChunk.SetString("NickName", outputPort.Nickname);
                //outputChunk.SetBoolean("Optional", inputPort.IsOptional);

                //should not contain any sources, but GH files contain SourceCount=0
                //should probably be careful with that (note : would have to filter "vertex" in "inEdges")
                outputChunk.SetInt32("SourceCount", 0);

                GH_IWriter outputAttributes = outputChunk.CreateChunk("Attributes");
                outputAttributes.SetDrawingRectangleF("Bounds", outputPort.VisualBounds);
                //outputAttributes.SetDrawingPointF("Pivot", outputPort.VisualPivot);

                count++;
            }

            return objectIndex + 1;
            //QUESTION: save the data that is currently commented out in Chunk subclasses? It doesnt seem to be necessary
        }

        public override bool Remove(Vertex vertex, BidirectionalGraph<Vertex, Edge> graph, List<Group> groups)
        {
            List<Edge> inEdges = new List<Edge>(graph.InEdges(vertex));
            foreach (Edge inEdge in inEdges)
            {
                Vertex inputVertex = inEdge.Source;
                inputVertex.Chunk.Remove(inputVertex, graph, groups);
            }

            List<Edge> outEdges = new List<Edge>(graph.OutEdges(vertex));
            foreach (Edge outEdge in outEdges)
            {
                Vertex outputVertex = outEdge.Target;
                outputVertex.Chunk.Remove(outputVertex, graph, groups);
            }

            return graph.RemoveVertex(vertex); //does not check whether something went wrong when removing in/out ports
        }
    }
}