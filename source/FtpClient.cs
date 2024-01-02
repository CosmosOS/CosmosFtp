/*
* PROJECT:          Cosmos Operating System Development
* CONTENT:          FTP Client
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CosmosFtpServer
{
    /// <summary>
    /// FtpClient class.
    /// </summary>
    internal class FtpClient
    {
        /// <summary>
        /// Client IP Address.
        /// </summary>
        internal IPAddress Address { get; set; }

        /// <summary>
        /// Client TCP Port.
        /// </summary>
        internal int Port { get; set; }

        /// <summary>
        /// TCP Control Client. Used to send commands.
        /// </summary>
        private TcpClient Control { get; set; }

        /// <summary>
        /// TCP Control Client. Used to send and receive commands.
        /// </summary>
        internal NetworkStream ControlStream { get; set; }

        /// <summary>
        /// TCP Data Transfer Client. Used to transfer data.
        /// </summary>
        private TcpClient Data { get; set; }

        /// <summary>
        /// TCP Control Client. Used to send and receive commands.
        /// </summary>
        internal NetworkStream DataStream { get; set; }

        /// <summary>
        /// TCP Data Transfer Listener. Used in PASV mode.
        /// </summary>
        internal TcpListener DataListener { get; set; }

        /// <summary>
        /// FTP client data transfer mode.
        /// </summary>
        internal TransferMode Mode { get; set; }

        /// <summary>
        /// FTP client username.
        /// </summary>
        internal string Username { get; set; }

        /// <summary>
        /// FTP client password.
        /// </summary>
        internal string Password { get; set; }

        /// <summary>
        /// Is user connected.
        /// </summary>
        internal bool Connected { get; set; }

        /// <summary>
        /// Create new instance of the <see cref="FtpClient"/> class.
        /// </summary>
        /// <param name="client">TcpClient used for control connection.</param>
        internal FtpClient(TcpClient client)
        {
            Control = client;
            Connected = false;
            Mode = TransferMode.NONE;
            ControlStream = client.GetStream();
        }

        /// <summary>
        /// Is user connected.
        /// </summary>
        /// <returns>Boolean value.</returns>
        internal bool IsConnected()
        {
            if (Connected == false)
            {
                SendReply(530, "Login incorrect.");
                return Connected;
            }
            else
            {
                return Connected;
            }
        }

        /// <summary>
        /// Send text to control socket (usually port 21)
        /// </summary>
        /// <param name="code">Reply code.</param>
        /// <param name="command">Reply content.</param>
        internal void SendReply(int code, string message)
        {
            message = message.Replace('\\', '/');
            byte[] response = Encoding.ASCII.GetBytes(code + " " + message + "\r\n");
            ControlStream.Write(response, 0, response.Length);
        }
    }
}
