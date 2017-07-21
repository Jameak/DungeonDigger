using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonDigger.Generation
{
    public class TestGenerator : IGenerator
    {
        private const string WidthKey = "width";
        private const string HeightKey = "height";
        private const string RandomizeKey = "randomize";
        private const string DropdownKey = "dropdown";

        private readonly IDictionary<string, object> _options;
        private readonly Random _rand;

        public static IReadOnlyList<GeneratorOption> Options { get; } = new List<GeneratorOption>
        {
            new GeneratorOption
            {
                Label = "Map Width",
                Control = GeneratorOption.ControlType.IntegerField,
                Key = WidthKey,
                DefaultContent = "50"
            },
            new GeneratorOption
            {
                Label = "Map Height",
                Control = GeneratorOption.ControlType.IntegerField,
                Key = HeightKey,
                DefaultContent = "40"
            },
            new GeneratorOption
            {
                Label = "Dropdown",
                Control = GeneratorOption.ControlType.Dropdown,
                ControlOptions = new List<Tuple<string, object>>
                {
                    new Tuple<string, object>("Option 1", "String value"),
                    new Tuple<string, object>("Option 2", 150),
                },
                Key = DropdownKey,
                DefaultContent = "0"
            },
            new GeneratorOption
            {
                Label = "Randomize Map",
                Control = GeneratorOption.ControlType.Checkbox,
                Key = RandomizeKey,
                DefaultContent = bool.TrueString
            }
        };

        public TestGenerator(IDictionary<string, object> options)
        {
            _rand = new Random();
            _options = options;
        }

        public Tile[,] Construct()
        {
            var locs = new Tile[int.Parse(_options[WidthKey].ToString()), int.Parse(_options[HeightKey].ToString())];
            var randomize = bool.Parse(_options[RandomizeKey].ToString());

            if (randomize)
            {
                for (int i = 0; i < locs.GetLength(0); i++)
                {
                    for (int j = 0; j < locs.GetLength(1); j++)
                    {
                        locs[i, j] = (j + i) % _rand.Next(1, 5) == 0 ? Tile.Room : Tile.Wall;
                    }
                }
            }
            else
            {
                for (int i = 0; i < locs.GetLength(0); i++)
                {
                    for (int j = 0; j < locs.GetLength(1); j++)
                    {
                        locs[i, j] = Tile.Wall;
                    }
                }
            }
            
            return locs;
        }
    }
}
