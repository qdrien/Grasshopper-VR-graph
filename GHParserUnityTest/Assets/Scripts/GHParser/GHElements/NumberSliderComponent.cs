using System;
using System.Collections.Generic;
using GHParser.Graph;
using GH_IO.Serialization;
using QuickGraph;
using UnityEngine;

namespace GHParser.GHElements
{
    public class NumberSliderComponent : PrimitiveComponent
    {
        public NumberSliderComponent(GH_Chunk objectChunk, Dictionary<Guid, Vertex> vertices, List<Guid[]> edges) :
            base(objectChunk, vertices, edges)
        {
            GH_Chunk container = objectChunk.FindChunk("Container") as GH_Chunk;
            if (container == null)
            {
                Debug.LogError("Container not found.");
                return;
            }

            GH_Chunk sliderAttributes = container.FindChunk("Slider") as GH_Chunk;
            if (sliderAttributes == null)
            {
                Debug.LogError(
                    "Cannot find Slider configuration, this slider will have default values for those attributes.");
                return;
            }

            DigitsCount = sliderAttributes.GetInt32("Digits");
            Interval = sliderAttributes.GetInt32("Interval");
            Max = sliderAttributes.GetDouble("Max");
            Min = sliderAttributes.GetDouble("Min");
            SnapCount = sliderAttributes.GetInt32("SnapCount");
            Value = sliderAttributes.GetDouble("Value");

            vertices.Add(Guid, new Vertex(this));
        }
        //The "GripDisplay" value is ignored since it defines how the handle is displayed 
        //(and we probably want to use our own representation)

        public int DigitsCount { get; set; }

        public int Interval { get; set; }

        public double Max { get; set; }

        public double Min { get; set; }

        public int SnapCount { get; set; }

        public double Value { get; set; }

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

            GH_IWriter attributes = container.CreateChunk("Attributes");
            attributes.SetDrawingRectangleF("Bounds", VisualBounds);

            GH_IWriter slider = container.CreateChunk("Slider");
            slider.SetInt32("Digits", DigitsCount);
            //slider.SetInt32("GripDisplay", Config.GripDisplay);
            slider.SetInt32("GripDisplay", 1); //NOTE: currently forced to 1
            slider.SetInt32("Interval", Interval);
            slider.SetDouble("Max", Max);
            slider.SetDouble("Min", Min);
            slider.SetInt32("SnapCount", SnapCount);
            slider.SetDouble("Value", Value);

            return objectIndex + 1;
        }
    }
}