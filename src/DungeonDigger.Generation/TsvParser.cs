using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DungeonDigger.Generation
{
    public static class TsvParser
    {
        private static readonly IDictionary<string, Symbol> SymbolMap = new Dictionary<string, Symbol>
        {
            {string.Empty, Symbol.Wall},
            {"F", Symbol.Floor},
            {"DR", Symbol.DoorRight },
            {"DL", Symbol.DoorLeft },
            {"DT", Symbol.DoorTop },
            {"DB", Symbol.DoorBottom },
            {"SD", Symbol.StairDown },
            {"SDD", Symbol.StairDownDown },
            {"SU", Symbol.StairUp },
            {"SUU", Symbol.StairUpUp },
        };
        
        /// <summary>
        /// Parses the given file containing TSV data.
        /// </summary>
        /// <param name="path">The path to the file to parse</param>
        public static Tile[,] Parse(string path)
        {
            using (var stream = new StreamReader(path))
            {
                return Parse(stream);
            }
        }

        /// <summary>
        /// Parses the given stream of TSV data.
        /// </summary>
        /// <param name="stream">Stream of TSV data</param>
        public static Tile[,] Parse(StreamReader stream)
        {
            var lines = new List<List<Symbol>>();

            string inputline;
            while ((inputline = stream.ReadLine()) != null)
            {
                var line = new List<Symbol>();
                lines.Add(line);

                var symbols = inputline.Split('\t');
                var parsedSymbols = symbols.Select(symbol => SymbolMap.ContainsKey(symbol) ? SymbolMap[symbol] : Symbol.Unknown);
                line.AddRange(parsedSymbols);
            }
            
            if(lines.Count == 0) return new Tile[0,0];

            //When parsing we are reading the file line by line which inverts the height and width of the map, so we have to transpose it.
            var longestLine = lines.Select(line => line.Count).Max();
            Tile?[,] transposedMap = new Tile?[longestLine, lines.Count];

            var x = 0;
            foreach (var line in lines)
            {
                var y = 0;
                foreach (var symbol in line)
                {
                    transposedMap[y, x] = ToTile(symbol);
                    y++;
                }
                x++;
            }

            //Make sure that in the case of a jagged input array, the conversion to a non-jagged map leaves the extra tiles as walls and not the default enum value.
            Tile[,] map = new Tile[transposedMap.GetLength(0), transposedMap.GetLength(1)];
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    map[i, j] = transposedMap[i, j] == null ? Tile.Wall : transposedMap[i, j].Value;
                }
            }

            return map;
        }

        private static Tile ToTile(Symbol symbol)
        {
            switch (symbol)
            {
                case Symbol.Floor:
                    return Tile.Room;
                case Symbol.Wall:
                    return Tile.Wall;
                case Symbol.DoorLeft:
                case Symbol.DoorRight:
                case Symbol.DoorTop:
                case Symbol.DoorBottom:
                    return Tile.DoorClosed;
                case Symbol.StairDown:
                    return Tile.StairPartOne;
                case Symbol.StairDownDown:
                    return Tile.StairPartTwo;
                case Symbol.StairUp:
                    return Tile.StairPartTwo;
                case Symbol.StairUpUp:
                    return Tile.StairPartOne;
                default:
                    return Tile.Unknown;
            }
        }

        private enum Symbol
        {
            Unknown,
            Floor,
            Wall,
            DoorLeft,
            DoorRight,
            DoorTop,
            DoorBottom,
            StairDown,
            StairDownDown,
            StairUp,
            StairUpUp
        }
    }
}
