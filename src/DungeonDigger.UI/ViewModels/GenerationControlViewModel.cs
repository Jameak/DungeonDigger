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
            foreach (var generator in Generation.Generators.BuiltinGenerators)
            {
                Generators.Add(new GeneratorEntry
                {
                    Name = generator.Item1,
                    Factory = generator.Item2,
                    Options = generator.Item3,
                    Validate = generator.Item4,
                });
            }
        }

        public class GeneratorEntry
        {
            public string Name { get; set; }
            public Generators.GeneratorFactory Factory { get; set; }
            public IReadOnlyList<GeneratorOption> Options { get; set; }
            public Generators.ValidateOptions Validate { get; set; }

            public override string ToString()
            {
                return Name;
            }
        }
    }
}
