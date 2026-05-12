using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MapNodeUI : MonoBehaviour
{
    [SerializeField] private Image _background;
    [SerializeField] private Text _label;

    [Header("Node Type Colors")]
    [SerializeField] private Color _combatColor = new Color(1f, 0.4f, 0.4f);
    [SerializeField] private Color _restColor = new Color(0.4f, 1f, 0.4f);
    [SerializeField] private Color _bossColor = new Color(0.7f, 0.3f, 1f);
    [SerializeField] private Color _eliteColor = new Color(1f, 0.6f, 0.2f);
    [SerializeField] private Color _shopColor = new Color(0.4f, 0.6f, 1f);
    [SerializeField] private Color _randomEventColor = new Color(0.4f, 1f, 1f);

    [Header("State Tints")]
    [SerializeField] private Color _unreachableTint = new Color(0.35f, 0.35f, 0.35f);
    [SerializeField] private Color _reachableTint = Color.white;
    [SerializeField] private Color _currentTint = Color.yellow;
    [SerializeField] private Color _visitedTint = new Color(0.55f, 0.55f, 0.55f);

    private MapNode _node;
    private MapManager _mapManager;
    private Button _button;

    public void Setup(MapNode node, MapManager manager)
    {
        _node = node;
        _mapManager = manager;
        _button = GetComponent<Button>();
        _background = GetComponent<Image>();
        _label = GetComponentInChildren<Text>();
        _button.onClick.AddListener(OnClicked);
        Refresh();
    }

    public void Refresh()
    {
        if (_node == null) return;

        if (_label != null)
            _label.text = _node.Type.ToString();

        if (_background != null)
        {
            Color baseColor = _node.Type switch
            {
                NodeType.Combat => _combatColor,
                NodeType.Rest => _restColor,
                NodeType.Boss => _bossColor,
                NodeType.Elite => _eliteColor,
                NodeType.Shop => _shopColor,
                NodeType.RandomEvent => _randomEventColor,
                _ => Color.white
            };

            Color tint = _node.State switch
            {
                NodeState.Unreachable => _unreachableTint,
                NodeState.Reachable => _reachableTint,
                NodeState.Current => _currentTint,
                NodeState.Visited => _visitedTint,
                _ => _unreachableTint
            };

            _background.color = baseColor * tint;
        }

        if (_button != null)
            _button.interactable = _node.State == NodeState.Reachable;
    }

    private void OnClicked()
    {
        _mapManager?.SelectNode(_node.Id);
    }
}
