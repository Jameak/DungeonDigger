using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonDigger.Generation
{
    public class Gauss : IGenerator
    {

        public static IReadOnlyList<GeneratorOption> Options { get; } = new List<GeneratorOption>
        {

            new GeneratorOption
            {
                Label = "Map Height",
                Control = GeneratorOption.ControlType.IntegerField,
                Key = HeightKey,
                DefaultContent = "30"
            },
            new GeneratorOption
            {
                Label = "Map Width",
                Control = GeneratorOption.ControlType.IntegerField,
                Key = WidthKey,
                DefaultContent = "30"
            },
            new GeneratorOption
            {
                Label = "Room Size",
                Control = GeneratorOption.ControlType.IntegerField,
                Key = RSKey,
                DefaultContent = "5"
            },
            new GeneratorOption
            {
                Label = "Room Size Variance",
                Control = GeneratorOption.ControlType.IntegerField,
                Key = RSVKey,
                DefaultContent = "5"
            }
        };
        public Tile[,] Construct()
        {
            double[,] prob = FreshProb();
            Tuple<int, int, double> Pos;
            int rx; int ry;
            if (Fullgen == true)
            {
                do
                {
                    rx = RS + rnd.RandomInt(-RSV, RSV);
                    ry = RS + rnd.RandomInt(-RSV, RSV);

                    Pos = SelectRoom(prob, rx, ry);
                    CreateRoom(Pos.Item1, Pos.Item2, rx, ry);
                    prob = UpdateProb(prob, Pos.Item1, Pos.Item2, rx, ry);
                }
                while (Pos.Item3 > 0);
            }
            return Map;
        }

        Rng rnd = new Rng();

        private const string HeightKey = "Height";
        private const string WidthKey = "Width";
        private const string RSKey = "RoomSize";
        private const string RSVKey = "RoomSizeVariance";
        private Tile[,] Map;
        private bool Fullgen;
        private int RS;     //RoomSize
        private int RSV;    //RoomSizeVariance
        
        public Gauss(IDictionary<string,object> options)
        {
            Map = new Tile[int.Parse(options[HeightKey].ToString()), int.Parse(options[WidthKey].ToString())];
            Fullgen = true;
            RS = int.Parse(options[RSKey].ToString());
            RSV = int.Parse(options[RSVKey].ToString());
        }

        /// <summary>
        /// Given a probability density, height and width of room. Chooses a point where the center of the room should be placed "randomly".
        /// </summary>
        /// <param name="prob"> probability density for room selection</param>
        /// <param name="x">height of room</param>
        /// <param name="y">width of room</param>
        /// <returns>center point for room and summed probability density</returns>
        public Tuple<int, int, double> SelectRoom(double[,] prob, int x, int y)
        {
            int w = 0; int h = 0;
            double probfield = 0;
            double current = 0;
            for (int i = x / 2 + 1; i < prob.GetLength(0) - x / 2 - 1; i++)
            {
                for (int k = y / 2 + 1; k < prob.GetLength(1) - y / 2 - 1; k++)
                {
                    probfield = prob[i, k] + probfield;
                }
            }
            double probpoint = rnd.RandomDouble(0, probfield);
            for (int i = x / 2 + 1; i < prob.GetLength(0) - x / 2 - 1; i++)
            {
                for (int k = y / 2 + 1; k < prob.GetLength(1) - y / 2 - 1; k++)
                {
                    if ((probpoint >= current) & (probpoint <= current + prob[i, k]))
                    {
                        w = k; h = i;
                        i = prob.GetLength(0);
                        break;
                    }
                    current = current + prob[i, k];
                }
            }
            return new Tuple<int, int, double>(h, w, probfield);
        }
        /// <summary>
        /// Creates a room of dimensions (x,y) at position (h,w)
        /// </summary>
        /// <param name="h"> Height on map coord</param>
        /// <param name="w"> Width on map coord</param>
        /// <param name="x">height of room</param>
        /// <param name="y">width of room</param>
        public void CreateRoom(int h, int w, int x, int y)
        {
            int p; int q;
            if (x % 2 == 1)
            {
                p = (x - 1) / 2;
            }
            else
            {
                p = (x / 2) + rnd.RandomInt(0, 1);
            }
            if (y % 2 == 1)
            {
                q = (y - 1) / 2;
            }
            else
            {
                q = (y / 2) + rnd.RandomInt(0, 1);
            }
            for (int i = 0; i < x; i++)
            {
                for (int k = 0; k < y; k++)
                {
                    Map[h - p + i, w - q + k] = Tile.Room;
                }
            }
        }
        /// <summary>
        /// Creates a fresh probability density with 0's on the edge and 1's everywhere else. Same size as Map
        /// </summary>
        /// <returns>0's on edge 1's everywhere else</returns>
        public double[,] FreshProb()
        {
            double[,] prob;
            int height = Map.GetLength(0); int width = Map.GetLength(1);
            prob = new double[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int k = 0; k < width; k++)
                {
                    if (k == 0 || i == 0 || k == width - 1 || i == height - 1) { prob[i, k] = 0; }
                    else { prob[i, k] = 1; }
                }
            }
            return prob;
        }
        /// <summary>
        /// Updates the probability density by substracting a gaussian from the original probability density.
        /// Gaussian is centered on room placement (h,w) and width of gaussian is determined by room size (x,y)
        /// </summary>
        /// <param name="oldprob">Old probability density</param>
        /// <param name="h">Height on map that room was placed</param>
        /// <param name="w">Width on map that room was placed</param>
        /// <param name="x">Height of room placed</param>
        /// <param name="y">Width of room placed</param>
        /// <returns> New probability density that dissuades rooms from being placed close to the one just made</returns>
        public double[,] UpdateProb(double[,] oldprob, int h, int w, int x, int y)
        {
            double[,] prob;
            int height = oldprob.GetLength(0); int width = oldprob.GetLength(1);
            prob = new double[height, width];
            double sigmax = x * 0.5;
            double sigmay = y * 0.5;
            double A = 30;
            for (int i = 0; i < height; i++)
            {
                for (int k = 0; k < width; k++)
                {
                    prob[i, k] = oldprob[i, k] - A * Math.Exp(-((h - i) * (h - i) / (2 * sigmax * sigmax) + (w - k) * (w - k) / (2 * sigmay * sigmay)));
                    if (prob[i, k] < 0)
                    {
                        prob[i, k] = 0;
                    }
                }
            }
            return prob;
        } //subtracts a gaussian from the prob distribution centered on h,w based on x,y as width of gaussian
    }
    class Rng
    {
        Random rnd = new Random();
        /// <summary>
        /// Grabs random integer between and including lowest and highest
        /// </summary>
        /// <param name="lowest">lowest integer that can appear</param>
        /// <param name="highest">highest integer that can appear</param>
        /// <returns>random number in between inputs</returns>
        public int RandomInt(int lowest, int highest)
        {
            return rnd.Next(lowest, highest);
        }
        /// <summary>
        /// Grabs random double in between two numbers
        /// </summary>
        /// <param name="lowest">Lower limit for random number</param>
        /// <param name="highest">Higher limit for random number</param>
        /// <returns>Random number</returns>
        public double RandomDouble(double lowest, double highest)
        {
            return rnd.NextDouble() * (highest - lowest) + lowest;
        }
    }

}
