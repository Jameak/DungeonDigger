using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DungeonDigger.Generation;
using DungeonDigger.UI.ViewModels.Base;

namespace DungeonDigger.UI.ViewModels
{
    public class GenerationControlViewModel : BaseViewModel
    {
        public ObservableCollection<GeneratorEntry> Generators { get; set; } = new ObservableCollection<GeneratorEntry>();

        public GenerationControlViewModel()
        {
            foreach (Tuple<string, Func<IDictionary<string, object>, IGenerator>, IReadOnlyList<GeneratorOption>> generator in Generation.Generators.BuiltinGenerators)
            {
                Generators.Add(new GeneratorEntry
                {
                    Name = generator.Item1,
                    Func = generator.Item2,
                    Options = generator.Item3
                });
            }
        }

        public class GeneratorEntry
        {
            public string Name { get; set; }
            public Func<IDictionary<string, object>, IGenerator> Func { get; set; }
            public IReadOnlyList<GeneratorOption> Options { get; set; }

            public override string ToString()
            {
                return Name;
            }
        }
    }
}
