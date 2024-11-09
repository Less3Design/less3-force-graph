using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Unity.VisualScripting;

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

    [CustomEditor(typeof(ForceGraph), true)]
    public class ForceGraphInspector : UnityEditor.Editor
    {
        public static readonly float DEFAULT_GRAPH_HEIGHT = 400f;
        public static readonly string HEIGHT_SETTING_KEY = "ForceGraphInspectorHeight";
        public static readonly float MIN_GRAPH_HEIGHT = 100f;

        public static readonly string FAST_FORWARD_SETTINGS_KEY = "ForceGraphFastForward";
        public static readonly string FIT_TO_SCREEN_SETTINGS_KEY = "ForceGraphFitToScreen";
        public static readonly string LAYERED_MODE_SETTINGS_KEY = "ForceGraphInspectorLayered";

        private UnityEditor.Editor graphParametersInspector;

        public VisualTreeAsset inspectorUXML;
        public VisualTreeAsset inspectorLayeredUXML;

        public VisualElement inspector;

        public VisualElement graphInspectorRoot;
        public VisualElement graphHeightSetter;
        public VisualElement selectionInspectorRoot;

        private ForceDirectedCanvas<ForceNode, ForceConnection> forceDirectedCanvas;
        private ToolbarBreadcrumbs breadcrumbs;
        private bool isLayeredMode;

        private void UpdateHeight()
        {
            if (inspector == null)
                return;

            // As far as i can tell there is no legit way for an inspector element to fill the inspector window without getting nasty.
            // This manually sets the element height based on some height values in parent elements.

            // contains the inspector header, body (force graph), and footer (unused)
            VisualElement inspectorContainer = inspector.hierarchy.parent?.parent;

            inspector.parent.style.paddingTop = 0;
            inspector.parent.style.paddingBottom = 0;

            if (inspectorContainer == null)
                return;

            int headerAndFooterHeight = 0;
            foreach (var child in inspectorContainer.Children())
            {
                if (child != inspector.parent)
                {
                    headerAndFooterHeight += (int)child.resolvedStyle.height;
                }
            }

            // Gets the "TemplateContainer" at the top of the inspector window. we want to fill this.
            VisualElement bigContainer = inspectorContainer.hierarchy.parent.hierarchy.parent.hierarchy.parent.hierarchy.parent.hierarchy.parent.hierarchy.parent;

            if (bigContainer == null)
            {
                return;
            }

            foreach (var child in bigContainer.Children())
            {
                if (child is ScrollView)
                {
                    continue;
                }
                headerAndFooterHeight += (int)child.resolvedStyle.height;
            }

            inspector.style.height = Mathf.FloorToInt(bigContainer.resolvedStyle.height - headerAndFooterHeight) - 4;
        }

        private bool init;

        public override VisualElement CreateInspectorGUI()
        {
            isLayeredMode = EditorPrefs.GetBool(LAYERED_MODE_SETTINGS_KEY, true);

            inspector = new VisualElement();
            inspector.name = "$ForceGraphInspector";
            if (isLayeredMode)
            {
                inspectorLayeredUXML.CloneTree(inspector);
            }
            else
            {
                inspectorUXML.CloneTree(inspector);
            }

            forceDirectedCanvas = new ForceDirectedCanvas<ForceNode, ForceConnection>();
            inspector.Q("GraphOrigin").Add(forceDirectedCanvas);

            init = false;
            inspector.RegisterCallback<GeometryChangedEvent>((evt) =>
            {
                if (!init)
                {
                    UpdateHeight();
                    init = true;
                }
            });

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

            forceDirectedCanvas.ConnectionValidator = (target as ForceGraph).ValidateConnectionRequest;
            forceDirectedCanvas.PossibleConnectionTypes = (target as ForceGraph).GraphConnectionTypes();
            forceDirectedCanvas.PossibleNodeTypes = (target as ForceGraph).GraphNodeTypes();

            breadcrumbs = inspector.Q<ToolbarBreadcrumbs>("Breadcrumbs");
            breadcrumbs.PushItem("<b>" + target.name, () => GotoGraph());

            inspector.Q<Label>("Typename").text = target.GetType().Name;

            var openScriptElement = inspector.Q("OpenScript");
            openScriptElement.AddManipulator(new Clickable(() => { AssetDatabase.OpenAsset(serializedObject.FindProperty("m_Script").objectReferenceValue); }));

            graphInspectorRoot = inspector.Q("GraphInspector");
            selectionInspectorRoot = inspector.Q("SelectionInspector");

            if (!isLayeredMode)
            {
                inspector.Q("Resize").AddManipulator(new ForceGraphInspectorResizeManipulator());
                graphHeightSetter = inspector.Q("GraphParent");
                graphHeightSetter.style.height = EditorPrefs.GetFloat(HEIGHT_SETTING_KEY, DEFAULT_GRAPH_HEIGHT);
            }

            graphParametersInspector = UnityEditor.Editor.CreateEditorWithContext(new[] { target }, target, typeof(ForceGraphParametersEditorBase));
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

            var fitToScreenButton = inspector.Q<Button>("ScaleToFit");
            bool fitToScreen = EditorPrefs.GetBool(FIT_TO_SCREEN_SETTINGS_KEY, true);
            if (fitToScreen)
                fitToScreenButton.AddToClassList("ToggleButtonEnabled");
            else
                fitToScreenButton.RemoveFromClassList("ToggleButtonEnabled");
            forceDirectedCanvas.FitInView = fitToScreen;

            fitToScreenButton.clicked += () =>
            {
                EditorPrefs.SetBool(FIT_TO_SCREEN_SETTINGS_KEY, !EditorPrefs.GetBool(FIT_TO_SCREEN_SETTINGS_KEY, true));
                bool fitToScreen = EditorPrefs.GetBool(FIT_TO_SCREEN_SETTINGS_KEY, true);
                if (fitToScreen)
                {
                    fitToScreenButton.AddToClassList("ToggleButtonEnabled");
                    forceDirectedCanvas.FitInView = true;
                    forceDirectedCanvas.UpdateCanvasToFit();
                }
                else
                {
                    fitToScreenButton.RemoveFromClassList("ToggleButtonEnabled");
                    forceDirectedCanvas.FitInView = false;
                }
            };

            var layeredModeButton = inspector.Q<Button>("LayeredMode");
            bool lMode = EditorPrefs.GetBool(LAYERED_MODE_SETTINGS_KEY, true);
            if (lMode)
            {
                layeredModeButton.AddToClassList("ToggleButtonEnabled");
            }
            else
            {
                layeredModeButton.RemoveFromClassList("ToggleButtonEnabled");
            }

            layeredModeButton.clicked += () =>
            {
                EditorPrefs.SetBool(LAYERED_MODE_SETTINGS_KEY, !EditorPrefs.GetBool(LAYERED_MODE_SETTINGS_KEY, true));
                bool lMode = EditorPrefs.GetBool(LAYERED_MODE_SETTINGS_KEY, true);
                if (lMode)
                {
                    layeredModeButton.AddToClassList("ToggleButtonEnabled");
                }
                else
                {
                    layeredModeButton.RemoveFromClassList("ToggleButtonEnabled");
                }
                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
            };

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

            if (EditorPrefs.GetBool(FIT_TO_SCREEN_SETTINGS_KEY, true))
            {
                forceDirectedCanvas.FitInView = true;
                // Cause small graphs to zoom in, and large graphs to zoom out when opening if fit is enabled .
                // Just makes it feel a bit more interesting when scrolling through graphs
                forceDirectedCanvas.SetViewScale(.5f);
            }

            return inspector;
        }

        public void OnEnable()
        {
            EditorApplication.update += Update;
        }

        private void OnDisable()
        {
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
            //breadcrumbs.PushItem(target.name, () => forceDirectedCanvas.ClearSelection());
            breadcrumbs.PushItem("<b>" + node.name, () => { });
            breadcrumbs.PushItem("<i>" + node.GetType().Name, () =>
            {
                var so = new SerializedObject(node);
                AssetDatabase.OpenAsset(so.FindProperty("m_Script").objectReferenceValue);
            });
        }

        private void GotoConnection(ForceConnection connection)
        {
            selectionInspectorRoot.Clear();
            selectionInspectorRoot.Add(new InspectorElement(forceDirectedCanvas.selectedConnection.data));
            graphInspectorRoot.style.display = DisplayStyle.None;
            selectionInspectorRoot.style.display = DisplayStyle.Flex;
            breadcrumbs.Clear();
            //breadcrumbs.PushItem(target.name, () => forceDirectedCanvas.ClearSelection());
            //breadcrumbs.PushItem(connection.from.name, () => { forceDirectedCanvas.TrySelectData(connection.from); });
            breadcrumbs.PushItem("<b>" + connection.name, () => { });

            breadcrumbs.PushItem("<i>" + connection.GetType().Name, () =>
            {
                var so = new SerializedObject(connection);
                AssetDatabase.OpenAsset(so.FindProperty("m_Script").objectReferenceValue);
            });
            //breadcrumbs.PushItem(connection.to.name, () => { forceDirectedCanvas.TrySelectData(connection.to); });
        }

        private void Update()
        {
            if (forceDirectedCanvas != null)
            {
                // 4 steps per tick on fast forward. That might be excessive for slow machines. 2 or 3 are still useful
                forceDirectedCanvas.Simulate(EditorPrefs.GetBool(FAST_FORWARD_SETTINGS_KEY, false) ? 4 : 1);

                foreach (var node in forceDirectedCanvas.nodes)
                {
                    // We ignore this to try and avoid unnecessary writes to the asset
                    if (Mathf.Approximately(node.data.position.x, node.simPosition.x) && Mathf.Approximately(node.data.position.y, node.simPosition.y))
                        continue;
                    node.data.position = node.simPosition;
                }
            }


            if (isLayeredMode)
            {
                UpdateHeight();
            }
            else
            {
                Debug.Log("UpdateHeight");
                if (graphHeightSetter != null)
                {
                    Debug.Log("UpdateHeight2");
                    graphHeightSetter.style.height = EditorPrefs.GetFloat(HEIGHT_SETTING_KEY, DEFAULT_GRAPH_HEIGHT);
                }
            }
        }
    }
}
