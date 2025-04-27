using ProtosInterfaceDispatcher.Protos;

namespace Dispatcher.Helpers;

using System.Collections.Concurrent;

public class NodeRegistry
{
    private readonly ConcurrentDictionary<string, NodeInfo> _nodes = new();
    private readonly ConsistentHashRing _ring = new();

    public void AddNode(string id, NodeInfo info)
    {
        _nodes[id] = info;
        _ring.Add(info);
    }

    public bool TryGetNode(string id, out NodeInfo? info) 
        => _nodes.TryGetValue(id, out info);

    public IEnumerable<NodeInfo> GetAllNodes() => _nodes.Values;

    public bool RemoveNode(string id)
    {
        if (_nodes.TryRemove(id, out var info))
        {
            _ring.Remove(info);
            return true;
        }
        return false;
    }

    public NodeInfo GetNodeByKey(string key) => _ring.GetNodeForKey(key);
}