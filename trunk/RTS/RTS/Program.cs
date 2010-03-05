using System;

namespace RTS
{
    static class Program
    {
        public static int NaN = -99999;     // Used througout project to indicate unitialized 
                                            // or otherwise unique values.

        public static int SCREENWIDTH  = 800; // */ 1280;
        public static int SCREENHEIGHT = 600; // */ 800;
        public static int HUDHEIGHT = 165;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (StratGame game = new StratGame())
            {
                game.Run();
            }
        }
    }
}

