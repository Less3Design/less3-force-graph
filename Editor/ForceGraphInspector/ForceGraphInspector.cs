using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Unity.VisualScripting;
using Sirenix.OdinInspector.Editor;

namespace Less3.ForceGraph.Editor
{

    /// <summary>
    /// The inspector, specifically for the parameters of the graph object.
    /// We need this empty class otherwise we creat an infinite loop of graph inspectors
    /// </summary>
    public class ForceGraphParametersEditorBase : OdinEditor { }

    [CustomEditor(typeof(ForceNode), true)]
    public class ForceNodeParametersEditorBase : OdinEditor { }

    [CustomEditor(typeof(ForceGraph), true)]
    public class ForceGraphInspector : OdinEditor
    {
        public static readonly float DEFAULT_GRAPH_HEIGHT = 400f;
        public static readonly string HEIGHT_SETTING_KEY = "ForceGraphInspectorHeight";
        public static readonly float MIN_GRAPH_HEIGHT = 100f;

        private UnityEditor.Editor graphParametersInspector;

        public VisualTreeAsset inspectorUXML;

        public VisualElement topRoot;
        public VisualElement inspector;

        public VisualElement graphInspectorRoot;
        public VisualElement graphHeightSetter;
        public VisualElement selectionInspectorRoot;

        private ForceDirectedCanvas<ForceNode, ForceConnection> forceDirectedCanvas;
        private ToolbarBreadcrumbs breadcrumbs;

