using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Callbacks;

namespace Less3.ForceGraph.Editor
{
    /// <summary>
    /// The inspector, specifically for the parameters of the graph object.
    /// We need this empty class otherwise we create an infinite loop of graph inspectors
    /// </summary>
    public class ForceGraphParametersEditorBase : UnityEditor.Editor
    {
        private readonly string[] EXCLUDED_PROPERTIES = new string[] { "m_Script", "nodes", "connections" };

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, EXCLUDED_PROPERTIES);
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(ForceNode), true)]
    public class ForceNodeParametersEditorBase : UnityEditor.Editor
    {
        private readonly string[] EXCLUDED_PROPERTIES = new string[] { "m_Script", "position" };

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, EXCLUDED_PROPERTIES);
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(ForceConnection), true)]
    public class ForceConnectionParametersEditorBase : UnityEditor.Editor
    {
        private readonly string[] EXCLUDED_PROPERTIES = new string[] { "m_Script", "from", "to" };

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, EXCLUDED_PROPERTIES);
            serializedObject.ApplyModifiedProperties();
        }
    }

    public class ForceGraphInspector : EditorWindow
    {
        public static readonly float DEFAULT_GRAPH_HEIGHT = 400f;
        public static readonly string HEIGHT_SETTING_KEY = "ForceGraphInspectorHeight";
        public static readonly float MIN_GRAPH_HEIGHT = 100f;

        public static readonly string FAST_FORWARD_SETTINGS_KEY = "ForceGraphFastForward";
        public static readonly string FIT_TO_SCREEN_SETTINGS_KEY = "ForceGraphFitToScreen";
        public static readonly string LAYERED_MODE_SETTINGS_KEY = "ForceGraphInspectorLayered";
        public static readonly string PIN_OVERLAY_SETTINGS_KEY = "ForceGraphInspectorPinOverlay";

        public static readonly string OVERLAY_X_SETTINGS_KEY = "ForceGraphInspectorOverlayX";
        public static readonly string OVERLAY_Y_SETTINGS_KEY = "ForceGraphInspectorOverlayY";

        [SerializeField]
        private ForceGraph target;
        [SerializeField]
        private bool wasInit;

        private UnityEditor.Editor graphParametersInspector;

        // assigned in the inspector
        public VisualTreeAsset inspectorLayeredUXML;
        // assigned in the inspector
        public Texture2D windowIcon;

        public VisualElement inspector;
        public Label inspectorLabel;
        public VisualElement inspectorOverlay;

        public VisualElement graphInspectorRoot;
        public VisualElement graphHeightSetter;
        public VisualElement selectionInspectorRoot;

        private Label typeNameLabel;
        private Object scriptTypeObjectRef;

        private LCanvas<ForceNode, ForceConnection, ForceGroup> forceDirectedCanvas;
        private ToolbarBreadcrumbs breadcrumbs;


        [OnOpenAsset(1)]
        public static bool DoubleClickAsset(int instanceID, int line)
        {
            Object obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj is ForceGraph forceGraph)
            {
                OpenInspector(forceGraph);
                return true; // we handled the open
            }
            return false; // we did not handle the open
        }

        public static void OpenInspector(ForceGraph graph)
        {
            var window = GetWindow<ForceGraphInspector>();
            window.InitGUI(graph);//
        }

        public void InitGUI(ForceGraph graph)
        {
            target = graph;
            titleContent = new GUIContent(target.name, windowIcon);
            if (inspector != null)
            {
                inspector.RemoveFromHierarchy();
            }

            inspector = new VisualElement();
            inspector.name = "$ForceGraphInspector";
            inspectorLayeredUXML.CloneTree(inspector);


            forceDirectedCanvas = new LCanvas<ForceNode, ForceConnection, ForceGroup>();
            inspector.Q("GraphOrigin").Add(forceDirectedCanvas);

            forceDirectedCanvas.OnSelectionChanged += OnSelectionChanged;

            // ? The graph UI creates UI elements, but has no concept of how to create the backing data.
            //   Here we are taking "create new object requests" and filling in correct data (creating new SO's)
            forceDirectedCanvas.OnGroupCreatedInternally += (group, type) =>
            {
                var asset = (target as ForceGraph).CreateGroup(type);
                group.data = asset;
            };

            forceDirectedCanvas.OnNodeAddedToGroupInternally += (node, group) =>
            {
                (target as ForceGraph).AddNodeToGroup(node, group);
            };

            forceDirectedCanvas.OnNodeRemovedFromGroupInternally += (node, group) =>
            {
                if (group == null || node == null)
                {
                    return;
                }
                // TODO maybe don't do from all. Arguably a bug
                (target as ForceGraph).RemoveNodeFromAllGroups(node);
            };
            forceDirectedCanvas.OnGroupDeletedInternally += (group) =>
            {
                (target as ForceGraph).DeleteGroup(group.data);
            };

            forceDirectedCanvas.OnNodeCreatedInternally += (node, type) =>
            {
                var asset = (target as ForceGraph).CreateNode(type);
                node.data = asset;
            };

            // ? Same as above, but for deletion and duplication etc
            forceDirectedCanvas.OnNodeDeletedInternally += (node) =>
            {
                (target as ForceGraph).DeleteNode(node.data);
            };
            forceDirectedCanvas.OnNodeDuplicatedInternally += (node) =>
            {
                var n = (target as ForceGraph).DuplicateNode(node.data);
                forceDirectedCanvas.InitNodeExternal(n, n.position);
            };
            forceDirectedCanvas.OnConnectionCreatedInternally += (connection, type) =>
            {
                var asset = (target as ForceGraph).CreateConnection(connection.from.data, connection.to.data, type);
                connection.data = asset;
            };
            forceDirectedCanvas.OnConnectionDeletedInternally += (connection) =>
            {
                (target as ForceGraph).DeleteConnection(connection.data);
            };

            forceDirectedCanvas.ConnectionValidator = target.ValidateConnectionRequest;
            forceDirectedCanvas.PossibleConnectionTypes = (target as ForceGraph).GraphConnectionTypes();
            forceDirectedCanvas.PossibleNodeTypes = (target as ForceGraph).GraphNodeTypes();
            forceDirectedCanvas.PossibleGroupTypes = (target as ForceGraph).GraphGroupTypes();

            breadcrumbs = inspector.Q<ToolbarBreadcrumbs>("Breadcrumbs");
            breadcrumbs.PushItem("<b>" + target.name, () => InspectGraph());

            inspector.Q<Label>("Typename").text = target.GetType().Name;
            typeNameLabel = inspector.Q<Label>("Typename");

            var openScriptElement = inspector.Q("OpenScript");
            openScriptElement.AddManipulator(new Clickable(() =>
            {
                if (scriptTypeObjectRef != null)
                    AssetDatabase.OpenAsset(scriptTypeObjectRef);
            }));

            graphInspectorRoot = inspector.Q("GraphInspector");
            selectionInspectorRoot = inspector.Q("SelectionInspector");
            graphInspectorRoot.Add(new InspectorElement(target));

            inspectorLabel = inspector.Q<Label>("InspectorLabel");
            inspectorLabel.text = target.GetType().Name;
            inspectorOverlay = inspector.Q("InspectorOverlay");
            // drag
            inspectorOverlay.AddManipulator(new ForceGraphInspectorOverlayManipulator(inspectorOverlay));

            // * Show / hide settings overlay
            var settingsOverlay = inspector.Q("SettingsOverlay");
            settingsOverlay.style.display = DisplayStyle.None;
            var settingsButton = inspector.Q<Button>("SettingsButton");
            settingsButton.clicked += () =>
            {
                bool show = settingsOverlay.style.display == DisplayStyle.None;
                settingsOverlay.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
                if (show)
                    settingsButton.AddToClassList("ToggleButtonEnabled");
                else
                    settingsButton.RemoveFromClassList("ToggleButtonEnabled");
            };

            var fastForwardButton = inspector.Q<Button>("FastForward");
            if (EditorPrefs.GetBool(FAST_FORWARD_SETTINGS_KEY, false))
                fastForwardButton.AddToClassList("ToggleButtonEnabled");
            else
                fastForwardButton.RemoveFromClassList("ToggleButtonEnabled");

            fastForwardButton.clicked += () =>
            {
                EditorPrefs.SetBool(FAST_FORWARD_SETTINGS_KEY, !EditorPrefs.GetBool(FAST_FORWARD_SETTINGS_KEY, false));
                bool fastForward = EditorPrefs.GetBool(FAST_FORWARD_SETTINGS_KEY, false);
                if (fastForward)
                    fastForwardButton.AddToClassList("ToggleButtonEnabled");
                else
                    fastForwardButton.RemoveFromClassList("ToggleButtonEnabled");
            };

            var snapButton = inspector.Q<Button>("Snap");
            if (EditorPrefs.GetBool(LCanvasPrefs.SNAP_SETTINGS_KEY, true))
                snapButton.AddToClassList("ToggleButtonEnabled");
            else
                snapButton.RemoveFromClassList("ToggleButtonEnabled");

            snapButton.clicked += () =>
            {
                EditorPrefs.SetBool(LCanvasPrefs.SNAP_SETTINGS_KEY, !EditorPrefs.GetBool(LCanvasPrefs.SNAP_SETTINGS_KEY, true));
                bool snap = EditorPrefs.GetBool(LCanvasPrefs.SNAP_SETTINGS_KEY, true);
                if (snap)
                    snapButton.AddToClassList("ToggleButtonEnabled");
                else
                    snapButton.RemoveFromClassList("ToggleButtonEnabled");
            };

            var pinOverlayButton = inspector.Q("PinInspector");
            if (EditorPrefs.GetBool(PIN_OVERLAY_SETTINGS_KEY, false))
                pinOverlayButton.AddToClassList("PinOn");
            else
                pinOverlayButton.RemoveFromClassList("PinOn");

            pinOverlayButton.AddManipulator(new Clickable(() =>
            {
                bool pinned = !EditorPrefs.GetBool(PIN_OVERLAY_SETTINGS_KEY, false);
                EditorPrefs.SetBool(PIN_OVERLAY_SETTINGS_KEY, pinned);
                if (pinned)
                    pinOverlayButton.AddToClassList("PinOn");
                else
                    pinOverlayButton.RemoveFromClassList("PinOn");
            }));

            foreach (var node in (target as ForceGraph).nodes)
            {
                if (node.position == Vector2.zero)
                    node.position = new Vector2(UnityEngine.Random.Range(-200, 200), UnityEngine.Random.Range(-200, 200));
                forceDirectedCanvas.InitNodeExternal(node, node.position);
            }

            foreach (var connection in (target as ForceGraph).connections)
            {
                forceDirectedCanvas.InitConnectionExternal(connection.from, connection.to, connection);
            }

            foreach (var group in (target as ForceGraph).groups)
            {
                forceDirectedCanvas.InitGroupExternal(group, group.position, group.nodes);
            }

            if (EditorPrefs.GetBool(FIT_TO_SCREEN_SETTINGS_KEY, true))
            {
                // Cause small graphs to zoom in, and large graphs to zoom out when opening if fit is enabled .
                // Just makes it feel a bit more interesting when scrolling through graphs
                forceDirectedCanvas.SetViewScale(.5f);
            }

            rootVisualElement.Add(inspector);
        }

        public void OnEnable()
        {
            EditorApplication.update += Update;//
            if (wasInit)
            {
                InitGUI(target);
                wasInit = false;
            }
        }

        private void OnDisable()//
        {
            EditorApplication.update -= Update;
            wasInit = true;
        }

        private void OnDestroy()
        {
            DestroyImmediate(graphParametersInspector);
        }

        private void OnSelectionChanged()
        {
            if (forceDirectedCanvas.selectedNode != null)
            {
                InspectObject(forceDirectedCanvas.selectedNode.data);
            }
            else if (forceDirectedCanvas.selectedConnection != null)
            {
                InspectObject(forceDirectedCanvas.selectedConnection.data);
            }
            else if (forceDirectedCanvas.selectedGroup != null)
            {
                InspectObject(forceDirectedCanvas.selectedGroup.data);
            }
            else
            {
                InspectGraph();
            }
        }

        // ? I don't remember why inspect graph is slightly different. I think its not needed anymore. SHould try removing:
        private void InspectGraph()
        {
            graphInspectorRoot.style.display = DisplayStyle.Flex;
            selectionInspectorRoot.style.display = DisplayStyle.None;

            typeNameLabel.text = target.GetType().Name;
            var so = new SerializedObject(target);
            inspectorLabel.text = target.GetType().Name;
            scriptTypeObjectRef = so.FindProperty("m_Script").objectReferenceValue;
        }

        private void InspectObject(Object connection)
        {
            selectionInspectorRoot.Clear();
            selectionInspectorRoot.Add(new InspectorElement(connection));
            graphInspectorRoot.style.display = DisplayStyle.None;
            selectionInspectorRoot.style.display = DisplayStyle.Flex;
            typeNameLabel.text = connection.GetType().Name;
            var so = new SerializedObject(connection);
            inspectorLabel.text = connection.GetType().Name;
            scriptTypeObjectRef = so.FindProperty("m_Script").objectReferenceValue;
        }

        private void Update()
        {
            if (forceDirectedCanvas != null)
            {
                forceDirectedCanvas.Update();

                // TODO: This is reacting to node position changes:
                // This should be a built in feature of the canvas with a callback like add/delete etc.

                foreach (var node in forceDirectedCanvas.nodes)
                {
                    // We ignore this to try and avoid unnecessary writes to the asset
                    if (Mathf.Approximately(node.data.position.x, node.position.x) && Mathf.Approximately(node.data.position.y, node.position.y))
                        continue;
                    node.data.position = node.position;
                }

                foreach (var group in forceDirectedCanvas.groups)
                {
                    if (Mathf.Approximately(group.data.position.x, group.position.x) && Mathf.Approximately(group.data.position.y, group.position.y))
                        continue;
                    group.data.position = group.position;
                }

                if (inspectorOverlay != null && inspectorOverlay.panel != null)
                {
                    // validate overlay position and prefs
                    Vector2 newPos = new Vector2(EditorPrefs.GetFloat(OVERLAY_X_SETTINGS_KEY, 0), EditorPrefs.GetFloat(OVERLAY_Y_SETTINGS_KEY, 0));
                    newPos.x = Mathf.Clamp(newPos.x, 0, inspectorOverlay.panel.visualTree.worldBound.width - inspectorOverlay.layout.width - 32);
                    newPos.y = Mathf.Clamp(newPos.y, 0, inspectorOverlay.panel.visualTree.worldBound.height - inspectorOverlay.layout.height - 48);
                    EditorPrefs.SetFloat("ForceGraphInspectorOverlayManipulatorX", newPos.x);
                    EditorPrefs.SetFloat("ForceGraphInspectorOverlayManipulatorY", newPos.y);
                    inspectorOverlay.transform.position = newPos;
                }
            }
        }
    }
}
