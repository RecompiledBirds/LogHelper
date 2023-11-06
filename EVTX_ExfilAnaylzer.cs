using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LogHelper
{
    public class EVTX_ExfilAnaylzer : EVTXAnaylzer
    {
        public override bool CanDoAnaylsis(string path)
        {
            return Path.GetExtension(path)==".evtx";
        }
        private bool potentialExfilDetected=false;

        public List<string> bytes = new List<string>();

        public override void DoAnaylsis(FileStream stream)
        {
            EventLogReader reader = new EventLogReader(stream.Name,PathType.FilePath);

            EventRecord record;
            while ((record = reader.ReadEvent()) != null)
            {
                using (record)
                {
                    //this anaylzer is looking for a few specific process launches, like NSLookup and certutil.
                    //so we're only looking at event ID 1 events.
                    if (record.Id != 1) continue;
                    XmlDocument doc= new XmlDocument();
                    doc.LoadXml(record.ToXml());
                    XmlNode root = doc.DocumentElement;
                    XmlNode sys = root.ChildNodes[0];
                    XmlNode data = root.ChildNodes[1];
                    if (sys.ChildNodes[1].InnerText != "1") continue;
                    string givenCommand = data.ChildNodes[4].InnerText;

                    //TODO: look for certutil.


                    if (givenCommand == "C:\\Windows\\System32\\nslookup.exe")
                    {
                        if (data.ChildNodes.Count < 17) continue;

                        string bytes = data.ChildNodes[10].InnerText.Replace("\"C:\\Windows\\system32\\nslookup.exe\" ", "").Replace(".cityinthe.cloud 10.0.2.5", "");
                        //TODO: if we see these are hex codes and not normal subdomains, flag potential exfil.
                            //Then add suspect bytes to list.
                    };
                    
                    
                   
                    
                    // Console.WriteLine(bytes);
                    //writer.WriteLine(bytes);
                    //lines.Add(bytes);
                    // string givenCommand = root.ChildNodes[4].InnerText;
                    //
                    // if (givenCommand != "C:\\Windows\\System32\\nslookup.exe") continue;
                    // Console.WriteLine(givenCommand);
                    //TODO: Filter out system as a user- this will reduce processing time.
                    // Console.WriteLine("{0} {1}: {2}", record.TimeCreated, record.LevelDisplayName, record.FormatDescription());
                }
            }
        }

        public override string Name()
        {
            return "EVTX_EXFIL_ANAYLSIS";
        }
    }
}
