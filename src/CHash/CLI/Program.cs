namespace CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO: Добавить DI
            
            // string[] testArgs1 = 
            // {
            //     "connect", 
            //     "--dispatcher-name", "TestDispatcher", 
            //     "--port", "5001", 
            //     "--config", "config.json"
            // };
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