using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Less3.ForceGraph.Editor
{
    public class LCanvasAddNodeWindow : EditorWindow
    {
        private const string ADD_NODE_UXML = "AddNodeMenu";
        private static VisualTreeAsset _addNodeUXML;
        public VisualTreeAsset addNodeUXML
        {
            get
            {
                if (_addNodeUXML == null)
                    _addNodeUXML = Resources.Load<VisualTreeAsset>(ADD_NODE_UXML);
                return _addNodeUXML;
            }
        }

        private TreeView treeView;
        private TextField searchField;
        private string addNodeFilter = "";
        private Type graphType;
        private Action<Type> nodeSelectedCallback;

        public static void OpenForCanvas(Type graphType, Action<Type> nodeSelectedCallback)
        {
            LCanvasAddNodeWindow window = ScriptableObject.CreateInstance<LCanvasAddNodeWindow>();
            //get mouse position in screen space
            Vector2 mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            window.position = new Rect(mousePos.x - 32, mousePos.y - 40, 256, 256);
            window.ShowPopup();
            window.Focus();
            window.Setup(graphType, nodeSelectedCallback);
        }

        void OnLostFocus()
        {
            if (this != null)
            {
                this.Close();
            }
        }

        public void Setup(Type graphType, Action<Type> nodeSelectedCallback)
        {
            this.graphType = graphType;
            this.nodeSelectedCallback = nodeSelectedCallback;

            treeView = rootVisualElement.Q<TreeView>("TreeView");



            treeView.makeItem = () =>
            {
                var label = new Label();
                label.AddToClassList("ListItem");
                return label;
            };

            treeView.bindItem = (element, i) =>
            {
                var item = treeView.GetItemDataForIndex<NodeTreeEntry>(i);
                var label = element as Label;
                if (!string.IsNullOrEmpty(addNodeFilter))
                {
                    label.text = item.path.Replace(addNodeFilter, $"<b><color=yellow>{addNodeFilter}</color></b>", comparisonType: System.StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    label.text = item.path;
                }
            };
            searchField = rootVisualElement.Q<TextField>("search");
            searchField.Focus();
            searchField.RegisterValueChangedCallback(evt =>
            {
                string filterText = evt.newValue.ToLower();
                addNodeFilter = filterText;
                if (string.IsNullOrEmpty(filterText))
                {
                    treeView.autoExpand = false;
                    treeView.SetRootItems(LCanvasCreateNodeCache.GetMenuForGraph(graphType));
                }
                else
                {
                    var filteredItems = LCanvasCreateNodeCache.GetFilteredMenuForGraph(graphType, filterText);
                    treeView.autoExpand = true;
                    treeView.SetRootItems(filteredItems);
                    treeView.ExpandAll();
                }
                treeView.Rebuild();
            });

            rootVisualElement.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Escape)
                {
                    this.Close();
                }

                // lets you focus the tree view with down arrow
                if (evt.keyCode == KeyCode.DownArrow && searchField.focusController.focusedElement == searchField)
                {
                    treeView.Focus();
                    treeView.selectedIndex = 0;
                }

                // return to search field if at top of tree view
                if (evt.keyCode == KeyCode.UpArrow && treeView.selectedIndex == 0)
                {
                    searchField.Focus();
                }
                //! trickledown lets this work even though the text field is focused etc.
            }, TrickleDown.TrickleDown);

            treeView.itemsChosen += objs =>
            {
                // get first selected item
                var item = objs.FirstOrDefault();
                if (item is NodeTreeEntry entry && entry.type != null)
                {
                    nodeSelectedCallback?.Invoke(entry.type);
                    this.Close();
                }
            };

            treeView.SetRootItems(LCanvasCreateNodeCache.GetMenuForGraph(graphType));
            treeView.Rebuild();
        }

        public void CreateGUI()
        {
            rootVisualElement.Add(addNodeUXML.Instantiate());
        }
    }
}
