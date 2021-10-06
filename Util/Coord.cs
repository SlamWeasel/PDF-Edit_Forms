using System;
using System.Drawing;

namespace PDF_Edit_Froms.Util
{
    class Coord
    {
        public long x { get; set; }
        public long y { get; set; }
        public string name { get; set; }

        public Coord(long _v)
        {
            x = _v;
            y = _v;
            name = null;
        }
        public Coord(long _x, long _y)
        {
            x = _x;
            y = _y;
            name = null;
        }
        public Coord(long _x, long _y, string _name)
        {
            x = _x;
            y = _y;
            name = _name;
        }
        public static implicit operator Point(Coord c)
            =>new Point(Convert.ToInt32(c.x), Convert.ToInt32(c.y));
        public static implicit operator Coord(Point p)
            =>new Coord(p.X, p.Y);
        public static implicit operator Size(Coord c)
            =>new Size(Convert.ToInt32(c.x), Convert.ToInt32(c.y));
        public static implicit operator Coord(Size s)
            =>new Coord(s.Width, s.Height);

        override public string ToString()
            => $"({x}, {y})";
        public Point ToPoint()
            => new Point(Convert.ToInt32(x), Convert.ToInt32(y));
        public static Coord FromPoint(Point p)
            => new Coord(p.X, p.Y);

        public static Coord operator +(Coord a, Coord b)
            => new Coord(a.x + b.x, a.y + b.y);
        public static Coord operator -(Coord a, Coord b)
            => new Coord(a.x - b.x, a.y - b.y);
    }
}
