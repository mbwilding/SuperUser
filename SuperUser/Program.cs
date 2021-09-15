namespace SuperUser
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Elevate.Run(args.Length == 0 ? "cmd.exe" : args[0]);
        }
    }
}