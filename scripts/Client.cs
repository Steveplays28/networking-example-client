using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Godot;

public static class Client
{
	#region Variables
	public static string ip = "127.0.0.1";
	public static string port = "24465";

	// Header for GD.Print() messages, default = "[Client]:"
	public static string printHeader = "[Client]:";

	public struct UdpState
	{
		public IPEndPoint serverEndPoint;
		public IPEndPoint localEndPoint;
		public UdpClient udpClient;
		public int packetCount;

		// Server ID, default = -1
		public int serverId;
		// The ID the server assigned to this client
		public int clientId;
		// The ID used to receive packets meant for newly connected clients or all clients, default = -1
		public int allClientsId;

		public bool hasStarted;
	}
	public static UdpState udpState = new UdpState();

	// Packet callback functions
	public static Dictionary<int, Action<Packet>> packetFunctions = new Dictionary<int, Action<Packet>>()
	{
		{ 0, OnConnect }
	};
	#endregion

	public static void StartClient()
	{
		if (udpState.hasStarted)
		{
			return;
		}

		// Creates the UDP client and initializes the UDP state struct
		udpState.serverEndPoint = new IPEndPoint(IPAddress.Parse(ip), port.ToInt());
		udpState.udpClient = new UdpClient(0);
		udpState.localEndPoint = (IPEndPoint)udpState.udpClient.Client.LocalEndPoint;
		udpState.packetCount = 0;

		udpState.serverId = -1;
		udpState.clientId = -1;
		udpState.allClientsId = -1;

		// "Connect" to the server
		// Quotes because UDP is a connectionless protocol, this function just sets a default send/receive address
		// Behind the scenes this function just sets udpClient.Client.RemoteEndPoint
		try
		{
			// Create and start a UDP receive thread for Server.ReceivePacket(), so it doesn't block Godot's main thread
			System.Threading.Thread udpReceiveThread = new System.Threading.Thread(new ThreadStart(ReceivePacket))
			{
				Name = "UDP receive thread",
				IsBackground = true
			};
			udpReceiveThread.Start();

			udpState.udpClient.Connect(udpState.serverEndPoint);
			udpState.hasStarted = true;
			GD.Print($"{printHeader} Started listening for messages from the server on {udpState.localEndPoint}.");

			// Ask server to connect
			using (Packet packet = new Packet(0, 0, udpState.clientId, udpState.serverId))
			{
				SendPacketToServer(packet);
				GD.Print($"{printHeader} Sending welcome packet to the server...");
			}
		}
		catch (Exception e)
		{
			GD.PrintErr($"{printHeader} Failed connecting to the server: {e}\nThere most likely is a port mismatch or the server hasn't been started yet.");
		}
	}

	#region Sending packets
	/// <summary>
	/// Sends a packet to the server.
	/// </summary>
	/// <param name="packet">The packet to send.</param>
	/// <returns></returns>
	public static void SendPacketToServer(Packet packet)
	{
		// Write packet header
		packet.WritePacketHeader();

		// Get data from the packet
		byte[] packetData = packet.ReturnData();

		// Send the packet to the server
		udpState.udpClient.Send(packetData, packetData.Length);
	}
	#endregion

	#region Receiving packets
	public static void ReceivePacket()
	{
		try
		{
			// Extract data from the received packet
			IPEndPoint remoteEndPoint = udpState.serverEndPoint;
			byte[] packetData = udpState.udpClient.Receive(ref remoteEndPoint);

			// Debug, lol
			GD.Print($"{printHeader} Received bytes: {string.Join(", ", packetData)}");

			// Construct new Packet object from the received packet
			using (Packet constructedPacket = new Packet(packetData))
			{
				if (constructedPacket.senderId == udpState.serverId && constructedPacket.recipientId != udpState.clientId)
				{
					packetFunctions[constructedPacket.connectedFunction].Invoke(constructedPacket);
					return;
				}

				if (constructedPacket.recipientId == udpState.clientId)
				{
					// Packet was sent by self, return out of function
					return;
				}
				if (constructedPacket.senderId != udpState.serverId)
				{
					GD.PrintErr($"{printHeader} Received a senderId of {constructedPacket.senderId}, which isn't equal to the serverId of {udpState.serverId}!");
				}
				if (constructedPacket.recipientId == udpState.allClientsId)
				{
					// Execute function anyway if the packet is meant for all clients or newly connected clients
					packetFunctions[constructedPacket.connectedFunction].Invoke(constructedPacket);
					return;
				}
			}
		}
		catch (Exception e)
		{
			GD.PrintErr($"{printHeader} Failed receiving a packet from the server: {e}\nCheck if the client is connected properly!");
		}
	}
	#endregion

	#region Packet callback functions
	// Packet callback functions must be static, else they cannot be stored in the packetFunctions dictionary
	private static void OnConnect(Packet packet)
	{
		// TODO: Check if packet.senderId (the server's supposed ID) is equal to the serverId

		string endPointString = packet.ReadString();
		int ipAddress = endPointString.Split(":")[0].ToInt();
		int port = endPointString.Split(":")[1].ToInt();
		IPEndPoint packetIPEndPoint = new IPEndPoint(ipAddress, port);

		if (packetIPEndPoint != udpState.localEndPoint)
		{
			GD.PrintErr($"{printHeader} Received an OnConnect packet containing a wrong IPEndPoint (received: {packetIPEndPoint}, expected: {udpState.localEndPoint}), uh oh...");
			return;
		}

		int clientId = packet.recipientId;
		string messageOfTheDay = packet.ReadString();

		GD.Print($"{printHeader} Client ID received from server ({udpState.serverEndPoint}, ID {packet.senderId}): {clientId}");
		GD.Print($"{printHeader} Welcome message received from server ({udpState.serverEndPoint}, ID {packet.senderId}): {messageOfTheDay}");
		GD.Print($"{printHeader} Local endpoint: {udpState.localEndPoint}");
		GD.Print($"{printHeader} Remote endpoint: {udpState.serverEndPoint}");

		// GetNode<Label>("/root/Spatial/Label").Text = welcomeMessage;
	}
	#endregion

	public static void CloseUdpClient()
	{
		try
		{
			udpState.udpClient.Close();
			GD.Print($"{printHeader} Successfully closed the UdpClient!");
		}
		catch (SocketException e)
		{
			GD.PrintErr($"{printHeader} Failed closing the UdpClient: {e}");
		}
	}
}
