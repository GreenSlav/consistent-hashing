using ProtosInterfaceDispatcher.Protos;

namespace Dispatcher.Helpers;

using System.Collections.Concurrent;

public class NodeRegistry
{
    private readonly ConcurrentDictionary<string, NodeInfo> _nodes = new();

    public void AddNode(string nodeId, NodeInfo nodeInfo)
    {
        _nodes[nodeId] = nodeInfo;
    }

    public bool TryGetNode(string nodeId, out NodeInfo? nodeInfo)
    {
        return _nodes.TryGetValue(nodeId, out nodeInfo);
    }

    public IEnumerable<NodeInfo> GetAllNodes()
    {
        return _nodes.Values;
    }
    
    public bool RemoveNode(string nodeId)
    {
        return _nodes.TryRemove(nodeId, out _);
    }

}