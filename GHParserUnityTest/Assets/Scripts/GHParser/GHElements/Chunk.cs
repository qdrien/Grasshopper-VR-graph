using System;
using System.Collections.Generic;
using GHParser.Graph;
using GH_IO.Serialization;
using GH_IO.Types;
using QuickGraph;
using UnityEngine;

namespace GHParser.GHElements
{
    [Serializable]
    public abstract class Chunk
    {
        [NonSerialized]
        private string _nickname;
        public Guid Guid { get; set; }

        public string Nickname
        {
            get { return _nickname; }
            set { _nickname = value; }
        }

        public override string ToString()
        {
            return "\"?:" + Guid.ToString().Split('-')[0] + "\"";
        }

        protected static void CopyChildren(GH_Chunk toCopy, GH_Chunk target)
        {
            //copy items first
            foreach (GH_Item item in toCopy.Items)
            {
                switch (item.Type) //this is ugly as f* but necessary since we have to use Set[Type]...
                {
                    case GH_Types.unset:
                        Debug.LogError("type unset, ignoring.");
                        break;
                    case GH_Types.gh_bool:
                        if (item.HasIndex)
                        {
                            target.SetBoolean(item.Name, item.Index, item._bool);
                        }
                        else
                        {
                            target.SetBoolean(item.Name, item._bool);
                        }

                        break;
                    case GH_Types.gh_byte:
                        if (item.HasIndex)
                        {
                            target.SetByte(item.Name, item.Index, item._byte);
                        }
                        else
                        {
                            target.SetByte(item.Name, item._byte);
                        }

                        break;
                    case GH_Types.gh_int32:
                        if (item.HasIndex)
                        {
                            target.SetInt32(item.Name, item.Index, item._int32);
                        }
                        else
                        {
                            target.SetInt32(item.Name, item._int32);
                        }

                        break;
                    case GH_Types.gh_int64:
                        if (item.HasIndex)
                        {
                            target.SetInt64(item.Name, item.Index, item._int64);
                        }
                        else
                        {
                            target.SetInt64(item.Name, item._int64);
                        }

                        break;
                    case GH_Types.gh_single:
                        if (item.HasIndex)
                        {
                            target.SetSingle(item.Name, item.Index, item._single);
                        }
                        else
                        {
                            target.SetSingle(item.Name, item._single);
                        }

                        break;
                    case GH_Types.gh_double:
                        if (item.HasIndex)
                        {
                            target.SetDouble(item.Name, item.Index, item._double);
                        }
                        else
                        {
                            target.SetDouble(item.Name, item._double);
                        }

                        break;
                    case GH_Types.gh_decimal:
                        if (item.HasIndex)
                        {
                            target.SetDecimal(item.Name, item.Index, item._decimal);
                        }
                        else
                        {
                            target.SetDecimal(item.Name, item._decimal);
                        }

                        break;
                    case GH_Types.gh_date:
                        if (item.HasIndex)
                        {
                            target.SetDate(item.Name, item.Index, item._date);
                        }
                        else
                        {
                            target.SetDate(item.Name, item._date);
                        }

                        break;
                    case GH_Types.gh_guid:
                        if (item.HasIndex)
                        {
                            target.SetGuid(item.Name, item.Index, item._guid);
                        }
                        else
                        {
                            target.SetGuid(item.Name, item._guid);
                        }

                        break;
                    case GH_Types.gh_string:
                        if (item.HasIndex)
                        {
                            target.SetString(item.Name, item.Index, item._string);
                        }
                        else
                        {
                            target.SetString(item.Name, item._string);
                        }

                        break;
                    case GH_Types.gh_bytearray:
                        if (item.HasIndex)
                        {
                            target.SetByteArray(item.Name, item.Index, item._bytearray);
                        }
                        else
                        {
                            target.SetByteArray(item.Name, item._bytearray);
                        }

                        break;
                    case GH_Types.gh_doublearray:
                        if (item.HasIndex)
                        {
                            target.SetDoubleArray(item.Name, item.Index, item._doublearray);
                        }
                        else
                        {
                            target.SetDoubleArray(item.Name, item._doublearray);
                        }

                        break;
                    case GH_Types.gh_drawing_point:
                        if (item.HasIndex)
                        {
                            target.SetDrawingPoint(item.Name, item.Index, item._drawing_point);
                        }
                        else
                        {
                            target.SetDrawingPoint(item.Name, item._drawing_point);
                        }

                        break;
                    case GH_Types.gh_drawing_pointf:
                        if (item.HasIndex)
                        {
                            target.SetDrawingPointF(item.Name, item.Index, item._drawing_pointf);
                        }
                        else
                        {
                            target.SetDrawingPointF(item.Name, item._drawing_pointf);
                        }

                        break;
                    case GH_Types.gh_drawing_size:
                        if (item.HasIndex)
                        {
                            target.SetDrawingSize(item.Name, item.Index, item._drawing_size);
                        }
                        else
                        {
                            target.SetDrawingSize(item.Name, item._drawing_size);
                        }

                        break;
                    case GH_Types.gh_drawing_sizef:
                        if (item.HasIndex)
                        {
                            target.SetDrawingSizeF(item.Name, item.Index, item._drawing_sizef);
                        }
                        else
                        {
                            target.SetDrawingSizeF(item.Name, item._drawing_sizef);
                        }

                        break;
                    case GH_Types.gh_drawing_rectangle:
                        if (item.HasIndex)
                        {
                            target.SetDrawingRectangle(item.Name, item.Index, item._drawing_rectangle);
                        }
                        else
                        {
                            target.SetDrawingRectangle(item.Name, item._drawing_rectangle);
                        }

                        break;
                    case GH_Types.gh_drawing_rectanglef:
                        if (item.HasIndex)
                        {
                            target.SetDrawingRectangleF(item.Name, item.Index, item._drawing_rectanglef);
                        }
                        else
                        {
                            target.SetDrawingRectangleF(item.Name, item._drawing_rectanglef);
                        }

                        break;
                    case GH_Types.gh_drawing_color:
                        if (item.HasIndex)
                        {
                            target.SetDrawingColor(item.Name, item.Index, item._drawing_color);
                        }
                        else
                        {
                            target.SetDrawingColor(item.Name, item._drawing_color);
                        }

                        break;
                    case GH_Types.gh_drawing_bitmap:
                        if (item.HasIndex)
                        {
                            target.SetDrawingBitmap(item.Name, item.Index, item._drawing_bitmap);
                        }
                        else
                        {
                            target.SetDrawingBitmap(item.Name, item._drawing_bitmap);
                        }

                        break;
                    case GH_Types.gh_point2d:
                        if (item.HasIndex)
                        {
                            target.SetPoint2D(item.Name, item.Index, item._point2d);
                        }
                        else
                        {
                            target.SetPoint2D(item.Name, item._point2d);
                        }

                        break;
                    case GH_Types.gh_point3d:
                        if (item.HasIndex)
                        {
                            target.SetPoint3D(item.Name, item.Index, item._point3d);
                        }
                        else
                        {
                            target.SetPoint3D(item.Name, item._point3d);
                        }

                        break;
                    case GH_Types.gh_point4d:
                        if (item.HasIndex)
                        {
                            target.SetPoint4D(item.Name, item.Index, item._point4d);
                        }
                        else
                        {
                            target.SetPoint4D(item.Name, item._point4d);
                        }

                        break;
                    case GH_Types.gh_interval1d:
                        if (item.HasIndex)
                        {
                            target.SetInterval1D(item.Name, item.Index, item._interval1d);
                        }
                        else
                        {
                            target.SetInterval1D(item.Name, item._interval1d);
                        }

                        break;
                    case GH_Types.gh_interval2d:
                        if (item.HasIndex)
                        {
                            target.SetInterval2D(item.Name, item.Index, item._interval2d);
                        }
                        else
                        {
                            target.SetInterval2D(item.Name, item._interval2d);
                        }

                        break;
                    case GH_Types.gh_line:
                        if (item.HasIndex)
                        {
                            target.SetLine(item.Name, item.Index, item._line);
                        }
                        else
                        {
                            target.SetLine(item.Name, item._line);
                        }

                        break;
                    case GH_Types.gh_boundingbox:
                        if (item.HasIndex)
                        {
                            target.SetBoundingBox(item.Name, item.Index, item._boundingbox);
                        }
                        else
                        {
                            target.SetBoundingBox(item.Name, item._boundingbox);
                        }

                        break;
                    case GH_Types.gh_plane:
                        if (item.HasIndex)
                        {
                            target.SetPlane(item.Name, item.Index, item._plane);
                        }
                        else
                        {
                            target.SetPlane(item.Name, item._plane);
                        }

                        break;
                    case GH_Types.gh_version:
                        if (item.HasIndex)
                        {
                            target.SetVersion(item.Name, item.Index, item._version);
                        }
                        else
                        {
                            target.SetVersion(item.Name, item._version);
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            //then copy chunks and call recursion
            foreach (GH_IChunk iChild in toCopy.Chunks)
            {
                GH_Chunk child = iChild as GH_Chunk;
                GH_Chunk copy;
                if (child.HasIndex)
                {
                    copy = target.CreateChunk(child.Name, child.Index) as GH_Chunk;
                }
                else
                {
                    copy = target.CreateChunk(child.Name) as GH_Chunk;
                }

                CopyChildren(child, copy);
            }
        }

        public abstract int Add(
            GH_Chunk definitionObjects, int objectIndex, BidirectionalGraph<Vertex, Edge> graph, Vertex vertex);

        public abstract bool Remove(Vertex vertex, BidirectionalGraph<Vertex, Edge> graph, List<Group> groups);
    }
}