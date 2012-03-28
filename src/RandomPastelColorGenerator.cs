using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.ComponentModel;

namespace NetEnvSwitcher
{
    /// <summary>
    /// Provides a range of tasteful random pastel colors
    /// http://blog.functionalfun.net/2008/07/random-pastel-colour-generator.html
    /// </summary>
    public class RandomPastelColorGenerator
    {
        private readonly Random _random;

        public RandomPastelColorGenerator()
        {
            // seed the generator with 3 because
            // this gives a good sequence of colors
            const int RandomSeed = 5;
            _random = new Random(RandomSeed);
        }

        /// <summary>
        /// Returns a random pastel brush
        /// </summary>
        /// <returns></returns>
        public SolidColorBrush GetNextBrush()
        {
            SolidColorBrush brush = new SolidColorBrush(GetNext());
            // freeze the brush for efficiency
            brush.Freeze();

            return brush;
        }

        /// <summary>
        /// Returns a random pastel color
        /// </summary>
        /// <returns></returns>
        public Color GetNext()
        {
            // to create lighter colours:
            // take a random integer between 0 & 128 (rather than between 0 and 255)
            // and then add 127 to make the colour lighter
            byte[] colorBytes = new byte[3];
            colorBytes[0] = (byte)(_random.Next(128) + 50);
            colorBytes[1] = (byte)(_random.Next(128) + 30);
            colorBytes[2] = (byte)(_random.Next(128) + 40);

            Color color = new Color();

            // make the color fully opaque
            color.A = 255;
            color.R = colorBytes[0];
            color.B = colorBytes[1];
            color.G = colorBytes[2];

            return color;
        }
    }
}
