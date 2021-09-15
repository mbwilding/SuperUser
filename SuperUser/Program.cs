namespace SuperUser
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string program = string.Empty;
            if (args.Length > 0)
            {
                foreach (var arg in args)
                {
                    program += arg + " ";
                }
            }
            else
            {
                program = "cmd.exe";
            }
            Elevate.Run(program);
        }
    }
}