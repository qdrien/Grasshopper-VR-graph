using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GHParser.Graph;
using GHParser.Utils;
using GH_IO.Serialization;
using QuickGraph;
using Rhino.Geometry;
using UnityEngine;

namespace GHParser.GHElements
{
    public class IoComponent : Component
    {
        public string ScriptSource { get; set; }
        
        private IoComponentType _componentType;
        private Guid _inputId;
        private Guid _outputId;

        public IoComponent(GH_Chunk objectChunk,
            Dictionary<Guid, Vertex> existingVertices, List<Guid[]> edges, IoComponentType componentType) 
            : base(objectChunk, existingVertices, edges)
        {
            GH_Chunk container = objectChunk.FindChunk("Container") as GH_Chunk;
            if (container == null)
            {
                Debug.LogError("Container not found.");
                return;
            }

            existingVertices.Add(Guid, new Vertex(this));

            _componentType = componentType;

            if (componentType == IoComponentType.Type1)
            {
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

            else if (componentType == IoComponentType.Type2)
            {
                if (GHConstants.Script.Equals(objectChunk.GetGuid("GUID")))
                {
                    ScriptSource = container.GetString("ScriptSource");
                }

                GH_Chunk parameterData = container.FindChunk("ParameterData") as GH_Chunk;
                
                int inputCount = parameterData.GetInt32("InputCount");
                int outputCount = parameterData.GetInt32("OutputCount");

                if(inputCount >= 1)
                    _inputId = parameterData.GetGuid("InputId", 0); //TODO: is this always the same value for all params? Seems to be the case
                if(outputCount >= 1)
                    _outputId = parameterData.GetGuid("OutputId", 0);

                for (int inputIndex = 0; inputIndex < inputCount; inputIndex++)
                {
                    GH_Chunk inputChunk = parameterData.FindChunk("InputParam", inputIndex) as GH_Chunk;
                    InputPort inputPort;
                    if (inputChunk.ChunkExists("PersistentData"))
                    {
                        inputPort = new InputPort(inputChunk.GetGuid("InstanceGuid"), inputChunk.GetString("NickName"),
                            inputChunk.GetString("Name"), inputChunk.FindChunk("Attributes").GetDrawingRectangleF("Bounds"),
                            inputChunk.FindChunk("PersistentData") as GH_Chunk);
                    }
                    else
                    {
                        inputPort = new InputPort(inputChunk.GetGuid("InstanceGuid"), inputChunk.GetString("NickName"),
                            inputChunk.GetString("Name"), inputChunk.FindChunk("Attributes").GetDrawingRectangleF("Bounds"));
                    }

                    if (inputChunk.ItemExists("TypeHintID"))
                    {
                        inputPort.TypeHintID = inputChunk.GetGuid("TypeHintID");
                    }

                    existingVertices.Add(inputPort.Guid, new Vertex(inputPort));
                    edges.Add(new[] {inputPort.Guid, Guid});

                    int sourceCount = inputChunk.GetInt32("SourceCount");
                    for (int i = 0; i < sourceCount; i++)
                    {
                        edges.Add(new[] {inputChunk.GetGuid("Source", i), inputPort.Guid});
                    }
                }

                for (int outputIndex = 0; outputIndex < outputCount; outputIndex++)
                {
                    GH_Chunk outputChunk = parameterData.FindChunk("OutputParam", outputIndex) as GH_Chunk;
                    OutputPort outputPort = new OutputPort(outputChunk.GetGuid("InstanceGuid"), outputChunk.GetString("NickName"),
                        outputChunk.GetString("Name"), outputChunk.FindChunk("Attributes").GetDrawingRectangleF("Bounds"));
                    existingVertices.Add(outputPort.Guid, new Vertex(outputPort));
                    edges.Add(new[] {Guid, outputPort.Guid});
                }
            }

            
        }

        public IoComponent(string defaultName, Guid typeGuid, string typeName, RectangleF visualBounds, Guid instanceGuid, string nickname, string scriptSource, IoComponentType componentType, Guid inputId, Guid outputId) : base(defaultName, typeGuid, typeName, visualBounds, instanceGuid, nickname)
        {
            ScriptSource = scriptSource;
            _componentType = componentType;
            _inputId = inputId;
            _outputId = outputId;
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

            if (_componentType == IoComponentType.Type1)
            {
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
            }
            else if (_componentType == IoComponentType.Type2)
            {
                if (!string.IsNullOrEmpty(ScriptSource))
                {
                    container.SetString("ScriptSource", ScriptSource);
                }

                GH_Chunk parameterData = container.CreateChunk("ParameterData") as GH_Chunk;

                List<Edge> inEdges = new List<Edge>(graph.InEdges(vertex));
                List<Edge> outEdges = new List<Edge>(graph.OutEdges(vertex));
                
                parameterData.SetInt32("InputCount", inEdges.Count);
                parameterData.SetInt32("OutputCount", outEdges.Count);
                
                //For each input port
                int count = 0;
                for (int inputIndex = 0; inputIndex < inEdges.Count; inputIndex++)
                {
                    parameterData.SetGuid("InputId", inputIndex, _inputId);

                    Edge inEdge = inEdges[inputIndex];
                    Vertex inputVertex = inEdge.Source;
                    InputPort inputPort = inputVertex.Chunk as InputPort;
                    GH_IWriter inputChunk = parameterData.CreateChunk("InputParam", count);
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

                    if (inputPort.TypeHintID != Guid.Empty)
                    {
                        inputChunk.SetGuid("TypeHintID", inputPort.TypeHintID);
                    }

                    count++;
                }

                count = 0;
                for (int outputIndex = 0; outputIndex < outEdges.Count; outputIndex++)
                {
                    parameterData.SetGuid("OutputId", outputIndex, _outputId);

                    Edge outEdge = outEdges[outputIndex];
                    Vertex outputVertex = outEdge.Target;
                    OutputPort outputPort = outputVertex.Chunk as OutputPort;
                    GH_IWriter outputChunk = parameterData.CreateChunk("OutputParam", count);
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
       
        public enum IoComponentType
        {
            Type1 = 0,
            Type2 = 1
        }
    }
}