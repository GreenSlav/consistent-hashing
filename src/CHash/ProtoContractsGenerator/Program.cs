using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Core.Abstractions; // Ссылка на сборку с определением EntityBase

namespace ProtoContractsGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            // Проверка входных параметров: первый аргумент - путь к внешней сборке, второй - путь к выходной директории.
            if (args.Length < 2)
            {
                Console.WriteLine("Использование: ProtoGenerator <путь_к_внешней_сборке> <путь_выходной_директории>");
                return;
            }

            string externalAssemblyPath = args[0];
            string outputDirectory = args[1];

            if (!File.Exists(externalAssemblyPath))
            {
                Console.WriteLine("Не удалось найти внешнюю сборку: " + externalAssemblyPath);
                return;
            }

            // Если выходная директория не существует, создаём её.
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            Assembly externalAssembly;
            try
            {
                externalAssembly = Assembly.LoadFrom(externalAssemblyPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при загрузке внешней сборки: " + ex.Message);
                return;
            }

            // Используем наш EntityBase из проекта (пространство Core.Abstractions)
            Type entityBaseType = typeof(EntityBase);

            // Находим все типы из внешней сборки, которые являются непустыми наследниками EntityBase.
            var entityTypes = externalAssembly.GetTypes()
                .Where(t => !t.IsAbstract && entityBaseType.IsAssignableFrom(t))
                .ToList();

            if (entityTypes.Count == 0)
            {
                Console.WriteLine("Не найдены типы, наследующие EntityBase, во внешней сборке.");
                return;
            }

            // Для каждого найденного типа генерируем proto-файл.
            foreach (var entityType in entityTypes)
            {
                string protoContent = GenerateProto(entityType);
                string fileName = $"{entityType.Name}.proto";
                string filePath = Path.Combine(outputDirectory, fileName);

                try
                {
                    File.WriteAllText(filePath, protoContent, Encoding.UTF8);
                    Console.WriteLine("Сгенерирован файл: " + filePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка при записи файла " + filePath + ": " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Генерирует содержимое proto-файла для указанного типа-сущности.
        /// Формируется message с полями для каждого публичного свойства (поддерживаются примитивы, строка, DateTime и enum),
        /// а также определение CRUD-сервиса с методами: Create, Get, Update, Delete.
        /// </summary>
        /// <param name="entityType">Тип, наследующий EntityBase.</param>
        /// <returns>Содержимое proto-файла в виде строки.</returns>
        static string GenerateProto(Type entityType)
        {
            StringBuilder sb = new StringBuilder();

            // Заголовок proto файла.
            sb.AppendLine("syntax = \"proto3\";");
            sb.AppendLine();
            sb.AppendLine("package Entities;");
            sb.AppendLine();
            sb.AppendLine("option csharp_namespace = \"MyGeneratedProtos\";");
            sb.AppendLine();

            // Определение message для сущности.
            sb.AppendLine($"message {entityType.Name} {{");
            int fieldNumber = 1;
            foreach (var prop in entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                string fieldName = prop.Name.ToLower();
                sb.AppendLine($"  string {fieldName} = {fieldNumber};");
                fieldNumber++;
                // string protoType = MapCSharpTypeToProtoType(prop.PropertyType);
                // Пропускаем свойства, тип которых не поддерживается (например, сложные объекты)
                // if (!string.IsNullOrEmpty(protoType))
                // {
                //     // Рекомендуется использовать имена полей в нижнем регистре.
                //     sb.AppendLine($"  {protoType} {prop.Name.ToLower()} = {fieldNumber};");
                //     fieldNumber++;
                // }
            }
            sb.AppendLine("}");
            sb.AppendLine();

            // Определение CRUD-сервиса для сущности.
            sb.AppendLine($"service {entityType.Name}Service {{");
            sb.AppendLine($"  rpc Create{entityType.Name}({entityType.Name}) returns ({entityType.Name});");
            sb.AppendLine($"  rpc Get{entityType.Name}(Get{entityType.Name}Request) returns ({entityType.Name});");
            sb.AppendLine($"  rpc Update{entityType.Name}({entityType.Name}) returns ({entityType.Name});");
            sb.AppendLine($"  rpc Delete{entityType.Name}(Delete{entityType.Name}Request) returns (Empty);");
            sb.AppendLine("}");
            sb.AppendLine();

            // Определение сообщения запроса для метода Get.
            sb.AppendLine($"message Get{entityType.Name}Request {{");
            sb.AppendLine("  int32 id = 1;");
            sb.AppendLine("}");
            sb.AppendLine();

            // Определение сообщения запроса для метода Delete.
            sb.AppendLine($"message Delete{entityType.Name}Request {{");
            sb.AppendLine("  int32 id = 1;");
            sb.AppendLine("}");
            sb.AppendLine();

            // Определение пустого сообщения, если оно требуется.
            sb.AppendLine("message Empty {}");

            return sb.ToString();
        }

        /// <summary>
        /// Преобразует тип свойства C# в соответствующий тип данных Proto3.
        /// Поддерживаются следующие типы: int, long, float, double, bool, string, DateTime (как string) и enum (как int32).
        /// Если тип не поддерживается, возвращается null.
        /// </summary>
        /// <param name="type">Тип свойства.</param>
        /// <returns>Соответствующий тип в синтаксисе proto или null.</returns>
        static string MapCSharpTypeToProtoType(Type type)
        {
            if (type == typeof(int))
                return "int32";
            else if (type == typeof(long))
                return "int64";
            else if (type == typeof(float))
                return "float";
            else if (type == typeof(double))
                return "double";
            else if (type == typeof(bool))
                return "bool";
            else if (type == typeof(string))
                return "string";
            else if (type == typeof(DateTime))
                // Альтернативно можно использовать google.protobuf.Timestamp.
                return "string";
            else if (type.IsEnum)
                return "int32"; // Можно расширить генерацию отдельного enum в proto.
            else
                return null; // Игнорируем сложные типы.
        }
    }
}