using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DungeonDigger.Generation
{
    public static class Generators
    {
        public static IReadOnlyList<Tuple<string, GeneratorFactory, IReadOnlyList<GeneratorOption>, ValidateOptions>> BuiltinGenerators { get; } = new List<Tuple<string, GeneratorFactory, IReadOnlyList<GeneratorOption>, ValidateOptions>>
        {
            CreateGen("Gaussian",       d => new Gauss(d)        , Gauss.Options        , d => BaseValidator(Gauss.Options, d)),
            CreateGen("Elimination",    d => new Elimination(d)  , Elimination.Options  , d => BaseValidator(Elimination.Options, d)),
#if DEBUG
            CreateGen("Test Generator", d => new TestGenerator(d), TestGenerator.Options, d => BaseValidator(TestGenerator.Options, d)),
#endif
        };

        public delegate IGenerator GeneratorFactory(IDictionary<string, object> generatorOptions);
        public delegate Tuple<bool, string> ValidateOptions(IDictionary<string, object> userOptions);

        private static Tuple<bool, string> BaseValidator(IReadOnlyList<GeneratorOption> options, IDictionary<string, object> userOptions)
        {
            foreach (var generatorOption in options)
            {
                switch (generatorOption.Control)
                {
                    case GeneratorOption.ControlType.IntegerField:
                        if (!int.TryParse(userOptions[generatorOption.Key].ToString(), out _)) return Tuple.Create(false, $"Invalid input for {generatorOption.Label}. Input must be a number");
                        break;
                    case GeneratorOption.ControlType.Dropdown:
                    case GeneratorOption.ControlType.Checkbox:
                    case GeneratorOption.ControlType.TextField:
                    default:
                        break;
                }
            }

            return Tuple.Create(true, string.Empty);
        }

        private static Tuple<string, GeneratorFactory, IReadOnlyList<GeneratorOption>, ValidateOptions> CreateGen(string label, GeneratorFactory fac, IReadOnlyList<GeneratorOption> options, ValidateOptions validatorFunc)
        {
#if DEBUG
            //Validate and warn about the generator options.
            var optionKeys = new HashSet<string>();
            foreach (var option in options)
            {
                if (option.Key == null) throw new ArgumentException($"Generator option key must be non-null. Option {option.Label} on generator named {label}"); //If the key is null, the program will crash when you try to make the generator construct a map.

                if (option.Label == null) Debug.WriteLine("Option doesn't have an associated label.");
                if (option.ControlOptions != null && option.Control != GeneratorOption.ControlType.Dropdown) Debug.WriteLine("Generator option has list of ControlOptions, but the ControlType doesn't use them.");

                switch (option.Control)
                {
                    case GeneratorOption.ControlType.Dropdown:
                        if(option.ControlOptions == null) Debug.WriteLine("Generator option is missing ControlOptions");
                        if(!int.TryParse(option.DefaultContent, out int _)) throw new ArgumentException($"Default content for option {option.Label} on generator {label} must be parseable to an integer because the ControlType is {option.Control}.");
                        break;
                    case GeneratorOption.ControlType.Checkbox:
                        if(!bool.TryParse(option.DefaultContent, out bool _)) throw new ArgumentException($"Default content for option {option.Label} on generator {label} must be parseable to a boolean because the ControlType is {option.Control}.");
                        break;
                    case GeneratorOption.ControlType.IntegerField:
                        if (!int.TryParse(option.DefaultContent, out int _)) throw new ArgumentException($"Default content for option {option.Label} on generator {label} must be parseable to an integer because the ControlType is {option.Control}.");
                        break;
                    default:
                        break;
                }

                if (optionKeys.Contains(option.Key)) Debug.WriteLine($"Options for generator has duplicate keys. Duplicate key: {option.Key}");
                else optionKeys.Add(option.Key);
            }
#endif

            return new Tuple<string, GeneratorFactory, IReadOnlyList<GeneratorOption>, ValidateOptions>(label, fac, options, validatorFunc);
        }
    }
}
