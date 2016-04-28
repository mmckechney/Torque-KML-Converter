using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
namespace Kml.Converter
{
    internal class CsvParser
    {
        public List<Dictionary<string, string>> ParseFile(string fileName)
        {
            string[] lines = File.ReadAllLines(fileName);

            List<string> headers = lines[0].Split(new char[] { ',' }).ToList();
            headers = (from h in headers
                       select h.Trim()).ToList();

            var vals = from l in lines
                       select l.Split(new char[] { ',' });

            List<string[]> values = vals.ToList();
            values.RemoveAt(0);

            return PrepareTypeData(headers, values);

        }

        private List<Dictionary<string, string>> PrepareTypeData(List<string> headers, List<string[]> values)
        {
            List<Dictionary<string, string>> typeData = new List<Dictionary<string,string>>();
            foreach(string[] value in values)
            {
                Dictionary<string, string> set = new Dictionary<string, string>();
                for (int i = 0; i < headers.Count(); i++)
                {
                    try
                    {
                        set.Add(headers[i], value[i]);
                    }catch(Exception exe)
                    { }
                }
                typeData.Add(set);
            }
            return typeData;
        }
    }
}
