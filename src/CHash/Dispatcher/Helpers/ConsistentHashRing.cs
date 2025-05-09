using ProtosInterfaceDispatcher.Protos;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Dispatcher.Helpers;

public class ConsistentHashRing
{
    private static readonly ILogger _logger = Log.ForContext<ConsistentHashRing>();
    
    private readonly SortedDictionary<int, NodeInfo> _positions = new();
    private readonly int _replicas = 100; // виртуальные узлы

    public void Add(NodeInfo node)
    {
        for (int i = 0; i < _replicas; i++)
        {
            string hex = HashUtils.ComputeSha256Id(node.NodeId + "#" + i);
            int pos = HashUtils.ComputeHashCode(hex);

            _positions[pos] = node;

            // Логируем добавление виртуальной точки
            _logger.Debug("Добавлена виртуальная точка {Position} для ноды {NodeId}", pos, node.NodeId);
        }

        // Логируем окончание добавления ноды
        _logger.Information("Нода {NodeId} добавлена в кольцо (всего точек: {TotalPositions})", node.NodeId, _positions.Count);
    }

    public void Remove(NodeInfo node)
    {
        for (int i = 0; i < _replicas; i++)
        {
            string hex = HashUtils.ComputeSha256Id(node.NodeId + "#" + i);
            int pos = HashUtils.ComputeHashCode(hex);

            if (_positions.Remove(pos))
            {
                //Логируем удаление виртуальной точки
                _logger.Debug("Удалена виртуальная точка {Position} для ноды {NodeId}", pos, node.NodeId);
            }
        }

        //Логируем завершение удаления
        _logger.Information("Нода {NodeId} удалена из кольца", node.NodeId);
    }

    public NodeInfo GetNodeForKey(string key)
    {
        if (_positions.Count == 0)
        {
            _logger.Warning("Кольцо пустое при поиске ноды для ключа {Key}", key);
            throw new InvalidOperationException("Ring is empty");
        }

        int hash = HashUtils.ComputeHashCode(key);

        _logger.Debug("Поиск ноды для ключа {Key}, вычисленный хэш: {Hash}", key, hash);

        foreach (var kv in _positions)
        {
            if (kv.Key >= hash)
            {
                _logger.Debug("Подходящая нода найдена: {Position} → {NodeId}", kv.Key, kv.Value.NodeId);
                return kv.Value;
            }
        }

        var first = _positions.First().Value;
        _logger.Debug("Ключ {Key} не попал ни под одну позицию. Используем первую ноду: {NodeId}", key, first.NodeId);
        return first;
    }

    [Obsolete("Используем HashUtils")]
    private static int Hash32(string s) => Math.Abs(s.GetHashCode());
}