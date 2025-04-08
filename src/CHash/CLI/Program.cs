namespace CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            // Если аргументы не переданы, выводим справку.
            if (args.Length == 0)
            {
                PrintUsage();
                return;
            }

            // Получаем имя команды (первый аргумент)
            string command = args[0].ToLowerInvariant();

            // Определяем словарь команд, где каждая команда соответствует действию.
            var commands = new Dictionary<string, Action<string[]>>
            {
                { "start", StartCommand },
                { "stop", StopCommand },
                { "status", StatusCommand }
                // Дополнительные команды можно добавлять сюда
            };

            // Если команда найдена, выполняем соответствующее действие, передавая оставшиеся аргументы.
            if (commands.TryGetValue(command, out var action))
            {
                try
                {
                    action(args.Skip(1).ToArray());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при выполнении команды '{command}': {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"Неизвестная команда: {command}");
                PrintUsage();
            }
        }

        /// <summary>
        /// Команда для запуска диспетчера.
        /// Пример параметров: start --port 5001 --managePort 5002
        /// </summary>
        private static void StartCommand(string[] args)
        {
            Console.WriteLine("Запуск диспетчера...");
            // Здесь можно добавить парсинг параметров команды start.
            if (args.Length > 0)
            {
                Console.WriteLine("Переданные параметры:");
                foreach (var arg in args)
                {
                    Console.WriteLine(arg);
                }
            }
            // Логика запуска диспетчера, например, настройка gRPC-сервиса.
        }

        /// <summary>
        /// Команда для остановки диспетчера.
        /// </summary>
        private static void StopCommand(string[] args)
        {
            Console.WriteLine("Остановка диспетчера...");
            // Логика остановки диспетчера.
        }

        /// <summary>
        /// Команда для запроса статуса диспетчера.
        /// </summary>
        private static void StatusCommand(string[] args)
        {
            Console.WriteLine("Получение статуса диспетчера...");
            // Логика получения и вывода статуса.
        }

        /// <summary>
        /// Вывод справки по использованию консольного приложения.
        /// </summary>
        private static void PrintUsage()
        {
            Console.WriteLine("Использование:");
            Console.WriteLine("  CLI.exe <command> [options]");
            Console.WriteLine();
            Console.WriteLine("Команды:");
            Console.WriteLine("  start   Запуск диспетчера. Пример: start --port 5001 --managePort 5002");
            Console.WriteLine("  stop    Остановка диспетчера.");
            Console.WriteLine("  status  Отображение статуса диспетчера.");
        }
    }
}