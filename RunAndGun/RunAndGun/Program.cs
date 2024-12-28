namespace RunAndGun
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Game game = new Game())
            {
                foreach (var arg in args)
                {
                    if (arg.Split('=').Length == 2)
                    {
                        game.LaunchParameters.Add(arg.Split('=')[0], arg.Split('=')[1]);
                    }
                }                
                game.Run();                
            }
        }
    }
#endif
}

