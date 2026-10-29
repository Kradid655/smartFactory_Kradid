using static smartFactoryApp.Models.Parameters;
using static smartFactoryApp.Models.ViewParameters;

namespace smartFactoryApp.Models
{
    public class LegoBrick : Beweger
    {
         bool gedockt=false;

        public bool Gedockt{get=> gedockt;set=>gedockt=value;}

        private LegoColor color;
        private LegoSize size;
        public LegoColor Color { get { return color; } set { color = value; } }
        public LegoSize Size { get { return size; } set { size = value; } }

        // Bricks bewegen sich nur innerhalb der Fabrikfläche (BrickBorders), nicht in den Roboter-Grenzen.
        public LegoBrick(Pose pose, LegoColor color, LegoSize size) : base(pose, BrickBorders)
        {
            Color = color;
            Size = size;
        }

 public override void Bewegen(Parameters.Direction dir)
        {   if(!Gedockt)
            base.Bewegen(dir);
        }
        public override string ToString()
        {
            return $"LegoBrick {Color} ({Size}) bei ({X}, {Y}), Winkel {Angle}°";
        }
    }
}
