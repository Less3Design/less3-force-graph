using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Less3.ForceGraph.Editor
{
    // Creates cached tree collections for building the create node menu.

    public static class LCanvasCreateNodeCache
    {
        public static Dictionary<Type, List<TreeViewItemData<NodeTreeEntry>>> nodeCreateMenuCache = new Dictionary<Type, List<TreeViewItemData<NodeTreeEntry>>>();

        public static List<TreeViewItemData<NodeTreeEntry>> GetMenuForGraph(Type graphType)
        {
            if (nodeCreateMenuCache.ContainsKey(graphType))
            {
                return nodeCreateMenuCache[graphType];
            }
            return new List<TreeViewItemData<NodeTreeEntry>>();
        }

        public static List<TreeViewItemData<NodeTreeEntry>> GetFilteredMenuForGraph(Type graphType, string filterText)
        {
            if (nodeCreateMenuCache.ContainsKey(graphType))
            {
                if (string.IsNullOrEmpty(filterText))
                {
                    return nodeCreateMenuCache[graphType];
                }
                else
                {
                    filterText = filterText.ToLower();
                    return GetFilteredTree(nodeCreateMenuCache[graphType], filterText);
                }
            }
            return new List<TreeViewItemData<NodeTreeEntry>>();
        }

        static LCanvasCreateNodeCache()
        {
            // get all LCreateNodeMenuAttribute types
            List<(Type graphType, Type nodeType, LCreateNodeMenuAttribute[] attrs)> types = new List<(Type, Type, LCreateNodeMenuAttribute[])>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    var attrs = (LCreateNodeMenuAttribute[])type.GetCustomAttributes(typeof(LCreateNodeMenuAttribute), false);
                    if (attrs.Length > 0)
                    {
                        types.Add((attrs[0].graphType, type, attrs));
                    }
                }
            }

            // sort by graph types
            Dictionary<Type, List<(Type nodeType, LCreateNodeMenuAttribute att)>> graphTypes = new Dictionary<Type, List<(Type, LCreateNodeMenuAttribute)>>();
            foreach (var (graphType, nodeType, attrs) in types)
            {
                if (!graphTypes.ContainsKey(graphType))
                    graphTypes[graphType] = new List<(Type, LCreateNodeMenuAttribute)>();

                foreach (var attr in attrs)
                {
                    if (attr.graphType == graphType)
                    {
                        graphTypes[graphType].Add((nodeType, attr));
                    }
                }
            }

            // build tree for each graph type
            foreach (var graphType in graphTypes.Keys)
            {
                BuildMenuForGraph(graphType, graphTypes[graphType]);
            }
        }

        public static void BuildMenuForGraph(Type graphType, List<(Type nodeType, LCreateNodeMenuAttribute att)> nodeTypes)
        {
            if (nodeCreateMenuCache.ContainsKey(graphType))
                return;

            // where object is either a leaf or another dictionary woohoo
            var tree = new Dictionary<string, object>();

            foreach (var (nodeType, att) in nodeTypes)
            {
                string[] pathParts = att.path.Split('/');

                Dictionary<string, object> currentLevel = tree;
                for (int i = 0; i < pathParts.Length; i++)
                {
                    if (i == pathParts.Length - 1)
                    {
                        // leaf
                        currentLevel[pathParts[i]] = nodeType;
                    }
                    else
                    {
                        // folder
                        if (!currentLevel.ContainsKey(pathParts[i]))
                        {
                            currentLevel[pathParts[i]] = new Dictionary<string, object>();
                            currentLevel = (Dictionary<string, object>)currentLevel[pathParts[i]];
                        }
                        else
                        {
                            currentLevel = (Dictionary<string, object>)currentLevel[pathParts[i]];
                        }
                    }
                }
            }

            // recursively create tree view items
            List<TreeViewItemData<NodeTreeEntry>> BuildTree(Dictionary<string, object> subtree)
            {
                List<TreeViewItemData<NodeTreeEntry>> items = new List<TreeViewItemData<NodeTreeEntry>>();

                foreach (var key in subtree.Keys.OrderBy(k => k))
                {
                    if (subtree[key] is Type nodeType)
                    {
                        // leaf
                        var entry = new NodeTreeEntry { path = key, type = nodeType };
                        items.Add(new TreeViewItemData<NodeTreeEntry>(entry.path.GetHashCode(), entry));
                    }
                    else if (subtree[key] is Dictionary<string, object> childSubtree)
                    {
                        // folder
                        var children = BuildTree(childSubtree);
                        var entry = new NodeTreeEntry { path = key, type = null };
                        items.Add(new TreeViewItemData<NodeTreeEntry>(entry.path.GetHashCode(), entry, children));
                    }
                }

                return items;
            }

            List<TreeViewItemData<NodeTreeEntry>> items = new List<TreeViewItemData<NodeTreeEntry>>();
            foreach (var key in tree.Keys)
            {
                if (tree[key] is Dictionary<string, object> branch)
                {
                    items.Add(new TreeViewItemData<NodeTreeEntry>(key.GetHashCode(), new NodeTreeEntry { path = key, type = null }, BuildTree(branch)));
                }
                else if (tree[key] is Type nodeType)
                {
                    var entry = new NodeTreeEntry { path = key, type = nodeType };
                    items.Add(new TreeViewItemData<NodeTreeEntry>(entry.path.GetHashCode(), entry));
                }
                else
                {
                    Debug.LogError("Unexpected structure in node create menu tree.");
                }
            }
            nodeCreateMenuCache[graphType] = items;
        }

        // Recursive function to filter TreeView items
        private static List<TreeViewItemData<NodeTreeEntry>> GetFilteredTree(IEnumerable<TreeViewItemData<NodeTreeEntry>> items, string filterText)
        {
            List<TreeViewItemData<NodeTreeEntry>> result = new List<TreeViewItemData<NodeTreeEntry>>();
            foreach (var item in items)
            {
                bool matches = item.data.path.ToLower().Contains(filterText);
                IEnumerable<TreeViewItemData<NodeTreeEntry>> filteredChildren = null;

                if (item.children != null && item.children.Count() > 0)
                {
                    filteredChildren = GetFilteredTree(item.children, filterText);
                }

                // Include the item if it matches or if any of its children match
                if (matches || (filteredChildren != null && filteredChildren.Count() > 0))
                {
                    // If the item itself doesn't match but its children do, create a new item with only the matching children
                    if (!matches && filteredChildren != null)
                    {
                        var newItem = new TreeViewItemData<NodeTreeEntry>(item.id, item.data, filteredChildren.ToList());
                        result.Add(newItem);
                    }
                    else // If the item matches, or if it matches and has children (even if they don't match), add it as is
                    {
                        result.Add(item);
                    }
                }
            }
            return result;
        }

    }

    public struct NodeTreeEntry
    {
        public string path;
        public System.Type type;
    }
}
