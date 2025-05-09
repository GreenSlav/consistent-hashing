using ProtosInterfaceDispatcher.Protos;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Dispatcher.Helpers;

using System.Collections.Concurrent;

public class NodeRegistry
{
    private static readonly ILogger _logger = Log.ForContext<NodeRegistry>();

    private readonly ConcurrentDictionary<string, NodeInfo> _nodes = new();
    private readonly ConsistentHashRing _ring = new();

    public void AddNode(string id, NodeInfo info)
    {
        if (_nodes.TryAdd(id, info))
        {
            _ring.Add(info);
            _logger.Information("Нода {NodeId} добавлена в реестр", info.NodeId);
        }
        else
        {
            _logger.Warning("Не удалось добавить ноду {NodeId}: уже существует", info.NodeId);
        }
    }

    public bool TryGetNode(string id, out NodeInfo? info)
    {
        var exists = _nodes.TryGetValue(id, out info);
        if (!exists)
        {
            _logger.Debug("Нода {NodeId} не найдена в реестре", id);
        }
        return exists;
    }

    public IEnumerable<NodeInfo> GetAllNodes()
    {
        _logger.Debug("Запрошен список всех нод. Кол-во: {Count}", _nodes.Count);
        return _nodes.Values;
    }

    public bool RemoveNode(string id)
    {
        if (_nodes.TryRemove(id, out var info))
        {
            _ring.Remove(info);
            _logger.Information("Нода {NodeId} удалена из реестра", id);
            return true;
        }
        _logger.Debug("Попытка удаления несуществующей ноды: {NodeId}", id);
        return false;
    }

    public NodeInfo GetNodeByKey(string key)
    {
        _logger.Debug("Поиск ноды по ключу: {KeyHash}", key);
        return _ring.GetNodeForKey(key);
    }
}