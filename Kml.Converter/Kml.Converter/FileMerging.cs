using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
namespace Kml.Converter
{
    internal class FileMerging
    {
        internal static List<List<Dictionary<string, string>>> MergeFiles(string[] files)
        {
            Dictionary<string, List<Dictionary<string, string>>> parsed = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, string>>>();
            CsvParser csvParse = new CsvParser();
            foreach (string file in files)
            {
                parsed.Add(file, csvParse.ParseFile(file));
            }

            foreach (var current in parsed)
            {
                if (current.Value.Count == 0)
                    continue;

                DateTime start = DateTime.Parse(current.Value[0][TorqueKnownTypes.DeviceTime]);
                DateTime end = DateTime.Parse(current.Value[current.Value.Count - 1][TorqueKnownTypes.DeviceTime]);

                foreach (var looper in parsed)
                {
                    if (current.Key == looper.Key)
                        continue;
                    try
                    {
                        DateTime looperStart = DateTime.Parse(looper.Value[0][TorqueKnownTypes.DeviceTime]);
                        DateTime looperEnd = DateTime.Parse(looper.Value[looper.Value.Count - 1][TorqueKnownTypes.DeviceTime]);

                        TimeSpan diffAppendEnd = looperStart - end;
                        if (diffAppendEnd.TotalMinutes > 0 && diffAppendEnd.TotalMinutes < 60)
                        {
                            current.Value.AddRange(looper.Value);
                            //File.Move(looper.Key, Path.GetDirectoryName(looper.Key) + @"\Merged-" + Path.GetFileName(looper.Key));
                            looper.Value.Clear();
                            Console.WriteLine(String.Format("Merging {0} into {1}", Path.GetFileName(looper.Key), Path.GetFileName(current.Key)));
                        }

                        TimeSpan diffAppendStart = start - looperEnd;
                        if (diffAppendStart.TotalMinutes > 0 && diffAppendStart.TotalMinutes < 60)
                        {
                            current.Value.InsertRange(0, looper.Value);
                            //File.Move(looper.Key, Path.GetDirectoryName(looper.Key) + @"\Merged-" + Path.GetFileName(looper.Key));
                            looper.Value.Clear();
                            Console.WriteLine(String.Format("Merging {0} into {1}", Path.GetFileName(looper.Key), Path.GetFileName(current.Key)));
                        }
                    }
                    catch
                    { }
                }
            }
            var merged = from p in parsed
                         where p.Value.Count > 0
                         select p.Value;

            return merged.ToList();
        }
    }
}
