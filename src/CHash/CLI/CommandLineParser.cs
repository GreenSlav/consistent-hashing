using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using CLI.Abstractions;
using CLI.Commands;
using CLI.Utils;

namespace CLI
{
    public class CommandLineParser : IParser
    {
        private readonly CommandDescriptor[] _commandNames;

        [UnconditionalSuppressMessage(
            "Trimming",
            "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' " +
            "require dynamic access otherwise can break functionality when trimming application code",
            Justification = "Все нормально, я отключил тримминг в настройках проекта")]
        public CommandLineParser()
        {
            _commandNames = typeof(CommandBase).Assembly.GetTypes()
                .Where(t => typeof(CommandBase).IsAssignableFrom(t) && !t.IsAbstract && t != typeof(CommandBase))
                .Select(t =>
                {
                    var command = (Activator.CreateInstance(t)! as CommandBase)!;
                    return new CommandDescriptor(command.Name, command.CommandType);
                })
                .ToArray();
        }

        public CommandBase? Parse(string[]? args)
        {
            if (args is null || args.Length == 0)
            {
                return null;
            }
            
            // Первый аргумент – имя команды
            string commandName = args![0].ToLowerInvariant();
            string commandArgumentValue = string.Empty;

            // Здесь можно реализовать маппинг имени команды к её типу.
            Type commandType = _commandNames.FirstOrDefault(t => t.Name == commandName)?.Type
                               ?? throw new Exception("Команда не найдена");
            
            // Собираем ключи и значения в словарь.
            var keyAndValues = new Dictionary<string, string>();
            for (int i = 1; i < args.Length; i++)
            {
                // TODO
                // TODO: Обязательно сделать парсинг ключей, не требующих значения
                // TODO
                
                // Ключ должен начинаться с "--"
                if (args[i].StartsWith("--") && args[i].Length > 3)
                {
                    string key = args[i].Substring(2);
                    string value = string.Empty;

                    // Если следующий аргумент существует и не начинается с "--", считаем его значением.
                    if (i + 1 < args.Length && !args[i + 1].StartsWith("--") && !args[i + 1].StartsWith("-"))
                    {
                        value = args[i + 1];
                        i++; // Пропускаем значение
                    }

                    keyAndValues[key] = value;
                }
                
                if (args[i].StartsWith("-") && args[i].Length == 2)
                {
                    string key = args[i].Substring(1);
                    string value = string.Empty;

                    // Если следующий аргумент существует и не начинается с "--", считаем его значением.
                    if (i + 1 < args.Length && !args[i + 1].StartsWith("--") && !args[i + 1].StartsWith("-"))
                    {
                        value = args[i + 1];
                        i++; // Пропускаем значение
                    }

                    keyAndValues[key] = value;
                }
            }

            // Создаём экземпляр нужной команды через Reflection.
            // Конструктор команды ожидает Dictionary<string, string>.
            var commandInstance = Activator.CreateInstance(commandType) as CommandBase;
            if (commandInstance is null)
            {
                throw new InvalidOperationException("Ошибка создания экземпляра команды.");
            }

            commandInstance.Value = commandArgumentValue;
            commandInstance.KeyAndValues = keyAndValues;

            return commandInstance;
        }

        private static void ShowHelp()
        {
            // TODO
        }
    }
}