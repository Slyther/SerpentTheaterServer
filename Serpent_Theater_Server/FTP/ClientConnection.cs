using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using log4net;

namespace Serpent_Theater_Server.FTP
{
    public class ClientConnection : IDisposable
    {
        private class DataConnectionOperation
        {
            public Func<NetworkStream, string, string> Operation { get; set; }
            public string Arguments { get; set; }
        }

        readonly ILog _log = LogManager.GetLogger(typeof(ClientConnection));

        #region Copy Stream Implementations

        private static long CopyStream(Stream input, Stream output, int bufferSize)
        {
            var buffer = new byte[bufferSize];
            int count;
            long total = 0;

            while ((count = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, count);
                total += count;
            }

            return total;
        }

        private static long CopyStreamAscii(Stream input, Stream output, int bufferSize)
        {
            var buffer = new char[bufferSize];
            long total = 0;

            using (var rdr = new StreamReader(input, Encoding.ASCII))
            {
                using (var wtr = new StreamWriter(output, Encoding.ASCII))
                {
                    int count;
                    while ((count = rdr.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        wtr.Write(buffer, 0, count);
                        total += count;
                    }
                }
            }
            return total;
        }

        private long CopyStream(Stream input, Stream output)
        {
            var limitedStream = output; // new RateLimitingStream(output, 131072, 0.5);

            return _connectionType == TransferType.Image ? CopyStream(input, limitedStream, 4096) : CopyStreamAscii(input, limitedStream, 4096);
        }

        #endregion

        #region Enums

        private enum TransferType
        {
            Ascii,
            Ebcdic,
            Image,
            Local,
        }

        private enum FormatControlType
        {
            NonPrint,
            Telnet,
            CarriageControl,
        }

        private enum DataConnectionType
        {
            Passive,
            Active,
        }

        private enum FileStructureType
        {
            File,
            Record,
            Page,
        }

        #endregion

        private bool _disposed;

        private TcpListener _passiveListener;

        private readonly TcpClient _controlClient;
        private TcpClient _dataClient;

        private NetworkStream _controlStream;
        private StreamReader _controlReader;
        private StreamWriter _controlWriter;

        private TransferType _connectionType = TransferType.Ascii;
        private FormatControlType _formatControlType = FormatControlType.NonPrint;
        private DataConnectionType _dataConnectionType = DataConnectionType.Active;
        private FileStructureType _fileStructureType = FileStructureType.File;

        private string _username;
        private string _root;
        private string _currentDirectory;
        private IPEndPoint _dataEndpoint;
        private IPEndPoint _remoteEndPoint;

        private X509Certificate _cert;
        private SslStream _sslStream;

        private string _clientIp;

        private User _currentUser;

        private readonly List<string> _validCommands;

        public ClientConnection(TcpClient client)
        {
            _controlClient = client;

            _validCommands = new List<string>();
        }

        private string CheckUser()
        {
            return _currentUser == null ? "530 Not logged in" : null;
        }

        public void HandleClient(object obj)
        {
            _remoteEndPoint = (IPEndPoint)_controlClient.Client.RemoteEndPoint;

            _clientIp = _remoteEndPoint.Address.ToString();

            _controlStream = _controlClient.GetStream();

            _controlReader = new StreamReader(_controlStream);
            _controlWriter = new StreamWriter(_controlStream);

            _controlWriter.WriteLine("220 Service Ready.");
            _controlWriter.Flush();

            _validCommands.AddRange(new[] { "AUTH", "USER", "PASS", "QUIT", "HELP", "NOOP" });

            _dataClient = new TcpClient();

            string renameFrom = null;

            try
            {
                string line;
                while ((line = _controlReader.ReadLine()) != null)
                {
                    string response = null;

                    var command = line.Split(' ');

                    var cmd = command[0].ToUpperInvariant();
                    var arguments = command.Length > 1 ? line.Substring(command[0].Length + 1) : null;

                    if (arguments != null && arguments.Trim().Length == 0)
                    {
                        arguments = null;
                    }

                    var logEntry = new LogEntry
                    {
                        Date = DateTime.Now,
                        Cip = _clientIp,
                        CsUriStem = arguments
                    };

                    if (!_validCommands.Contains(cmd))
                    {
                        response = CheckUser();
                    }

                    if (cmd != "RNTO")
                    {
                        renameFrom = null;
                    }

                    if (response == null)
                    {
                        switch (cmd)
                        {
                            case "USER":
                                response = User(arguments);
                                break;
                            case "PASS":
                                response = Password(arguments);
                                logEntry.CsUriStem = "******";
                                break;
                            case "CWD":
                                response = ChangeWorkingDirectory(arguments);
                                break;
                            case "CDUP":
                                response = ChangeWorkingDirectory("..");
                                break;
                            case "QUIT":
                                response = "221 Service closing control connection";
                                break;
                            case "REIN":
                                _currentUser = null;
                                _username = null;
                                _passiveListener = null;
                                _dataClient = null;

                                response = "220 Service ready for new user";
                                break;
                            case "PORT":
                                response = Port(arguments);
                                logEntry.CPort = _dataEndpoint.Port.ToString(CultureInfo.InvariantCulture);
                                break;
                            case "PASV":
                                response = Passive();
                                logEntry.SPort = ((IPEndPoint)_passiveListener.LocalEndpoint).Port.ToString(CultureInfo.InvariantCulture);
                                break;
                            case "TYPE":
                                response = Type(command[1], command.Length == 3 ? command[2] : null);
                                logEntry.CsUriStem = command[1];
                                break;
                            case "STRU":
                                response = Structure(arguments);
                                break;
                            case "MODE":
                                response = Mode(arguments);
                                break;
                            case "RNFR":
                                renameFrom = arguments;
                                response = "350 Requested file action pending further information";
                                break;
                            case "RNTO":
                                response = Rename(renameFrom, arguments);
                                break;
                            case "DELE":
                                response = Delete(arguments);
                                break;
                            case "RMD":
                                response = RemoveDir(arguments);
                                break;
                            case "MKD":
                                response = CreateDir(arguments);
                                break;
                            case "PWD":
                                response = PrintWorkingDirectory();
                                break;
                            case "RETR":
                                response = Retrieve(arguments);
                                logEntry.Date = DateTime.Now;
                                break;
                            case "STOR":
                                response = Store(arguments);
                                logEntry.Date = DateTime.Now;
                                break;
                            case "STOU":
                                response = StoreUnique();
                                logEntry.Date = DateTime.Now;
                                break;
                            case "APPE":
                                response = Append(arguments);
                                logEntry.Date = DateTime.Now;
                                break;
                            case "LIST":
                                response = List(arguments ?? _currentDirectory);
                                logEntry.Date = DateTime.Now;
                                break;
                            case "SYST":
                                response = "215 UNIX Type: L8";
                                break;
                            case "NOOP":
                                response = "200 OK";
                                break;
                            case "ACCT":
                                response = "200 OK";
                                break;
                            case "ALLO":
                                response = "200 OK";
                                break;
                            case "NLST":
                                response = "502 Command not implemented";
                                break;
                            case "SITE":
                                response = "502 Command not implemented";
                                break;
                            case "STAT":
                                response = "502 Command not implemented";
                                break;
                            case "HELP":
                                response = "502 Command not implemented";
                                break;
                            case "SMNT":
                                response = "502 Command not implemented";
                                break;
                            case "REST":
                                response = "502 Command not implemented";
                                break;
                            case "ABOR":
                                response = "502 Command not implemented";
                                break;

                            // Extensions defined by rfc 2228
                            case "AUTH":
                                response = Auth(arguments);
                                break;

                            // Extensions defined by rfc 2389
                            case "FEAT":
                                response = FeatureList();
                                break;
                            case "OPTS":
                                response = Options(arguments);
                                break;

                            // Extensions defined by rfc 3659
                            case "MDTM":
                                response = FileModificationTime(arguments);
                                break;
                            case "SIZE":
                                response = FileSize(arguments);
                                break;

                            // Extensions defined by rfc 2428
                            case "EPRT":
                                response = EPort(arguments);
                                logEntry.CPort = _dataEndpoint.Port.ToString(CultureInfo.InvariantCulture);
                                break;
                            case "EPSV":
                                response = EPassive();
                                logEntry.SPort = ((IPEndPoint)_passiveListener.LocalEndpoint).Port.ToString();
                                break;

                            default:
                                response = "502 Command not implemented";
                                break;
                        }
                    }

                    logEntry.CsMethod = cmd;
                    logEntry.CsUsername = _username;
                    logEntry.ScStatus = response.Substring(0, response.IndexOf(' '));

                    _log.Info(logEntry);

                    if (_controlClient == null || !_controlClient.Connected)
                    {
                        break;
                    }
                    _controlWriter.WriteLine(response);
                    _controlWriter.Flush();

                    if (response.StartsWith("221"))
                    {
                        break;
                    }

                    if (cmd != "AUTH") continue;
                    _cert = new X509Certificate("server.cer");

                    _sslStream = new SslStream(_controlStream);

                    _sslStream.AuthenticateAsServer(_cert);

                    _controlReader = new StreamReader(_sslStream);
                    _controlWriter = new StreamWriter(_sslStream);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
            }

            Dispose();
        }

        private bool IsPathValid(string path)
        {
            return path.StartsWith(_root);
        }

        private string NormalizeFilename(string path)
        {
            if (path == null)
            {
                path = string.Empty;
            }

            if (path == "/")
            {
                return _root;
            }
            path = path.StartsWith("/") ? new FileInfo(Path.Combine(_root, path.Substring(1))).FullName : new FileInfo(Path.Combine(_currentDirectory, path)).FullName;

            return IsPathValid(path) ? path : null;
        }

        #region FTP Commands

        private string FeatureList()
        {
            _controlWriter.WriteLine("211- Extensions supported:");
            _controlWriter.WriteLine(" MDTM");
            _controlWriter.WriteLine(" SIZE");
            return "211 End";
        }

        private string Options(string arguments)
        {
            return "200 Looks good to me...";
        }

        private string Auth(string authMode)
        {
            return authMode == "TLS" ? "234 Enabling TLS Connection" : "504 Unrecognized AUTH mode";
        }

        private string User(string username)
        {
            _username = username;

            return "331 Username ok, need password";
        }

        private string Password(string password)
        {
            _currentUser = UserStore.Validate(_username, password);

            if (_currentUser == null) return "530 Not logged in";
            _root = _currentUser.HomeDir;
            _currentDirectory = _root;

            return "230 User logged in";
        }

        private string ChangeWorkingDirectory(string pathname)
        {
            if (pathname == "/")
            {
                _currentDirectory = _root;
            }
            else
            {
                string newDir;

                if (pathname.StartsWith("/"))
                {
                    pathname = pathname.Substring(1).Replace('/', '\\');
                    newDir = Path.Combine(_root, pathname);
                }
                else
                {
                    pathname = pathname.Replace('/', '\\');
                    newDir = Path.Combine(_currentDirectory, pathname);
                }

                if (Directory.Exists(newDir))
                {
                    _currentDirectory = new DirectoryInfo(newDir).FullName;

                    if (!IsPathValid(_currentDirectory))
                    {
                        _currentDirectory = _root;
                    }
                }
                else
                {
                    _currentDirectory = _root;
                }
            }

            return "250 Changed to new directory";
        }

        private string Port(string hostPort)
        {
            _dataConnectionType = DataConnectionType.Active;

            var ipAndPort = hostPort.Split(',');

            var ipAddress = new byte[4];
            var port = new byte[2];

            for (var i = 0; i < 4; i++)
            {
                ipAddress[i] = Convert.ToByte(ipAndPort[i]);
            }

            for (var i = 4; i < 6; i++)
            {
                port[i - 4] = Convert.ToByte(ipAndPort[i]);
            }

            if (BitConverter.IsLittleEndian)
                Array.Reverse(port);

            _dataEndpoint = new IPEndPoint(new IPAddress(ipAddress), BitConverter.ToInt16(port, 0));

            return "200 Data Connection Established";
        }

        private string EPort(string hostPort)
        {
            _dataConnectionType = DataConnectionType.Active;

            var delimiter = hostPort[0];

            var rawSplit = hostPort.Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);

            var ipType = rawSplit[0][0];

            var ipAddress = rawSplit[1];
            var port = rawSplit[2];

            _dataEndpoint = new IPEndPoint(IPAddress.Parse(ipAddress), int.Parse(port));

            return "200 Data Connection Established";
        }

        private string Passive()
        {
            _dataConnectionType = DataConnectionType.Passive;

            var localIp = ((IPEndPoint)_controlClient.Client.LocalEndPoint).Address;

            _passiveListener = new TcpListener(localIp, 0);
            _passiveListener.Start();

            var passiveListenerEndpoint = (IPEndPoint)_passiveListener.LocalEndpoint;

            var address = passiveListenerEndpoint.Address.GetAddressBytes();
            var port = (short)passiveListenerEndpoint.Port;

            var portArray = BitConverter.GetBytes(port);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(portArray);

            return string.Format("227 Entering Passive Mode ({0},{1},{2},{3},{4},{5})", address[0], address[1], address[2], address[3], portArray[0], portArray[1]);
        }

        private string EPassive()
        {
            _dataConnectionType = DataConnectionType.Passive;

            var localIp = ((IPEndPoint)_controlClient.Client.LocalEndPoint).Address;

            _passiveListener = new TcpListener(localIp, 0);
            _passiveListener.Start();

            var passiveListenerEndpoint = (IPEndPoint)_passiveListener.LocalEndpoint;

            return string.Format("229 Entering Extended Passive Mode (|||{0}|)", passiveListenerEndpoint.Port);
        }

        private string Type(string typeCode, string formatControl)
        {
            switch (typeCode.ToUpperInvariant())
            {
                case "A":
                    _connectionType = TransferType.Ascii;
                    break;
                case "I":
                    _connectionType = TransferType.Image;
                    break;
                default:
                    return "504 Command not implemented for that parameter";
            }

            if (!string.IsNullOrWhiteSpace(formatControl))
            {
                switch (formatControl.ToUpperInvariant())
                {
                    case "N":
                        _formatControlType = FormatControlType.NonPrint;
                        break;
                    default:
                        return "504 Command not implemented for that parameter";
                }
            }

            return string.Format("200 Type set to {0}", _connectionType);
        }

        private string Delete(string pathname)
        {
            pathname = NormalizeFilename(pathname);

            if (pathname == null) return "550 File Not Found";
            if (File.Exists(pathname))
            {
                File.Delete(pathname);
            }
            else
            {
                return "550 File Not Found";
            }

            return "250 Requested file action okay, completed";
        }

        private string RemoveDir(string pathname)
        {
            pathname = NormalizeFilename(pathname);

            if (pathname == null) return "550 Directory Not Found";
            if (Directory.Exists(pathname))
            {
                Directory.Delete(pathname);
            }
            else
            {
                return "550 Directory Not Found";
            }

            return "250 Requested file action okay, completed";
        }

        private string CreateDir(string pathname)
        {
            pathname = NormalizeFilename(pathname);

            if (pathname == null) return "550 Directory Not Found";
            if (!Directory.Exists(pathname))
            {
                Directory.CreateDirectory(pathname);
            }
            else
            {
                return "550 Directory already exists";
            }

            return "250 Requested file action okay, completed";
        }

        private string FileModificationTime(string pathname)
        {
            pathname = NormalizeFilename(pathname);

            if (pathname == null) return "550 File Not Found";
            return File.Exists(pathname) ? string.Format("213 {0}", File.GetLastWriteTime(pathname).ToString("yyyyMMddHHmmss.fff")) : "550 File Not Found";
        }

        private string FileSize(string pathname)
        {
            pathname = NormalizeFilename(pathname);

            if (pathname == null) return "550 File Not Found";
            if (!File.Exists(pathname)) return "550 File Not Found";
            long length;

            using (var fs = File.Open(pathname, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                length = fs.Length;
            }

            return string.Format("213 {0}", length);
        }

        private string Retrieve(string pathname)
        {
            pathname = NormalizeFilename(pathname);

            if (pathname == null) return "550 File Not Found";
            if (!File.Exists(pathname)) return "550 File Not Found";
            var state = new DataConnectionOperation { Arguments = pathname, Operation = RetrieveOperation };

            SetupDataConnectionOperation(state);

            return string.Format("150 Opening {0} mode data transfer for RETR", _dataConnectionType);
        }

        private string Store(string pathname)
        {
            pathname = NormalizeFilename(pathname);

            if (pathname == null) return "450 Requested file action not taken";
            var state = new DataConnectionOperation { Arguments = pathname, Operation = StoreOperation };

            SetupDataConnectionOperation(state);

            return string.Format("150 Opening {0} mode data transfer for STOR", _dataConnectionType);
        }

        private string Append(string pathname)
        {
            pathname = NormalizeFilename(pathname);

            if (pathname == null) return "450 Requested file action not taken";
            var state = new DataConnectionOperation { Arguments = pathname, Operation = AppendOperation };

            SetupDataConnectionOperation(state);

            return string.Format("150 Opening {0} mode data transfer for APPE", _dataConnectionType);
        }

        private string StoreUnique()
        {
            var pathname = NormalizeFilename(new Guid().ToString());

            var state = new DataConnectionOperation { Arguments = pathname, Operation = StoreOperation };

            SetupDataConnectionOperation(state);

            return string.Format("150 Opening {0} mode data transfer for STOU", _dataConnectionType);
        }

        private string PrintWorkingDirectory()
        {
            var current = _currentDirectory.Replace(_root, string.Empty).Replace('\\', '/');

            if (current.Length == 0)
            {
                current = "/";
            }

            return string.Format("257 \"{0}\" is current directory.", current);
        }

        private string List(string pathname)
        {
            pathname = NormalizeFilename(pathname);

            if (pathname == null) return "450 Requested file action not taken";
            var state = new DataConnectionOperation { Arguments = pathname, Operation = ListOperation };

            SetupDataConnectionOperation(state);

            return string.Format("150 Opening {0} mode data transfer for LIST", _dataConnectionType);
        }

        private string Structure(string structure)
        {
            switch (structure)
            {
                case "F":
                    _fileStructureType = FileStructureType.File;
                    break;
                case "R":
                case "P":
                    return string.Format("504 STRU not implemented for \"{0}\"", structure);
                default:
                    return string.Format("501 Parameter {0} not recognized", structure);
            }

            return "200 Command OK";
        }

        private string Mode(string mode)
        {
            return mode.ToUpperInvariant() == "S" ? "200 OK" : "504 Command not implemented for that parameter";
        }

        private string Rename(string renameFrom, string renameTo)
        {
            if (string.IsNullOrWhiteSpace(renameFrom) || string.IsNullOrWhiteSpace(renameTo))
            {
                return "450 Requested file action not taken";
            }

            renameFrom = NormalizeFilename(renameFrom);
            renameTo = NormalizeFilename(renameTo);

            if (renameFrom == null || renameTo == null) return "450 Requested file action not taken";
            if (File.Exists(renameFrom))
            {
                File.Move(renameFrom, renameTo);
            }
            else if (Directory.Exists(renameFrom))
            {
                Directory.Move(renameFrom, renameTo);
            }
            else
            {
                return "450 Requested file action not taken";
            }

            return "250 Requested file action okay, completed";
        }

        #endregion

        #region DataConnection Operations

        private void HandleAsyncResult(IAsyncResult result)
        {
            if (_dataConnectionType == DataConnectionType.Active)
            {
                _dataClient.EndConnect(result);
            }
            else
            {
                _dataClient = _passiveListener.EndAcceptTcpClient(result);
            }
        }

        private void SetupDataConnectionOperation(DataConnectionOperation state)
        {
            if (_dataConnectionType == DataConnectionType.Active)
            {
                _dataClient = new TcpClient(_dataEndpoint.AddressFamily);
                _dataClient.BeginConnect(_dataEndpoint.Address, _dataEndpoint.Port, DoDataConnectionOperation, state);
            }
            else
            {
                _passiveListener.BeginAcceptTcpClient(DoDataConnectionOperation, state);
            }
        }

        private void DoDataConnectionOperation(IAsyncResult result)
        {
            HandleAsyncResult(result);

            var op = result.AsyncState as DataConnectionOperation;

            string response;

            using (var dataStream = _dataClient.GetStream())
            {
                response = op.Operation(dataStream, op.Arguments);
            }

            _dataClient.Close();
            _dataClient = null;

            _controlWriter.WriteLine(response);
            _controlWriter.Flush();
        }

        private string RetrieveOperation(NetworkStream dataStream, string pathname)
        {
            long bytes = 0;

            using (var fs = new FileStream(pathname, FileMode.Open, FileAccess.Read))
            {
                bytes = CopyStream(fs, dataStream);
            }

            return "226 Closing data connection, file transfer successful";
        }

        private string StoreOperation(NetworkStream dataStream, string pathname)
        {
            long bytes = 0;

            using (var fs = new FileStream(pathname, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 4096, FileOptions.SequentialScan))
            {
                bytes = CopyStream(dataStream, fs);
            }

            var logEntry = new LogEntry
            {
                Date = DateTime.Now,
                Cip = _clientIp,
                CsMethod = "STOR",
                CsUsername = _username,
                ScStatus = "226",
                CsBytes = bytes.ToString(CultureInfo.InvariantCulture)
            };

            _log.Info(logEntry);

            return "226 Closing data connection, file transfer successful";
        }

        private string AppendOperation(NetworkStream dataStream, string pathname)
        {
            long bytes = 0;

            using (var fs = new FileStream(pathname, FileMode.Append, FileAccess.Write, FileShare.None, 4096, FileOptions.SequentialScan))
            {
                bytes = CopyStream(dataStream, fs);
            }

            var logEntry = new LogEntry
            {
                Date = DateTime.Now,
                Cip = _clientIp,
                CsMethod = "APPE",
                CsUsername = _username,
                ScStatus = "226",
                CsBytes = bytes.ToString(CultureInfo.InvariantCulture)
            };

            _log.Info(logEntry);

            return "226 Closing data connection, file transfer successful";
        }

        private string ListOperation(NetworkStream dataStream, string pathname)
        {
            var dataWriter = new StreamWriter(dataStream, Encoding.ASCII);

            var directories = Directory.EnumerateDirectories(pathname);

            foreach (var line in from dir in directories select new DirectoryInfo(dir) into d let date = d.LastWriteTime < DateTime.Now - TimeSpan.FromDays(180) ?
                d.LastWriteTime.ToString("MMM dd  yyyy") :
                d.LastWriteTime.ToString("MMM dd HH:mm") select string.Format("drwxr-xr-x    2 2003     2003     {0,8} {1} {2}", "4096", date, d.Name))
            {
                dataWriter.WriteLine(line);
                dataWriter.Flush();
            }

            var files = Directory.EnumerateFiles(pathname);

            foreach (var line in from file in files select new FileInfo(file) into f let date = f.LastWriteTime < DateTime.Now - TimeSpan.FromDays(180) ?
                f.LastWriteTime.ToString("MMM dd  yyyy") :
                f.LastWriteTime.ToString("MMM dd HH:mm") select string.Format("-rw-r--r--    2 2003     2003     {0,8} {1} {2}", f.Length, date, f.Name))
            {
                dataWriter.WriteLine(line);
                dataWriter.Flush();
            }

            var logEntry = new LogEntry
            {
                Date = DateTime.Now,
                Cip = _clientIp,
                CsMethod = "LIST",
                CsUsername = _username,
                ScStatus = "226"
            };
            _log.Info(logEntry);

            return "226 Transfer complete";
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_controlClient != null)
                    {
                        _controlClient.Close();
                    }
                    if (_dataClient != null)
                    {
                        _dataClient.Close();
                    }
                    if (_controlStream != null)
                    {
                        _controlStream.Close();
                    }
                    if (_controlReader != null)
                    {
                        _controlReader.Close();
                    }
                    if (_controlWriter != null)
                    {
                        _controlWriter.Close();
                    }
                }
            }
            _disposed = true;
        }
        #endregion
    }
}