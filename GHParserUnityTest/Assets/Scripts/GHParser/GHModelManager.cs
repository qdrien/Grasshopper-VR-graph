using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using GHParser.GHElements;
using GHParser.Graph;
using GHParser.Utils;
using GH_IO.Serialization;
using HoloToolkit.Unity;
using QuickGraph;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using VRTK;
using Color = UnityEngine.Color;
using Component = GHParser.GHElements.Component;

//TODO: Move drawing parts to other classes (use component classes)

public class GHModelManager : Singleton<GHModelManager>
{
    private ParametricModel _parametricModel;
    public string BaseTemplateFile = "/GH files/base.ghx";
    public GameObject ComponentPrefab;
    public GameObject CurvedLinePrefab;
    public GameObject PlaceHolderPrefab;
    public Material DefaultGroupMaterial;
    public Transform DrawingSurface;
    public Transform Toolbar;
    public Transform GroupsContainer;
    public GameObject InputBlockPrefab;
    public GameObject InputPrefab;
    public GameObject InputPlaceholderPrefab;
    public GameObject LinePointPrefab;
    public Transform LinesContainer;
    public GameObject OutputBlockPrefab;
    public GameObject OutputPrefab;
    public GameObject OutputPlaceholderPrefab;
    public string RelativePath = "/GH files/test.ghx";
    public string ComponentTemplatesFile;
    public float PlaceholderSpotLength = .2f;

    private void Start()
    {
        _parametricModel = new ParametricModel();
        _parametricModel.LoadComponentTemplates(ComponentTemplatesFile);
        _parametricModel.LoadFromGrasshopper(RelativePath);
        DrawBasicGraph(DrawingSurface, _parametricModel);
        Quaternion savedRotation = DrawingSurface.rotation;
        DrawingSurface.rotation = Quaternion.identity;
        for (int i = 0; i < _parametricModel.ComponentTemplates.Count; i++)
        {
            IoComponentTemplate template = _parametricModel.ComponentTemplates[i];
            AddComponentToToolbar(template, i);
        }

        DrawingSurface.rotation = savedRotation;
    }

