using System.Diagnostics.CodeAnalysis;
using Core.Abstractions;
using Core.Exceptions;

namespace CLI
{
    public class CommandLineParser : IParser
    {
        private readonly CommandBase[] _commandsAvailable;
        private readonly KeyBase[] _keysAvailable;

        [UnconditionalSuppressMessage(
            "Trimming",
            "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' " +
            "require dynamic access otherwise can break functionality when trimming application code",
            Justification = "Все нормально, я отключил тримминг в настройках проекта")]
        public CommandLineParser()
        {
            _commandsAvailable = typeof(CommandBase).Assembly.GetTypes()
                .Where(t => typeof(CommandBase).IsAssignableFrom(t) && !t.IsAbstract && t != typeof(CommandBase))
                .Select(t => (Activator.CreateInstance(t)! as CommandBase)!)
                .ToArray();

            _keysAvailable = typeof(KeyBase).Assembly.GetTypes()
                .Where(t => typeof(KeyBase).IsAssignableFrom(t) && !t.IsAbstract && t != typeof(KeyBase))
                .Select(t => (Activator.CreateInstance(t)! as KeyBase)!)
                .ToArray();
        }

        public CommandBase? Parse(string[]? args)
        {
            try
            {
                if (args is null || args.Length == 0)
                {
                    return null;
                }

                // Первый аргумент – имя команды
                string commandName = args![0].ToLowerInvariant();
                string? commandArgumentValue = null;

                // Здесь можно реализовать маппинг имени команды к её типу.
                var command = _commandsAvailable.FirstOrDefault(t => t.Name == commandName)
                              ?? throw new Exception("Команда не найдена");

                // Собираем ключи и значения в словарь.
                var dictKeyAndValue = new Dictionary<string, string?>();
                for (int i = 1; i < args.Length; i++)
                {
                    if (i == args.Length - 1 && command.ValueIsRequired)
                    {
                        commandArgumentValue = args[i];
                        break;
                    }

                    // Ключ должен начинаться с "--"
                    if (args[i].StartsWith("--") && args[i].Length > 3)
                    {
                        string key = args[i].Substring(2);
                        string? value = null;

                        if (command.AllowedKeys.FirstOrDefault(x => x.KeyName == key) is null)
                            throw new KeyNotAllowedException($"Key \'{key}\' is not allowed");

                        var keyFromAssembly = _keysAvailable.FirstOrDefault(x => x.KeyName == key)
                                              ?? throw new KeyNotFoundException(
                                                  $"Key \'{key}\' is not found in assembly");

                        if (!keyFromAssembly.ValueIsRequired)
                        {
                            dictKeyAndValue[key] = null;
                            continue;
                        }

                        // Если следующий аргумент существует и не начинается с "--", считаем его значением.
                        if (i + 1 < args.Length && !args[i + 1].StartsWith("--") && !args[i + 1].StartsWith("-"))
                        {
                            value = args[i + 1];
                            i++; // Пропускаем значение
                        }

                        if (value is null)
                            throw new KeyValueRequiredException($"Key \'{key}\' value is required");

                        dictKeyAndValue[key] = value;
                    }

                    if (args[i].StartsWith("-") && args[i].Length == 2)
                    {
                        string key = args[i].Substring(1);
                        string? value = null;

                        if (command.AllowedKeys.FirstOrDefault(x => x.ShortName == key) is null)
                            throw new KeyNotAllowedException($"Key \'{key}\' is not allowed");

                        var keyFromAssembly = _keysAvailable.FirstOrDefault(x => x.ShortName == key)
                                              ?? throw new KeyNotFoundException(
                                                  $"Key \'{key}\' is not found in assembly");

                        if (!keyFromAssembly.ValueIsRequired)
                        {
                            dictKeyAndValue[key] = null;
                            continue;
                        }

                        // Если следующий аргумент существует и не начинается с "--", считаем его значением.
                        if (i + 1 < args.Length && !args[i + 1].StartsWith("--") && !args[i + 1].StartsWith("-"))
                        {
                            value = args[i + 1];
                            i++; // Пропускаем значение
                        }

                        if (value is null)
                            throw new KeyValueRequiredException($"Key \'{key}\' value is required");

                        dictKeyAndValue[key] = value;
                    }
                }

                if (commandArgumentValue is null && command.ValueIsRequired)
                    throw new CommandValueRequired("Command value wasn't specified");


                foreach (var requiredKey in command.RequiredKeys)
                {
                    if (!dictKeyAndValue
                            .Any(x => x.Key == requiredKey.KeyName || x.Key == requiredKey.ShortName))
                        throw new KeyRequiredException($"Key \'{requiredKey}\' is missing");
                }
                
                var keyAndValuesToSet = dictKeyAndValue
                    .Select(x => new KeyValuePair<KeyBase, string?>(_keysAvailable
                        .First(y => y.KeyName == x.Key || y.ShortName == x.Key), x.Value));
                
                // Создаём экземпляр нужной команды через Reflection.
                // Конструктор команды ожидает Dictionary<string, string>.
                var commandInstance = Activator.CreateInstance(command.CommandType) as CommandBase;
                if (commandInstance is null)
                {
                    throw new InvalidOperationException("Ошибка создания экземпляра команды.");
                }

                commandInstance.Value = commandArgumentValue;
                commandInstance.KeyAndValues = keyAndValuesToSet;

                return commandInstance;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }


        private static void ShowHelp()
        {
            // TODO
        }
    }
}