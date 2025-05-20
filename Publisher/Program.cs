namespace Publisher
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Program.Main");
            await Startup.Initialze(args).ConfigureAwait(false);
            Console.Read();
        }
    }
}
