using System.Collections.Generic;
using UnityEngine;

public class MapPanelUI : MonoBehaviour
{
    [SerializeField] private RectTransform _nodesContainer;
    [SerializeField] private GameObject _nodeButtonPrefab;
    [SerializeField] private GameObject _connectionLinePrefab;

    [SerializeField] private float _floorHeight = 120f;
    [SerializeField] private float _mapWidth = 400f;

    private readonly Dictionary<string, MapNodeUI> _nodeUIs = new();
    private MapManager _mapManager;

    private void OnEnable()
    {
        _mapManager ??= ServiceLocator.Instance.Get<MapManager>();
        if (_mapManager == null) return;

        _mapManager.OnMapStateChanged -= RefreshNodeStates;
        _mapManager.OnMapStateChanged += RefreshNodeStates;

        RebuildMap();
    }

    private void OnDisable()
    {
        if (_mapManager != null)
            _mapManager.OnMapStateChanged -= RefreshNodeStates;
    }

    private void RebuildMap()
    {
        ClearContainer();
        if (_mapManager.Floors.Count == 0) return;

        BuildNodes();
        if (_connectionLinePrefab != null)
            BuildConnections();
    }

    private void ClearContainer()
    {
        _nodeUIs.Clear();
        foreach (Transform child in _nodesContainer)
            Destroy(child.gameObject);
    }

    private void BuildNodes()
    {
        var floors = _mapManager.Floors;
        for (int f = 0; f < floors.Count; f++)
        {
            var floor = floors[f];
            for (int n = 0; n < floor.Count; n++)
            {
                var node = floor[n];
                var go = Instantiate(_nodeButtonPrefab, _nodesContainer);
                var rt = go.GetComponent<RectTransform>();
                rt.anchoredPosition = GetNodePosition(f, n, floor.Count);

                var nodeUI = go.GetComponent<MapNodeUI>();
                nodeUI.Setup(node, _mapManager);
                _nodeUIs[node.Id] = nodeUI;
            }
        }
    }

    private void BuildConnections()
    {
        foreach (var kvp in _nodeUIs)
        {
            var node = _mapManager.GetNode(kvp.Key);
            if (node == null) continue;

            var fromPos = kvp.Value.GetComponent<RectTransform>().anchoredPosition;

            foreach (var nextId in node.NextNodeIds)
            {
                if (!_nodeUIs.TryGetValue(nextId, out var targetUI)) continue;

                var lineGo = Instantiate(_connectionLinePrefab, _nodesContainer);
                lineGo.transform.SetSiblingIndex(0); // render behind nodes

                var toPos = targetUI.GetComponent<RectTransform>().anchoredPosition;
                SetLineTransform(lineGo.GetComponent<RectTransform>(), fromPos, toPos);
            }
        }
    }

    private Vector2 GetNodePosition(int floor, int column, int nodesOnFloor)
    {
        float x = nodesOnFloor == 1
            ? 0f
            : Mathf.Lerp(-_mapWidth / 2f, _mapWidth / 2f, (float)column / (nodesOnFloor - 1));
        return new Vector2(x, (floor + 1) * _floorHeight - (_nodesContainer.rect.height / 2f));
    }

    private void SetLineTransform(RectTransform line, Vector2 from, Vector2 to)
    {
        Vector2 dir = to - from;
        line.anchoredPosition = (from + to) / 2f;
        line.sizeDelta = new Vector2(dir.magnitude, 4f);
        line.localEulerAngles = new Vector3(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
    }

    private void RefreshNodeStates()
    {
        foreach (var nodeUI in _nodeUIs.Values)
            nodeUI.Refresh();
    }
}
