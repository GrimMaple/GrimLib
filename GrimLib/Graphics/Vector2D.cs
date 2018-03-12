using System;
using System.Collections.Generic;
using System.Text;

namespace GrimLib.Graphics
{
    public struct Vector2D
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Vector2D(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static Vector2D operator+(Vector2D a, Vector2D b)
        {
            return new Vector2D(a.X + b.X, a.Y + b.Y);
        }

        public static Vector2D operator-(Vector2D a, Vector2D b)
        {
            return a + (-b);
        }

        public static Vector2D operator-(Vector2D a)
        {
            return new Vector2D(-a.X, -a.Y);
        }

        public static Vector2D operator*(Vector2D a, float b)
        {
            return new Vector2D(a.X * b, a.Y * b);
        }

        public static float Distance(Vector2D a, Vector2D b)
        {
            return (float)Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        }
    }
}
