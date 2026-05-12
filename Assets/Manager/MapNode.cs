using System.Collections.Generic;

public class MapNode
{
    public string Id { get; }
    public int Floor { get; }
    public int Column { get; }
    public NodeType Type { get; }
    public NodeState State { get; set; }

    private readonly List<string> _nextNodeIds = new();
    public IReadOnlyList<string> NextNodeIds => _nextNodeIds;

    public MapNode(string id, int floor, int column, NodeType type)
    {
        Id = id;
        Floor = floor;
        Column = column;
        Type = type;
        State = NodeState.Unreachable;
    }

    public void AddConnection(string targetId)
    {
        if (!_nextNodeIds.Contains(targetId))
            _nextNodeIds.Add(targetId);
    }
}
