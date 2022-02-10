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

		public bool hasStarted;
		public bool isConnected;
	}
	public static UdpState udpState = new UdpState();

	// Packet callback functions
	public static Dictionary<int, Action<Packet>> packetFunctions = new Dictionary<int, Action<Packet>>()
	{
		{ 0, OnConnected },
		{ 1, OnDisconnected }
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

		udpState.hasStarted = false;
		udpState.isConnected = false;

		try
		{
			// Create and start a UDP receive thread for Server.ReceivePacket(), so it doesn't block Godot's main thread
			System.Threading.Thread udpReceiveThread = new System.Threading.Thread(new ThreadStart(ReceivePacket))
			{
				Name = "UDP receive thread",
				IsBackground = true
			};
			udpReceiveThread.Start();
			udpState.hasStarted = true;
			GD.Print($"{printHeader} Started listening for messages from the server on {udpState.localEndPoint}.");

			// "Connect" to the server (sets udpClient.Client.RemoteEndPoint)
			udpState.udpClient.Connect(udpState.serverEndPoint);

			// Ask server to connect
			using (Packet packet = new Packet(0, 0))
			{
				SendPacketToServer(packet);
				GD.Print($"{printHeader} Sending connect packet to the server...");
			}
		}
		catch (Exception e)
		{
			GD.PrintErr($"{printHeader} Failed connecting to the server: {e}\nCheck if the remote endpoint is correct and that the server has been started.");
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
				packetFunctions[constructedPacket.connectedFunction].Invoke(constructedPacket);
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
	private static void OnConnected(Packet packet)
	{
		int clientId = packet.ReadInt32();
		string messageOfTheDay = packet.ReadString();

		udpState.isConnected = true;
		ClientController.instance.EmitSignal(nameof(ClientController.OnConnected), clientId, messageOfTheDay);
		GD.Print($"{printHeader} Connected to server {udpState.serverEndPoint}, received client ID of {clientId}.\nMessage of the day received from server: {messageOfTheDay}");
	}

	private static void OnDisconnected(Packet packet)
	{
		udpState.isConnected = false;
		ClientController.instance.EmitSignal(nameof(ClientController.OnDisconnected));
		GD.Print($"{printHeader} Disconnected from the server.");
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