        public override VisualElement CreateInspectorGUI()
        {
            inspector = new VisualElement();
            inspector.name = "$ForceGraphInspector";
            inspectorUXML.CloneTree(inspector);

            forceDirectedCanvas = new ForceDirectedCanvas<ForceNode, ForceConnection>();
            inspector.Q("GraphOrigin").Add(forceDirectedCanvas);

            forceDirectedCanvas.OnSelectionChanged += OnSelectionChanged;
            forceDirectedCanvas.OnNodeCreatedInternally += (node, type) =>
            {
                var asset = (target as ForceGraph).CreateNode(type);
                node.data = asset;
            };
            forceDirectedCanvas.OnNodeDeletedInternally += (node) =>
            {
                (target as ForceGraph).DeleteNode(node.data);
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

            forceDirectedCanvas.ConnectionValidator = (target as ForceGraph).ValidateConnectionRequest;
            forceDirectedCanvas.PossibleConnectionTypes = (target as ForceGraph).GraphConnectionTypes();
            forceDirectedCanvas.PossibleNodeTypes = (target as ForceGraph).GraphNodeTypes();

            breadcrumbs = inspector.Q<ToolbarBreadcrumbs>("Breadcrumbs");
            breadcrumbs.PushItem(target.name, () => GotoGraph());

            inspector.Q<Label>("Typename").text = target.GetType().Name;

            var openScriptElement = inspector.Q("OpenScript");
            openScriptElement.AddManipulator(new Clickable(() => { AssetDatabase.OpenAsset(serializedObject.FindProperty("m_Script").objectReferenceValue); }));

            graphInspectorRoot = inspector.Q("GraphInspector");
            selectionInspectorRoot = inspector.Q("SelectionInspector");

            inspector.Q("Resize").AddManipulator(new ForceGraphInspectorResizeManipulator());

            graphHeightSetter = inspector.Q("GraphParent");
            graphHeightSetter.style.height = EditorPrefs.GetFloat(HEIGHT_SETTING_KEY, DEFAULT_GRAPH_HEIGHT);

            graphParametersInspector = OdinEditor.CreateEditorWithContext(new[] { target }, target, typeof(ForceGraphParametersEditorBase));
            graphInspectorRoot.Add(new InspectorElement(graphParametersInspector));

            // Setup the node graph settings panel

            // Gravity is currently disabled because its kind of redundant with the repulsion force
            var gravitySlider = inspector.Q<Slider>("GravitySlider");
            gravitySlider.value = EditorPrefs.GetFloat(ForceDirectedCanvasSettings.GRAVITY_KEY, ForceDirectedCanvasSettings.DEFAULT_GRAVITY);
            gravitySlider.lowValue = ForceDirectedCanvasSettings.GRAVITY_RANGE.x;
            gravitySlider.highValue = ForceDirectedCanvasSettings.GRAVITY_RANGE.y;
            gravitySlider.RegisterCallback<ChangeEvent<float>>((evt) =>
                EditorPrefs.SetFloat(ForceDirectedCanvasSettings.GRAVITY_KEY, evt.newValue)
            );

            var repulsionSlider = inspector.Q<Slider>("RepulsionSlider");
            repulsionSlider.value = EditorPrefs.GetFloat(ForceDirectedCanvasSettings.NODE_REPULSION_KEY, ForceDirectedCanvasSettings.DEFAULT_NODE_REPULSION);
            repulsionSlider.lowValue = ForceDirectedCanvasSettings.NODE_REPULSION_RANGE.x;
            repulsionSlider.highValue = ForceDirectedCanvasSettings.NODE_REPULSION_RANGE.y;
            repulsionSlider.RegisterCallback<ChangeEvent<float>>((evt) =>
                EditorPrefs.SetFloat(ForceDirectedCanvasSettings.NODE_REPULSION_KEY, evt.newValue)
            );

            var connectionSlider = inspector.Q<Slider>("ConnectionSlider");
            connectionSlider.value = EditorPrefs.GetFloat(ForceDirectedCanvasSettings.CONNECTION_FORCE_KEY, ForceDirectedCanvasSettings.DEFAULT_CONNECTION_FORCE);
            connectionSlider.lowValue = ForceDirectedCanvasSettings.CONNECTION_FORCE_RANGE.x;
            connectionSlider.highValue = ForceDirectedCanvasSettings.CONNECTION_FORCE_RANGE.y;
            connectionSlider.RegisterCallback<ChangeEvent<float>>((evt) =>
                EditorPrefs.SetFloat(ForceDirectedCanvasSettings.CONNECTION_FORCE_KEY, evt.newValue)
            );

            var aspectField = inspector.Q<Vector2Field>("AspectField");
            aspectField.value = new Vector2(
                EditorPrefs.GetFloat(ForceDirectedCanvasSettings.ASPECT_RATIO_X_KEY, ForceDirectedCanvasSettings.DEFAULT_ASPECT_RATIO.x),
                EditorPrefs.GetFloat(ForceDirectedCanvasSettings.ASPECT_RATIO_Y_KEY, ForceDirectedCanvasSettings.DEFAULT_ASPECT_RATIO.y)
            );
            aspectField.RegisterCallback<ChangeEvent<Vector2>>((evt) =>
            {
                EditorPrefs.SetFloat(ForceDirectedCanvasSettings.ASPECT_RATIO_X_KEY, evt.newValue.x);
                EditorPrefs.SetFloat(ForceDirectedCanvasSettings.ASPECT_RATIO_Y_KEY, evt.newValue.y);
            });

            inspector.Q<Button>("ResetSettingsToDefault").clicked += () =>
            {
                EditorPrefs.SetFloat(ForceDirectedCanvasSettings.GRAVITY_KEY, ForceDirectedCanvasSettings.DEFAULT_GRAVITY);
                EditorPrefs.SetFloat(ForceDirectedCanvasSettings.NODE_REPULSION_KEY, ForceDirectedCanvasSettings.DEFAULT_NODE_REPULSION);
                EditorPrefs.SetFloat(ForceDirectedCanvasSettings.CONNECTION_FORCE_KEY, ForceDirectedCanvasSettings.DEFAULT_CONNECTION_FORCE);
                EditorPrefs.SetFloat(ForceDirectedCanvasSettings.ASPECT_RATIO_X_KEY, ForceDirectedCanvasSettings.DEFAULT_ASPECT_RATIO.x);
                EditorPrefs.SetFloat(ForceDirectedCanvasSettings.ASPECT_RATIO_Y_KEY, ForceDirectedCanvasSettings.DEFAULT_ASPECT_RATIO.y);
                gravitySlider.value = ForceDirectedCanvasSettings.DEFAULT_GRAVITY;
                repulsionSlider.value = ForceDirectedCanvasSettings.DEFAULT_NODE_REPULSION;
                connectionSlider.value = ForceDirectedCanvasSettings.DEFAULT_CONNECTION_FORCE;
                aspectField.value = ForceDirectedCanvasSettings.DEFAULT_ASPECT_RATIO;
            };

            // * Shoe / hide settings overlay
            var settingsOverlay = inspector.Q("SettingsOverlay");
            settingsOverlay.style.display = DisplayStyle.None;
            inspector.Q<Button>("SettingsButton").clicked += () => settingsOverlay.style.display = settingsOverlay.style.display == DisplayStyle.None ? DisplayStyle.Flex : DisplayStyle.None;


            foreach (var node in (target as ForceGraph).nodes)
            {
                forceDirectedCanvas.InitNodeExternal(node);
            }

            foreach (var connection in (target as ForceGraph).connections)
            {
                forceDirectedCanvas.InitConnectionExternal(connection.from, connection.to, connection);
            }

            return inspector;
        }

        public new void OnEnable()
        {
            base.OnEnable();
            EditorApplication.update += Update;
        }

        private new void OnDisable()
        {
            base.OnDisable();
            EditorApplication.update -= Update;
        }

        private void OnDestroy()
        {
            DestroyImmediate(graphParametersInspector);
        }

        private void OnSelectionChanged()
        {
            if (forceDirectedCanvas.selectedNode != null)
            {
                GotoNode(forceDirectedCanvas.selectedNode.data);
            }
            else if (forceDirectedCanvas.selectedConnection != null)
            {
                GotoConnection(forceDirectedCanvas.selectedConnection.data);
            }
            else
            {
                GotoGraph();
            }
        }

        private void GotoGraph()
        {
            graphInspectorRoot.style.display = DisplayStyle.Flex;
            selectionInspectorRoot.style.display = DisplayStyle.None;

            breadcrumbs.Clear();
            breadcrumbs.PushItem("<b>" + target.name, () => forceDirectedCanvas.ClearSelection());
        }

        private void GotoNode(ForceNode node)
        {
            selectionInspectorRoot.Clear();
            selectionInspectorRoot.Add(new InspectorElement(forceDirectedCanvas.selectedNode.data));
            graphInspectorRoot.style.display = DisplayStyle.None;
            selectionInspectorRoot.style.display = DisplayStyle.Flex;
            breadcrumbs.Clear();
            breadcrumbs.PushItem(target.name, () => forceDirectedCanvas.ClearSelection());
            breadcrumbs.PushItem("<b>" + node.name, () => { });
        }

        private void GotoConnection(ForceConnection connection)
        {
            selectionInspectorRoot.Clear();
            selectionInspectorRoot.Add(new InspectorElement(forceDirectedCanvas.selectedConnection.data));
            graphInspectorRoot.style.display = DisplayStyle.None;
            selectionInspectorRoot.style.display = DisplayStyle.Flex;
            breadcrumbs.Clear();
            breadcrumbs.PushItem(target.name, () => forceDirectedCanvas.ClearSelection());
            breadcrumbs.PushItem(connection.from.name, () => { forceDirectedCanvas.TrySelectData(connection.from); });
            breadcrumbs.PushItem("<b>" + connection.name, () => { });
            breadcrumbs.PushItem(connection.to.name, () => { forceDirectedCanvas.TrySelectData(connection.to); });
        }

        private void Update()
        {
            if (forceDirectedCanvas != null)
                forceDirectedCanvas.Simulate(1);

            if (graphHeightSetter != null)
                graphHeightSetter.style.height = EditorPrefs.GetFloat(HEIGHT_SETTING_KEY, DEFAULT_GRAPH_HEIGHT);
        }

        public void OnGUI()
        {
            Debug.Log("OnGUI");
        }
    }
}