using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Xml;

namespace LogHelper
{
    public class EVTX_ExfilAnaylzer : EVTXAnaylzer
    {
        const string hexChars = "0123456789abcdefABCDEF";
        public override bool CanDoAnaylsis(string path)
        {
            return Path.GetExtension(path)==".evtx";
        }
        private bool potentialExfilDetected=false;
        private bool loggedPotentialEXFIL = false;
        public List<string> bytes = new List<string>();
        string hexFileName = "unknown.hex";
        string decodedFileName = "unknown";
        bool foundCertutil = false;
        string hexEncoded;

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

                    XmlNode execNode = data.ChildNodes[10];
                    string command = execNode.InnerText;
                    //TODO: look for certutil.
                    if(givenCommand== "C:\\Windows\\System32\\certutil.exe")
                    {
                        if (!command.Contains("encodehex")) continue;
                        foundCertutil = true;
                        Logger.Log("Found certutil encode hex execution. ");
                        string[] cmd=command.Split(' ');
                        hexFileName = cmd[3];
                        decodedFileName = cmd[2].Replace("/","");
                        hexEncoded = cmd[4];
                    }

                    if (givenCommand == "C:\\Windows\\System32\\nslookup.exe")
                    {

                        string line = command.Replace("\"C:\\Windows\\system32\\nslookup.exe\" ", "").Replace(".cityinthe.cloud 10.0.2.5", "");

                        //TODO: if we see these are hex codes and not normal subdomains, flag potential exfil.
                        //Then add suspect bytes to list.
                        if (line.All(hexChars.Contains))
                        {
                            Logger.Log("Detected potential dns exfil.", condition: !loggedPotentialEXFIL);
                            loggedPotentialEXFIL = true;
                            bytes.Add(line);
                            potentialExfilDetected = true;
                        }
                       
                    };
                }
            }
            if (potentialExfilDetected)
            {
                string path = Program.LogHelperPath;
                string filePath = Path.Combine(path, hexFileName);
                filePath = filePath.Replace("\\", "/");
                StreamWriter writer = new StreamWriter(filePath);
                Logger.Log($"Wrote hex values to {filePath}");
                foreach (string b in bytes)
                {
                    writer.WriteLine(b);
                }
                writer.Close();
                if(foundCertutil)
                {
                    try
                    {
                        Process process = new Process();
                        ProcessStartInfo info = new ProcessStartInfo(Environment.ExpandEnvironmentVariables("%SystemRoot%") + @"\System32\certutil.exe", $"/C -decodehex {filePath} {Path.Combine(path, decodedFileName)} {hexEncoded}");
                        info.Verb = "runas";
                        info.CreateNoWindow = true;
                        info.UseShellExecute = true;
                        process.StartInfo = info;
                        

                        process.Start();
                    }catch(Exception ex)
                    {
                        Logger.Log($"Certutil failed: {ex}");
                        Logger.Log($"Please try this command manually: certutil -decodehex {filePath} {Path.Combine(path, decodedFileName)} {hexEncoded}");
                    }
                }
            }
        }

        public override string Name()
        {
            return "EVTX_EXFIL_ANAYLSIS";
        }
    }
}
