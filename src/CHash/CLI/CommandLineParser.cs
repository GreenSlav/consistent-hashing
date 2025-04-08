using System;
using System.Collections.Generic;
using System.Linq;
using CLI.Abstractions;
using CLI.Commands;

namespace CLI
{
    public static class CommandLineParser
    {
        /// <summary>
        /// Парсит аргументы командной строки и возвращает экземпляр команды.
        /// </summary>
        /// <param name="args">Аргументы командной строки.</param>
        /// <returns>Экземпляр CommandBase, соответствующий введённой команде.</returns>
        /// <exception cref="ArgumentException">Выбрасывается, если команда не распознана или аргументы отсутствуют.</exception>
        public static CommandBase Parse(string[] args)
        {
            // Проверяем, что хоть что-то передано
            if (args == null || args.Length == 0)
            {
                throw new ArgumentException("Не указана команда. Используйте ключ help для получения справки.");
            }

            // Первый аргумент – имя команды
            string commandName = args[0].ToLowerInvariant();

            // Здесь можно реализовать маппинг имени команды к её типу.
            // Для примера обрабатываем только команду "connect".
            Type commandType = commandName switch
            {
                "connect" => typeof(ConnectCommand),
                // Можно добавить другие команды по аналогии, например:
                // "start" => typeof(StartCommand),
                // "stop" => typeof(StopCommand),
                _ => null
            };

            if (commandType == null)
            {
                throw new ArgumentException($"Неизвестная команда: {commandName}");
            }

            // Собираем ключи и значения в словарь.
            var keyAndValues = new Dictionary<string, string>();
            for (int i = 1; i < args.Length; i++)
            {
                // Ключ должен начинаться с "--"
                if (args[i].StartsWith("--"))
                {
                    string key = args[i].Substring(2);
                    string value = string.Empty;

                    // Если следующий аргумент существует и не начинается с "--", считаем его значением.
                    if (i + 1 < args.Length && !args[i + 1].StartsWith("--"))
                    {
                        value = args[i + 1];
                        i++; // Пропускаем значение
                    }
                    keyAndValues[key] = value;
                }
                else
                {
                    // Если аргумент не начинается с "--", можно либо пропустить, либо считать ошибкой.
                    // Для простоты здесь просто пропускаем.
                    continue;
                }
            }

            // Создаём экземпляр нужной команды через Reflection.
            // Конструктор команды ожидает Dictionary<string, string>.
            var commandInstance = Activator.CreateInstance(commandType, new object[] { keyAndValues }) as CommandBase;
            if (commandInstance == null)
            {
                throw new InvalidOperationException("Ошибка создания экземпляра команды.");
            }

            return commandInstance;
        }
    }
}