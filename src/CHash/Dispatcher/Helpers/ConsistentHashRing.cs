using ProtosInterfaceDispatcher.Protos;

namespace Dispatcher.Helpers;

public class ConsistentHashRing
{
    private readonly SortedDictionary<int, NodeInfo> _positions = new();
    private readonly int _replicas = 100; // виртуальные узлы

    public void Add(NodeInfo node)
    {
        for (int i = 0; i < _replicas; i++)
        {
            int pos = Hash32(node.NodeId + "#" + i);
            _positions[pos] = node;
        }
    }

    public void Remove(NodeInfo node)
    {
        for (int i = 0; i < _replicas; i++)
        {
            int pos = Hash32(node.NodeId + "#" + i);
            _positions.Remove(pos);
        }
    }

    public NodeInfo GetNodeForKey(string key)
    {
        if (_positions.Count == 0) throw new InvalidOperationException("Ring is empty");
        int hash = Hash32(key);
        // ищём первый ключ >= hash, иначе берём первый в словаре
        foreach (var kv in _positions)
            if (kv.Key >= hash)
                return kv.Value;
        return _positions.First().Value;
    }

    private static int Hash32(string s)
    {
        // любая стабильная 32-бит хэш-функция, например CRC32 или MurmurHash
        // тут для примера:
        return Math.Abs(s.GetHashCode());
    }
}