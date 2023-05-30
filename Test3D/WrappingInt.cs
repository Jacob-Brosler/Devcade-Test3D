using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test3D
{
    internal class WrappingInt
    {
        private int _value;
        private int _min;
        private int _max;

        public WrappingInt(int initialValue, int min, int max) { _value = initialValue; _min = min; _max = max; }

        public int Value { get { return _value;} }
        public int Min { get { return _min;} }
        public int Max { get { return _max;} }

        /// <summary>
        /// Adds a number to the stored value, wrapping if needed to keep it inside the boundaries
        /// Taken from https://stackoverflow.com/questions/14415753/wrap-value-into-range-min-max-without-division
        /// </summary>
        /// <param name="value"></param>
        /// <returns>New value</returns>
        public int Add(int value)
        {
            _value += value;
            if (_value < _min)
                _value = _max - (_min - _value) % (_max - _min);
            else
                _value = _min + (_value - _min) % (_max - _min);
            return _value;
        }
    }
}