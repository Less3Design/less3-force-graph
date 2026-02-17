<img width="1080" height="240" alt="Frame 50" src="https://github.com/user-attachments/assets/34f8dffb-b8a6-439a-ac6f-d82fa8021949" />

A scriptable object framework and node graph UI that lets you easily create complex branching trees of data.

## Scriptable Object Graph
A `Graph` object can contain `Node` and `Connection` objects that connect with each other to create a tree.

Inside the `Graph` object you define what sort of nodes and connection the class supports, and any connection rules between the two you require.

```csharp
using Less3.ForceGraph;
public class GenerationGraph : ForceGraph
{
    public override List<(string, Type)> GraphNodeTypes()
    {
        return new List<(string, Type)>
        {
            ("GenNode" , typeof(GenerationNode))
        };
    }
    public override Dictionary<Type, List<(string, Type)>> GraphConnectionTypes()
    {
        return new Dictionary<Type, List<(string, Type)>>
        {
            {typeof(GenerationNode), new List<(string,Type)>() {
                ("Gen Connection", typeof(GenerationConnection))
            }}
        };
    }

    public override bool ValidateConnectionRequest(ForceNode from, ForceNode to, Type connectionType)
    {
        return true;// assume all connections are valid
    }
}
```

`Connections` and `Nodes` behave as simple scriptable objects you can fill with whatever data you need.

```csharp
public class GenerationNode : ForceNode
{
    public float exampleData;
}

public class GenerationConnection : ForceConnection
{
    public float helloThere;
}
```

## Canvas
The graph objects are rendered using a Generic `LCanvas<N,C,G>` VisualElement. This element handles the entire graph GUI, with callbacks to apply changes made inside the editor. Meaning you should be able to substitute the `L3Graph` types if you wish, or use the graph to read-only render some data.

Refer to `L3GraphInspector` to see how things are currently piped.

## Words of warning.
1. This tool is in a "just good enough" state. It's missing many QOL features like robust undo and multi-selection.
2. Be wary of using tools like this that create very complex cofiguration. Consider the simplest method to define your data, before you decide you _need_ to spread it out across many objects in a tree.
3. Designed for humans.
