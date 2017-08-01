using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonDigger.Generation
{
    /*class Generatorhelper
    {
        /// <summary>
        /// Creates a room of dimensions (x,y) at position (h,w)
        /// </summary>
        /// <param name="h"> Height on map coord</param>
        /// <param name="w"> Width on map coord</param>
        /// <param name="x">height of room</param>
        /// <param name="y">width of room</param>
        public void CreateRoom(Tile[,] Map,Rng rnd, int h, int w, int x, int y)
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
        public void CreateUnevenRoom(int h, int w, int x, int y)
        {
            x = x % 2 == 1 ? x : x - 1;
            y = y % 2 == 1 ? y : y - 1;

        }
        /// <summary>
        /// Creates a fresh probability density with 0's on the edge and 1's everywhere else. Same size as Map
        /// </summary>
        /// <returns>0's on edge 1's everywhere else</returns>
        public double[,] FreshProb(int height, int width)
        {
            double[,] prob;
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

    } */

    public class Gauss : IGenerator
    {
        Random rnd = new Random();
        private const string HeightKey = "Height";
        private const string WidthKey = "Width";
        private const string RSKey = "RoomSize";
        private const string RSVKey = "RoomSizeVariance";
        private const string gwidthKey = "GaussianWidth";
        private const string gheightKey = "GaussianAmplitude";
        private Tile[,] Map;
        private bool Fullgen;
        private int RS;     //RoomSize
        private int RSV;    //RoomSizeVariance
        private int gwidth;
        private int gheight;

        public static IReadOnlyList<GeneratorOption> Options { get; } = new List<GeneratorOption>
        {
            new GeneratorOption
            {
                Label = "Map Height",
                Control = GeneratorOption.ControlType.IntegerField,
                Key = HeightKey,
                DefaultContent = "50"
            },
            new GeneratorOption
            {
                Label = "Map Width",
                Control = GeneratorOption.ControlType.IntegerField,
                Key = WidthKey,
                DefaultContent = "50"
            },
            new GeneratorOption
            {
                Label = "Room Size",
                Control = GeneratorOption.ControlType.IntegerField,
                Key = RSKey,
                DefaultContent = "7"
            },
            new GeneratorOption
            {
                Label = "Room Size Variance",
                Control = GeneratorOption.ControlType.IntegerField,
                Key = RSVKey,
                DefaultContent = "3"
            },
            new GeneratorOption
            {
                Label = "Width of Gaussian",
                Control = GeneratorOption.ControlType.IntegerField, //double field
                Key = gwidthKey,
                DefaultContent = "50"
            },
            new GeneratorOption
            {
                Label = "Amplitude",
                Control = GeneratorOption.ControlType.IntegerField,
                Key = gheightKey,
                DefaultContent = "30"
            }
        };

        public Gauss(IDictionary<string, object> options)
        {
            Map = new Tile[int.Parse(options[WidthKey].ToString()), int.Parse(options[HeightKey].ToString())];
            Fullgen = true;
            RS = int.Parse(options[RSKey].ToString());
            RSV = int.Parse(options[RSVKey].ToString());
            gwidth = int.Parse(options[gwidthKey].ToString());
            gheight = int.Parse(options[gheightKey].ToString());
        }

    public Tile[,] Construct()
        {
            double[,] prob = FreshProb();
            Tuple<int, int, double> Pos;
            int rx; int ry;
            if (Fullgen == true)
            {
                while (true)
                {
                    rx = RS + rnd.Next(-RSV, RSV);
                    ry = RS + rnd.Next(-RSV, RSV);

                    Pos = SelectRoom(prob, rx, ry);
                    if (Pos.Item3 <= 0)
                    {
                        break;
                    }
                        CreateRoom(Pos.Item1, Pos.Item2, rx, ry);
                    prob = UpdateProb(prob, Pos.Item1, Pos.Item2, rx, ry);
                }
                
                FindDoors obj = new FindDoors(Map,rnd);
                Map = obj.GetMapWithDoor();
               
               //Map = Hallways(Map);
                
            }
            return Map;
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
                    if ((probpoint >= current) && (probpoint <= current + prob[i, k]))
                    {
                        w = k; h = i;
                        i = prob.GetLength(0); //break out of both loops
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
        private void CreateRoom(int h, int w, int x, int y)
        {
            int p; int q;
            if (x % 2 == 1)
            {
                p = (x - 1) / 2;
            }
            else
            {
                p = (x / 2) + rnd.Next(0, 1);
            }
            if (y % 2 == 1)
            {
                q = (y - 1) / 2;
            }
            else
            {
                q = (y / 2) + rnd.Next(0, 1);
            }
            for (int i = 0; i < x; i++)
            {
                for (int k = 0; k < y; k++)
                {
                    Map[h - p + i, w - q + k] = Tile.Room;
                }
            }
        }

        public void TestPrint()
        {
            for (int i = 0; i < Map.GetLength(0); i++)
            {
                for (int k = 0; k < Map.GetLength(1); k++)
                {
                    if (Map[i, k] == Tile.Room) { System.Diagnostics.Debug.Write("1"); }
                    else
                    {
                        System.Diagnostics.Debug.Write("0");
                    }
                }
                System.Diagnostics.Debug.WriteLine(" ");
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
            double sigmax = x * gwidth/ (double)100;
            double sigmay = y * gwidth/ (double)100;
            double denominatorx = 2 * sigmax * sigmax;
            double denominatory = 2 * sigmay * sigmay;
            double A = gheight;
            for (int i = 0; i < height; i++)
            {
                    for (int k = 0; k < width; k++)
                    {
                            prob[i, k] = oldprob[i, k] - A * Math.Exp(-((h - i) * (h - i) / denominatorx + (w - k) * (w - k) / denominatory));
                            if (prob[i, k] < 0)
                            {
                                prob[i, k] = 0;
                            }
                    }
            }
            return prob;
        }
    }
}
