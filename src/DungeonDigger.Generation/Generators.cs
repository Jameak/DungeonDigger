using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DungeonDigger.Generation
{
    public static class Generators
    {
        public static IReadOnlyList<Tuple<string,Func<IDictionary<string,object>,IGenerator>, IReadOnlyList<GeneratorOption>>> BuiltinGenerators { get; } = new List<Tuple<string, Func<IDictionary<string, object>,IGenerator>, IReadOnlyList<GeneratorOption>>>
        {
            CreateGen("Test Generator", d => new TestGenerator(d), TestGenerator.Options),
            CreateGen("Test Gen 2", d => new TestGenerator(d), TestGenerator.Options),
            CreateGen("Gaussian", d => new Gauss(d), Gauss.Options)
        };

        private static Tuple<string, Func<IDictionary<string, object>, IGenerator>, IReadOnlyList<GeneratorOption>> CreateGen(string label, Func<IDictionary<string, object>, IGenerator> func, IReadOnlyList<GeneratorOption> options)
        {
#if DEBUG
            //Validate and warn about the generator options.
            var optionKeys = new HashSet<string>();
            foreach (var option in options)
            {
                if (option.Key == null) throw new ArgumentException($"Generator option key must be non-null. {option.Label}"); //If the key is null, the program will crash when you try to make the generator construct a map.

                if (option.Label == null) Debug.WriteLine("Option doesn't have an associated label.");
                if (option.ControlOptions != null && option.Control != GeneratorOption.ControlType.Dropdown) Debug.WriteLine("Generator option has list of ControlOptions, but the ControlType doesn't use them.");
                if (option.ControlOptions == null && option.Control == GeneratorOption.ControlType.Dropdown) Debug.WriteLine("Generator option is missing ControlOptions");

                if (optionKeys.Contains(option.Key)) Debug.WriteLine($"Options for generator has duplicate keys. Duplicate key: {option.Key}");
                else optionKeys.Add(option.Key);
            }
#endif

            return new Tuple<string, Func<IDictionary<string, object>, IGenerator>, IReadOnlyList<GeneratorOption>>(label, func, options);
        }
    }
}
