using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;


namespace Forever.Gaze
{
    public class PupilFinder
    {
        private int SmallestPupilSize { get; set; }
        private float Threshold { get; set; }
        Bitmap Image { get; set; }

        Pen debugPen = new Pen(Color.YellowGreen);


        public PupilFinder(int smallestPupilSize, float threshold)
        {
            SmallestPupilSize = smallestPupilSize;
            Threshold = threshold;
        }

        public Rectangle? FindPupil(Bitmap image)
        {
            List<Rectangle> possibleHits = GetPossibleHits(image, SmallestPupilSize, Threshold);

            using (Graphics g = Graphics.FromImage(image))
            {
                foreach (var hit in possibleHits)
                {
                    g.DrawRectangle(debugPen, hit);
                }
            }

            if (!possibleHits.Any()) return null;
            return possibleHits.FirstOrDefault();
        }



        private List<Rectangle> GetPossibleHits(Bitmap image, int scan, float threshold)
        {
            var possibleHits = new List<Rectangle>();
            if (scan == 0) return possibleHits;

            var shortColor = GetShortestColor(image, scan);
            var thresholdColorLength = threshold;

            for (int i = 0; i < image.Width / scan; i++)
            {
                for (int j = 0; j < image.Height / scan; j++)
                {
                    var p = new Point(i * scan, j * scan);
                    if (PossibleHit(image, p, scan, thresholdColorLength))
                    {

                        possibleHits.Add(new Rectangle(p.X, p.Y, scan, scan));
                    }
                }
            }
            return possibleHits;
        }


        private bool PossibleHit(Bitmap image, Point p, int scan, float thresholdColorLength)
        {
            var pixel = Pixel(image, p);
            float epsilon = 0f;
            for (int i = 0; i < scan ; i++)
            {
                for (int j = 0; j < scan ; j++)
                {
                    var query = new Point(p.X + i, p.Y + j);
                    var queried = Pixel(image, query);
                    var queriedLength = ColorLength(queried);
                    if (queriedLength - thresholdColorLength < epsilon)
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        private Color GetShortestColor(Bitmap image, int stride)
        {
            var shortest = Color.White;
            float shortestLength = 1000000f;
            float length;
            for (int i = 0; i < image.Width / stride; i++)
            {
                for (int j = 0; j < image.Height / stride; j++)
                {
                    var qPoint = new Point(i*stride,  j*stride);
                    
                    length =  AverageColorLength(image, qPoint, stride);
                    if(length < shortestLength)
                    {

                        shortest = Pixel(image, qPoint);
                        shortestLength = length;
                    }
                }
            }
            return shortest;
        }

        private Color Pixel(Bitmap image, Point p)
        {
            return image.GetPixel(p.X, p.Y);
        }
        private float ColorLength(Color pixel)
        {
            var x = (double) pixel.R;
            var y = (double)pixel.G;
            var z = (double)pixel.B;
            
            return (float) Math.Sqrt(x * x + y * y + z * z);
        }


        private float AverageColorLength(Bitmap image, Point p, int spray)
        {
            int total = 0;
            float totalLength = 0f;
            for (int i = 0; i < spray; i++)
            {
                for (int j = 0; j < spray; j++)
                {
                    var smudgePoint =  new Point(p.X + i, p.Y + j);

                    if (smudgePoint.X < 0 || smudgePoint.Y < 0) continue;
                    if (smudgePoint.X >= image.Width || smudgePoint.Y >= image.Height) continue;
                    totalLength += ColorLength(Pixel(image,smudgePoint));
                    total++;
                }
            }

            return totalLength / (float)total;
        }

    }
}
