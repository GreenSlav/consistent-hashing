using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Core.Abstractions;
using Core.Keys;
using Core.Enums;
using Grpc.Net.Client;

namespace Core.Commands
{
    /// <summary>
    /// Команда запуска диспетчера
    /// </summary>
    public class StartCommand : CommandBase
    {
        public override string? Value { get; set; }

        public override required IEnumerable<KeyValuePair<KeyBase, string?>> KeyAndValues { get; set; }

        public override string Name { get; } = "start";

        public override string Description { get; } = "Запуск нового экземпляра диспетчера";

        // Добавляем обязательные ключи для порта и пути к диспетчеру, а также имя (например, для идентификации)
        public override KeyBase[] AllowedKeys { get; } =
        {
            new DispatcherPortKey(),
            new PathToDispatcherKey(),
            new BackgroundKey(),
        };

        public override KeyBase[] RequiredKeys { get; } =
        {
            new DispatcherPortKey(),
            new PathToDispatcherKey(),
        };

        public override bool ValueIsRequired { get; } = false;

        public override string ExpectedValue { get; } = string.Empty;

        public override string[] Examples { get; } =
        {
            "start -n dispatcher1 -p 8080 -d C:\\Path\\To\\Dispatcher.exe",
            "start -n dispatcher2 -p 9090 -d /usr/local/bin/dispatcher"
        };

        public override Type CommandType { get; } = typeof(StartCommand);

        public override async Task ExecuteAsync()
        {
            // Проверяем, что коллекция ключей не null
            if (KeyAndValues == null)
            {
                throw new Exception("Не переданы параметры для команды.");
            }

            // Извлекаем значение порта из словаря по ключу CommandKey.Port
            var portEntry = KeyAndValues.FirstOrDefault(x => x.Key.KeyName == CommandKey.Port);
            string? portStr = portEntry.Value;

            if (string.IsNullOrEmpty(portStr))
            {
                throw new Exception("Не указан порт для диспетчера.");
            }

            // Извлекаем путь к исполняемому файлу диспетчера по ключу, определённому в PathToDispatcherKey.
            var pathEntry = KeyAndValues.FirstOrDefault(x => x.Key.KeyName == "dispatcher-path");
            string? dispatcherPath = pathEntry.Value;

            if (string.IsNullOrEmpty(dispatcherPath))
            {
                throw new Exception("Не указан путь к исполняемому файлу диспетчера.");
            }

            // Преобразуем порт в число
            if (!int.TryParse(portStr, out int port))
            {
                throw new Exception("Порт должен быть числовым значением.");
            }

            // Определяем, был ли передан флаг фонового режима.
            bool runInBackground = KeyAndValues.Any(x => x.Key.KeyName == CommandKey.Background);

            // Формируем аргументы для запуска диспетчера.
            // Обычно указывают порт через параметр --urls (например, для Kestrel).
            string urlsArg = $"--urls=http://localhost:{port}";
            string arguments = urlsArg;

            // Настраиваем параметры процесса для запуска диспетчера.
            var psi = new ProcessStartInfo
            {
                FileName = dispatcherPath,
                Arguments = arguments,
                RedirectStandardOutput = runInBackground,
                RedirectStandardError = runInBackground,
                // Если нужно запустить в фоне, скрываем окно; иначе – оставляем по умолчанию.
                UseShellExecute = false,
                CreateNoWindow = runInBackground, // Если runInBackground == true, окно не создаётся.
            };

            // Дополнительно, для фонового режима можем установить скрытый стиль окна.
            if (runInBackground)
            {
                psi.WindowStyle = ProcessWindowStyle.Hidden;
            }
            else
            {
                // Если окно должно быть видно, можно установить WindowStyle обычным (например, Normal)
                psi.WindowStyle = ProcessWindowStyle.Normal;
            }

            try
            {
                Process? process = Process.Start(psi);
                if (process == null)
                {
                    throw new Exception("Не удалось запустить процесс диспетчера.");
                }

                Console.WriteLine($"Диспетчер запущен успешно. PID: {process.Id}, слушает на порту {port}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при запуске диспетчера: {ex.Message}");
                throw;
            }

            await Task.CompletedTask;
        }
    }
}