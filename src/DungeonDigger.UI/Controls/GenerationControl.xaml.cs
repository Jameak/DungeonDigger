using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DungeonDigger.Generation;
using DungeonDigger.UI.Events;
using DungeonDigger.UI.ViewModels;

namespace DungeonDigger.UI.Controls
{
    public partial class GenerationControl : UserControl
    {
        private GenerationControlViewModel _vm;

        public GenerationControl()
        {
            InitializeComponent();
            _vm = new GenerationControlViewModel();
            DataContext = _vm;
        }

        private void Generators_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OptionGrid.Children.Clear();
            OptionGrid.RowDefinitions.Clear();

            var selection = Generators.SelectedItem as GenerationControlViewModel.GeneratorEntry;
            if (selection == null) return;

            int index = 0;
            foreach (var option in selection.Options)
            {
                var rd = new RowDefinition();
                rd.Height = GridLength.Auto;
                OptionGrid.RowDefinitions.Add(rd);
                var block = new TextBlock();
                block.Text = option.Label;
                Grid.SetColumn(block, 0);
                Grid.SetRow(block, index);
                OptionGrid.Children.Add(block);
                switch (option.Control)
                {
                    case GeneratorOption.ControlType.TextField:
                        var textfield = new TextBoxOption();
                        textfield.Key = option.Key;
                        textfield.Text = option.DefaultContent;
                        Grid.SetColumn(textfield, 1);
                        Grid.SetRow(textfield, index);
                        OptionGrid.Children.Add(textfield);
                        break;
                    case GeneratorOption.ControlType.Dropdown:
                        var combobox = new ComboBoxOption();
                        combobox.Key = option.Key;
                        combobox.ItemsSource = ComboBoxTupleSource.CreateSource(option.ControlOptions);
                        if (option.DefaultContent != null && int.TryParse(option.DefaultContent, out int defaultIndex))
                        {
                            combobox.SelectedIndex = defaultIndex;
                        }

                        Grid.SetColumn(combobox, 1);
                        Grid.SetRow(combobox, index);
                        OptionGrid.Children.Add(combobox);
                        break;
                    case GeneratorOption.ControlType.Checkbox:
                        var checkbox = new CheckBoxOption();
                        checkbox.Key = option.Key;
                        checkbox.Content = option.Label;
                        if (bool.TryParse(option.DefaultContent, out bool defaultValue))
                        {
                            checkbox.IsChecked = defaultValue;
                        }
                        Grid.SetColumn(checkbox, 1);
                        Grid.SetRow(checkbox, index);
                        OptionGrid.Children.Add(checkbox);
                        break;
                    case GeneratorOption.ControlType.IntegerField:
                        var intField = new TextBoxOption();
                        intField.Key = option.Key;
                        intField.Text = option.DefaultContent;
                        Grid.SetColumn(intField, 1);
                        Grid.SetRow(intField, index);
                        OptionGrid.Children.Add(intField);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(GeneratorOption.Control), option.Control, "Control specified in generatoroption doesn't have a UI element.");
                }
                index++;
            }
        }

        private async void ConstructButton_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedGen = Generators.SelectedItem as GenerationControlViewModel.GeneratorEntry;
            if (selectedGen == null) return;
            
            var options = new Dictionary<string,object>();
            foreach (var option in OptionGrid.Children)
            {
                if (option is IOption opt)
                {
                    options.Add(opt.Key, opt.Value);
                }
            }

            var map = await Task.Run(() =>
            {
                var generator = selectedGen.Func.Invoke(options);
                return generator.Construct();
            });

            RaiseEvent(new MapGeneratedEvent(MapGeneratedEvent, map));
        }

        #region Events
        public static readonly RoutedEvent MapGeneratedEvent =
            EventManager.RegisterRoutedEvent(nameof(MapGeneratedEvent), RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(GenerationControl));

        public event RoutedEventHandler MapGenerated
        {
            add { AddHandler(MapGeneratedEvent, value); }
            remove { RemoveHandler(MapGeneratedEvent, value); }
        }
        #endregion

        #region Private classes
        private class ComboBoxTupleSource
        {
            public string Label { get; private set; }
            public object Value { get; private set; }

            public override string ToString()
            {
                return Label.ToString();
            }

            public static IEnumerable<ComboBoxTupleSource> CreateSource(IEnumerable<Tuple<string, object>> source)
            {
                return source.Select(i => new ComboBoxTupleSource { Label = i.Item1, Value = i.Item2 });
            }
        }

        private interface IOption
        {
            string Key { get; set; }
            object Value { get; }
        }

        private class TextBoxOption : TextBox, IOption
        {
            public string Key { get; set; }
            public object Value => Text;
        }

        private class ComboBoxOption : ComboBox, IOption
        {
            public string Key { get; set; }
            public object Value => (SelectedItem as ComboBoxTupleSource)?.Value;
        }

        private class CheckBoxOption : CheckBox, IOption
        {
            public string Key { get; set; }
            public object Value => IsChecked;
        }
        #endregion
    }
}
