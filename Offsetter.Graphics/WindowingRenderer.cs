using Offsetter.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace Offsetter
{
    public class WindowingRenderer : WireframeRenderer
    {
        private int count;

        public WindowingRenderer(Shader shader)
            : base(shader, new Vertex[4], VColor.RED)
        {
            count = 0;
            IsMasked = true;
            LineWidth = 2;

            // Override the defaut which is PrimitiveType.LineStrip.
            drawingMode = PrimitiveType.LineLoop;
        }

        public float Width { get { return System.Math.Abs(data[2].x - data[0].x); } }

        public float Height { get { return System.Math.Abs(data[2].y - data[0].y); } }

        public void Clear()
        {
            count = 0;
            IsMasked = true;
        }

        // Returns: [xmin, ymin, xmax, ymax]
        public double[] GLCoordinates()
        {
            double xmin = double.MaxValue;
            double ymin = double.MaxValue;
            double xmax = double.MinValue;
            double ymax = double.MinValue;

            int i = -1;
            while (true)
            {
                ++i;
                if (i >= data.Length)
                    break;

                if (data[i].x < xmin)
                    xmin = data[i].x;
                else if (data[i].x > xmax)
                    xmax = data[i].x;

                if (data[i].y < ymin)
                    ymin = data[i].y;
                else if (data[i].y > ymax)
                    ymax = data[i].y;
            }

            double[] coords = { xmin, ymin, xmax, ymax };
            return coords;
        }

        public void FirstPt(float x, float y)
        {
            Clear();

            data[0].x = x;
            data[0].y = y;

            data[1].x = x;
            data[1].y = y;

            data[2].x = x;
            data[2].y = y;

            data[3].x = x;
            data[3].y = y;

            count = 1;
        }

        public void SecondPt(float x, float y)
        {
            if (count < 1)
                return;

            data[1].x = x;

            data[2].x = x;
            data[2].y = y;

            data[3].x = data[0].x;
            data[3].y = y;

            bool hasArea = ((data[2].x != data[0].x) && (data[2].y != data[0].y));
            count = (hasArea ? 2 : 1);

            IsMasked = (count != 2);
        }
    }
}
