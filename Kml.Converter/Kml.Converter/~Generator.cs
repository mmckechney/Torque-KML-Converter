using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
namespace Kml.Converter
{
    class Generator
    {
        private static decimal lastHeading = 0.0M;
        private static decimal lastInvertedHeading = 0.0M;
        private static int pointCounter = 0;
        public Generator()
        {
        }

        internal XmlDocument GenerateKmlFile(List<Dictionary<string,string>> torqueData, out string kmlFileName)
        {
            XmlDocument kmlDoc = new XmlDocument();
        
            XmlElement kml = kmlDoc.CreateElement(KmlElementTypes.Kml);
            kml.SetAttribute("xmlns", KmlElementTypes.XmlNamespace);

            XmlElement doc = kmlDoc.CreateElement(KmlElementTypes.Document);

            XmlElement name = kmlDoc.CreateElement(KmlElementTypes.Name);
            name.InnerText = ExtractTripName(torqueData, out kmlFileName);

            kmlFileName = kmlFileName.Replace("/","-").Replace(":",".");
            doc.AppendChild(name);

            List<XmlElement> styles = CreateStyles(ref kmlDoc);
            foreach(var x in styles)
            {
                kml.AppendChild(x);
            }

            foreach (var point in torqueData)
            {
                XmlElement tmp = CreatePoint(point, ref kmlDoc);
                if(tmp != null)
                    doc.AppendChild(tmp);
            }
            
            kml.AppendChild(doc);
            kmlDoc.AppendChild(kml);

            lastHeading = 0.0M;
            pointCounter = 0;

            return kmlDoc;


        }
        private XmlElement CreatePoint(Dictionary<string, string> pointData, ref XmlDocument kmlDoc)
        {
            try
            {
                XmlElement placemark = kmlDoc.CreateElement(KmlElementTypes.Placemark);

                //set the name to the speed...
                XmlElement name = kmlDoc.CreateElement(KmlElementTypes.Name);
                double mph = Math.Round(double.Parse(pointData[TorqueKnownTypes.Speed]) * 2.23694, 1);
                string mphLabel = mph.ToString() + " mph";
                pointData.Add("MPH", mph.ToString());

                if (pointCounter % 10 == 0)
                {
                    name.InnerText = DateTime.Parse(pointData[TorqueKnownTypes.DeviceTime]).ToString("hh:mm:ss tt");
                }

                //Set the heading (bearing)
                XmlElement heading = kmlDoc.CreateElement(KmlElementTypes.Heading);
                if (mph > 2)
                {
                    lastHeading = decimal.Parse(pointData[TorqueKnownTypes.Bearing]);
                    lastInvertedHeading = InverseBearing(lastHeading);
                    heading.InnerText = lastInvertedHeading.ToString();
                }
                else
                {
                    heading.InnerText = lastInvertedHeading.ToString();
                    pointData[TorqueKnownTypes.Bearing] = lastHeading.ToString();
                }

                //Set Coordinates
                XmlElement coord = kmlDoc.CreateElement(KmlElementTypes.Coordinates);
                coord.InnerText = String.Format("{0},{1}", pointData[TorqueKnownTypes.Longitude], pointData[TorqueKnownTypes.Latitude]);

                //XmlElement icon = kmlDoc.CreateElement(KmlElementTypes.Icon);
                //icon.InnerText = KmlElementTypes.ArrowIcon;

                XmlElement styleUrl = kmlDoc.CreateElement(KmlElementTypes.StyleUrl);
                if (mph == 0)
                    styleUrl.SetAttribute("id", SpeedStyles.Stopped);
                else if (mph > 0 && mph < 10)
                    styleUrl.SetAttribute("id", SpeedStyles.Crawl);
                else if (mph >= 10 && mph < 30)
                    styleUrl.SetAttribute("id", SpeedStyles.Slow);
                else if (mph >= 30 && mph < 45)
                    styleUrl.SetAttribute("id", SpeedStyles.Medium);
                else if (mph >= 45 && mph < 55)
                    styleUrl.SetAttribute("id", SpeedStyles.Fast);
                else if (mph >= 55)
                    styleUrl.SetAttribute("id", SpeedStyles.Highway);
                

                //XmlElement color = kmlDoc.CreateElement(KmlElementTypes.Color);
                //if (mph == 0)
                //    style.InnerText = SpeedColors.Stopped;
                //else if (mph > 0 && mph < 10)
                //    color.InnerText = SpeedColors.Crawl;
                //else if (mph >= 10 && mph < 30)
                //    color.InnerText = SpeedColors.Slow;
                //else if (mph >=30 && mph < 45)
                //    color.InnerText = SpeedColors.Medium;
                //else if (mph >= 45 && mph < 55)
                //    color.InnerText = SpeedColors.Fast;
                //else if (mph >= 55)
                //    color.InnerText = SpeedColors.Highway;
                //else 
                //    color.InnerText = "cc00ff00";

                //XmlElement labelStyle= kmlDoc.CreateElement("LabelStyle");
                //XmlElement labelScale = kmlDoc.CreateElement("scale");
                //labelScale.InnerText = ".75";
                //labelStyle.AppendChild(labelScale);

                XmlElement description = kmlDoc.CreateElement(KmlElementTypes.Description);

                string supportingData = CreateSupportingData(pointData);
                XmlCDataSection cData = kmlDoc.CreateCDataSection(supportingData);
                description.AppendChild(cData);

                
                XmlElement iconStyle = kmlDoc.CreateElement(KmlElementTypes.IconStyle);
                XmlElement point = kmlDoc.CreateElement(KmlElementTypes.Point);

                //Put it all together...
                
                //iconStyle.AppendChild(icon);
                //iconStyle.AppendChild(color);
                iconStyle.AppendChild(heading);

                XmlElement style = kmlDoc.CreateElement(KmlElementTypes.Style);
                style.AppendChild(iconStyle);
                //style.AppendChild(labelStyle);
                point.AppendChild(coord);

                placemark.AppendChild(name);
                placemark.AppendChild(style);
                placemark.AppendChild(styleUrl);
               
                placemark.AppendChild(description);
                placemark.AppendChild(point);

                pointCounter++;
                return placemark;
            }
            catch (Exception exe)
            {
                return null;
            }

        }

