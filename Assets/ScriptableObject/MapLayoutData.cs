using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapLayoutData", menuName = "Games/MapLayoutData")]
public class MapLayoutData : ScriptableObject
{
    public int floorCount = 10;
    public int minNodesPerFloor = 2;
    public int maxNodesPerFloor = 4;

    [Serializable]
    public class NodeTypeWeight
    {
        public NodeType nodeType;
        [Range(0f, 1f)]
        public float weight;
    }

    [Tooltip("Last floor is always Boss. Weights apply to all other floors.")]
    public List<NodeTypeWeight> nodeTypeWeights = new()
    {
        new NodeTypeWeight { nodeType = NodeType.Combat, weight = 0.6f },
        new NodeTypeWeight { nodeType = NodeType.Rest, weight = 0.25f },
        new NodeTypeWeight { nodeType = NodeType.Elite, weight = 0.1f },
        new NodeTypeWeight { nodeType = NodeType.Shop, weight = 0.05f },
    };
}