    private void DrawBasicGraph(Transform drawingSurface, ParametricModel parametricModel)
    {
        Vector3 bottomLeft = drawingSurface.TransformPoint(new Vector3(-.5f, .5f, -.5f));
        Vector3 topRight = drawingSurface.TransformPoint(new Vector3(.5f, .5f, .5f));
        Vector3 dimensions = topRight - bottomLeft;
        
        BidirectionalGraph<Vertex, Edge> graph = parametricModel.Graph;
        List<Group> groups = parametricModel.Groups;

        RectangleF modelBounds = parametricModel.FindBounds();
        Debug.Log("Bounds in the model coordinate system: " + modelBounds);

        Quaternion savedRotation = drawingSurface.rotation;
        drawingSurface.rotation = Quaternion.identity;

        foreach (Vertex vertex in graph.Vertices)
        {
            AddComponent(drawingSurface, vertex, modelBounds, graph);
        }

        drawingSurface.rotation = savedRotation;
        
        RefreshEdges(graph);
        
        OrderedDictionary orderedGroups = ParametricModel.OrderGroupsWithDepth(groups);
        List<Vector3> lines = new List<Vector3>(orderedGroups.Count * 24);
        foreach (Group group in orderedGroups.Keys)
        {
            Debug.LogError("Ignoring groups for testing purposes");
            break;
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

        StartCoroutine(SetGroupOutlines(lines, 1f)); //in case the following line throws an NPE
        //(happens when the VR setup did not have enough time to be initialized)
        //LineDrawer.Instance.Lines = lines;
    }

    private GameObject AddComponent(Transform drawingSurface, Vertex vertex, RectangleF modelBounds, BidirectionalGraph<Vertex, Edge> graph)
    {
        if (drawingSurface.rotation != Quaternion.identity)
        {
            Debug.LogWarning("Drawing surface is rotated, this will produce unexpected results");
        }
        
        Component component = vertex.Chunk as Component;
        if (component != null)
        {
            GameObject cube = Instantiate(ComponentPrefab, drawingSurface, true);
            cube.name = component.Guid.ToString();
            cube.AddComponent<InteractableVertex>().Vertex = vertex;
            Text textComponent = cube.GetComponentInChildren<Text>();

            textComponent.text =
                string.IsNullOrEmpty(component.Nickname)
                    ? component.DefaultName
                    : component.Nickname;
            
            cube.transform.position = drawingSurface.TransformPoint(new Vector3(
                (component.VisualBounds.X - modelBounds.X + component.VisualBounds.Width / 2f) / modelBounds.Width -.5f,
                1f,
                1-((component.VisualBounds.Y - modelBounds.Y + component.VisualBounds.Height / 2f) / modelBounds.Height) -.5f));

            cube.transform.localScale = new Vector3( //*.7f is a slight adjustment to take ports into account
                component.VisualBounds.Width * .7f / modelBounds.Width,
                1f,
                component.VisualBounds.Height * .7f / modelBounds.Height);

            
            if (component is IoComponent)
            {
                textComponent.transform.Rotate(0, 0, 90f);

                GameObject inputBlock = Instantiate(InputBlockPrefab, cube.transform.Find("InputBlockSpot"), true);
                Vector3 inputBlockExtents = inputBlock.GetComponentInChildren<Renderer>().bounds.extents;
                inputBlock.transform.localPosition = new Vector3(-inputBlockExtents.x, 0f, 0f);
                inputBlock.transform.localScale = Vector3.one;
                List<Edge> inEdges = new List<Edge>(graph.InEdges(vertex));
                for (int portIndex = 0; portIndex < inEdges.Count; portIndex++)
                {
                    InputPort port = inEdges[portIndex].Source.Chunk as InputPort;
                    GameObject input = Instantiate(InputPrefab, inputBlock.transform.Find("Input Spot"), true);
                    input.GetComponentInChildren<TextMesh>().text = port.DefaultName;
                    input.transform.localPosition =
                        new Vector3(0f, 0f, 1 - (portIndex + 1f) / (inEdges.Count + 1)); //n/n+1

                    Transform sphere = input.transform.Find("Sphere");
                    sphere.name = port.Guid.ToString();
                    input.transform.localScale = Vector3.one;
                }

                GameObject outputBlock = Instantiate(OutputBlockPrefab, cube.transform.Find("OutputBlockSpot"), true);
                Vector3 outputBlockExtents = outputBlock.GetComponentInChildren<Renderer>().bounds.extents;
                outputBlock.transform.localPosition = new Vector3(outputBlockExtents.x, 0f, 0f);
                outputBlock.transform.localScale = Vector3.one;
                List<Edge> outEdges = new List<Edge>(graph.OutEdges(vertex));
                for (int portIndex = 0; portIndex < outEdges.Count; portIndex++)
                {
                    OutputPort port = outEdges[portIndex].Target.Chunk as OutputPort;
                    GameObject output = Instantiate(OutputPrefab, outputBlock.transform.Find("Output Spot"), true);
                    output.GetComponentInChildren<TextMesh>().text = port.DefaultName;
                    output.transform.localPosition =
                        new Vector3(0f, 0f, 1 - (portIndex + 1f) / (outEdges.Count + 1)); //n/n+1
                    output.transform.localScale = Vector3.one;
                    Transform sphere = output.transform.Find("Sphere");
                    sphere.name = port.Guid.ToString();
                }
            }
            //"only used as input" components
            else if (component is NumberSliderComponent || component is BooleanToggleComponent)
            {
                GameObject outputBlock = Instantiate(OutputBlockPrefab, cube.transform.Find("OutputBlockSpot"), true);
                Vector3 outputBlockExtents = outputBlock.GetComponentInChildren<Renderer>().bounds.extents;
                outputBlock.transform.localPosition = new Vector3(outputBlockExtents.x, 0f, 0f);
                outputBlock.transform.localScale = Vector3.one;
                
                GameObject output = Instantiate(OutputPrefab, outputBlock.transform.Find("Output Spot"), true);
                output.GetComponentInChildren<TextMesh>().text = "";
                output.transform.localPosition = new Vector3(0f, 0f, .5f); //n/n+1
                
                Transform sphere = output.transform.Find("Sphere");
                sphere.name = cube.name;
                output.transform.localScale = Vector3.one;
                sphere.transform.localScale = new Vector3(sphere.transform.localScale.x, sphere.transform.localScale.y, .5f);
            }
            //"only used as output" components
            //uncomment and code the following if any of those gets used (e.g. cluster output, galapagos?)
            /*else if (component is <class here>)
            {
                
            }*/
            //"input/output" components
            else if (component is PanelComponent)
            {
                GameObject inputBlock = Instantiate(InputBlockPrefab, cube.transform.Find("InputBlockSpot"), true);
                Vector3 inputBlockExtents = inputBlock.GetComponentInChildren<Renderer>().bounds.extents;
                inputBlock.transform.localPosition = new Vector3(-inputBlockExtents.x, 0f, 0f);
                inputBlock.transform.localScale = Vector3.one;
                
                GameObject input = Instantiate(InputPrefab, inputBlock.transform.Find("Input Spot"), true);
                input.GetComponentInChildren<TextMesh>().text = "";
                input.transform.localPosition = new Vector3(0f, 0f, .5f);

                Transform sphere = input.transform.Find("Sphere");
                sphere.name = cube.name;
                input.transform.localScale = Vector3.one;
                
                
                GameObject outputBlock = Instantiate(OutputBlockPrefab, cube.transform.Find("OutputBlockSpot"), true);
                Vector3 outputBlockExtents = outputBlock.GetComponentInChildren<Renderer>().bounds.extents;
                outputBlock.transform.localPosition = new Vector3(outputBlockExtents.x, 0f, 0f);
                outputBlock.transform.localScale = Vector3.one;
                
                GameObject output = Instantiate(OutputPrefab, outputBlock.transform.Find("Output Spot"), true);
                output.GetComponentInChildren<TextMesh>().text = "";
                output.transform.localPosition = new Vector3(0f, 0f, .5f);
                
                Transform sphere2 = output.transform.Find("Sphere");
                sphere2.name = cube.name;
                output.transform.localScale = Vector3.one;
            }

            GameObject testCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testCube.transform.name = "TestCollider";
            testCube.transform.SetParent(cube.transform);
            testCube.transform.localScale = Vector3.one;
            testCube.transform.localPosition = Vector3.zero;
            
            
            return cube;
        }
        return null;
    }

    public void RefreshEdges()
    {
        Debug.Log("Refreshing edges");
        BidirectionalGraph<Vertex,Edge> graph = _parametricModel.Graph;
        RefreshEdges(graph);
    }

    //Following 2 methods are probably unnecessary since we can parse the input value
    /*public void AttachComponent(string componentName, string type, float value)
    {
        
    }
    
    public void AttachComponent(string componentName, string type, bool value)
    {
        
    }*/

    /// <summary>
    /// Trying to create a new component (input type string could match multiple templates as well as a primitive).
    /// GThis method handles the routing accordingly.
    /// </summary>
    /// <param name="componentName"></param>
    /// <param name="type"></param>
    /// <param name="value"></param>
    public void AttachComponent(string type, string componentName = "", string value = "")
    {
        //TODO: should check whether it could be a primitive component here and update the conditions below
        List<IoComponentTemplate> templates = _parametricModel.ComponentTemplates.FindAll(o => o.TypeName.Equals(type));
        
        if (templates.Count <= 0)
        {
            //if(not a primitive)
            //{
                Debug.LogError("Did not find a template for type: " + type);
            //}
            //else
            //{
            //create primitive here
            //}
        }
        else
        {
            if (templates.Count == 1) // && not a primitive
            {
                StartCoroutine(AttachTemplateComponent(templates.First().TypeGuid, componentName));
            }
            else
            {
                foreach (IoComponentTemplate template in templates) 
                {
                    //TODO: show a list here and let the user choose, then call AttachComponent(name,guid,value) with the selected type guid
                    //(should include primitives that also match if any)
                    Debug.LogWarning("Multiple components with the same type name were found in the template library, aborting for now.");
                }   
            }
        }
    }

    /// <summary>
    /// Creating a new IOComponent from a template.
    /// </summary>
    /// <param name="componentName"></param>
    /// <param name="guid"></param>
    /// <returns></returns>
    public IEnumerator AttachTemplateComponent(Guid guid, string componentName = "")
    {
        yield return new WaitForEndOfFrame(); //todo: probably not necessary anymore (can also change the method's return type if so)

        //TODO: the user could have an object attached already or could have selected a port, should first abort those
        //if an object is already attached, could also attach this new one to the other controller
        GameObject rightControllerAlias = VRTK_DeviceFinder.GetControllerRightHand();
        Debug.Log(rightControllerAlias.name);
        
        //todo: should use "value" when (1) it is not empty and (2) the value is valid for that component

        IoComponentTemplate template = _parametricModel.ComponentTemplates.Find(o => o.TypeGuid.Equals(guid));

        GameObject newComponent = CreateIoComponentVertex(template, componentName);
        
        VRTK_InteractTouch interactTouch = rightControllerAlias.GetComponent<VRTK_InteractTouch>();
        VRTK_InteractGrab interactGrab = rightControllerAlias.GetComponent<VRTK_InteractGrab>();

        newComponent.transform.position = rightControllerAlias.transform.position;

        interactTouch.ForceStopTouching();
        interactTouch.ForceTouch(newComponent);
        interactGrab.AttemptGrab();
        
    }

    private GameObject CreateIoComponentVertex(IoComponentTemplate template, string componentName)
    {
        Guid instanceGuid = Guid.NewGuid();
        IoComponent ioComponent = new IoComponent(template.DefaultName, template.TypeGuid, template.TypeName, template.VisualBounds, instanceGuid, componentName);
        Vertex newVertex = new Vertex(ioComponent);
        _parametricModel.Graph.AddVertex(newVertex);
        
        foreach (InputPort inputPort in template.InputPorts)
        {
            InputPort newInputPort = new InputPort(Guid.NewGuid(), inputPort.Nickname, inputPort.DefaultName, inputPort.VisualBounds);
            _parametricModel.Graph.AddVertex(new Vertex(newInputPort));
            _parametricModel.AddEdge(newInputPort.Guid.ToString(), instanceGuid.ToString());
        }
        
        foreach (OutputPort outputPort in template.OutputPorts)
        {
            OutputPort newOutputPort = new OutputPort(Guid.NewGuid(), outputPort.Nickname, outputPort.DefaultName, outputPort.VisualBounds);
            _parametricModel.Graph.AddVertex(new Vertex(newOutputPort));
            _parametricModel.AddEdge(instanceGuid.ToString(), newOutputPort.Guid.ToString());
        }
        
        Quaternion savedRotation = DrawingSurface.rotation;
        DrawingSurface.rotation = Quaternion.identity;

        GameObject newComponent = AddComponent(DrawingSurface, newVertex, _parametricModel.FindBounds(),
            _parametricModel.Graph);

        DrawingSurface.rotation = savedRotation;

        return newComponent;
    }

    /// <summary>
    /// Selective removal of edges related to a specific vertex.
    /// (only this vertex's edges and its ports' edges if this is an IOComponent)
    /// </summary>
    /// <param name="vertex"></param>
    public void RemoveEdges(Vertex vertex)
    {
        string targetVertex = vertex.Chunk.Guid.ToString();
        if (vertex.Chunk is IoComponent)
        {
            foreach (Transform child in LinesContainer.transform)
            {
                if (child.name.Contains(targetVertex))
                {
                    string edge = child.name;
                    if (edge.StartsWith(targetVertex))
                    {
                        string outputPort = edge.Split(new []{"->"}, StringSplitOptions.None)[1];
                        foreach (Transform potentialOutEdge in LinesContainer.transform)
                        {
                            if (potentialOutEdge.name.StartsWith(outputPort))
                            {
                                foreach (Transform potentialOutEdgeChild in potentialOutEdge)
                                {
                                    Destroy(potentialOutEdgeChild.gameObject);
                                }
                                Destroy(potentialOutEdge.gameObject);
                            }
                        }
                    }
                    else if (edge.EndsWith(targetVertex))
                    {
                        string inputPort = edge.Split(new []{"->"}, StringSplitOptions.None)[0];
                        foreach (Transform potentialInEdge in LinesContainer.transform)
                        {
                            if (potentialInEdge.name.EndsWith(inputPort))
                            {
                                foreach (Transform potentialInEdgeChild in potentialInEdge)
                                {
                                    Destroy(potentialInEdgeChild.gameObject);
                                }
                                Destroy(potentialInEdge.gameObject);
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("Something went wrong, should have either started or ended with the target vertex's id");
                    }

                    foreach (Transform grandChild in child)
                    {
                        Destroy(grandChild.gameObject);
                    }
                    Destroy(child.gameObject);
                }
            }
        }
        else if (vertex.Chunk is PrimitiveComponent)
        {
            foreach (Transform child in LinesContainer.transform)
            {
                if (child.name.Contains(targetVertex))
                {
                    foreach (Transform grandChild in child)
                    {
                        Destroy(grandChild.gameObject);
                    }
                    Destroy(child.gameObject);
                }
            }
        }
    }
    
    /// <summary>
    /// Selective refreshing of edges related to a specific vertex.
    /// (only this vertex's edges and its ports' edges if this is an IOComponent)
    /// </summary>
    /// <param name="vertex"></param>
    public void RefreshEdges(Vertex vertex)
    {
        List<string> toRefresh = new List<string>();

        string targetVertex = vertex.Chunk.Guid.ToString();
        if (vertex.Chunk is IoComponent)
        {
            foreach (Transform child in LinesContainer.transform)
            {
                if (child.name.Contains(targetVertex))
                {
                    string edge = child.name;
                    toRefresh.Add(edge);
                    if (edge.StartsWith(targetVertex))
                    {
                        string outputPort = edge.Split(new []{"->"}, StringSplitOptions.None)[1];
                        foreach (Transform potentialOutEdge in LinesContainer.transform)
                        {
                            if (potentialOutEdge.name.StartsWith(outputPort))
                            {
                                toRefresh.Add(potentialOutEdge.name);
                                foreach (Transform potentialOutEdgeChild in potentialOutEdge)
                                {
                                    Destroy(potentialOutEdgeChild.gameObject);
                                }
                                Destroy(potentialOutEdge.gameObject);
                            }
                        }
                    }
                    else if (edge.EndsWith(targetVertex))
                    {
                        string inputPort = edge.Split(new []{"->"}, StringSplitOptions.None)[0];
                        foreach (Transform potentialInEdge in LinesContainer.transform)
                        {
                            if (potentialInEdge.name.EndsWith(inputPort))
                            {
                                toRefresh.Add(potentialInEdge.name);
                                foreach (Transform potentialInEdgeChild in potentialInEdge)
                                {
                                    Destroy(potentialInEdgeChild.gameObject);
                                }
                                Destroy(potentialInEdge.gameObject);
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("Something went wrong, should have either started or ended with the target vertex's id");
                    }

                    foreach (Transform grandChild in child)
                    {
                        Destroy(grandChild.gameObject);
                    }
                    Destroy(child.gameObject);
                }
            }
        }
        else if (vertex.Chunk is PrimitiveComponent)
        {
            foreach (Transform child in LinesContainer.transform)
            {
                if (child.name.Contains(targetVertex))
                {
                    toRefresh.Add(child.name);
                    
                    foreach (Transform grandChild in child)
                    {
                        Destroy(grandChild.gameObject);
                    }
                    Destroy(child.gameObject);
                }
            }
        }
        
        //After the deletion process because otherwise AddLine would modify the LinesContainer we are iterating on
        foreach (string edge in toRefresh)
        {
            string[] endpoints = edge.Split(new[] {"->"}, StringSplitOptions.None);
            Guid startGuid = new Guid(endpoints[0]);
            Guid endGuid = new Guid(endpoints[1]);
            Material white = new Material(Shader.Find("Unlit/Color")) {color = Color.white};
            AddLine(startGuid, endGuid, white, Easings.Functions.HermiteEaseInOut);
        }
    }

    public void RefreshEdges(BidirectionalGraph<Vertex, Edge> graph)
    {
        foreach (Transform child in LinesContainer.transform)
        {
            foreach (Transform grandChild in child)
            {
                Destroy(grandChild.gameObject);
            }
            Destroy(child.gameObject);
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
    }

    IEnumerator SetGroupOutlines(List<Vector3> lines, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        LineDrawer.Instance.Lines = lines;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A)) //TODO: remove this
        {
            AttachComponent("Line", "testname", "testvalue"); 
        }
        
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
                    if (_parametricModel.RemoveEdge(lineCollider.name))
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

    public void RemoveEdge(string edge)
    {
        _parametricModel.RemoveEdge(edge);

        foreach (Transform line in LinesContainer.transform)
        {
            if (line.name.Equals(edge))
            {
                foreach (Transform child in line)
                {
                    Destroy(child.gameObject);
                }

                Destroy(line.gameObject);
            }
        }
            
    }

    public void RemoveVertex(Vertex vertex)
    {
        _parametricModel.RemoveVertex(vertex);
        RemoveEdges(vertex);
    }

    public void AddEdge(GameObject start, GameObject end)
    {
        Debug.Log("would create an edge from " + start.name + " to " + end.name);
        bool wasValid = _parametricModel.AddEdge(start.name, end.name);
        if (!wasValid)
        {
            Debug.LogError("Could not add the edge, the parametric model refused it (e.g. because both ports are from the same component).");
            return;
        }

        Material white = new Material(Shader.Find("Unlit/Color")) {color = Color.white};
        AddLine(new Guid(start.name), new Guid(end.name), white, Easings.Functions.HermiteEaseInOut);
    }

    public void MoveVertex(Vertex vertex, Vector3 position)
    {
        List<Group> groups = _parametricModel.Groups; //todo: should recalculate the groups as well

        Bounds drawingSurfaceBounds = DrawingSurface.GetComponent<Renderer>().bounds;

        Vector3 dimensions = drawingSurfaceBounds.extents * 2;
        Vector3 bottomLeft = drawingSurfaceBounds.min;
        Vector3 topRight = drawingSurfaceBounds.max;

        RectangleF modelBounds = _parametricModel.FindBounds();
        
        Component component = vertex.Chunk as Component;
        if (component != null)
        {
            component.VisualBounds = new RectangleF(
                ((position.x - bottomLeft.x) / dimensions.x) * modelBounds.Width + modelBounds.X - component.VisualBounds.Width/2f, 
                -((position.z - topRight.z) / dimensions.z) * modelBounds.Height + modelBounds.Y - component.VisualBounds.Height/2f,
                component.VisualBounds.Width,
                component.VisualBounds.Height);
        }

        RefreshEdges(vertex);
    }

    public void AddComponentToToolbar(IoComponentTemplate template, int i)  //TODO: use a more generic type here to include primitives?
    {//TODO: extract parts to methods that can also be called from AddComponent(), especially the if/elif/... part
        if (DrawingSurface.rotation != Quaternion.identity)
        {
            Debug.LogWarning("Drawing surface is rotated, this will produce unexpected results");
        }
        
        RectangleF modelBounds = _parametricModel.FindBounds();

        GameObject cube = Instantiate(PlaceHolderPrefab, DrawingSurface, true);
        cube.name = template.TypeGuid.ToString();
        Text textComponent = cube.GetComponentInChildren<Text>();

        textComponent.text = template.TypeName;

        //TODO: extract that and call the extracted method in AddComponent() as well
        cube.transform.localScale = new Vector3( //*.7f is a slight adjustment to take ports into account
            template.VisualBounds.Width * .7f / modelBounds.Width,
            1f,
            template.VisualBounds.Height * .7f / modelBounds.Height);

        cube.transform.parent = Toolbar;
        cube.transform.localPosition = new Vector3(-.5f, 1f, 0f);
        cube.transform.Translate(PlaceholderSpotLength/2f + i*PlaceholderSpotLength, 0f, 0f);

        if (template is IoComponentTemplate)
        {
            textComponent.transform.Rotate(0, 0, 90f);
            
            GameObject inputBlock = Instantiate(InputBlockPrefab, cube.transform.Find("InputBlockSpot"), true);
            Vector3 inputBlockExtents = inputBlock.GetComponentInChildren<Renderer>().bounds.extents;
            inputBlock.transform.localPosition = new Vector3(-inputBlockExtents.x, 0f, 0f);
            inputBlock.transform.localScale = Vector3.one;
            List<InputPort> inputPorts = template.InputPorts;
            for (int portIndex = 0; portIndex < inputPorts.Count; portIndex++)
            {
                InputPort port = inputPorts[portIndex];
                GameObject input = Instantiate(InputPlaceholderPrefab, inputBlock.transform.Find("Input Spot"), true);
                input.GetComponentInChildren<TextMesh>().text = port.DefaultName;
                input.transform.localPosition =
                    new Vector3(0f, 0f, 1 - (portIndex + 1f) / (inputPorts.Count + 1)); //n/n+1

                Transform sphere = input.transform.Find("Sphere");
                sphere.name = port.DefaultName;
                input.transform.localScale = Vector3.one;
            }

            GameObject outputBlock = Instantiate(OutputBlockPrefab, cube.transform.Find("OutputBlockSpot"), true);
            Vector3 outputBlockExtents = outputBlock.GetComponentInChildren<Renderer>().bounds.extents;
            outputBlock.transform.localPosition = new Vector3(outputBlockExtents.x, 0f, 0f);
            outputBlock.transform.localScale = Vector3.one;
            List<OutputPort> outputPorts = template.OutputPorts;
            for (int portIndex = 0; portIndex < outputPorts.Count; portIndex++)
            {
                OutputPort port = outputPorts[portIndex];
                GameObject output = Instantiate(OutputPlaceholderPrefab, outputBlock.transform.Find("Output Spot"), true);
                output.GetComponentInChildren<TextMesh>().text = port.DefaultName;
                output.transform.localPosition =
                    new Vector3(0f, 0f, 1 - (portIndex + 1f) / (outputPorts.Count + 1)); //n/n+1
                output.transform.localScale = Vector3.one;
                Transform sphere = output.transform.Find("Sphere");
                sphere.name = port.DefaultName;
            }
        }
        //"only used as input" components
        else if (template is NumberSliderComponent || template is BooleanToggleComponent)
        {
            GameObject outputBlock = Instantiate(OutputBlockPrefab, cube.transform.Find("OutputBlockSpot"), true);
            Vector3 outputBlockExtents = outputBlock.GetComponentInChildren<Renderer>().bounds.extents;
            outputBlock.transform.localPosition = new Vector3(outputBlockExtents.x, 0f, 0f);
            outputBlock.transform.localScale = Vector3.one;
            
            GameObject output = Instantiate(OutputPrefab, outputBlock.transform.Find("Output Spot"), true);
            output.GetComponentInChildren<TextMesh>().text = "";
            output.transform.localPosition = new Vector3(0f, 0f, .5f); //n/n+1
            
            Transform sphere = output.transform.Find("Sphere");
            sphere.name = cube.name;
            output.transform.localScale = Vector3.one;
            sphere.transform.localScale = new Vector3(sphere.transform.localScale.x, sphere.transform.localScale.y, .5f);
        }
        //"only used as output" components
        //uncomment and code the following if any of those gets used (e.g. cluster output, galapagos?)
        /*else if (component is <class here>)
        {
            
        }*/
        //"input/output" components
        else if (template is PanelComponent)
        {
            GameObject inputBlock = Instantiate(InputBlockPrefab, cube.transform.Find("InputBlockSpot"), true);
            Vector3 inputBlockExtents = inputBlock.GetComponentInChildren<Renderer>().bounds.extents;
            inputBlock.transform.localPosition = new Vector3(-inputBlockExtents.x, 0f, 0f);
            inputBlock.transform.localScale = Vector3.one;
            
            GameObject input = Instantiate(InputPrefab, inputBlock.transform.Find("Input Spot"), true);
            input.GetComponentInChildren<TextMesh>().text = "";
            input.transform.localPosition = new Vector3(0f, 0f, .5f);

            Transform sphere = input.transform.Find("Sphere");
            sphere.name = cube.name;
            input.transform.localScale = Vector3.one;
            
            
            GameObject outputBlock = Instantiate(OutputBlockPrefab, cube.transform.Find("OutputBlockSpot"), true);
            Vector3 outputBlockExtents = outputBlock.GetComponentInChildren<Renderer>().bounds.extents;
            outputBlock.transform.localPosition = new Vector3(outputBlockExtents.x, 0f, 0f);
            outputBlock.transform.localScale = Vector3.one;
            
            GameObject output = Instantiate(OutputPrefab, outputBlock.transform.Find("Output Spot"), true);
            output.GetComponentInChildren<TextMesh>().text = "";
            output.transform.localPosition = new Vector3(0f, 0f, .5f);
            
            Transform sphere2 = output.transform.Find("Sphere");
            sphere2.name = cube.name;
            output.transform.localScale = Vector3.one;
        }

        GameObject testCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        testCube.transform.name = "TestCollider";
        testCube.transform.SetParent(cube.transform);
        testCube.transform.localScale = Vector3.one;
        testCube.transform.localPosition = Vector3.zero;
    }
}