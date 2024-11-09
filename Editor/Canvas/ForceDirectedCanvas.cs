using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Less3.ForceGraph;
using UnityEditor;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class ForceCanvasNodeElementBase
{
    public VisualElement element;
    public float mass = 10f;
    public Vector2 force;
    protected bool _pinned;
    public virtual bool pinned
    {
        get
        {
            return _pinned;
        }
        set
        {
            _pinned = value;
            element.Q("Pin").style.display = _pinned ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
    public bool frozen;

    public Vector2 simPosition { get; protected set; }
    public Vector2 elementPosition { get; protected set; }
    public Vector2 simAspectRatio = Vector2.one;

    public void SetElementPosition(Vector2 pos)
    {
        this.simPosition = pos * (new Vector2(1f / simAspectRatio.x, 1f / simAspectRatio.y));
        elementPosition = pos;
        element.transform.position = pos;
    }
}

public class ForceCanvasNodeElement<T> : ForceCanvasNodeElementBase
{
    public override bool pinned
    {
        get
        {
            if (_data is IForceNodePinnable pinnable)
            {
                return pinnable.pinned;
            }
            else
            {
                return _pinned;
            }
        }
        set
        {
            if (_data is IForceNodePinnable pinnable)
            {
                pinnable.pinned = value;
                if (_data is UnityEngine.Object uObj)
                {
                    UnityEditor.EditorUtility.SetDirty(uObj);
                }
            }
            else
            {
                _pinned = value;
            }

            element.Q("Pin").style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    private T _data;
    public T data
    {
        get => _data;
        set
        {
            _data = value;
            if (_data != null)
            {
                if (_data is IForceNodeTitle title)
                    element.Q<Label>("Label").text = title.NodeTitle;
                else
                    element.Q<Label>("Label").text = value.ToString();

                var icon = element.Q<VisualElement>("Icon");
                if (_data is IForceNodeStyle style)
                {
                    element.Q("NodeContainer").style.backgroundColor = style.NodeBackgroundColor;
                    element.Q<Label>("Label").style.color = style.NodeLabelColor;
                    icon.style.unityBackgroundImageTintColor = style.NodeLabelColor;
                }
                if (_data is IForceNodeScale scale)
                {
                    element.transform.scale = new Vector3(scale.NodeScale, scale.NodeScale, 1f);
                }
                if (_data is IForceNodeIcon iconData)
                {
                    icon.style.display = DisplayStyle.Flex;
                    icon.style.backgroundImage = Resources.Load<Texture2D>(iconData.NodeIcon);
                }
                else
                {
                    icon.style.display = DisplayStyle.None;
                }
            }
        }
    }

    public ForceCanvasNodeElement(T data, VisualElement element, Vector2 newPos)
    {
        this.element = element;
        this.data = data;
        simPosition = newPos;

        element.Q("Pin").style.display = pinned ? DisplayStyle.Flex : DisplayStyle.None;

        //TODO cache this sheesh
        Vector2 aspectRatio = new Vector2(
            EditorPrefs.GetFloat(ForceDirectedCanvasSettings.ASPECT_RATIO_X_KEY, ForceDirectedCanvasSettings.DEFAULT_ASPECT_RATIO.x),
            EditorPrefs.GetFloat(ForceDirectedCanvasSettings.ASPECT_RATIO_Y_KEY, ForceDirectedCanvasSettings.DEFAULT_ASPECT_RATIO.y)
        );
        elementPosition = newPos * aspectRatio;
        element.transform.position = elementPosition;
    }

    public void Update(Vector2 aspectRatio)
    {
        if (frozen || pinned)
            return;

        simAspectRatio = aspectRatio;
        simPosition += force / mass;

        // TODO ideally we validate nan's somewhere else. This can be evaluated multiple times per frame.
        if (float.IsNaN(simPosition.x) || float.IsNaN(simPosition.y) || float.IsInfinity(simPosition.x) || float.IsInfinity(simPosition.y))
            simPosition = new Vector2(UnityEngine.Random.Range(-5f, 5f), UnityEngine.Random.Range(-5, 5));

        elementPosition = simPosition * aspectRatio;
        element.transform.position = elementPosition;
    }
}

public class ForceCanvasConnection<T, U>
{
    public static readonly string CONNECTION_DASH_TEXTURE = "TiledConnection";
    private U _data;
    public U data
    {
        get => _data;
        set
        {
            _data = value;
            if (_data != null)
            {
                if (_data is IForceConnectionStyle style && _element != null)
                {
                    if (style.Dashed)
                    {
                        _element.style.backgroundImage = Background.FromTexture2D(Resources.Load<Texture2D>(CONNECTION_DASH_TEXTURE));
                        _element.style.unityBackgroundImageTintColor = style.ConnectionColor;
                        _element.style.backgroundColor = Color.clear;
                        _element.style.backgroundRepeat = new BackgroundRepeat(Repeat.Repeat, Repeat.NoRepeat);
                        _element.style.backgroundSize = new BackgroundSize(Length.Auto(), Length.Auto());
                    }
                    else
                    {
                        _element.style.backgroundColor = style.ConnectionColor;
                    }
                }
            }
        }
    }

    private VisualElement _element;
    public VisualElement element
    {
        get => _element;
        set
        {
            _element = value;
            if (_data != null && _data is IForceConnectionStyle style)
            {
                if (style.Dashed)
                {
                    _element.style.backgroundImage = Background.FromTexture2D(Resources.Load<Texture2D>(CONNECTION_DASH_TEXTURE));
                    _element.style.unityBackgroundImageTintColor = style.ConnectionColor;
                    _element.style.backgroundColor = Color.clear;
                    _element.style.backgroundRepeat = new BackgroundRepeat(Repeat.Repeat, Repeat.Repeat);
                    _element.style.backgroundSize = new BackgroundSize(Length.Auto(), Length.Auto());
                }
                else
                {
                    _element.style.backgroundColor = style.ConnectionColor;
                }
            }
        }
    }
    public ForceCanvasNodeElement<T> from;
    public ForceCanvasNodeElement<T> to;
}

public static class ForceDirectedCanvasSettings
{
    public static readonly string GRAVITY_KEY = "ForceDirectedCanvasGravity";
    public static readonly string NODE_REPULSION_KEY = "ForceDirectedCanvasNodeRepulsion";
    public static readonly string CONNECTION_FORCE_KEY = "ForceDirectedCanvasConnectionForce";
    public static readonly string ASPECT_RATIO_X_KEY = "ForceDirectedCanvasAspectRatioX";
    public static readonly string ASPECT_RATIO_Y_KEY = "ForceDirectedCanvasAspectRatioY";

    public static readonly float DEFAULT_GRAVITY = 1f;
    public static readonly Vector2 GRAVITY_RANGE = new Vector2(.1f, 2f);

    public static readonly float DEFAULT_NODE_REPULSION = 2.5f;
    public static readonly Vector2 NODE_REPULSION_RANGE = new Vector2(.1f, 5f);
    public static readonly float NODE_REPULSTION_MULTIPLIER = 2000f;

    public static readonly float DEFAULT_CONNECTION_FORCE = .4f;
    public static readonly Vector2 CONNECTION_FORCE_RANGE = new Vector2(.1f, 1.5f);

    public static readonly Vector2 DEFAULT_ASPECT_RATIO = new Vector2(1.6f, 0.6f);
}


/// <summary>
/// A canvas that uses "Force directed drawing" to position nodes. T is type of node data, U is type of connection data.
/// </summary>
public class ForceDirectedCanvas<T, U> : VisualElement where T : class where U : class
{
    private float _gravity;
    private float _nodeRepulsion;
    private float _connectionForce = .4f;
    private Vector2 _aspectRatio;

    private const string NODE_UXML = "ForceNode";
    private static VisualTreeAsset _nodeUXML;
    public VisualTreeAsset nodeUXML
    {
        get
        {
            if (_nodeUXML == null)
                _nodeUXML = Resources.Load<VisualTreeAsset>(NODE_UXML);
            return _nodeUXML;
        }
    }

    public new class UxmlFactory : UxmlFactory<ForceDirectedCanvas<T, U>, UxmlTraits> { }

    public ForceDirectedCanvas()
    {
        this.style.flexGrow = 1;

        bg = new VisualElement();
        bg.name = "BG";
        bg.style.flexGrow = 1;
        Add(bg);
        bg.AddManipulator(new ForceDirectedCanvasBGManipulator()
        {
            OnLeftClick = () =>
            {
                ClearSelection();
            },
            // * BG Right click menu (add node)
            OnRightClick = () =>
            {
                ClearSelection();
                var menu = new GenericDropdownMenu();

                menu.AddDisabledItem("Add Node", false);
                foreach (var entry in PossibleNodeTypes)
                {
                    menu.AddItem($"{entry.Item1}", false, () =>
                    {
                        AddNodeInternal(entry.Item2);
                    });
                }
                menu.DropDown(new Rect(Event.current.mousePosition, Vector2.zero), this);
            },
            OnDrag = (delta) =>
            {
                translationContainer.transform.position += new Vector3(delta.x, delta.y, 0);
            }
        });

        translationContainer = new VisualElement();
        translationContainer.name = "TranslationContainer";
        translationContainer.pickingMode = PickingMode.Ignore;
        translationContainer.style.position = Position.Absolute;
        translationContainer.style.left = 0;
        translationContainer.style.right = 0;
        translationContainer.style.top = 0;
        translationContainer.style.bottom = 0;
        Add(translationContainer);

        connectionsContainer = new VisualElement();
        connectionsContainer.pickingMode = PickingMode.Ignore;
        connectionsContainer.name = "ConnectionsContainer";
        connectionsContainer.style.position = Position.Absolute;
        connectionsContainer.style.left = Length.Percent(50);
        connectionsContainer.style.top = Length.Percent(50);
        connectionsContainer.style.bottom = 0;
        translationContainer.Add(connectionsContainer);

        effectsContainer = new VisualElement();
        effectsContainer.pickingMode = PickingMode.Ignore;
        effectsContainer.name = "EffectsContainer";
        effectsContainer.style.position = Position.Absolute;
        effectsContainer.style.left = Length.Percent(50);
        effectsContainer.style.top = Length.Percent(50);
        effectsContainer.style.bottom = 0;
        translationContainer.Add(effectsContainer);

        // Create the new connection line
        var line = new VisualElement();
        line.style.position = Position.Absolute;
        line.style.backgroundColor = UnityEngine.ColorUtility.TryParseHtmlString("#FD6D40", out var color) ? color : UnityEngine.Color.green;
        line.style.height = 8f;
        line.style.transformOrigin = new TransformOrigin(0, Length.Percent(50));
        line.AddToClassList("NewConnectionLineBase");
        newConnectionLine = line;
        newConnectionLine.name = "NewConnectionLine";
        effectsContainer.Add(line);

        nodesContainer = new VisualElement();
        nodesContainer.pickingMode = PickingMode.Ignore;
        nodesContainer.name = "NodesContainer";
        nodesContainer.style.position = Position.Absolute;
        nodesContainer.style.left = Length.Percent(50);
        nodesContainer.style.top = Length.Percent(50);
        translationContainer.Add(nodesContainer);
    }

    public List<ForceCanvasNodeElement<T>> nodes { get; private set; } = new List<ForceCanvasNodeElement<T>>();
    private List<ForceCanvasConnection<T, U>> connections = new List<ForceCanvasConnection<T, U>>();
    private List<VisualElement> connectionLines = new List<VisualElement>();

    public ForceCanvasNodeElement<T> selectedNode { get; private set; }
    public ForceCanvasConnection<T, U> selectedConnection { get; private set; }

    private VisualElement bg;
    private VisualElement translationContainer;
    private VisualElement nodesContainer;
    private VisualElement connectionsContainer;
    private VisualElement effectsContainer;
    private ForceCanvasNodeElement<T> newConnectionFromNode;
    private Type newConnectionType;
    private VisualElement newConnectionLine;
    private ForceCanvasNodeElementBase hoveredNode;

    // * Events & funcs
    public Action OnSelectionChanged;
    public Func<T, T, Type, bool> ConnectionValidator;
    /// <summary>
    /// A connection was created by interacting with the node canvas. This likely requires an asset to be created. 2nd param is the specific type of the connection (U)
    /// </summary>
    public Action<ForceCanvasConnection<T, U>, Type> OnConnectionCreatedInternally;
    public Action<ForceCanvasNodeElement<T>, Type> OnNodeCreatedInternally;
    public Action<ForceCanvasNodeElement<T>> OnNodeDeletedInternally;
    public Action<ForceCanvasNodeElement<T>> OnNodeDuplicatedInternally;
    public Action<ForceCanvasConnection<T, U>> OnConnectionDeletedInternally;

    //* Public Settings
    public Dictionary<Type, List<(string, Type)>> PossibleConnectionTypes = new Dictionary<Type, List<(string, Type)>>();
    public List<(string, Type)> PossibleNodeTypes = new List<(string, Type)>();
    /// <summary>
    /// If true the canvas will automatically zoom and pan to fit all nodes in view
    /// </summary>
    public bool FitInView;

    private void AddNodeInternal(Type type)
    {
        var node = InitNodeExternal(null, Vector2.zero);//TODO: get position from mouse
        OnNodeCreatedInternally?.Invoke(node, type);
    }

    /// <summary>
    /// Create a node on the canvas. Usually called externally when initializing the graph, or when the external data source has a new node.
    /// </summary>
    /// <param name="data"></param>
    public ForceCanvasNodeElement<T> InitNodeExternal(T data, Vector2 startPosition)
    {
        VisualElement ui = nodeUXML.Instantiate();
        ui.style.position = Position.Absolute;
        nodesContainer.Add(ui);
        var node = new ForceCanvasNodeElement<T>(data, ui, startPosition);
        nodes.Add(node);
        ui.AddManipulator(new ForceNodeDragManipulator(
            node,
            leftClickAction: (n) =>
            {
                if (newConnectionFromNode != null && newConnectionFromNode != n)
                {
                    AddConnectionInternal(newConnectionFromNode, (ForceCanvasNodeElement<T>)n, newConnectionType);
                    newConnectionFromNode.element.Q("Border").RemoveFromClassList("CreatingConnection");
                    newConnectionFromNode = null;
                }
                var castNode = n as ForceCanvasNodeElement<T>;
                SelectNode(castNode);
            },
            // * Node right click menu
            righClickAction: (n) =>
                {
                    var castNode = n as ForceCanvasNodeElement<T>;
                    var nodeType = castNode.data.GetType();
                    var menu = new GenericDropdownMenu();

                    SelectNode(castNode);
                    menu.AddDisabledItem("Add Connection", false);
                    if (PossibleConnectionTypes.ContainsKey(nodeType))
                        foreach (var connectionEntry in PossibleConnectionTypes[nodeType])
                        {
                            menu.AddItem($"{connectionEntry.Item1}", false, () =>
                            {
                                newConnectionFromNode = castNode;
                                newConnectionType = connectionEntry.Item2;
                                castNode.element.Q("Border").AddToClassList("CreatingConnection");
                            });
                        }
                    menu.AddSeparator("");
                    menu.AddItem(n.pinned ? "Unpin" : "Pin", false, () =>
                    {
                        n.pinned = !n.pinned;
                    });
                    menu.AddItem("Duplicate", false, () =>
                    {
                        DuplicateNodeInternal(castNode);
                    });
                    menu.AddItem("Delete", false, () =>
                    {
                        DeleteNodeInternal(castNode);
                    });

                    menu.DropDown(n.element.worldBound, n.element);

                },
            enterAction: (n) =>
            { hoveredNode = n; },
            exitAction: (n) => { if (hoveredNode == n) hoveredNode = null; }
            )
        );
        return node;
    }

    private void DeleteNodeInternal(ForceCanvasNodeElement<T> node)
    {
        //find connections with node
        var connectionsToDelete = connections.FindAll(c => c.from == node || c.to == node);
        foreach (var connection in connectionsToDelete)
        {
            DeleteConnectionInternal(connection);
        }
        nodes.Remove(node);
        node.element.RemoveFromHierarchy();
        OnNodeDeletedInternally?.Invoke(node);
    }

    private void DuplicateNodeInternal(ForceCanvasNodeElement<T> node)
    {
        OnNodeDuplicatedInternally?.Invoke(node);
    }

    private void DeleteConnectionInternal(ForceCanvasConnection<T, U> connection)
    {
        int index = connections.IndexOf(connection);
        connections.Remove(connection);
        connectionLines[index].RemoveFromHierarchy();
        connectionLines.RemoveAt(index);
        OnConnectionDeletedInternally?.Invoke(connection);
    }

    private void AddConnectionInternal(ForceCanvasNodeElement<T> from, ForceCanvasNodeElement<T> to, Type type)
    {
        if (ConnectionValidator != null && !ConnectionValidator(from.data, to.data, type))
            return;

        var connection = AddConnection(from.data, to.data);
        OnConnectionCreatedInternally?.Invoke(connection, type);
    }

    public void InitConnectionExternal(T from, T to, U data)
    {
        var connection = AddConnection(from, to);
        connection.data = data;
    }

    private ForceCanvasConnection<T, U> AddConnection(T data1, T data2)
    {
        ForceCanvasNodeElement<T> node1 = nodes.Find(n => n.data.Equals(data1));
        ForceCanvasNodeElement<T> node2 = nodes.Find(n => n.data.Equals(data2));
        if (node1 == null || node2 == null)
            return null;
        ForceCanvasConnection<T, U> connection = new ForceCanvasConnection<T, U>
        {
            from = node1,
            to = node2,
        };
        connections.Add(connection);
        return connection;
    }

    /// <summary>
    /// Simulate the forces on nodes. This should be called every frame. Usually in OnGUI() somewhere.
    /// </summary>
    public void Simulate(int iterations = 1)
    {
        _gravity = EditorPrefs.GetFloat(ForceDirectedCanvasSettings.GRAVITY_KEY, ForceDirectedCanvasSettings.DEFAULT_GRAVITY);
        _nodeRepulsion = EditorPrefs.GetFloat(ForceDirectedCanvasSettings.NODE_REPULSION_KEY, ForceDirectedCanvasSettings.DEFAULT_NODE_REPULSION) * ForceDirectedCanvasSettings.NODE_REPULSTION_MULTIPLIER;
        _connectionForce = EditorPrefs.GetFloat(ForceDirectedCanvasSettings.CONNECTION_FORCE_KEY, ForceDirectedCanvasSettings.DEFAULT_CONNECTION_FORCE);
        _aspectRatio = new Vector2(
            EditorPrefs.GetFloat(ForceDirectedCanvasSettings.ASPECT_RATIO_X_KEY, ForceDirectedCanvasSettings.DEFAULT_ASPECT_RATIO.x),
            EditorPrefs.GetFloat(ForceDirectedCanvasSettings.ASPECT_RATIO_Y_KEY, ForceDirectedCanvasSettings.DEFAULT_ASPECT_RATIO.y)
        );

        for (int iteration = 0; iteration < iterations; iteration++)
        {
            // Gravity to center of canvas
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].force = -nodes[i].simPosition * _gravity;
            }
            // Node repulsion
            for (int i = 0; i < nodes.Count; i++)
            {
                for (int j = 0; j < nodes.Count; j++)
                {
                    var a = nodes[i];
                    var b = nodes[j];
                    if (a == b) continue;
                    Vector2 dir = a.simPosition - b.simPosition;
                    Vector2 force2 = dir / (dir.magnitude * dir.magnitude);
                    force2 *= _nodeRepulsion;
                    a.force += new Vector2(force2.x, force2.y);
                    b.force -= new Vector2(force2.x, force2.y);
                }
            }

            // Connection forces
            for (int i = 0; i < connections.Count; i++)
            {
                var connection = connections[i];
                var dir = connection.from.simPosition - connection.to.simPosition;
                connection.from.force -= dir * _connectionForce;
                connection.to.force += dir * _connectionForce;
            }

            // Apply forces to nodes
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Update(_aspectRatio);
            }
        }

        DrawConnections();
        DrawNewConnection();
        if (FitInView)
        {
            UpdateCanvasToFit();
        }
        else
        {
            //todo add scaling with mouse wheel maybe
            translationContainer.transform.scale = Vector3.one;
        }
    }

    private void DrawConnections()
    {
        for (int i = 0; i < connections.Count; i++)
        {
            if (i >= connectionLines.Count)
                AddConnectionLine(connections[i]);

            var connection = connections[i];
            var connectionLine = connectionLines[i];
            var pos1 = connection.from.elementPosition + connection.from.element.layout.size * new Vector2(0f, 0.5f);
            var pos2 = connection.to.elementPosition + connection.to.element.layout.size * new Vector2(0f, 0.5f);

            connectionLine.transform.position = pos1;
            var dist = (pos1 - pos2).magnitude;
            var thicknessFloat = Mathf.InverseLerp(100f, 500f, dist);
            connectionLine.style.height = Mathf.Lerp(4f, 2f, thicknessFloat);

            if (selectedConnection == connection)
                connectionLine.style.opacity = 1f;
            else
                connectionLine.style.opacity = Mathf.Lerp(.7f, 0.3f, thicknessFloat);

            connectionLine.style.width = dist;

            if (float.IsNaN(pos1.x) || float.IsNaN(pos1.y) || float.IsNaN(pos2.x) || float.IsNaN(pos2.y))
            {
                connectionLine.style.width = 0;
                connectionLine.style.height = 0;
            }
            else if (pos1 == pos2)
            {
                connectionLine.style.width = 0;
                connectionLine.style.height = 0;
            }
            else
            {
                connectionLine.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(pos2.y - pos1.y, pos2.x - pos1.x) * Mathf.Rad2Deg);
            }
        }
    }

    //Draw a connection from a node to the mouse. Initiated through the right click menu
    private void DrawNewConnection()
    {
        if (newConnectionFromNode == null)
        {
            newConnectionLine.style.display = DisplayStyle.None;
            newConnectionLine.RemoveFromClassList("NewConnectionLineAnim");
            return;
        }

        if (hoveredNode == null)
        {
            newConnectionLine.style.display = DisplayStyle.None;
            newConnectionLine.RemoveFromClassList("NewConnectionLineAnim");
            return;
        }
        newConnectionLine.style.display = DisplayStyle.Flex;
        newConnectionLine.AddToClassList("NewConnectionLineAnim");
        Vector2 offset = newConnectionFromNode.element.layout.size;
        offset.x = 0f;
        offset.y *= 0.5f;
        var pos1 = newConnectionFromNode.elementPosition + offset;
        Vector2 pos2 = Vector2.zero;

        offset = hoveredNode.element.layout.size;
        offset.x = 0f;
        offset.y *= 0.5f;
        pos2 = hoveredNode.elementPosition + offset;

        newConnectionLine.transform.position = pos1;
        var dist = (pos1 - pos2).magnitude;
        //var thicknessFloat = Mathf.InverseLerp(200f, 1000f, dist);
        //newConnectionLine.style.height = Mathf.Lerp(5f, 3f, thicknessFloat);
        newConnectionLine.style.width = dist;
        newConnectionLine.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(pos2.y - pos1.y, pos2.x - pos1.x) * Mathf.Rad2Deg);
    }

    private void AddConnectionLine(ForceCanvasConnection<T, U> connection)
    {
        var line = new VisualElement();
        line.style.position = Position.Absolute;
        line.AddToClassList("Connection");

        line.style.height = 4f;
        line.style.transformOrigin = new TransformOrigin(0, Length.Percent(50));

        // Make the line element easier to hover/click
        var hitslop = new VisualElement();
        line.Add(hitslop);
        hitslop.style.position = Position.Absolute;
        hitslop.style.left = -6;
        hitslop.style.right = -6;
        hitslop.style.top = -6;
        hitslop.style.bottom = -6;

        connectionsContainer.Add(line);
        connectionLines.Add(line);

        connection.element = line;
        line.style.backgroundRepeat = new BackgroundRepeat(Repeat.Repeat, Repeat.NoRepeat);

        int index = connectionLines.Count - 1;
        line.AddManipulator(new LeftRightClickable(
            (evt) =>
            {
                SelectConnection(connection);
            },
            (evt) =>
            {
                // * Connection right click menu
                var menu = new GenericDropdownMenu();
                menu.AddDisabledItem("Connection", false);
                menu.AddItem("Delete", false, () =>
                {
                    DeleteConnectionInternal(connection);
                });
                menu.DropDown(new Rect(Event.current.mousePosition, Vector2.zero), this);
            }
        ));
    }

    public void UpdateCanvasToFit()
    {
        // Really useful property that Unity doesnt expose right now (:facepalm:)
        // This gives us the rect for all children inside the container
        Rect containerBounds = (Rect)typeof(VisualElement).GetProperty("boundingBox", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(translationContainer);
        Rect containerRect = translationContainer.layout;

        Vector2 scaleFact = new Vector2(containerRect.width / (containerBounds.width * 1.1f), containerRect.height / (containerBounds.height * 1.1f));
        float scale = Mathf.Min(scaleFact.x, scaleFact.y, 1f);
        scale = scale / 1f;

        translationContainer.transform.scale = Vector3.Lerp(translationContainer.transform.scale, new Vector3(scale, scale, 1f), .09f);
    }

    public void TrySelectData(T data)
    {
        var node = nodes.Find(n => n.data.Equals(data));
        if (node != null)
            SelectNode(node);
    }

    private void SelectNode(ForceCanvasNodeElement<T> node)
    {
        ClearSelection(false);
        selectedNode = node;
        node.element.Q("Border").AddToClassList("Selected");
        node.element.BringToFront();
        OnSelectionChanged?.Invoke();
    }

    private void SelectConnection(ForceCanvasConnection<T, U> connection)
    {
        ClearSelection(false);
        selectedConnection = connection;
        int index = connections.IndexOf(selectedConnection);
        connectionLines[index].AddToClassList("ConnectionSelected");
        OnSelectionChanged?.Invoke();
    }

    public void ClearSelection(bool notifySelectionChanged = true)
    {
        if (selectedNode != null)
        {
            selectedNode.element.Q("Border").RemoveFromClassList("Selected");
            selectedNode = null;
        }
        if (selectedConnection != null)
        {
            int index = connections.IndexOf(selectedConnection);
            selectedConnection.element.RemoveFromClassList("ConnectionSelected");
            selectedConnection = null;
        }
        if (newConnectionFromNode != null)
        {
            newConnectionFromNode.element.Q("Border").RemoveFromClassList("CreatingConnection");
            newConnectionFromNode = null;
        }
        if (notifySelectionChanged)
            OnSelectionChanged?.Invoke();
    }

    public void SetViewScale(float scale)
    {
        translationContainer.transform.scale = new Vector3(scale, scale, 1f);
    }
}
