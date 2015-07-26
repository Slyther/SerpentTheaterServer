using System;

namespace Serpent_Theater_Server.FTP
{
    // Fields: date time c-ip c-port cs-username cs-method cs-uri-stem sc-status sc-bytes cs-bytes s-name s-port

    internal class LogEntry
    {
        public DateTime Date { get; set; }
        public string Cip { get; set; }
        public string CPort { get; set; }
        public string CsUsername { get; set; }
        public string CsMethod { get; set; }
        public string CsUriStem { get; set; }
        public string ScStatus { get; set; }
        public string ScBytes { get; set; }
        public string CsBytes { get; set; }
        public string SName { get; set; }
        public string SPort { get; set; }

        public override string ToString()
        {
            return string.Format("{0:yyyy-MM-dd HH:mm:ss} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10}",
                Date,
                Cip,
                CPort ?? "-",
                CsUsername,
                CsMethod,
                CsUriStem ?? "-",
                ScStatus,
                ScBytes ?? "-",
                CsBytes ?? "-",
                SName ?? "-",
                SPort ?? "-"
                );
        }
    }
}
