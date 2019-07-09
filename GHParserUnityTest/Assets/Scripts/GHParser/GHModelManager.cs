using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Text;
using GHParser.GHElements;
using GHParser.Graph;
using GHParser.Utils;
using GH_IO.Serialization;
using HoloToolkit.Unity;
using QuickGraph;
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;
using Component = GHParser.GHElements.Component;

//TODO: Move drawing parts to other classes (use component classes)

public class GHModelManager : Singleton<GHModelManager>
{
    private ParametricModel _parametricModel;
    public string BaseTemplateFile = "/GH files/base.ghx";
    public GameObject ComponentPrefab;
    public GameObject CurvedLinePrefab;
    public Material DefaultGroupMaterial;
    public Transform DrawingSurface;
    public Transform GroupsContainer;
    public GameObject InputBlockPrefab;
    public GameObject InputPrefab;
    public GameObject LinePointPrefab;
    public Transform LinesContainer;
    public GameObject OutputBlockPrefab;
    public GameObject OutputPrefab;
    public string RelativePath = "/GH files/test.ghx";

    private void Start()
    {
        _parametricModel = new ParametricModel();
        _parametricModel.LoadFromGrasshopper(RelativePath);
        DrawBasicGraph(DrawingSurface, _parametricModel);
    }

    private void DrawBasicGraph(Transform drawingSurface, ParametricModel parametricModel)
    {
        BidirectionalGraph<Vertex, Edge> graph = parametricModel.Graph;
        List<Group> groups = parametricModel.Groups;

        Bounds drawingSurfaceBounds = drawingSurface.GetComponent<Renderer>().bounds;

        Vector3 dimensions = drawingSurfaceBounds.extents * 2;
        Vector3 bottomLeft = drawingSurfaceBounds.min;
        Vector3 topRight = drawingSurfaceBounds.max;

        RectangleF modelBounds = parametricModel.FindBounds();
        Debug.Log("Bounds in the model coordinate system: " + modelBounds);

        /*
        bl.x + ((c.x - m.x) / m.w) * d.x
        tr.z + ((c.y - m.y) / m.h) * d.z
         
         */

        foreach (Vertex vertex in graph.Vertices)
        {
            Component component = vertex.Chunk as Component;
            if (component != null)
            {
                GameObject cube = Instantiate(ComponentPrefab);
                cube.name = component.Guid.ToString();
                cube.AddComponent<InteractableVertex>().Vertex = vertex;
                Text textComponent = cube.GetComponentInChildren<Text>();

                textComponent.text =
                    string.IsNullOrEmpty(component.Nickname)
                        ? component.DefaultName
                        : component.Nickname;
                cube.transform.position = new Vector3(
                    bottomLeft.x + (component.VisualBounds.X - modelBounds.X + component.VisualBounds.Width / 2f) /
                    modelBounds.Width * dimensions.x,
                    drawingSurface.transform.position.y + .03f,
                    topRight.z - (component.VisualBounds.Y - modelBounds.Y + component.VisualBounds.Height / 2f) /
                    modelBounds.Height * dimensions.z);

                SetScaleWithDimensions(cube.transform, component.VisualBounds.Width / modelBounds.Width,
                    component.VisualBounds.Height / modelBounds.Height);

                cube.transform.SetParent(drawingSurface);

                if (component is IoComponent)
                {
                    textComponent.transform.Rotate(0, 0, 90f);

                    GameObject inputBlock = Instantiate(InputBlockPrefab);
                    inputBlock.transform.SetParent(cube.transform.Find("InputBlockSpot"));
                    Vector3 inputBlockExtents = inputBlock.GetComponentInChildren<Renderer>().bounds.extents;
                    inputBlock.transform.localPosition = new Vector3(-inputBlockExtents.x, 0f, 0f);
                    inputBlock.transform.localScale = Vector3.one;
                    List<Edge> inEdges = new List<Edge>(graph.InEdges(vertex));
                    for (int portIndex = 0; portIndex < inEdges.Count; portIndex++)
                    {
                        InputPort port = inEdges[portIndex].Source.Chunk as InputPort;
                        GameObject input = Instantiate(InputPrefab);
                        input.GetComponentInChildren<TextMesh>().text = port.DefaultName;
                        input.transform.SetParent(inputBlock.transform.Find("Input Spot"));
                        input.transform.localPosition = Vector3.zero;
                        input.transform.localPosition =
                            new Vector3(0f, 0f, 1 - (portIndex + 1f) / (inEdges.Count + 1)); //n/n+1

                        Transform sphere = input.transform.Find("Sphere");
                        sphere.name = port.Guid.ToString();
                        input.transform.localScale = Vector3.one;
                    }

                    GameObject outputBlock = Instantiate(OutputBlockPrefab);
                    outputBlock.transform.SetParent(cube.transform.Find("OutputBlockSpot"));
                    Vector3 outputBlockExtents = outputBlock.GetComponentInChildren<Renderer>().bounds.extents;
                    outputBlock.transform.localPosition = new Vector3(outputBlockExtents.x, 0f, 0f);
                    outputBlock.transform.localScale = Vector3.one;
                    List<Edge> outEdges = new List<Edge>(graph.OutEdges(vertex));
                    for (int portIndex = 0; portIndex < outEdges.Count; portIndex++)
                    {
                        OutputPort port = outEdges[portIndex].Target.Chunk as OutputPort;
                        GameObject output = Instantiate(OutputPrefab);
                        output.GetComponentInChildren<TextMesh>().text = port.DefaultName;
                        output.transform.SetParent(outputBlock.transform.Find("Output Spot"));
                        output.transform.localPosition = Vector3.zero;
                        output.transform.localPosition =
                            new Vector3(0f, 0f, 1 - (portIndex + 1f) / (outEdges.Count + 1)); //n/n+1
                        output.transform.localScale = Vector3.one;
                        Transform sphere = output.transform.Find("Sphere");
                        sphere.name = port.Guid.ToString();
                    }
                }
            }
        }

        foreach (Edge edge in graph.Edges)
        {
            Guid startGuid = edge.Source.Chunk.Guid;
            Guid endGuid = edge.Target.Chunk.Guid;

            Material white = new Material(Shader.Find("Unlit/Color")) {color = Color.white};
            AddLine(startGuid, endGuid, white, Easings.Functions.HermiteEaseInOut);
            ////Material magenta = new Material(Shader.Find("Unlit/Color")) { color = Color.magenta };
            ////AddLine(startObject.position, endObject.position, magenta, Easings.Functions.ElasticEaseInOut);
            ////Material green = new Material(Shader.Find("Unlit/Color")) { color = Color.green };
            ////AddLine(startObject.position, endObject.position, green, Easings.Functions.BackEaseInOut);
            ////Material black = new Material(Shader.Find("Unlit/Color")) { color = Color.black };
            ////AddLine(startObject.position, endObject.position, black, Easings.Functions.BounceEaseInOut);
            //Material blue = new Material(Shader.Find("Unlit/Color")) { color = Color.blue };
            //AddLine(startGuid, endGuid, blue, Easings.Functions.CircularEaseInOut);
            ////Material gray = new Material(Shader.Find("Unlit/Color")) { color = Color.gray };
            ////AddLine(startObject.position, endObject.position, gray, Easings.Functions.CubicEaseInOut);
            //Material red = new Material(Shader.Find("Unlit/Color")) { color = Color.red };
            //AddLine(startGuid, endGuid, red, Easings.Functions.ExponentialEaseInOut);
            ////Material cyan = new Material(Shader.Find("Unlit/Color")) { color = Color.cyan };
            ////AddLine(startObject.position, endObject.position, cyan, Easings.Functions.QuadraticEaseInOut);
            ////Material yellow = new Material(Shader.Find("Unlit/Color")) { color = Color.yellow };
            ////AddLine(startObject.position, endObject.position, yellow, Easings.Functions.QuinticEaseInOut);

            //Debug.DrawLine(startObject.position, endObject.position, Color.white, 100f);
        }

        OrderedDictionary orderedGroups = ParametricModel.OrderGroupsWithDepth(groups);
        List<Vector3> lines = new List<Vector3>(orderedGroups.Count * 24);
        foreach (Group group in orderedGroups.Keys)
        {
            //Debug.Log("Group " + group.Nickname + "(" + group.Guid + ")");
            float minX, maxX, minY, maxY, minZ, maxZ;
            minX = minY = minZ = float.PositiveInfinity;
            maxX = maxY = maxZ = float.NegativeInfinity;

            foreach (Guid guid in group.Constituents)
            {
                //Debug.Log("Searching for " + guid);
                Transform component = FindDescendant(drawingSurface, guid.ToString());
                if (component == null)
                {
                    Debug.LogError("Component " + guid + " not found even though Group " + group.Guid +
                                   " has a ref to it.");
                    //since GH does not care about groups that point to the guid of a component
                    //that does not exist (anymore), we'll just ignore it
                    continue;
                }


                Collider[] descendantsCollider = component.GetComponentsInChildren<Collider>();
                //Debug.Log("Found " + descendantsCollider.Length + " colliders");
                foreach (Collider descendantCollider in descendantsCollider)
                {
                    Bounds bounds = descendantCollider.bounds;
                    Vector3 corner1 = bounds.center - bounds.extents;
                    Vector3 corner2 = bounds.center + bounds.extents;
                    //since extents has a positive value on all axes, corner1 < corner2 for all axes
                    //(technically, extents could have a negative value but in that case something went wrong)
                    if (corner1.x < minX)
                    {
                        minX = corner1.x;
                    }
                    
                    if (corner2.x > maxX)
                    {
                        maxX = corner2.x;
                    }

                    if (corner1.y < minY)
                    {
                        minY = corner1.y;
                    }

                    if (corner2.y > maxY)
                    {
                        maxY = corner2.y;
                    }

                    if (corner1.z < minZ)
                    {
                        minZ = corner1.z;
                    }

                    if (corner2.z > maxZ)
                    {
                        maxZ = corner2.z;
                    }
                }
            }

            if (float.IsInfinity(minX) || float.IsInfinity(maxX) || float.IsInfinity(minY)
                || float.IsInfinity(maxY) || float.IsInfinity(minZ) || float.IsInfinity(maxZ))
            {
                Debug.LogError(
                    "parametric model apparently contained an empty group (" + group.Guid + "), we'll not render it.");
                continue;
            }

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            float xDiff = maxX - minX;
            float yDiff = maxY - minY;
            float zDiff = maxZ - minZ;
            int layer = (int) orderedGroups[group];
            float yScale = .5f / layer;
            float marginX = 20f / modelBounds.Width * dimensions.x;
            float marginZ = 20f / modelBounds.Height * dimensions.z;

            cube.transform.localScale = new Vector3(xDiff + marginX, yDiff * yScale, zDiff + marginZ);
            cube.transform.position = new Vector3(
                minX + xDiff / 2f,
                minY + yDiff * .5f * yScale,
                minZ + zDiff / 2f);
            cube.transform.SetParent(GroupsContainer);
            cube.name = group.Guid.ToString();
            cube.AddComponent<InteractableGroup>().Group = group;

            Material material;
            if (GHConstants.IsDefaultColor(group.Color))
            {
                material = DefaultGroupMaterial;
            }
            else
            {
                material = new Material(DefaultGroupMaterial)
                {
                    color = new Color(group.Color.R / 255f, group.Color.G / 255f, group.Color.B / 255f, 150f / 255)
                };
            }

            cube.GetComponent<Renderer>().material = material;

            Bounds groupBounds = cube.GetComponent<Renderer>().bounds;
            Vector3 p1 = groupBounds.center - groupBounds.extents;
            Vector3 p2 = p1 + Vector3.forward * groupBounds.extents.z * 2f;
            Vector3 p3 = p2 + Vector3.right * groupBounds.extents.x * 2f;
            Vector3 p4 = p1 + Vector3.right * groupBounds.extents.x * 2f;
            Vector3 p5 = p1 + Vector3.up * groupBounds.extents.y * 2f;
            Vector3 p6 = p2 + Vector3.up * groupBounds.extents.y * 2f;
            Vector3 p7 = groupBounds.center + groupBounds.extents;
            Vector3 p8 = p4 + Vector3.up * groupBounds.extents.y * 2f;

            lines.Add(p1);
            lines.Add(p2);
            lines.Add(p2);
            lines.Add(p3);
            lines.Add(p3);
            lines.Add(p4);
            lines.Add(p4);
            lines.Add(p1);
            lines.Add(p1);
            lines.Add(p5);
            lines.Add(p2);
            lines.Add(p6);
            lines.Add(p3);
            lines.Add(p7);
            lines.Add(p4);
            lines.Add(p8);
            lines.Add(p5);
            lines.Add(p6);
            lines.Add(p6);
            lines.Add(p7);
            lines.Add(p7);
            lines.Add(p8);
            lines.Add(p8);
            lines.Add(p5);

            /*for (int i = 0; i < lines.Count; i = i + 2)
            {
                Debug.Log(lines[i] + "->" + lines[i + 1]);
            }*/
        }

        LineDrawer.Instance.Lines = lines;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100.0f))
            {
                if (hit.transform != null)
                {
                    Debug.Log("hit " + hit.transform.name);
                }
                else
                {
                    return;
                }
            }

            if (Input.GetMouseButtonDown(0)) //left click
            {
                _parametricModel.SaveToGrasshopper("/GH files/test-simplified-copy.ghx");
            }
            else //right click
            {
                InteractableVertex interactableVertex = hit.transform.GetComponent<InteractableVertex>();
                InteractableGroup interactableGroup = hit.transform.GetComponent<InteractableGroup>();
                LineCollider lineCollider = hit.transform.parent.GetComponent<LineCollider>();
                bool isInput = hit.transform.parent.name.Contains("Input Parameter");
                bool isOutput = hit.transform.parent.name.Contains("Output Parameter");

                //=================== Component (IOC/Primitive) ====================
                if (interactableVertex != null)
                {
                    Debug.Log("Attempting to remove vertex " + interactableVertex.Vertex);

                    if (interactableVertex.Vertex.Chunk.Remove(interactableVertex.Vertex, _parametricModel.Graph,
                        _parametricModel.Groups))
                    {
                        StartCoroutine(RedrawGraph(DrawingSurface));
                    }
                }
                //=================== Group ====================
                else if (interactableGroup != null)
                {
                    Debug.Log("Attempting to remove group " + interactableGroup.Group);
                    if (interactableGroup.Group.Remove(null, _parametricModel.Graph, _parametricModel.Groups))
                    {
                        StartCoroutine(RedrawGraph(DrawingSurface));
                    }
                }
                //=================== Link ====================
                else if (lineCollider != null)
                {
                    Debug.Log("Attempting to delete line " + lineCollider.name);
                    if (_parametricModel.DeleteEdge(lineCollider.name))
                    {
                        StartCoroutine(RedrawGraph(DrawingSurface));
                    }
                }
                //=================== Input port ====================
                else if (isInput)
                {
                    Debug.Log("Attempting to delete all inEdges from " + hit.transform.name);
                    if (_parametricModel.UnplugInput(hit.transform.name))
                    {
                        StartCoroutine(RedrawGraph(DrawingSurface));
                    }
                }
                //=================== Output port ====================
                else if (isOutput)
                {
                    Debug.Log("Attempting to delete all outEdges from " + hit.transform.name);
                    if (_parametricModel.UnplugOutput(hit.transform.name))
                    {
                        StartCoroutine(RedrawGraph(DrawingSurface));
                    }
                }
            }
        }
    }

    private IEnumerator RedrawGraph(Transform drawingSurface)
    {
        RemoveGraph(drawingSurface);
        yield return StartCoroutine(DelayedDrawBasicGraph(drawingSurface, _parametricModel, .01f));
    }

    private void RemoveGraph(Transform drawingSurface)
    {
        foreach (Transform child in drawingSurface)
        {
            if (!child.CompareTag("DoNotDelete"))
            {
                Destroy(child.gameObject);
            }
            else
            {
                foreach (Transform grandChild in child.transform)
                {
                    Destroy(grandChild.gameObject);
                }
            }
        }

        LineDrawer.Instance.Lines.Clear();
    }

    private IEnumerator DelayedDrawBasicGraph(Transform drawingSurface, ParametricModel parametricModel, float delay)
    {
        yield return new WaitForSeconds(delay);
        DrawBasicGraph(drawingSurface, parametricModel);
    }

    private void AddLine(Guid startGuid, Guid endGuid, Material lineMaterial,
        Easings.Functions easingType = Easings.Functions.Linear)
    {
        Transform startObject = FindDescendant(DrawingSurface, startGuid.ToString());
        Transform endObject = FindDescendant(DrawingSurface, endGuid.ToString());
        Vector3 start = startObject.position;
        Vector3 end = endObject.position;

        GameObject line = Instantiate(CurvedLinePrefab, Vector3.zero, Quaternion.identity, LinesContainer);
        line.GetComponent<Renderer>().material = lineMaterial;
        line.name = startGuid + "->" + endGuid;

        Instantiate(LinePointPrefab, start, Quaternion.identity, line.transform);

        Instantiate(LinePointPrefab, EaseYZ(start, end, .05f, easingType), Quaternion.identity, line.transform);
        Instantiate(LinePointPrefab, EaseYZ(start, end, .1f, easingType), Quaternion.identity, line.transform);
        Instantiate(LinePointPrefab, EaseYZ(start, end, .2f, easingType), Quaternion.identity, line.transform);
        Instantiate(LinePointPrefab, EaseYZ(start, end, .4f, easingType), Quaternion.identity, line.transform);
        Instantiate(LinePointPrefab, EaseYZ(start, end, .6f, easingType), Quaternion.identity, line.transform);
        Instantiate(LinePointPrefab, EaseYZ(start, end, .8f, easingType), Quaternion.identity, line.transform);
        Instantiate(LinePointPrefab, EaseYZ(start, end, .9f, easingType), Quaternion.identity, line.transform);
        Instantiate(LinePointPrefab, EaseYZ(start, end, .95f, easingType), Quaternion.identity, line.transform);

        Instantiate(LinePointPrefab, end, Quaternion.identity, line.transform);
    }

    private void AddLine(Vector3 start, Vector3 end, Material lineMaterial,
        Easings.Functions easingType = Easings.Functions.Linear)
    {
        Debug.Log(start + "->" + end);

        GameObject line = Instantiate(CurvedLinePrefab, Vector3.zero, Quaternion.identity, LinesContainer);
        line.GetComponent<Renderer>().material = lineMaterial;

        Instantiate(LinePointPrefab, start, Quaternion.identity, line.transform);

        Instantiate(LinePointPrefab, EaseYZ(start, end, .05f, easingType), Quaternion.identity, line.transform);
        Instantiate(LinePointPrefab, EaseYZ(start, end, .1f, easingType), Quaternion.identity, line.transform);
        Instantiate(LinePointPrefab, EaseYZ(start, end, .2f, easingType), Quaternion.identity, line.transform);
        Instantiate(LinePointPrefab, EaseYZ(start, end, .4f, easingType), Quaternion.identity, line.transform);
        Instantiate(LinePointPrefab, EaseYZ(start, end, .6f, easingType), Quaternion.identity, line.transform);
        Instantiate(LinePointPrefab, EaseYZ(start, end, .8f, easingType), Quaternion.identity, line.transform);
        Instantiate(LinePointPrefab, EaseYZ(start, end, .9f, easingType), Quaternion.identity, line.transform);
        Instantiate(LinePointPrefab, EaseYZ(start, end, .95f, easingType), Quaternion.identity, line.transform);

        Instantiate(LinePointPrefab, end, Quaternion.identity, line.transform);
    }

    private void AddLine(Vector3 start, Vector3 end, Color color,
        Easings.Functions easingType = Easings.Functions.Linear)
    {
        Material material = new Material(Shader.Find("Unlit/Color")) {color = color};
        AddLine(start, end, material, easingType);
    }

    private void AddLine(Vector3 start, Vector3 end, Easings.Functions easingType = Easings.Functions.Linear)
    {
        AddLine(start, end, Color.white, easingType);
    }

    private Vector3 EaseYZ(Vector3 start, Vector3 end, float progress, Easings.Functions type)
    {
        return new Vector3(
            Mathf.Lerp(start.x, end.x, progress),
            Mathf.Lerp(start.y, end.y, Easings.Interpolate(progress, type)),
            Mathf.Lerp(start.z, end.z, Easings.Interpolate(progress, type)));
    }

    private static Transform FindDescendant(Transform parent, string name)
    {
        if (parent.name.Equals(name))
        {
            return parent;
        }

        foreach (Transform child in parent)
        {
            Transform result = FindDescendant(child, name);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    public void SetScaleWithDimensions(Transform target, float targetXLength, float targetYLength)
    {
        float currentXLength = target.GetComponent<Renderer>().bounds.size.x;
        float currentYLength = target.GetComponent<Renderer>().bounds.size.z;
        Vector3 rescale = target.localScale;
        rescale.x = targetXLength * rescale.x / currentXLength;
        rescale.z = targetYLength * rescale.z / currentYLength;
        target.localScale = rescale;
    }

    public void SetScaleWithDimensions(Transform target, float targetXLength, float targetYLength, float targetZLength)
    {
        float currentXLength = target.GetComponent<Renderer>().bounds.size.x;
        float currentYLength = target.GetComponent<Renderer>().bounds.size.y;
        float currentZLength = target.GetComponent<Renderer>().bounds.size.z;
        Vector3 rescale = target.localScale;
        rescale.x = targetXLength * rescale.x / currentXLength;
        rescale.y = targetYLength * rescale.y / currentYLength;
        rescale.z = targetZLength * rescale.z / currentZLength;
        target.localScale = rescale;
    }
}