using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SteveNetworking
{
	public static class Client
	{
		#region Variables
		public static readonly string defaultIp = "127.0.0.1";
		public static readonly int defaultPort = 24465;

		public struct UdpState
		{
			public IPEndPoint serverEndPoint;
			public IPEndPoint localEndPoint;
			public UdpClient udpClient;
			public int packetCount;

			public bool hasInitialized;
			public bool isConnected;
		}
		public static UdpState udpState = new UdpState();
		#endregion

		public static void InitializeClient()
		{
			if (udpState.hasInitialized)
			{
				LogHelper.LogError(true, $"Failed initializing the UdpClient: UdpClient has already been initialized.");
				return;
			}

			// Creates the UDP client and initializes the UDP state struct
			udpState.serverEndPoint = new IPEndPoint(IPAddress.Parse(defaultIp), defaultPort);
			udpState.udpClient = new UdpClient(0);
			udpState.localEndPoint = (IPEndPoint)udpState.udpClient.Client.LocalEndPoint;
			udpState.packetCount = 0;

			udpState.hasInitialized = false;
			udpState.isConnected = false;

			try
			{
				// Create and start a background thread for ReceivePacket(), so it doesn't block Godot's main thread
				Thread udpReceiveThread = new Thread(new ThreadStart(ReceivePacket))
				{
					Name = "UDP receive thread",
					IsBackground = true
				};
				udpReceiveThread.Start();
				udpState.hasInitialized = true;
				LogHelper.Log(true, $"Started listening for messages from the server on {udpState.localEndPoint}.");
			}
			catch (Exception e)
			{
				LogHelper.LogError(true, $"Failed initializing the UdpClient: couldn't create background thread \"UDP receive thread\".\n{e}");
			}
		}

		public static void CloseUdpClient()
		{
			try
			{
				udpState.udpClient.Close();
				udpState.hasInitialized = false;
				LogHelper.Log(true, $"Successfully closed the UdpClient!");
			}
			catch (SocketException e)
			{
				LogHelper.LogError(true, $"Failed closing the UdpClient: {e}");
			}
		}

		public static void Connect(string ip, int port)
		{
			if (udpState.isConnected)
			{
				LogHelper.LogWarning(true, "Failed connecting to the server: already connected to a server.");
				return;
			}

			// Set server endpoint
			udpState.serverEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);

			// Send connect packet
			using (Packet packet = new Packet(0, 0))
			{
				SendPacket(packet);
				LogHelper.Log(true, $"Sent connect packet to the server.");
			}
		}

		public static void Disconnect()
		{
			if (!udpState.isConnected)
			{
				LogHelper.LogWarning(true, $"Failed disconnecting from the server: not connected to a server.");
				return;
			}

			// Reset server endpoint
			udpState.serverEndPoint = new IPEndPoint(IPAddress.Parse(defaultIp), defaultPort);

			// Send disconnect packet
			using (Packet packet = new Packet(0, 0))
			{
				SendPacket(packet);
				LogHelper.Log(true, $"Sent disconnect packet to the server.");
			}
		}

		#region Sending packets
		/// <summary>
		/// Sends a packet to the server.
		/// </summary>
		/// <param name="packet">The packet to send.</param>
		/// <returns></returns>
		public static void SendPacket(Packet packet)
		{
			// Write packet header
			packet.WritePacketHeader();

			// Get data from the packet
			byte[] packetData = packet.ReturnData();

			// Send the packet to the server
			udpState.udpClient.Send(packetData, packetData.Length, udpState.serverEndPoint);
		}
		#endregion

		#region Receiving packets
		private static void ReceivePacket()
		{
			try
			{
				// Extract data from the received packet
				IPEndPoint remoteEndPoint = udpState.serverEndPoint;
				byte[] packetData = udpState.udpClient.Receive(ref remoteEndPoint);

				// Debug, lol
				LogHelper.Log(true, $"Received bytes: {string.Join(", ", packetData)}");

				// Construct new Packet object from the received packet
				using (Packet constructedPacket = new Packet(packetData))
				{
					PacketCallbacksClient.packetCallbacks[constructedPacket.connectedFunction].Invoke(constructedPacket);
				}
			}
			catch (Exception e)
			{
				LogHelper.LogError(true, $"Failed receiving a packet from the server: {e}\nCheck if the client is connected properly!");
			}
		}
		#endregion
	}
}
