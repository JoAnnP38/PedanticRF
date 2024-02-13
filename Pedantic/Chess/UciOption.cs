using Pedantic.Utilities;
using System.Text;

namespace Pedantic.Chess
{
    public class UciOptionBase
    {
        public UciOptionBase(string name, UciOptionType type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; init; }
        public UciOptionType Type { get; init; }

        public override string ToString()
        {
            return $"option name {Name} type {StringType}";
        }

        private string StringType
        {
            get
            {
                return Type switch
                {
                    UciOptionType.String => "string",
                    UciOptionType.Spin => "spin",
                    UciOptionType.Combo => "combo",
                    UciOptionType.Button => "button",
                    UciOptionType.Check => "check",
                    _ => "string"
                };
            }
        }
    }

    public class UciOptionButton : UciOptionBase
    {
        public UciOptionButton(string name) : base(name, UciOptionType.Button) 
        { }
    }

    public class UciOptionCheck : UciOptionBase
    {
        public UciOptionCheck(string name, bool defaultValue) : base(name, UciOptionType.Check)
        {
            DefaultValue = defaultValue;
            CurrentValue = DefaultValue;
        }

        public bool DefaultValue { get; init; }
        public bool CurrentValue { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()} default {StringValue}";
        }

        private string StringValue
        {
            get
            {
                return DefaultValue ? "true" : "false";
            }
        }
    }

    public class UciOptionString : UciOptionBase
    {
        public UciOptionString(string name, string defaultValue) : base(name, UciOptionType.String)
        {
            DefaultValue = defaultValue;
            CurrentValue = DefaultValue;
        }

        public string DefaultValue 
        { 
            get
            {
                if (string.IsNullOrWhiteSpace(defaultValue))
                {
                    return "<empty>";
                }
                return defaultValue;
            }
            init
            {
                if (string.IsNullOrWhiteSpace(value) || string.Compare(value, "<empty>", true) == 0)
                {
                    defaultValue = string.Empty;
                }
                else
                {
                    defaultValue = value;
                }
            }
        }

        public string CurrentValue { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()} default {DefaultValue}";
        }

        private string defaultValue = string.Empty;
    }

    public class UciOptionSpin : UciOptionBase
    {
        public UciOptionSpin(string name, int defaultValue, int minValue, int maxValue) : base(name, UciOptionType.Spin)
        {
            MinValue = minValue;
            MaxValue = maxValue;
            DefaultValue = defaultValue;
            CurrentValue = DefaultValue;
        }

        public int DefaultValue 
        { 
            get
            {
                return defaultValue;
            }

            init
            {
                Util.Assert(value >= MinValue && value <= MaxValue);
                if (value >= MinValue && value <= MaxValue)
                {
                    defaultValue = value;
                }
            }
        }

        public int CurrentValue
        {
            get => currentValue;
            set
            {
                Util.Assert(value >= MinValue && value <= MaxValue);
                if (value >= MinValue && value <= MaxValue)
                {
                    currentValue = value;
                }
            }
        }

        public int MinValue { get; init; }
        public int MaxValue { get; init; } 

        public override string ToString()
        {
            return $"{base.ToString()} default {DefaultValue} min {MinValue} max {MaxValue}";
        }

        private int defaultValue;
        private int currentValue;
    }

    public class UciOptionCombo : UciOptionBase
    {
        public UciOptionCombo(string name, string defaultValue, params string[] options) : base(name, UciOptionType.Combo)
        {
            this.options = new string[options.Length];
            Array.Copy(options, this.options, options.Length);
            DefaultValue = defaultValue;
            currentValue = DefaultValue;
        }

        public string DefaultValue
        {
            get
            {
                return defaultValue;
            }

            init
            {
                if (Array.Exists(options, (s) => s.Equals(value)))
                {
                    defaultValue = value;
                }
                else
                {
                    Util.Fail("Attempt to set uci combo value to an unsupported option.");
                }
            }
        }

        public string CurrentValue
        {
            get
            {
                return currentValue;
            }

            set
            {
                if (options.Contains(value, StringComparer.InvariantCultureIgnoreCase))
                {
                    currentValue = value;
                }
            }
        }

        public string[] Options => options;

        public override string ToString()
        {
            StringBuilder sb = new(base.ToString());
            sb.Append($" default {DefaultValue}");
            foreach (string option in options)
            {
                sb.Append($" var {option}");
            }
            return sb.ToString();
        }

        private string defaultValue = string.Empty;
        private string[] options;
        private string currentValue;
    }
}