        private string CreateSupportingData(Dictionary<string, string> pointData)
        {
            List<string> knownTypes = TorqueKnownTypes.GetTorqueKnownTypeStrings();
            StringBuilder sb = new StringBuilder();

            //var support = from p in pointData
            //              where !knownTypes.Contains(p.Key)
            //              select p;
            var support = pointData;

            var enumer = support.GetEnumerator();
            while(enumer.MoveNext())
            {
                sb.AppendFormat("<b>{0}</b>:&nbsp;{1}<br/>", enumer.Current.Key, enumer.Current.Value);
            }
            return sb.ToString();
        }

        private string ExtractTripName(List<Dictionary<string, string>> torqueData, out string fileName)
        {
            DateTime start;
            DateTime end;

            start = DateTime.Parse(torqueData[0][TorqueKnownTypes.DeviceTime]);
            end = DateTime.Parse(torqueData[torqueData.Count - 1][TorqueKnownTypes.DeviceTime]);

            fileName = string.Format("{0}-{1}-{2} {3} {4} to {5}", start.Year, start.Month, start.Day, start.DayOfWeek, start.ToShortTimeString(), end.ToShortTimeString());
            return string.Format("{0} {1} {2} to {3}", start.DayOfWeek, start.ToShortDateString(), start.ToShortTimeString(), end.ToShortTimeString());

        }

