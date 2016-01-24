using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace AmbiBeam
{
    public class LEDTest
    {
        private List<Color> _colors;
        private int _pos;

        public LEDTest(int numLEDs)
        {
            _colors = new List<Color>();
            for (int i = 0; i < numLEDs; ++i)
            {
                _colors.Add(Color.Black);
            }
            _pos = 0;
        }

        public List<Color> GetColors()
        {
            _colors[_pos] = Color.Black;
            _pos++;
            if (_pos == _colors.Count)
                _pos = 0;
            _colors[_pos] = Color.Blue;

            return _colors;
        }
    }
}
