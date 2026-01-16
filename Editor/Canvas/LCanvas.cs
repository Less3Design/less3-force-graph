using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Less3.ForceGraph.Editor
{
    public static class LCanvasPrefs
    {
        public const string ZOOM_KEY = "ForceDirectedCanvasZoom";
        public const string SNAP_SETTINGS_KEY = "ForceGraphSnapToGrid";
        public const float DEFAULT_ZOOM = 1f;
        public static readonly Vector2 ZOOM_RANGE = new Vector2(.25f, 2f);
    }

    /// <summary>
    /// A node graph canvas. N is type of node data, C is type of connection data. G is type of group data.
    /// All types must be classes.
    /// </summary>
    public class LCanvas<N, C, G> : VisualElement where N : class where C : class where G : class
    {
        #region Constants & Assets
        private const string NODE_UXML = "ForceNode";
        private const string GROUP_UXML = "ForceGroup";

        private static VisualTreeAsset _nodeUXML;
        private static VisualTreeAsset _groupUXML;

        public VisualTreeAsset nodeUXML => _nodeUXML ??= Resources.Load<VisualTreeAsset>(NODE_UXML);
        public VisualTreeAsset groupUXML => _groupUXML ??= Resources.Load<VisualTreeAsset>(GROUP_UXML);
        #endregion

        #region Constructor
        public LCanvas(Type graphType)
        {
            style.flexGrow = 1;
            this.graphType = graphType;

            InitializeContainers();
            InitializeNewConnectionLine();
            InitializeBackgroundManipulators();
        }

        private void InitializeContainers()
        {
            translationContainer = CreateContainer("TranslationContainer", fullStretch: true);

            dragBox = CreateContainer("DragBox");
            dragBox.style.backgroundColor = Color.red;
            dragBox.style.width = 8f;
            dragBox.style.height = 8f;
            dragBox.style.opacity = 0f;
            translationContainer.Add(dragBox);

            bg = new VisualElement { name = "BG" };
            bg.style.flexGrow = 1;

            Add(bg);
            Add(translationContainer);

            groupsContainer = CreateCenteredContainer("GroupsContainer");
            connectionsContainer = CreateCenteredContainer("ConnectionsContainer");
            effectsContainer = CreateCenteredContainer("EffectsContainer");
            nodesContainer = CreateCenteredContainer("NodesContainer");
            nodesContainer.style.bottom = StyleKeyword.Auto;

            translationContainer.Add(groupsContainer);
            translationContainer.Add(connectionsContainer);
            translationContainer.Add(effectsContainer);
            translationContainer.Add(nodesContainer);
        }

        private void InitializeNewConnectionLine()
        {
            newConnectionLine = new VisualElement { name = "NewConnectionLine" };
            newConnectionLine.style.position = Position.Absolute;
            newConnectionLine.style.backgroundColor = ColorUtility.TryParseHtmlString("#FD6D40", out var color) ? color : Color.green;
            newConnectionLine.style.height = 8f;
            newConnectionLine.style.transformOrigin = new TransformOrigin(0, Length.Percent(50));
            newConnectionLine.AddToClassList("NewConnectionLineBase");
            effectsContainer.Add(newConnectionLine);
        }

        private void InitializeBackgroundManipulators()
        {
            bg.AddManipulator(new ForceDirectedCanvasScrollManipulator(translationContainer));
            bg.AddManipulator(new ForceDirectedCanvasBGManipulator
            {
                OnLeftClick = () => ClearSelection(false),
                OnRightClick = ShowBackgroundContextMenu,
                OnMiddleDrag = delta => translationContainer.transform.position += new Vector3(delta.x, delta.y, 0),
                OnLeftDragStart = pos =>
                {
                    _leftDragging = true;
                    _leftDragStartPos = nodesContainer.WorldToLocal(pos);
                    _leftDragPos = _leftDragStartPos;
                },
                OnLeftDrag = pos =>
                {
                    _leftDragPos += pos;
                    dragBox.transform.position = _leftDragPos;
                }
            });
        }

        private void ShowBackgroundContextMenu(Vector2 mousePos)
        {
            ClearSelection();
            var menu = new GenericDropdownMenu();

            menu.AddItem("Add Node", false, () =>
            {
                LCanvasAddNodeWindow.OpenForCanvas(graphType, nodeType =>
                    AddNullNodeInternal(nodeType, nodesContainer.WorldToLocal(mousePos)));
            });

            if (PossibleGroupTypes.Count > 0)
            {
                menu.AddSeparator("");
                menu.AddDisabledItem("Add Group", false);
                foreach (var (name, groupType) in PossibleGroupTypes)
                {
                    menu.AddItem(name, false, () =>
                        AddNullGroupInternal(groupType, nodesContainer.WorldToLocal(mousePos)));
                }
            }

            menu.DropDown(new Rect(Event.current.mousePosition, Vector2.zero), this, false);
        }

        private static VisualElement CreateContainer(string name, bool fullStretch = false)
        {
            var container = new VisualElement
            {
                name = name,
                pickingMode = PickingMode.Ignore
            };
            container.style.position = Position.Absolute;

            if (fullStretch)
            {
                container.style.left = 0;
                container.style.right = 0;
                container.style.top = 0;
                container.style.bottom = 0;
            }

            return container;
        }

        private static VisualElement CreateCenteredContainer(string name)
        {
            var container = CreateContainer(name);
            container.style.left = Length.Percent(50);
            container.style.top = Length.Percent(50);
            container.style.bottom = 0;
            return container;
        }
        #endregion

        #region Public Properties
        public Type graphType { get; private set; }
        public List<LCanvasNode<N>> nodes { get; private set; } = new();
        public List<LCanvasConnection<N, C>> connections = new();
        public List<LCanvasGroup<G, N>> groups = new();

        public LCanvasNode<N> selectedNode { get; private set; }
        public LCanvasConnection<N, C> selectedConnection { get; private set; }
        public LCanvasGroup<G, N> selectedGroup { get; private set; }
        public LCanvasNode<N> newConnectionFromNode;
        #endregion

        #region UI Containers
        private VisualElement bg;
        private VisualElement dragBox;
        private VisualElement translationContainer;
        private VisualElement nodesContainer;
        private VisualElement connectionsContainer;
        private VisualElement groupsContainer;
        private VisualElement effectsContainer;
        private VisualElement newConnectionLine;
        private List<VisualElement> connectionLines = new();
        #endregion

        #region Internal State
        private Type newConnectionType;
        private LCanvasNode<N> hoveredNode;
        private bool _leftDragging;
        private Vector2 _leftDragStartPos;
        private Vector2 _leftDragPos;
        #endregion

        #region Events
        public Action OnSelectionChanged;
        public Func<N, N, Type, bool> ConnectionValidator;
        public Func<N, N, Type> AutoConnectionValidator;
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
        #endregion

        #region Configuration
        public Dictionary<Type, List<(string, Type)>> PossibleConnectionTypes = new();
        public List<(string, Type)> PossibleNodeTypes = new();
        public List<(string, Type)> PossibleGroupTypes = new();
        #endregion

        #region Node Operations
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
            ui.AddManipulator(new ForceDirectedCanvasBGManipulator()
            {
                OnMiddleDrag = (delta) =>
                {
                    translationContainer.transform.position += new Vector3(delta.x, delta.y, 0);
                },
            });

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

            ui.Q("NodeContainer").AddManipulator(new ForceNodeDragManipulator<N, C, G>(
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
            if (from == null || to == null || from == to)
                return;

            var autoType = AutoConnectionValidator?.Invoke(from.data, to.data);
            if (autoType != null)
                AddConnectionInternal(from, to, autoType);
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
        #endregion

        #region Update & Drawing
        public void Update()
        {
            DrawConnections();
            DrawNewConnection();
            DrawGroups();

            selectedNode?.UpdateContent();
            selectedConnection?.UpdateContent();

            foreach (var connection in connections.Where(c => c.from == selectedNode || c.to == selectedNode))
                connection.UpdateContent();

            translationContainer.transform.scale = Vector3.one * EditorPrefs.GetFloat(LCanvasPrefs.ZOOM_KEY, LCanvasPrefs.DEFAULT_ZOOM);
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
                var pos1 = GetNodeCenterPosition(connection.from);
                var pos2 = GetNodeCenterPosition(connection.to);

                bool isValid = !float.IsNaN(pos1.x) && !float.IsNaN(pos1.y) &&
                               !float.IsNaN(pos2.x) && !float.IsNaN(pos2.y) && pos1 != pos2;

                connectionLine.transform.position = pos1;
                connectionLine.style.height = isValid ? 4f : 0;
                connectionLine.style.width = isValid ? (pos1 - pos2).magnitude : 0;

                if (isValid)
                    connectionLine.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(pos2.y - pos1.y, pos2.x - pos1.x) * Mathf.Rad2Deg);

                connection.UpdateRotation();
            }
        }

        private void DrawNewConnection()
        {
            bool showLine = newConnectionFromNode != null && hoveredNode != null;
            newConnectionLine.style.display = showLine ? DisplayStyle.Flex : DisplayStyle.None;
            newConnectionLine.EnableInClassList("NewConnectionLineAnim", showLine);

            if (!showLine) return;

            var pos1 = GetNodeCenterPosition(newConnectionFromNode);
            var pos2 = GetNodeCenterPosition(hoveredNode);

            newConnectionLine.transform.position = pos1;
            newConnectionLine.style.width = (pos1 - pos2).magnitude;
            newConnectionLine.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(pos2.y - pos1.y, pos2.x - pos1.x) * Mathf.Rad2Deg);
        }

        private static Vector2 GetNodeCenterPosition(LCanvasNode<N> node)
        {
            return node.position + new Vector2(0f, node.element.layout.size.y * 0.5f);
        }

        private void AddConnectionLine(LCanvasConnection<N, C> connection)
        {
            var line = new VisualElement();
            line.style.position = Position.Absolute;
            line.AddToClassList("Connection");
            line.style.alignItems = Align.Center;
            line.style.justifyContent = Justify.SpaceAround;
            line.style.overflow = Overflow.Visible;
            line.style.flexDirection = FlexDirection.Row;

            line.style.height = 4f;
            line.style.transformOrigin = new TransformOrigin(0, Length.Percent(50));

            // Make the line element easier to hover/click
            var hitslop = new VisualElement();
            hitslop.name = "Hitslop";
            hitslop.style.position = Position.Absolute;
            hitslop.style.left = -6;
            hitslop.style.right = -6;
            hitslop.style.top = -6;
            hitslop.style.bottom = -6;
            line.Add(hitslop);

            var arrowsContainer = new VisualElement();
            arrowsContainer.name = "ArrowsContainer";
            arrowsContainer.style.flexDirection = FlexDirection.Row;
            arrowsContainer.style.alignItems = Align.Center;
            arrowsContainer.style.justifyContent = Justify.SpaceAround;
            arrowsContainer.style.flexShrink = 0;
            arrowsContainer.style.flexGrow = 1f;
            line.Add(arrowsContainer);

            var dirArrow = new VisualElement { name = "DirArrow" };
            dirArrow.AddToClassList("ConnectionDirectionContainer");
            arrowsContainer.Add(dirArrow);

            var labelsContainer = new VisualElement();
            labelsContainer.name = "labelsContainer";
            labelsContainer.style.position = Position.Absolute;
            labelsContainer.style.left = 0;
            labelsContainer.style.right = 0;
            labelsContainer.style.top = 0;
            labelsContainer.style.bottom = 0;
            labelsContainer.style.alignItems = Align.Center;
            labelsContainer.style.justifyContent = Justify.SpaceAround;
            line.Add(labelsContainer);

            var connectionLabel = new Label();
            connectionLabel.name = "ConnectionLabel";
            connectionLabel.text = "test label";
            connectionLabel.AddToClassList("ConnectionLabel");
            labelsContainer.Add(connectionLabel);

            connectionsContainer.Add(line);
            connectionLines.Add(line);

            connection.element = line;
            line.style.backgroundRepeat = new BackgroundRepeat(Repeat.Repeat, Repeat.NoRepeat);

            line.AddManipulator(new ForceDirectedCanvasScrollManipulator(translationContainer));
            line.AddManipulator(new LeftRightClickable(
                evt => SelectConnection(connection),
                evt => ShowConnectionContextMenu(connection)
            ));
        }

        private void ShowConnectionContextMenu(LCanvasConnection<N, C> connection)
        {
            var menu = new GenericDropdownMenu();
            menu.AddDisabledItem("Connection", false);
            menu.AddItem("Delete", false, () => DeleteConnectionInternal(connection));
            menu.DropDown(new Rect(Event.current.mousePosition, Vector2.zero), this, false);
        }

        public void UpdateCanvasToFit()
        {
            // Really useful property that Unity doesnt expose right now (:facepalm:)
            // This gives us the rect for all children inside the container
            Rect containerBounds = (Rect)typeof(VisualElement).GetProperty("boundingBox", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(translationContainer);
            Rect containerRect = translationContainer.layout;

            var scaleFact = new Vector2(
                containerRect.width / (containerBounds.width * 1.1f),
                containerRect.height / (containerBounds.height * 1.1f));
            float scale = Mathf.Min(scaleFact.x, scaleFact.y, 1f);
            var targetScale = new Vector3(scale, scale, 1f);
            translationContainer.transform.scale = Vector3.Lerp(translationContainer.transform.scale, targetScale, 0.09f);
        }

        #endregion

        #region Selection
        public void TrySelectData(N data)
        {
            var node = nodes.Find(n => n.data.Equals(data));
            if (node != null)
                SelectNode(node);
        }

        private void SelectNode(LCanvasNode<N> node)
        {
            ClearSelection(false);
            selectedNode = node;
            node.element.Q("Border").AddToClassList("Selected");
            node.element.BringToFront();
            OnSelectionChanged?.Invoke();
        }

        private void SelectConnection(LCanvasConnection<N, C> connection)
        {
            ClearSelection(false);
            selectedConnection = connection;
            int index = connections.IndexOf(selectedConnection);
            connectionLines[index].AddToClassList("ConnectionSelected");
            OnSelectionChanged?.Invoke();
        }

        private void SelectGroup(LCanvasGroup<G, N> group)
        {
            ClearSelection(false);
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
        #endregion

        #region Utilities
        public void SetViewScale(float scale)
        {
            translationContainer.transform.scale = new Vector3(scale, scale, 1f);
        }

        public Vector2 TryGetNodeSnapPosition(Vector2 pos, LCanvasNode<N> ignore)
        {
            var snapPos = pos;
            float snapDist = 6f / EditorPrefs.GetFloat(LCanvasPrefs.ZOOM_KEY, LCanvasPrefs.DEFAULT_ZOOM);
            bool snappedX = false, snappedY = false;

            foreach (var node in nodes)
            {
                if (node == ignore) continue;

                if (!snappedX && Mathf.Abs(node.position.x - snapPos.x) < snapDist)
                {
                    snapPos.x = node.position.x;
                    snappedX = true;
                }
                if (!snappedY && Mathf.Abs(node.position.y - snapPos.y) < snapDist)
                {
                    snapPos.y = node.position.y;
                    snappedY = true;
                }
                if (snappedX && snappedY) break;
            }
            return snapPos;
        }

        public bool TryGetGroupAtPosition(Vector2 pos, out LCanvasGroup<G, N> group)
        {
            group = groups.FirstOrDefault(g => g.element.localBound.Contains(pos));
            return group != null;
        }

        public bool TryGetGroupNodeIsIn(LCanvasNode<N> node, out LCanvasGroup<G, N> group)
        {
            group = groups.FirstOrDefault(g => g.nodes.Contains(node));
            return group != null;
        }

        public bool ElementIsNode(VisualElement element, out LCanvasNode<N> node)
        {
            node = element != null ? nodes.Find(n => n.element == element) : null;
            return node != null;
        }

        public void RepaintAllElements()
        {
            foreach (var node in nodes)
                node.UpdateContent();
            foreach (var connection in connections)
                connection.UpdateContent();
        }
        #endregion
    }
}
