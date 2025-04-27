using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Dispatcher.Helpers;

public static class HashUtils
{
    /// <summary>
    /// Сериализует объект в JSON (косметику убираем, сортируем свойства) и возвращает SHA256-хэш в виде hex-строки.
    /// </summary>
    public static string ComputeSha256Id<T>(T obj)
    {
        // Сериализуем без отступов, с упорядочиванием полей
        var options = new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            // Для гарантии порядка:
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
        };
        string json = JsonSerializer.Serialize(obj, options);

        using var sha = SHA256.Create();
        byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(json));
        // конвертим в hex
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    /// <summary>
    /// Берёт первые 4 байта hex-хэша и превращает в 32-битное число для консистентного хэширования.
    /// </summary>
    public static int ComputeHashCode(string hexHash)
    {
        // первые 8 символов hex = 4 байта
        string prefix = hexHash.Substring(0, 8);
        return int.Parse(prefix, System.Globalization.NumberStyles.HexNumber);
    }

    /// <summary>
    /// Удобный метод: сразу из объекта получить детерминированный ID (hex SHA256).
    /// </summary>
    public static string ComputeId<T>(T obj) => ComputeSha256Id(obj);

    /// <summary>
    /// Сочетание: из объекта получить int-хэш для выбора ноды.
    /// </summary>
    public static int ComputeKeyHash<T>(T obj) => ComputeHashCode(ComputeSha256Id(obj));
}