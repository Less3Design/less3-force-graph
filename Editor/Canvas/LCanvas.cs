using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Less3.ForceGraph;
using Less3.ForceGraph.Editor;
using UnityEditor;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.UIElements;

namespace Less3.ForceGraph.Editor
{
    public static class LCanvasPrefs
    {
        public static readonly string ZOOM_KEY = "ForceDirectedCanvasZoom";//EditorPrefs setting key
        public static readonly string SNAP_SETTINGS_KEY = "ForceGraphSnapToGrid";

        public static readonly float DEFAULT_ZOOM = 1f;
        public static readonly Vector2 ZOOM_RANGE = new Vector2(.25f, 2f);
    }

    /// <summary>
    /// A node graph canvas. N is type of node data, C is type of connection data. G is type of group data.
    /// All types must be classes.
    /// </summary>
    public class LCanvas<N, C, G> : VisualElement where N : class where C : class where G : class
    {
        private const string NODE_UXML = "ForceNode";
        private const string GROUP_UXML = "ForceGroup";

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
        private static VisualTreeAsset _groupUXML;
        public VisualTreeAsset groupUXML
        {
            get
            {
                if (_groupUXML == null)
                    _groupUXML = Resources.Load<VisualTreeAsset>(GROUP_UXML);
                return _groupUXML;
            }
        }

        public new class UxmlFactory : UxmlFactory<LCanvas<N, C, G>, UxmlTraits> { }

