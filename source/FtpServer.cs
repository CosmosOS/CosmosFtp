/*
* PROJECT:          Cosmos Operating System Development
* CONTENT:          FTP Server
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Cosmos.System.FileSystem;

namespace CosmosFtpServer
{
    /// <summary>
    /// FTP data transfer mode.
    /// </summary>
    public enum TransferMode
    {
        /// <summary>
        /// No mode set.
        /// </summary>
        NONE,

        /// <summary>
        /// Active mode.
        /// </summary>
        ACTV,

        /// <summary>
        /// Passive Mode.
        /// </summary>
        PASV
    }

    /// <summary>
    /// FTPCommand class.
    /// </summary>
    internal class FtpCommand
    {
        /// <summary>
        /// FTP Command Type.
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// FTP Command Content.
        /// </summary>
        public string Content { get; set; }
    }

    /// <summary>
    /// FtpServer class. Used to handle FTP client connections.
    /// </summary>
    public class FtpServer : IDisposable
    {
        /// <summary>
        /// Command Manager.
        /// </summary>
        internal FtpCommandManager CommandManager { get; set; }

        /// <summary>
        /// TCP Listener used to handle new FTP client connection.
        /// </summary>
        internal TcpListener tcpListener;

        /// <summary>
        /// Is FTP server listening for new FTP clients.
        /// </summary>
        internal bool Listening;

        /// <summary>
        /// Are debug logs enabled.
        /// </summary>
        internal bool Debug;

        /// <summary>
        /// Create new instance of the <see cref="FtpServer"/> class.
        /// </summary>
        /// <exception cref="Exception">Thrown if directory does not exists.</exception>
        /// <param name="fs">Initialized Cosmos Virtual Filesystem.</param>
        /// <param name="directory">FTP Server root directory path.</param>
        /// <param name="debug">Is debug logging enabled.</param>
        public FtpServer(CosmosVFS fs, string directory, bool debug = false)
        {
            if (Directory.Exists(directory) == false)
            {
                throw new Exception("FTP server can't open specified directory.");
            }

            IPAddress address = IPAddress.Any;
            tcpListener = new TcpListener(address, 21);

            CommandManager = new FtpCommandManager(fs, directory);

            Listening = true;
            Debug = debug;
        }

        /// <summary>
        /// Listen for new FTP clients.
        /// </summary>
        public void Listen()
        {
            tcpListener.Start();

            while (Listening)
            {
                TcpClient client = tcpListener.AcceptTcpClient();

                IPEndPoint endpoint = client.Client.RemoteEndPoint as IPEndPoint;

                Log("Client : New connection from " + endpoint.Address.ToString()); ;

                ReceiveNewClient(client);

                client.Close();
            }
        }

        /// <summary>
        /// Handle new FTP client.
        /// </summary>
        /// <param name="ftpClient">FTP Client.</param>
        private void ReceiveNewClient(TcpClient client)
        {
            var ftpClient = new FtpClient(client);

            ftpClient.SendReply(220, "Service ready for new user.");

            while (ftpClient.Control.Connected)
            {
                ReceiveRequest(ftpClient);
            }

            ftpClient.ControlStream.Close();

            //TODO: Support multiple FTP client connection
            Close();
        }

        /// <summary>
        /// Parse and execute FTP command.
        /// </summary>
        /// <param name="ftpClient">FTP Client.</param>
        private void ReceiveRequest(FtpClient ftpClient)
        {
            int bytesRead = 0;

            try
            {
                byte[] buffer = new byte[ftpClient.Control.ReceiveBufferSize];
                bytesRead = ftpClient.ControlStream.Read(buffer, 0, buffer.Length);

                string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                data = data.TrimEnd(new char[] { '\r', '\n' });

                string[] splitted = data.Split(' ');
                FtpCommand command = new FtpCommand
                {
                    Command = splitted[0],
                    Content = splitted.Length > 1 ? string.Join(" ", splitted.Skip(1)).Replace('/', '\\') : string.Empty
                };

                Log("Client : '" + command.Command + "'");
                CommandManager.ProcessRequest(ftpClient, command);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }
        }

        /// <summary>
        /// Write logs to console
        /// </summary>
        /// <param name="str">String to write.</param>
        private void Log(string str)
        {
            if (Debug)
            {
                Cosmos.System.Global.Debugger.Send(str);
            }
        }

        /// <summary>
        /// Close FTP server.
        /// </summary>
        public void Close()
        {
            Listening = false;
            tcpListener.Stop();
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Close();
        }
    }
}
