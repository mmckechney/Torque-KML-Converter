using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
namespace Kml.Converter
{
    internal class SpeedColors
    {
        internal static string Stopped =  ConvertColorToHex(Color.White);
        internal static string Crawl = ConvertColorToHex(Color.LightBlue); //"ccFF6600"; //
        internal static string Slow = ConvertColorToHex(Color.Yellow); //"ccFFCC00"; //
        internal static string Medium = ConvertColorToHex(Color.PeachPuff); //"ccCCFFBB"; //
        internal static string Fast = ConvertColorToHex(Color.Orange); //"cc3A5F0B"; //
        internal static string Highway = ConvertColorToHex(Color.Red); //"cc00FF00"; //

        private static string ConvertColorToHex(Color c)
        {
            return "cc" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }
    }
}