        public LCanvas()
        {
            this.style.flexGrow = 1;

            translationContainer = new VisualElement();
            translationContainer.name = "TranslationContainer";
            translationContainer.pickingMode = PickingMode.Ignore;
            translationContainer.style.position = Position.Absolute;
            translationContainer.style.left = 0;
            translationContainer.style.right = 0;
            translationContainer.style.top = 0;
            translationContainer.style.bottom = 0;

            dragBox = new VisualElement();
            dragBox.name = "DragBox";
            dragBox.style.position = Position.Absolute;
            dragBox.style.backgroundColor = Color.red;
            dragBox.style.width = 8f;
            dragBox.style.height = 8f;
            dragBox.style.opacity = 0f;
            dragBox.pickingMode = PickingMode.Ignore;
            translationContainer.Add(dragBox);

            bg = new VisualElement();
            bg.name = "BG";
            bg.style.flexGrow = 1;
            Add(bg);
            Add(translationContainer);
            bg.AddManipulator(new ForceDirectedCanvasScrollManipulator(translationContainer));
            bg.AddManipulator(new ForceDirectedCanvasBGManipulator()
            {
                OnLeftClick = () =>
                {
                    ClearSelection();
                },
                // * BG Right click menu (add node)
                OnRightClick = (Vector2 mousePos) =>
                {
                    ClearSelection();
                    var menu = new GenericDropdownMenu();

                    menu.AddDisabledItem("Add Node", false);
                    foreach ((string name, Type nodeType) in PossibleNodeTypes)
                    {
                        menu.AddItem($"{name}", false, () =>
                        {
                            AddNullNodeInternal(nodeType, nodesContainer.WorldToLocal(mousePos));
                        });
                    }
                    if (PossibleGroupTypes.Count > 0)
                    {
                        menu.AddSeparator("");
                        menu.AddDisabledItem("Add Group", false);
                        foreach ((string name, Type groupType) in PossibleGroupTypes)
                        {
                            menu.AddItem($"{name}", false, () =>
                            {
                                AddNullGroupInternal(groupType, nodesContainer.WorldToLocal(mousePos));
                            });
                        }
                    }

                    menu.DropDown(new Rect(Event.current.mousePosition, Vector2.zero), this, false);
                },
                OnMiddleDrag = (delta) =>
                {
                    translationContainer.transform.position += new Vector3(delta.x, delta.y, 0);
                },
                OnLeftDragStart = (pos) =>
                {
                    _leftDragging = true;
                    _leftDragStartPos = nodesContainer.WorldToLocal(pos);
                    _leftDragPos = _leftDragStartPos;
                },
                OnLeftDrag = (pos) =>
                {
                    _leftDragPos = _leftDragPos + pos;
                    dragBox.transform.position = _leftDragPos;
                },
            });


            connectionsContainer = new VisualElement();
            connectionsContainer.pickingMode = PickingMode.Ignore;
            connectionsContainer.name = "ConnectionsContainer";
            connectionsContainer.style.position = Position.Absolute;
            connectionsContainer.style.left = Length.Percent(50);
            connectionsContainer.style.top = Length.Percent(50);
            connectionsContainer.style.bottom = 0;
            translationContainer.Add(connectionsContainer);

            groupsContainer = new VisualElement();
            groupsContainer.pickingMode = PickingMode.Ignore;
            groupsContainer.name = "GroupsContainer";
            groupsContainer.style.position = Position.Absolute;
            groupsContainer.style.left = Length.Percent(50);
            groupsContainer.style.top = Length.Percent(50);
            groupsContainer.style.bottom = 0;
            translationContainer.Add(groupsContainer);

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

        public List<LCanvasNode<N>> nodes { get; private set; } = new List<LCanvasNode<N>>();
        public List<LCanvasConnection<N, C>> connections = new List<LCanvasConnection<N, C>>();
        public List<LCanvasGroup<G, N>> groups = new List<LCanvasGroup<G, N>>();

        private List<VisualElement> connectionLines = new List<VisualElement>();

        public LCanvasNode<N> selectedNode { get; private set; }
        public LCanvasConnection<N, C> selectedConnection { get; private set; }
        public LCanvasGroup<G, N> selectedGroup { get; private set; }

        // Different element types are put into different containers to organize layers
        private VisualElement bg;
        private VisualElement dragBox;
        private VisualElement translationContainer;
        private VisualElement nodesContainer;
        private VisualElement connectionsContainer;
        private VisualElement groupsContainer;
        private VisualElement effectsContainer;

        public LCanvasNode<N> newConnectionFromNode;// used for creating connections
        private Type newConnectionType;
        private VisualElement newConnectionLine;
        private LCanvasNode<N> hoveredNode;// used for creating connections

        // edit state
        private bool _leftDragging;// a left click drag is in proress.
        private Vector2 _leftDragStartPos;
        private Vector2 _leftDragPos;

        // * Events & funcs
        public Action OnSelectionChanged;
        public Func<N, N, Type, bool> ConnectionValidator;
        public Func<N, N, Type> AutoConnectionValidator;
        /// <summary>
        /// A connection was created by interacting with the node canvas. This likely requires an asset to be created. 2nd param is the specific type of the connection (U)
        /// </summary>
        public Action<LCanvasConnection<N, C>, Type> OnConnectionCreatedInternally;
        public Action<LCanvasNode<N>, Type> OnNodeCreatedInternally;
        public Action<LCanvasGroup<G, N>, Type> OnGroupCreatedInternally;
        public Action<LCanvasGroup<G, N>> OnGroupDeletedInternally;
        public Action<N, G> OnNodeAddedToGroupInternally;
        public Action<N, G> OnNodeRemovedFromGroupInternally;
        public Action<LCanvasNode<N>> OnNodeDeletedInternally;
        public Action<LCanvasNode<N>> OnNodeDuplicatedInternally;
        public Action<LCanvasConnection<N, C>> OnConnectionDeletedInternally;
        public Action<N> OnNodeDoubleClickedInternally;

        //* Public Settings
        public Dictionary<Type, List<(string, Type)>> PossibleConnectionTypes = new Dictionary<Type, List<(string, Type)>>();
        public List<(string, Type)> PossibleNodeTypes = new List<(string, Type)>();
        public List<(string, Type)> PossibleGroupTypes = new List<(string, Type)>();

        private void AddNullNodeInternal(Type type, Vector2 pos)
        {
            var node = InitNodeExternal(null, pos);
            OnNodeCreatedInternally?.Invoke(node, type);
        }

        public void AddNodeToGroupInternal(LCanvasNode<N> node, LCanvasGroup<G, N> group)
        {
            group.AddNode(node);
            OnNodeAddedToGroupInternally?.Invoke(node.data, group.data);
        }

        private void RemoveNodeFromGroupInternal(LCanvasNode<N> node, LCanvasGroup<G, N> group)
        {
            group.RemoveNode(node);
            OnNodeRemovedFromGroupInternally?.Invoke(node.data, group.data);
        }

        private LCanvasGroup<G, N> AddNullGroupInternal(Type type, Vector2 pos)
        {
            var group = InitGroupExternal(null, pos);
            OnGroupCreatedInternally?.Invoke(group, type);
            return group;
        }

        public LCanvasGroup<G, N> InitGroupExternal(G data, Vector2 pos, List<N> nodesInGroup = null)
        {
            VisualElement ui = groupUXML.Instantiate();
            ui.style.position = Position.Absolute;
            groupsContainer.Add(ui);
            var group = new LCanvasGroup<G, N>(data, ui, pos);
            groups.Add(group);
            ui.AddManipulator(new ForceDirectedCanvasScrollManipulator(translationContainer));
            ui.AddManipulator(new LGroupManipulator<N, C, G>(
                group,
                this,
                (LCanvasGroup<G, N> g) =>
                {
                    // left click
                    SelectGroup(g);
                },
                (LCanvasGroup<G, N> g) =>
                {
                    // right click
                    SelectGroup(g);
                    var menu = new GenericDropdownMenu();
                    menu.AddItem("Delete Group", false, () =>
                    {
                        DeleteGroupInternal(g);
                    });
                    menu.DropDown(new Rect(Event.current.mousePosition, Vector2.zero), this, false);
                }
            ));

            if (nodesInGroup != null)
            {
                foreach (N node in nodesInGroup)
                {
                    LCanvasNode<N> canvasNode = nodes.Find(n => n.data.Equals(node));
                    if (canvasNode != null)
                    {
                        group.AddNode(canvasNode);
                    }
                }
            }

            group.UpdateContent();
            group.UpdateShape();
            return group;
        }

        /// <summary>
        /// Create a node on the canvas. Usually called externally when initializing the graph, or when the external data source has a new node.
        /// </summary>
        /// <param name="data"></param>
        public LCanvasNode<N> InitNodeExternal(N data, Vector2 startPosition)
        {
            VisualElement ui = nodeUXML.Instantiate();
            ui.style.position = Position.Absolute;
            nodesContainer.Add(ui);
            var node = new LCanvasNode<N>(data, ui, startPosition);
            nodes.Add(node);
            ui.AddManipulator(new ForceDirectedCanvasScrollManipulator(translationContainer));

            // handles auto-connection
            ui.Q("DragHandle").AddManipulator(new ForceNodeAutoConnectionDragManipulator<N, C, G>(
                node,
                this
            ));

            // Double click
            var doubleClickable = new Clickable(() =>
            {
                if (data is ILNodeEditorDoubleClick doubleClick)
                {
                    doubleClick.EditorOnNodeDoubleClick();
                }
                OnNodeDoubleClickedInternally?.Invoke(data);
            });
            doubleClickable.activators.Clear();
            doubleClickable.activators.Add(new ManipulatorActivationFilter()
            {
                button = MouseButton.LeftMouse,
                clickCount = 2// makes this check for double clicks
            });
            ui.AddManipulator(doubleClickable);

            ui.AddManipulator(new ForceNodeDragManipulator<N, C, G>(
                node,
                this,
                leftClickAction: (n) =>
                {
                    if (newConnectionFromNode != null && newConnectionFromNode != n)
                    {
                        AddConnectionInternal(newConnectionFromNode, (LCanvasNode<N>)n, newConnectionType);
                        newConnectionFromNode.element.Q("Border").RemoveFromClassList("CreatingConnection");
                        newConnectionFromNode = null;
                    }
                    var castNode = n as LCanvasNode<N>;
                    SelectNode(castNode);
                },
                // * Node right click menu
                rightClickAction: (n) =>
                    {
                        var castNode = n as LCanvasNode<N>;
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

                        if (TryGetGroupNodeIsIn(n, out LCanvasGroup<G, N> group))
                        {
                            menu.AddSeparator("");
                            menu.AddItem("Ungroup Node", false, () =>
                            {
                                RemoveNodeFromGroupInternal(castNode, group);
                            });
                        }
                        else if (PossibleGroupTypes.Count > 0)
                        {
                            menu.AddSeparator("");
                            foreach ((string name, Type groupType) in PossibleGroupTypes)
                            {
                                menu.AddItem($"Create {name}", false, () =>
                                {
                                    var newGroup = AddNullGroupInternal(groupType, n.position);
                                    AddNodeToGroupInternal(castNode, newGroup);
                                });
                            }
                        }

                        menu.AddSeparator("");
                        menu.AddItem("Duplicate", false, () =>
                        {
                            DuplicateNodeInternal(castNode);
                        });
                        menu.AddItem("Delete", false, () =>
                        {
                            DeleteNodeInternal(castNode);
                        });

                        menu.DropDown(n.element.worldBound, n.element, false);

                    },
                enterAction: (n) =>
                { hoveredNode = n; },
                exitAction: (n) => { if (hoveredNode == n) hoveredNode = null; }
                )
            );
            return node;
        }

        private void DeleteNodeInternal(LCanvasNode<N> node)
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

        private void DuplicateNodeInternal(LCanvasNode<N> node)
        {
            OnNodeDuplicatedInternally?.Invoke(node);
        }

        private void DeleteGroupInternal(LCanvasGroup<G, N> group)
        {
            // Remove all nodes from the group
            foreach (var node in group.nodes.ToList())
            {
                RemoveNodeFromGroupInternal(node, group);
            }
            groups.Remove(group);
            group.element.RemoveFromHierarchy();
            OnGroupDeletedInternally?.Invoke(group);
        }

        private void DeleteConnectionInternal(LCanvasConnection<N, C> connection)
        {
            int index = connections.IndexOf(connection);
            connections.Remove(connection);
            connectionLines[index].RemoveFromHierarchy();
            connectionLines.RemoveAt(index);
            OnConnectionDeletedInternally?.Invoke(connection);
        }

        private void AddConnectionInternal(LCanvasNode<N> from, LCanvasNode<N> to, Type type)
        {
            if (ConnectionValidator != null && !ConnectionValidator(from.data, to.data, type))
                return;

            var connection = AddConnection(from.data, to.data);
            OnConnectionCreatedInternally?.Invoke(connection, type);
        }

        public void InitConnectionExternal(N from, N to, C data)
        {
            var connection = AddConnection(from, to);
            connection.data = data;
        }

        public void TryCreateAutoConnection(LCanvasNode<N> from, LCanvasNode<N> to)
        {
            if (from == null || to == null)
                return;

            Type autoType = AutoConnectionValidator.Invoke(from.data, to.data);
            if (autoType != null)
            {
                AddConnectionInternal(from, to, autoType);
            }
        }

        private LCanvasConnection<N, C> AddConnection(N data1, N data2)
        {
            LCanvasNode<N> node1 = nodes.Find(n => n.data.Equals(data1));
            LCanvasNode<N> node2 = nodes.Find(n => n.data.Equals(data2));
            if (node1 == null || node2 == null)
                return null;
            LCanvasConnection<N, C> connection = new LCanvasConnection<N, C>
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
        public void Update()
        {
            DrawConnections();
            DrawNewConnection();
            DrawGroups();

            selectedNode?.UpdateContent();

            Vector3 desiredScale = Vector3.one * EditorPrefs.GetFloat(LCanvasPrefs.ZOOM_KEY, LCanvasPrefs.DEFAULT_ZOOM);
            translationContainer.transform.scale = desiredScale;
        }

        private void DrawGroups()
        {
            foreach (var group in groups)
            {
                if (group.element == null)
                {
                    group.element = groupUXML.Instantiate();
                    group.element.style.position = Position.Absolute;
                    group.element.style.backgroundColor = Color.gray;
                    group.element.AddToClassList("GroupElement");
                    group.label = new Label(group.data.ToString());
                    group.label.AddToClassList("GroupLabel");
                    group.element.Add(group.label);
                    nodesContainer.Add(group.element);
                }

                group.UpdateShape();
                group.UpdateContent();
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
                var pos1 = connection.from.position + connection.from.element.layout.size * new Vector2(0f, 0.5f);
                var pos2 = connection.to.position + connection.to.element.layout.size * new Vector2(0f, 0.5f);
                var dist = (pos1 - pos2).magnitude;
                connectionLine.transform.position = pos1;
                connectionLine.style.height = 4f;
                if (selectedConnection == connection)
                    connectionLine.style.opacity = 1f;
                else
                    connectionLine.style.opacity = 0.7f;
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
            var pos1 = newConnectionFromNode.position + offset;
            Vector2 pos2 = Vector2.zero;

            offset = hoveredNode.element.layout.size;
            offset.x = 0f;
            offset.y *= 0.5f;
            pos2 = hoveredNode.position + offset;

            newConnectionLine.transform.position = pos1;
            var dist = (pos1 - pos2).magnitude;
            //var thicknessFloat = Mathf.InverseLerp(200f, 1000f, dist);
            //newConnectionLine.style.height = Mathf.Lerp(5f, 3f, thicknessFloat);
            newConnectionLine.style.width = dist;
            newConnectionLine.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(pos2.y - pos1.y, pos2.x - pos1.x) * Mathf.Rad2Deg);
        }

        private void AddConnectionLine(LCanvasConnection<N, C> connection)
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
            line.AddManipulator(new ForceDirectedCanvasScrollManipulator(translationContainer));
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
                    menu.DropDown(new Rect(Event.current.mousePosition, Vector2.zero), this, false);
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

        public void TrySelectData(N data)
        {
            var node = nodes.Find(n => n.data.Equals(data));
            if (node != null)
                SelectNode(node);
        }

        private void SelectNode(LCanvasNode<N> node)
        {
            ClearSelection(false);// no notify because we notify at end of this method
            selectedNode = node;
            node.element.Q("Border").AddToClassList("Selected");
            node.element.BringToFront();
            OnSelectionChanged?.Invoke();
        }

        private void SelectConnection(LCanvasConnection<N, C> connection)
        {
            ClearSelection(false);// no notify because we notify at end of this method
            selectedConnection = connection;
            int index = connections.IndexOf(selectedConnection);
            connectionLines[index].AddToClassList("ConnectionSelected");
            OnSelectionChanged?.Invoke();
        }

        private void SelectGroup(LCanvasGroup<G, N> group)
        {
            ClearSelection(false);// no notify because we notify at end of this method
            selectedGroup = group;
            group.element.Q("Border").AddToClassList("Selected");
            group.element.BringToFront();
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
            if (selectedGroup != null)
            {
                selectedGroup.element.Q("Border").RemoveFromClassList("Selected");
                selectedGroup = null;
            }
            if (notifySelectionChanged)
                OnSelectionChanged?.Invoke();
        }

        public void SetViewScale(float scale)
        {
            translationContainer.transform.scale = new Vector3(scale, scale, 1f);
        }

        public Vector2 TryGetNodeSnapPosition(Vector2 pos, LCanvasNode<N> ignore)
        {
            Vector2 snapPos = pos;

            float snapDist = 6f * (1f / EditorPrefs.GetFloat(LCanvasPrefs.ZOOM_KEY, LCanvasPrefs.DEFAULT_ZOOM));
            bool x = false;
            bool y = false;
            foreach (var node in nodes)
            {
                if (node == ignore)
                    continue;

                if (!x && node.position.x < snapPos.x + snapDist && node.position.x > snapPos.x - snapDist)
                {
                    snapPos.x = node.position.x;
                    x = true;
                }
                if (!y && node.position.y < snapPos.y + snapDist && node.position.y > snapPos.y - snapDist)
                {
                    snapPos.y = node.position.y;
                    y = true;
                }

                if (x && y)
                    break;
            }
            return snapPos;
        }

        public bool TryGetGroupAtPosition(Vector2 pos, out LCanvasGroup<G, N> group)
        {
            group = null;
            foreach (var g in groups)
            {
                if (g.element.localBound.Contains(pos))
                {
                    group = g;
                    return true;
                }
            }
            return false;
        }

        public bool TryGetGroupNodeIsIn(LCanvasNode<N> node, out LCanvasGroup<G, N> group)
        {
            group = null;
            foreach (var g in groups)
            {
                if (g.nodes.Contains(node))
                {
                    group = g;
                    return true;
                }
            }
            return false;
        }

        public bool ElementIsNode(VisualElement element, out LCanvasNode<N> node)
        {
            node = null;
            if (element == null)
                return false;

            node = nodes.Find(n => n.element == element);
            return node != null;
        }
    }
}
