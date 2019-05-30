using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonDigger.Generation
{
    public class Elimination : IGenerator
    {
        private Random rnd = new Random();
        private const string HeightKey = "Height";
        private const string WidthKey = "Width";
        private const string RSKey = "RoomSize";
        private const string RSVKey = "RoomSizeVariance";
        private Tile[,] Map; //make private
        private bool Fullgen;
        private int RS;
        private int RSV;

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
                DefaultContent = "5"
            },
            new GeneratorOption
            {
                Label = "Room Size Variance",
                Control = GeneratorOption.ControlType.IntegerField,
                Key = RSVKey,
                DefaultContent = "2"
            }
        };

        public Elimination(IDictionary<string, object> options)
        {
            Map = new Tile[int.Parse(options[HeightKey].ToString()), int.Parse(options[WidthKey].ToString())];
            Fullgen = true;
            RS = int.Parse(options[RSKey].ToString());
            RSV = int.Parse(options[RSVKey].ToString());
        }

        public Tile[,] Construct()
        {
            double[,] prob = FreshProb();
            Tuple<int, int, double> Pos;
            int rx; int ry;
            if (Fullgen == true)
            {
                do
                {
                    rx = RS + rnd.Next(-RSV, RSV);
                    ry = RS + rnd.Next(-RSV, RSV);
                    Pos = SelectRoom(prob, rx, ry);
                    CreateRoom(Pos.Item1, Pos.Item2, rx, ry);
                    prob = UpdateProb(prob);
                }
                while (Pos.Item3 > 0);
            }
            return Map;
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

        private double[,] Copy(double[,] orig)
        {
            double[,] temp = new double[orig.GetLength(0), orig.GetLength(1)];
            for (int i = 0; i < orig.GetLength(0); i++)
            {
                for (int j = 0; j < orig.GetLength(1); j++)
                {
                    temp[i, j] = orig[i, j];
                }
            }
            return temp;
        }

        public Tuple<int, int, double> SelectRoom(double[,] prob, int x, int y)
        {
            int w = 0; int h = 0;
            double probfield = 0;
            double current = 0;
            double[,] tempprob = Copy(prob);

            for (int i = 0; i < prob.GetLength(0); i++)
            {
                for (int k = 0; k < prob.GetLength(1); k++)
                {
                    if (prob[i, k] == 0)
                    {
                        for (int j = -(x / 2 + 1); j < (x / 2 + 1); j++)
                        {
                            for (int l = -(y / 2 + 1); l < (y / 2 + 1); l++)
                            {
                                if ((i + j < prob.GetLength(0) & i + j >= 0) & (k + l < prob.GetLength(1) & k + l >= 0))
                                {
                                    tempprob[i + j, k + l] = 0;
                                }
                            }
                        }
                    }
                }
            }
            for (int i = x / 2 + 1; i < tempprob.GetLength(0) - x / 2 - 1; i++)
            {
                for (int k = y / 2 + 1; k < tempprob.GetLength(1) - y / 2 - 1; k++)
                {
                    probfield = tempprob[i, k] + probfield;
                }
            }
            double probpoint = rnd.RandomDouble(0, probfield);
            for (int i = x / 2 + 1; i < tempprob.GetLength(0) - x / 2 - 1; i++)
            {
                for (int k = y / 2 + 1; k < tempprob.GetLength(1) - y / 2 - 1; k++)
                {
                    if ((probpoint >= current) & (probpoint <= current + tempprob[i, k]))
                    {
                        w = k; h = i;
                        i = tempprob.GetLength(0);
                        break;
                    }
                    current = current + tempprob[i, k];
                }
            }
            return new Tuple<int, int, double>(h, w, probfield);
        }

        public void CreateRoom(int h, int w, int x, int y)
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

        public double[,] FreshProb()
        {
            double[,] prob;
            int height = Map.GetLength(0); int width = Map.GetLength(1);
            prob = new double[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int k = 0; k < width; k++)
                {
                    if (k == 0 || i == 0 || (k == width - 1) || (i == height - 1)) { prob[i, k] = 0; }
                    else { prob[i, k] = 1; }
                }
            }
            return prob;
        }

        public double[,] UpdateProb(double[,] oldprob)
        {
            double[,] prob;
            int height = oldprob.GetLength(0);
            int width = oldprob.GetLength(1);
            prob = new double[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int k = 0; k < width; k++)
                {
                    if (Map[i, k] == Tile.Room)
                    {
                        prob[i, k] = 0;
                    }
                    else
                    {
                        prob[i, k] = oldprob[i, k];
                    }
                }
            }
            return prob;
        }
    }
}
