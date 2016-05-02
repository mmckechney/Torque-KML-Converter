using System.Collections.Generic;

namespace Kml.Converter
{
    internal class TorqueKnownTypes
    {
        internal const string Speed = "GPS Speed (Meters/second)";
        internal const string Longitude = "Longitude";
        internal const string Latitude = "Latitude";
        internal const string Bearing = "Bearing";
        internal const string Time = "GPS Time";
        internal const string DeviceTime = "Device Time";
        internal static List<string> GetTorqueKnownTypeStrings()
        {
            List<string> tmp = new List<string> { Speed, Longitude, Latitude, Bearing, Time };
            return tmp;
        }
    }

}