        private List<XmlElement> CreateStyles(ref XmlDocument kmlDoc)
        {
            string labelScale = ".75";

            List<XmlElement> lstStyles = new List<XmlElement>();

            //Stopped Style
            XmlElement stoppedStyle = kmlDoc.CreateElement(KmlElementTypes.Style);
            stoppedStyle.SetAttribute("id", SpeedStyles.Slow);
            XmlElement stoppedScale = kmlDoc.CreateElement(KmlElementTypes.Scale);
            stoppedScale.InnerText = labelScale;
            XmlElement stoppedColor = kmlDoc.CreateElement(KmlElementTypes.Color);
            stoppedColor.InnerText = SpeedColors.Stopped;
            XmlElement stoppedIcon = kmlDoc.CreateElement(KmlElementTypes.Icon);
            stoppedIcon.InnerText = KmlElementTypes.ArrowIcon;
            XmlElement stoppedIconStyle = kmlDoc.CreateElement(KmlElementTypes.IconStyle);
            XmlElement stoppedLabelStyle = kmlDoc.CreateElement(KmlElementTypes.LabelStyle);
            stoppedIconStyle.AppendChild(stoppedIcon);
            stoppedIconStyle.AppendChild(stoppedColor);
            stoppedLabelStyle.AppendChild(stoppedScale);
            stoppedStyle.AppendChild(stoppedIconStyle);
            stoppedStyle.AppendChild(stoppedLabelStyle);
            lstStyles.Add(stoppedStyle);

            //Crawl Style
            XmlElement crawlStyle = kmlDoc.CreateElement(KmlElementTypes.Style);
            crawlStyle.SetAttribute("id", SpeedStyles.Crawl);
            XmlElement crawlScale = kmlDoc.CreateElement(KmlElementTypes.Scale);
            crawlScale.InnerText = labelScale;
            XmlElement crawlColor = kmlDoc.CreateElement(KmlElementTypes.Color);
            crawlColor.InnerText = SpeedColors.Crawl;
            XmlElement crawlIcon = kmlDoc.CreateElement(KmlElementTypes.Icon);
            crawlIcon.InnerText = KmlElementTypes.ArrowIcon;
            XmlElement crawlIconStyle = kmlDoc.CreateElement(KmlElementTypes.IconStyle);
            XmlElement crawlLabelStyle = kmlDoc.CreateElement(KmlElementTypes.LabelStyle);
            crawlIconStyle.AppendChild(crawlIcon);
            crawlIconStyle.AppendChild(crawlColor);
            crawlLabelStyle.AppendChild(crawlScale);
            crawlStyle.AppendChild(crawlIconStyle);
            crawlStyle.AppendChild(crawlLabelStyle);
            lstStyles.Add(crawlStyle);

            //Slow Style
            XmlElement slowStyle = kmlDoc.CreateElement(KmlElementTypes.Style);
            slowStyle.SetAttribute("id", SpeedStyles.Slow);
            XmlElement slowScale = kmlDoc.CreateElement(KmlElementTypes.Scale);
            slowScale.InnerText = labelScale;
            XmlElement slowColor = kmlDoc.CreateElement(KmlElementTypes.Color);
            slowColor.InnerText = SpeedColors.Slow;
            XmlElement slowIcon = kmlDoc.CreateElement(KmlElementTypes.Icon);
            slowIcon.InnerText = KmlElementTypes.ArrowIcon;
            XmlElement slowIconStyle = kmlDoc.CreateElement(KmlElementTypes.IconStyle);
            XmlElement slowLabelStyle = kmlDoc.CreateElement(KmlElementTypes.LabelStyle);
            slowIconStyle.AppendChild(slowIcon);
            slowIconStyle.AppendChild(slowColor);
            slowLabelStyle.AppendChild(slowScale);
            slowStyle.AppendChild(slowIconStyle);
            slowStyle.AppendChild(slowLabelStyle);
            lstStyles.Add(slowStyle);

            //Medium Style
            XmlElement mediumStyle = kmlDoc.CreateElement(KmlElementTypes.Style);
            mediumStyle.SetAttribute("id", SpeedStyles.Medium);
            XmlElement mediumScale = kmlDoc.CreateElement(KmlElementTypes.Scale);
            mediumScale.InnerText = labelScale;
            XmlElement mediumColor = kmlDoc.CreateElement(KmlElementTypes.Color);
            mediumColor.InnerText = SpeedColors.Medium;
            XmlElement mediumIcon = kmlDoc.CreateElement(KmlElementTypes.Icon);
            mediumIcon.InnerText = KmlElementTypes.ArrowIcon;
            XmlElement mediumIconStyle = kmlDoc.CreateElement(KmlElementTypes.IconStyle);
            XmlElement mediumLabelStyle = kmlDoc.CreateElement(KmlElementTypes.LabelStyle);
            mediumIconStyle.AppendChild(mediumIcon);
            mediumIconStyle.AppendChild(mediumColor);
            mediumLabelStyle.AppendChild(mediumScale);
            mediumStyle.AppendChild(mediumIconStyle);
            mediumStyle.AppendChild(mediumLabelStyle);
            lstStyles.Add(mediumStyle);

            //Fast Style
            XmlElement fastStyle = kmlDoc.CreateElement(KmlElementTypes.Style);
            fastStyle.SetAttribute("id", SpeedStyles.Fast);
            XmlElement fastScale = kmlDoc.CreateElement(KmlElementTypes.Scale);
            fastScale.InnerText = labelScale;
            XmlElement fastColor = kmlDoc.CreateElement(KmlElementTypes.Color);
            fastColor.InnerText = SpeedColors.Fast;
            XmlElement fastIcon = kmlDoc.CreateElement(KmlElementTypes.Icon);
            fastIcon.InnerText = KmlElementTypes.ArrowIcon;
            XmlElement fastIconStyle = kmlDoc.CreateElement(KmlElementTypes.IconStyle);
            XmlElement fastLabelStyle = kmlDoc.CreateElement(KmlElementTypes.LabelStyle);
            fastIconStyle.AppendChild(fastIcon);
            fastIconStyle.AppendChild(fastColor);
            fastLabelStyle.AppendChild(fastScale);
            fastStyle.AppendChild(fastIconStyle);
            fastStyle.AppendChild(fastLabelStyle);
            lstStyles.Add(fastStyle);

            //Highway Style
            XmlElement highwayStyle = kmlDoc.CreateElement(KmlElementTypes.Style);
            highwayStyle.SetAttribute("id", SpeedStyles.Highway);
            XmlElement highwayScale = kmlDoc.CreateElement(KmlElementTypes.Scale);
            highwayScale.InnerText = labelScale;
            XmlElement highwayColor = kmlDoc.CreateElement(KmlElementTypes.Color);
            highwayColor.InnerText = SpeedColors.Highway;
            XmlElement highwayIcon = kmlDoc.CreateElement(KmlElementTypes.Icon);
            highwayIcon.InnerText = KmlElementTypes.ArrowIcon;
            XmlElement highwayIconStyle = kmlDoc.CreateElement(KmlElementTypes.IconStyle);
            XmlElement highwayLabelStyle = kmlDoc.CreateElement(KmlElementTypes.LabelStyle);
            highwayIconStyle.AppendChild(highwayIcon);
            highwayIconStyle.AppendChild(highwayColor);
            highwayLabelStyle.AppendChild(highwayScale);
            highwayStyle.AppendChild(highwayIconStyle);
            highwayStyle.AppendChild(highwayLabelStyle);
            lstStyles.Add(highwayStyle);

            return lstStyles;
        }
        private decimal InverseBearing(decimal bearing)
        {
            bearing = Math.Round(bearing,0) + 180;
            return bearing < 360 ? bearing : bearing - 360;

        }

    }
}
