using System;
using System.Collections.Generic;
using UnityEngine;

public class MapManager
{
    public event Action<string> OnNodeSelected;
    public event Action OnMapStateChanged;

    private readonly MapLayoutData _data;
    private System.Random _rng;
    private readonly Dictionary<string, MapNode> _nodes = new();
    private readonly List<List<MapNode>> _floors = new();

    public IReadOnlyList<List<MapNode>> Floors => _floors;

    public MapManager(MapLayoutData data)
    {
        _data = data;
    }

    public void GenerateMap(int? seed = null)
    {
        _rng = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
        _nodes.Clear();
        _floors.Clear();

        int floorCount = _data != null ? _data.floorCount : 10;
        int minNodes = _data != null ? _data.minNodesPerFloor : 2;
        int maxNodes = _data != null ? _data.maxNodesPerFloor : 4;

        for (int f = 0; f < floorCount; f++)
        {
            bool isBoss = f == floorCount - 1;
            int nodeCount = isBoss ? 1 : _rng.Next(minNodes, maxNodes + 1);

            var floor = new List<MapNode>();
            for (int n = 0; n < nodeCount; n++)
            {
                var node = new MapNode($"f{f}n{n}", f, n, isBoss ? NodeType.Boss : GetRandomNodeType());
                floor.Add(node);
                _nodes[node.Id] = node;
            }
            _floors.Add(floor);
        }

        for (int f = 0; f < floorCount - 1; f++)
            ConnectFloors(_floors[f], _floors[f + 1]);

        foreach (var node in _floors[0])
            node.State = NodeState.Reachable;

        OnMapStateChanged?.Invoke();
    }

    public MapNode GetNode(string nodeId)
    {
        _nodes.TryGetValue(nodeId, out var node);
        return node;
    }

    public IReadOnlyList<MapNode> GetReachableNodes(string currentNodeId)
    {
        if (string.IsNullOrEmpty(currentNodeId))
            return _floors.Count > 0 ? _floors[0] : new List<MapNode>();

        if (!_nodes.TryGetValue(currentNodeId, out var current))
            return new List<MapNode>();

        var result = new List<MapNode>();
        foreach (var id in current.NextNodeIds)
            if (_nodes.TryGetValue(id, out var n)) result.Add(n);
        return result;
    }

    public void SelectNode(string nodeId)
    {
        if (!_nodes.TryGetValue(nodeId, out var node)) return;
        if (node.State != NodeState.Reachable) return;

        node.State = NodeState.Current;
        OnNodeSelected?.Invoke(nodeId);
        OnMapStateChanged?.Invoke();
    }

    public void CompleteCurrentNode(string nodeId)
    {
        if (!_nodes.TryGetValue(nodeId, out var node)) return;

        node.State = NodeState.Visited;

        foreach (var nextId in node.NextNodeIds)
            if (_nodes.TryGetValue(nextId, out var next) && next.State == NodeState.Unreachable)
                next.State = NodeState.Reachable;

        OnMapStateChanged?.Invoke();
    }

    private void ConnectFloors(List<MapNode> lower, List<MapNode> upper)
    {
        // Every upper node gets at least one incoming connection
        foreach (var up in upper)
            lower[_rng.Next(0, lower.Count)].AddConnection(up.Id);

        // Every lower node gets at least one outgoing connection
        foreach (var lo in lower)
            if (lo.NextNodeIds.Count == 0)
                lo.AddConnection(upper[_rng.Next(0, upper.Count)].Id);

        // Add a few random extra connections for variety
        int extras = _rng.Next(0, Math.Min(lower.Count, upper.Count));
        for (int i = 0; i < extras; i++)
            lower[_rng.Next(0, lower.Count)].AddConnection(upper[_rng.Next(0, upper.Count)].Id);
    }

    private NodeType GetRandomNodeType()
    {
        if (_data == null || _data.nodeTypeWeights == null || _data.nodeTypeWeights.Count == 0)
            return NodeType.Combat;

        float total = 0f;
        foreach (var w in _data.nodeTypeWeights) total += w.weight;
        if (total <= 0f) return NodeType.Combat;

        float roll = (float)_rng.NextDouble() * total;
        float cumulative = 0f;
        foreach (var w in _data.nodeTypeWeights)
        {
            cumulative += w.weight;
            if (roll <= cumulative) return w.nodeType;
        }

        return NodeType.Combat;
    }
}
