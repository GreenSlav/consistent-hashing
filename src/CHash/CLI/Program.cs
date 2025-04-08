namespace CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO: Добавить DI
            
            string[] testArgs1 = 
            {
                "connect", 
                "--dispatcher-name", "TestDispatcher", 
                "--port", "5001", 
                "--config", "config.json"
            };
            var command = CommandLineParser.Parse(testArgs1);
            Console.WriteLine(command.Name);
            Console.WriteLine(command.Description);
            foreach (var VARIABLE in command._keyAndValues)
            {
                Console.WriteLine(VARIABLE.Key + " = " + VARIABLE.Value);
            }
        }
    }
}