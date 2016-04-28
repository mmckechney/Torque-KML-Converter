﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
namespace Kml.Converter
{
    class Program
    {
        private static Generator gen;
        private static CsvParser csv;
        static void Main(string[] args)
        {
            gen = new Generator();
            csv = new CsvParser();

            if (args.Length == 1)
            {
                string csvFileName = args[0];
                Process(csvFileName);
            }

            if(args.Length == 2)
            {
                if(args[0].Trim().ToLower() == "/dir")
                {
                    var files = Directory.GetFiles(args[1], "*.csv");
                    foreach(var file in files)
                    {
                        Process(file);
                    }
                }

                if (args[0].Trim().ToLower() == "/merge")
                {
                    var files = Directory.GetFiles(args[1], "*.csv");
                    var mergedFileData = FileMerging.MergeFiles(files);
                    Process(mergedFileData, args[1]);
                    Console.WriteLine("Processing complete. Press any key...");
                    Console.ReadLine();
                }

            }
            
        }


        private static void Process(string csvFileName)
        {
            var data = csv.ParseFile(csvFileName);
            string kmlFileName;
            XmlDocument kml = gen.GenerateKmlFile(data, out kmlFileName);

            string filename = Path.GetDirectoryName(csvFileName) + "\\" + kmlFileName + ".kml"; // Path.GetFileNameWithoutExtension(csvFileName) + ".kml";
            kml.Save(filename);
        }
        private static void Process(List<List<Dictionary<string, string>>> data, string folder)
        {
            if (!folder.EndsWith("\\")) folder = folder + "\\";
            string kmlFileName;
            foreach (var dataItem in data)
            {
                XmlDocument kml = gen.GenerateKmlFile(dataItem, out kmlFileName);

                string filename = folder + kmlFileName + ".kml"; // Path.GetFileNameWithoutExtension(csvFileName) + ".kml";
                kml.Save(filename);
            }
        }
    }
}
