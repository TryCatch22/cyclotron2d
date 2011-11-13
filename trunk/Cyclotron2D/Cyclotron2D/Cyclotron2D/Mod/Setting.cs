using System;

namespace Cyclotron2D.Mod
{
    public class Setting<T>
    {
        public string Name { get; private set; }

        public T Value { get; private set; }

        public T DefaultValue { get; private set; }

        public Setting(string name, T defaultValue)
        {
            Name = name;
            Value = DefaultValue = defaultValue;
        }

        public Func<T, bool> Validate { get; set; }

        public virtual void TrySetValue(T value)
        {
            if (Validate(value))
            {
                Value = value;
                return;
            }
            throw new InvalidValueException("Invalid value for Setting");
        }

        public void Reset()
        {
            Value = DefaultValue;
        }

        public override string ToString()
        {
            return Name;
        }

    }



    public class RangedIntegerSetting : Setting<int>
    {

        public int MinValue { get; set; }

        public int MaxValue { get; set; }

        public RangedIntegerSetting(string name, int defaultValue)
            : base(name, defaultValue)
        {
            Validate = val => val >= MinValue && val <= MaxValue;
        }

        public void TrySetValue(string value)
        {
            int result;
            if (int.TryParse(value, out result))
            {
                TrySetValue(result);
                return;
            }
            throw new InvalidValueException("Invalid value for Setting");
        }

        public override string ToString()
        {
            return base.ToString() + "("+MinValue+"-"+MaxValue+")";
        }
    }

    public class RangedFloatSetting : Setting<float>
    {

        public float MinValue { get; set; }

        public float MaxValue { get; set; }

        public RangedFloatSetting(string name, float defaultValue)
            : base(name, defaultValue)
        {
            Validate = val => val >= MinValue && val <= MaxValue;
        }

        public void TrySetValue(string value)
        {
            float result;
            if (float.TryParse(value, out result))
            {
                TrySetValue(result);
                return;
            }
            throw new InvalidValueException("Invalid value for Setting");
        }

        public override string ToString()
        {
            return base.ToString() + "(" + MinValue + "-" + MaxValue + ")";
        }
    }
}
