namespace CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO: Добавить DI
            
            var command = new CommandLineParser().Parse(args);

            if (command is not null)
            {
                Console.WriteLine(command!.Name);
                Console.WriteLine(command!.Description);
                foreach (var pair in command!.KeyAndValues!)
                {
                    Console.WriteLine(pair.Key + " = " + pair.Value);
                }
            }
        }
    }
}