using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace RTS
{
    public class Coordinate : Queueable
    {
        public Vector2 location { get; set; }

        public Coordinate()
        {
            location = new Vector2(Program.NaN, Program.NaN);
        }
        public Coordinate(Vector2 vec)
        {
            location = vec;
        }
        public Coordinate(int a, int b)
        {
            location = new Vector2(a, b);
        }
        public Coordinate(double a, double b)
        {
            location = new Vector2((float)a, (float)b);
        }
        public Coordinate(float a, float b)
        {
            location = new Vector2(a, b);
        }
    }
}
