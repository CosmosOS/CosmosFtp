﻿/*
* PROJECT:          Cosmos Operating System Development
* CONTENT:          FTP Command Manager
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.Listing;

namespace CosmosFtpServer
{
    /// <summary>
    ///  FtpCommandManager class. Used to handle incoming FTP commands.
    /// </summary>
    internal class FtpCommandManager
    {
        /// <summary>
        /// Cosmos Virtual Filesystem.
        /// </summary>
        internal CosmosVFS FileSystem { get; set; }

        /// <summary>
        /// Base path.
        /// </summary>
        internal string BaseDirectory { get; set; }

        /// <summary>
        /// Current path.
        /// </summary>
        internal string CurrentDirectory { get; set; }

        /// <summary>
        /// Create new instance of the <see cref="FtpCommandManager"/> class.
        /// </summary>
        /// <param name="fs">Cosmos Virtual Filesystem.</param>
        /// <param name="directory">Base directory used by the FTP server.</param>
        internal FtpCommandManager(CosmosVFS fs, string directory)
        {
            FileSystem = fs;
            CurrentDirectory = directory;
            BaseDirectory = directory;
        }

        /// <summary>
        /// Process incoming FTP command.
        /// </summary>
        /// <param name="ftpClient">FTP Client.</param>
        /// <param name="command">FTP Command.</param>
        internal void ProcessRequest(FtpClient ftpClient, FtpCommand command)
        {
            if (command.Command == "USER")
            {
                ProcessUser(ftpClient, command);
            }
            else if (command.Command == "PASS")
            {
                ProcessPass(ftpClient, command);
            }
            else
            {
                if (ftpClient.IsConnected())
                {
                    switch (command.Command)
                    {
                        case "CWD":
                            ProcessCwd(ftpClient, command);
                            break;
                        case "SYST":
                            ftpClient.SendReply(215, "CosmosOS");
                            break;
                        case "CDUP":
                            ProcessCdup(ftpClient, command);
                            break;
                        case "QUIT":
                            ProcessQuit(ftpClient, command);
                            break;
                        case "DELE":
                            ProcessDele(ftpClient, command);
                            break;
                        case "PWD":
                            ProcessPwd(ftpClient, command);
                            break;
                        case "PASV":
                            ProcessPasv(ftpClient, command);
                            break;
                        case "PORT":
                            ProcessPort(ftpClient, command);
                            break;
                        case "HELP":
                            ftpClient.SendReply(200, "Help done.");
                            break;
                        case "NOOP":
                            ftpClient.SendReply(200, "Command okay.");
                            break;
                        case "RETR":
                            ProcessRetr(ftpClient, command);
                            break;
                        case "STOR":
                            ProcessStor(ftpClient, command);
                            break;
                        case "RMD":
                            ProcessRmd(ftpClient, command);
                            break;
                        case "MKD":
                            ProcessMkd(ftpClient, command);
                            break;
                        case "LIST":
                            ProcessList(ftpClient, command);
                            break;
                        case "TYPE":
                            ftpClient.SendReply(200, "Command okay.");
                            break;
                        default:
                            ftpClient.SendReply(500, "Unknown command.");
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Process USER command.
        /// </summary>
        /// <param name="ftpClient">FTP Client.</param>
        /// <param name="command">FTP Command.</param>
        internal void ProcessUser(FtpClient ftpClient, FtpCommand command)
        {
            if (string.IsNullOrEmpty(command.Content))
            {
                ftpClient.SendReply(501, "Syntax error in parameters or arguments.");
                return;
            }
            if (command.Content == "anonymous")
            {
                ftpClient.Username = command.Content;
                ftpClient.Connected = true;
                ftpClient.SendReply(230, "User logged in, proceed.");
            }
            else if (string.IsNullOrEmpty(ftpClient.Username))
            {
                ftpClient.Username = command.Content;
                ftpClient.SendReply(331, "User name okay, need password.");
            }
            else
            {
                ftpClient.SendReply(550, "Requested action not taken.");
            }
        }

        /// <summary>
        /// Process PASS command.
        /// </summary>
        /// <param name="ftpClient">FTP Client.</param>
        /// <param name="command">FTP Command.</param>
        internal void ProcessPass(FtpClient ftpClient, FtpCommand command)
        {
            if (string.IsNullOrEmpty(command.Content))
            {
                ftpClient.SendReply(501, "Syntax error in parameters or arguments.");
                return;
            }
            if (ftpClient.Username == "anonymous")
            {
                ftpClient.SendReply(530, "Login incorrect.");
            }
            else if (string.IsNullOrEmpty(ftpClient.Username))
            {
                ftpClient.SendReply(332, "Need account for login.");
            }
            else
            {
                ftpClient.Password = command.Content;
                ftpClient.Connected = true;
                ftpClient.SendReply(230, "User logged in, proceed.");
            }
        }

        /// <summary>
        /// Process CWD command.
        /// </summary>
        /// <param name="ftpClient">FTP Client.</param>
        /// <param name="command">FTP Command.</param>
        internal void ProcessCwd(FtpClient ftpClient, FtpCommand command)
        {
            if (string.IsNullOrEmpty(command.Content))
            {
                ftpClient.SendReply(501, "Syntax error in parameters or arguments.");
                return;
            }
            try
            {
                if (command.Content.StartsWith("\\"))
                {
                    //Client asking for a path
                    if (command.Content == "\\")
                    {
                        CurrentDirectory = BaseDirectory;
                    }
                    else
                    {
                        CurrentDirectory = BaseDirectory + command.Content;
                    }
                }
                else
                {
                    //Client asking for a folder in current directory
                    if (CurrentDirectory == BaseDirectory)
                    {
                        CurrentDirectory += command.Content;
                    }
                    else
                    {
                        CurrentDirectory += "\\" + command.Content;
                    }
                }

                if (Directory.Exists(CurrentDirectory))
                {
                    ftpClient.SendReply(250, "Requested file action okay.");
                }
                else
                {
                    ftpClient.SendReply(550, "Requested action not taken.");
                }
            }
            catch
            {
                ftpClient.SendReply(550, "Requested action not taken.");
            }
        }

        /// <summary>
        /// Process PWD command.
        /// </summary>
        /// <param name="ftpClient">FTP Client.</param>
        /// <param name="command">FTP Command.</param>
        internal void ProcessPwd(FtpClient ftpClient, FtpCommand command)
        {
            //Replace 0:/ by /Cosmos/ for FTP client
            int i = CurrentDirectory.IndexOf(":") + 2;
            var tmp = CurrentDirectory.Substring(i);

            if (tmp.Length == 0)
            {
                ftpClient.SendReply(257, "/ created.");
            }
            else
            {
                ftpClient.SendReply(257, "\"/" + tmp + "\" created.");
            }
        }

        /// <summary>
        /// Process PASV command.
        /// </summary>
        /// <param name="ftpClient">FTP Client.</param>
        /// <param name="command">FTP Command.</param>
        internal void ProcessPasv(FtpClient ftpClient, FtpCommand command)
        {
            ushort port = Cosmos.System.Network.IPv4.TCP.Tcp.GetDynamicPort();
            var address = ftpClient.Control.Client.LocalEndPoint.ToString();

            ftpClient.DataListener = new TcpListener(IPAddress.Any, port);
            ftpClient.DataListener.Start();

            ftpClient.SendReply(200, $"Entering Passive Mode ({address},{port / 256},{port % 256})");

            ftpClient.Mode = TransferMode.PASV;
        }

        /// <summary>
        /// Process PORT command.
        /// </summary>
        /// <param name="ftpClient">FTP Client.</param>
        /// <param name="command">FTP Command.</param>
        internal void ProcessPort(FtpClient ftpClient, FtpCommand command)
        {
            string[] splitted = command.Content.Split(',');
            byte[] array = new byte[] {
                (byte)int.Parse(splitted[0]), (byte)int.Parse(splitted[1]), (byte)int.Parse(splitted[2]), (byte)int.Parse(splitted[3])
            };
            IPAddress address = new IPAddress(array);

            ftpClient.Data = new TcpClient();

            ftpClient.Address = address;
            ftpClient.Port = int.Parse(splitted[4]) * 256 + int.Parse(splitted[5]);

            ftpClient.SendReply(200, "Entering Active Mode.");

            ftpClient.Mode = TransferMode.ACTV;
        }

        /// <summary>
        /// Process LIST command.
        /// </summary>
        /// <param name="ftpClient">FTP Client.</param>
        /// <param name="command">FTP Command.</param>
        internal void ProcessList(FtpClient ftpClient, FtpCommand command)
        {
            try
            {
                if (ftpClient.Mode == TransferMode.NONE)
                {
                    ftpClient.SendReply(425, "Can't open data connection.");
                }
                else if (ftpClient.Mode == TransferMode.ACTV)
                {
                    ftpClient.Data.Connect(ftpClient.Address, ftpClient.Port);
                    ftpClient.DataStream = ftpClient.Data.GetStream();

                    DoList(ftpClient, command);

                    return;
                }
                else if (ftpClient.Mode == TransferMode.PASV)
                {
                    ftpClient.Data = ftpClient.DataListener.AcceptTcpClient();
                    ftpClient.DataStream = ftpClient.Data.GetStream();

                    DoList(ftpClient, command);

                    ftpClient.DataListener.Stop();

                    return;
                }
            }
            catch
            {
                ftpClient.SendReply(425, "Can't open data connection.");
            }

            ftpClient.SendReply(425, "Can't open data connection.");
        }

        /// <summary>
        /// Make a file/directory listing and send it to FTP data connection.
        /// </summary>
        /// <param name="ftpClient">FTP Client.</param>
        /// <param name="command">FTP Command.</param>
        private void DoList(FtpClient ftpClient, FtpCommand command)
        {
            var directory_list = FileSystem.GetDirectoryListing(CurrentDirectory + "\\" + command.Content);

            var sb = new StringBuilder();
            foreach (var directoryEntry in directory_list)
            {
                if (directoryEntry.mEntryType == DirectoryEntryTypeEnum.Directory)
                {
                    sb.Append("d");
                }
                else
                {
                    sb.Append("-");
                }
                sb.Append("rwxrwxrwx 1 unknown unknown ");
                sb.Append(directoryEntry.mSize);
                sb.Append(" Jan 1 09:00 ");
                sb.AppendLine(directoryEntry.mName);
            }

            ftpClient.DataStream.Write(Encoding.ASCII.GetBytes(sb.ToString()));

            ftpClient.Data.Close();

            ftpClient.SendReply(226, "Transfer complete.");
        }

        /// <summary>
        /// Process DELE command.
        /// </summary>
        /// <param name="ftpClient">FTP Client.</param>
        /// <param name="command">FTP Command.</param>
        internal void ProcessDele(FtpClient ftpClient, FtpCommand command)
        {
            if (string.IsNullOrEmpty(command.Content))
            {
                ftpClient.SendReply(501, "Syntax error in parameters or arguments.");
                return;
            }
            try
            {
                if (File.Exists(CurrentDirectory + "\\" + command.Content))
                {
                    File.Delete(CurrentDirectory + "\\" + command.Content);
                    ftpClient.SendReply(250, "Requested file action okay, completed.");
                }
                else
                {
                    ftpClient.SendReply(550, "Requested action not taken.");
                }
            }
            catch
            {
                ftpClient.SendReply(550, "Requested action not taken.");
            }
        }

        /// <summary>
        /// Process RMD command.
        /// </summary>
        /// <param name="ftpClient">FTP Client.</param>
        /// <param name="command">FTP Command.</param>
        internal void ProcessRmd(FtpClient ftpClient, FtpCommand command)
        {
            if (string.IsNullOrEmpty(command.Content))
            {
                ftpClient.SendReply(501, "Syntax error in parameters or arguments.");
                return;
            }
            try
            {
                if (Directory.Exists(CurrentDirectory + "\\" + command.Content))
                {
                    Directory.Delete(CurrentDirectory + "\\" + command.Content, true);
                    ftpClient.SendReply(200, "Command okay.");
                }
                else
                {
                    ftpClient.SendReply(550, "Requested action not taken.");
                }
            }
            catch
            {
                ftpClient.SendReply(550, "Requested action not taken.");
            }
        }

        /// <summary>
        /// Process MKD command.
        /// </summary>
        /// <param name="ftpClient">FTP Client.</param>
        /// <param name="command">FTP Command.</param>
        internal void ProcessMkd(FtpClient ftpClient, FtpCommand command)
        {
            if (string.IsNullOrEmpty(command.Content))
            {
                ftpClient.SendReply(501, "Syntax error in parameters or arguments.");
                return;
            }
            try
            {
                if (Directory.Exists(CurrentDirectory + "\\" + command.Content))
                {
                    ftpClient.SendReply(550, "Requested action not taken.");
                }
                else
                {
                    Directory.CreateDirectory(CurrentDirectory + "\\" + command.Content);
                    ftpClient.SendReply(200, "Command okay.");
                }
            }
            catch
            {
                ftpClient.SendReply(550, "Requested action not taken.");
            }
        }

        /// <summary>
        /// Process CDUP command.
        /// </summary>
        /// <param name="ftpClient">FTP Client.</param>
        /// <param name="command">FTP Command.</param>
        internal void ProcessCdup(FtpClient ftpClient, FtpCommand command)
        {
            try
            {
                var root = FileSystem.GetDirectory(CurrentDirectory);

                if (CurrentDirectory.Length > 3)
                {
                    CurrentDirectory = root.mParent.mFullPath;
                    ftpClient.SendReply(250, "Requested file action okay.");
                }
                else
                {
                    ftpClient.SendReply(550, "Requested action not taken.");
                }
            }
            catch
            {
                ftpClient.SendReply(550, "Requested action not taken.");
            }
        }

        /// <summary>
        /// Process STOR command.
        /// </summary>
        /// <param name="ftpClient">FTP Client.</param>
        /// <param name="command">FTP Command.</param>
        internal void ProcessStor(FtpClient ftpClient, FtpCommand command)
        {
            if (string.IsNullOrEmpty(command.Content))
            {
                ftpClient.SendReply(501, "Syntax error in parameters or arguments.");
                return;
            }
            try
            {
                if (ftpClient.Mode == TransferMode.NONE)
                {
                    ftpClient.SendReply(425, "Can't open data connection.");
                }
                else if (ftpClient.Mode == TransferMode.ACTV)
                {
                    ftpClient.Data.Connect(ftpClient.Address, ftpClient.Port);
                    ftpClient.DataStream = ftpClient.Data.GetStream();

                    DoStor(ftpClient, command);

                    return;
                }
                else if (ftpClient.Mode == TransferMode.PASV)
                {
                    ftpClient.Data = ftpClient.DataListener.AcceptTcpClient();
                    ftpClient.DataStream = ftpClient.Data.GetStream();

                    DoStor(ftpClient, command);

                    ftpClient.DataListener.Stop();

                    return;
                }
            }
            catch
            {
                ftpClient.SendReply(425, "Can't open data connection.");
            }

            ftpClient.SendReply(425, "Can't open data connection.");
        }

        /// <summary>
        /// Receive file from FTP data connection and write it to filesystem.
        /// </summary>
        /// <param name="ftpClient">FTP Client.</param>
        /// <param name="command">FTP Command.</param>
        private void DoStor(FtpClient ftpClient, FtpCommand command)
        {
            try
            {
                string filePath = Path.Combine(CurrentDirectory, command.Content);

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    byte[] buffer = new byte[ftpClient.Data.ReceiveBufferSize];
                    int count;

                    while ((count = ftpClient.DataStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fileStream.Write(buffer, 0, count);
                    }
                }
            }
            catch
            {
                ftpClient.SendReply(550, "Requested action not taken.");
            }
            finally
            {
                ftpClient.Data.Close();

                ftpClient.SendReply(226, "Transfer complete.");
            }
        }

        /// <summary>
        /// Process RETR command.
        /// </summary>
        /// <param name="ftpClient">FTP Client.</param>
        /// <param name="command">FTP Command.</param>
        internal void ProcessRetr(FtpClient ftpClient, FtpCommand command)
        {
            if (string.IsNullOrEmpty(command.Content))
            {
                ftpClient.SendReply(501, "Syntax error in parameters or arguments.");
                return;
            }
            try
            {
                if (ftpClient.Mode == TransferMode.NONE)
                {
                    ftpClient.SendReply(425, "Can't open data connection.");
                }
                else if (ftpClient.Mode == TransferMode.ACTV)
                {
                    ftpClient.Data.Connect(ftpClient.Address, ftpClient.Port);
                    ftpClient.DataStream = ftpClient.Data.GetStream();

                    DoRetr(ftpClient, command);

                    return;
                }
                else if (ftpClient.Mode == TransferMode.PASV)
                {
                    ftpClient.Data = ftpClient.DataListener.AcceptTcpClient();
                    ftpClient.DataStream = ftpClient.Data.GetStream();

                    DoRetr(ftpClient, command);

                    ftpClient.DataListener.Stop();

                    return;
                }
            }
            catch
            {
                ftpClient.SendReply(425, "Can't open data connection.");
            }

            ftpClient.SendReply(425, "Can't open data connection.");
        }

        /// <summary>
        /// Read file from filesystem and send it to FTP data connection.
        /// </summary>
        private void DoRetr(FtpClient ftpClient, FtpCommand command)
        {
            try
            {
                string filePath = Path.Combine(CurrentDirectory, command.Content);
                byte[] data = File.ReadAllBytes(filePath);

                ftpClient.DataStream.Write(data, 0, data.Length);
            }
            catch
            {
                ftpClient.SendReply(550, "Requested action not taken.");
            }
            finally
            {
                ftpClient.Data.Close();

                ftpClient.SendReply(226, "Transfer complete.");
            }
        }

        /// <summary>
        /// Process QUIT command.
        /// </summary>
        /// <param name="ftpClient">FTP Client.</param>
        /// <param name="command">FTP Command.</param>
        internal void ProcessQuit(FtpClient ftpClient, FtpCommand command)
        {
            ftpClient.SendReply(221, "Service closing control connection.");

            ftpClient.ControlStream.Close();
        }
    }
}
