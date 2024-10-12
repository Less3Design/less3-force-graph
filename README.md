![Frame 51](https://github.com/user-attachments/assets/d8d4fab8-37e8-4cf3-ac71-dca490e52337)

A scriptable object framework and node graph UI that lets you easily create complex branching trees of data.

## Force Directed
Nodes are positioned automically using a [force-direction algorithm](https://en.wikipedia.org/wiki/Force-directed_graph_drawing).

Nodes can be pinned freezing them in place.

https://github.com/user-attachments/assets/a4fd7aac-9905-4d00-9ad7-9edd75c85538

## Scriptable Objects
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
