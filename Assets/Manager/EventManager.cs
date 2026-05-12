public class EventManager
{
    // Slice 2 stub: immediately completes any node.
    // Slices 3-5 will replace this with proper per-NodeType dispatch.
    public void HandleNodeEntered(string nodeId)
    {
        ServiceLocator.Instance.Get<MapManager>()?.CompleteCurrentNode(nodeId);
    }
}
