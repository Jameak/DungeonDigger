using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonDigger.Generation
{
    public class GeneratorOption
    {
        /// <summary>
        /// The text to show in the textblock for the option.
        /// </summary>
        public string Label { get; internal set; }

        /// <summary>
        /// The type of control to show the user.
        /// </summary>
        public ControlType Control { get; internal set; }

        /// <summary>
        /// The choices to give the user, if the chosen ControlType only gives the possibility of specific answers (such as a dropdown).
        /// </summary>
        public IReadOnlyList<Tuple<string, object>> ControlOptions { get; internal set; }

        /// <summary>
        /// The key to use in the response-dictionary to get the user-provided response from the chosen ControlType.
        /// Must be unique among the options that a generator provides.
        /// </summary>
        public string Key { get; internal set; }

        /// <summary>
        /// The default content of the control.
        /// For ControlType.Checkbox, use bool.TrueString or bool.FalseString.
        /// For Dropdown, specify the index as a string. E.g. "0"
        /// </summary>
        public string DefaultContent { get; internal set; }
        
        public enum ControlType
        {
            /// <summary>
            /// The option returned through the option-dictionary for this ControlType is the object associated with the selected item, as specified in the ControlOptions-property.
            /// </summary>
            Dropdown,
            /// <summary>
            /// The option returned through the option-dictionary for this ControlType is of type boolean.
            /// </summary>
            Checkbox,
            /// <summary>
            /// The option returned through the option-dictionary for this ControlType is of type string.
            /// </summary>
            TextField,
            /// <summary>
            /// The option returned through the option-dictionary for this ControlType is of type integer.
            /// </summary>
            IntegerField
        }
    }
    
}
