using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Godot;

public static class Client
{
	#region Variables
	public static string ip = "127.0.0.1";
	public static string port = "24465";

	public struct UdpState
	{
		public IPEndPoint endPoint;
		public UdpClient udpClient;
		public int packetCount;

		// The ID the server assigned to this client
		public int clientId;
	}
	public static UdpState udpState = new UdpState();

	// Packet callback functions
	public static Dictionary<int, Action<int, Packet>> packetFunctions = new Dictionary<int, Action<int, Packet>>()
	{
		{ 0, OnConnect }
	};
	#endregion

	public static void StartClient()
	{
		// Creates the UDP client and initializes the UDP state struct
		udpState.endPoint = new IPEndPoint(IPAddress.Parse(ip), port.ToInt());
		udpState.udpClient = new UdpClient(udpState.endPoint);
		udpState.clientId = 0;
		udpState.packetCount = 0;

		// "Connect" to the server
		// Quotes because UDP is a connectionless protocol, this function just sets a default send/receive address
		// Behind the scenes this function just sets udpClient.Client.RemoteEndPoint
		udpState.udpClient.Connect(udpState.endPoint);
		GD.Print("Started listening for messages");
	}

	#region Sending packets
	/// <summary>
	/// Sends a packet to the server asynchronously.
	/// </summary>
	/// <param name="packet">The packet to send.</param>
	/// <returns></returns>
	public static async Task SendPacketToServerAsync(Packet packet)
	{
		// Write packet header
		packet.WritePacketHeader();

		// Get data from the packet
		byte[] packetData = packet.ReturnData();

		// Send the packet to the server
		await udpState.udpClient.SendAsync(packetData, packetData.Length, udpState.endPoint);
	}
	#endregion

	#region Receiving packets
	public static async Task ReceivePacketAsync()
	{
		UdpReceiveResult receivedPacket = await udpState.udpClient.ReceiveAsync();

		// Extract data from the received packet
		byte[] packetData = receivedPacket.Buffer;
		IPEndPoint remoteEndPoint = receivedPacket.RemoteEndPoint;

		// Debug, lol
		GD.Print(string.Join(",", packetData));

		// Construct new Packet object from the received packet
		using (Packet constructedPacket = new Packet(packetData))
		{
			packetFunctions[constructedPacket.connectedFunction].Invoke(constructedPacket.clientId, constructedPacket);
		}
	}
	#endregion

	#region Packet callback functions
	// Packet callback functions must be static, else they cannot be stored in the packetFunctions dictionary
	private static void OnConnect(int clientId, Packet packet)
	{
		string welcomeMessage = packet.ReadString();

		GD.Print($"Welcome message received from server ({udpState.endPoint}): {welcomeMessage}");
		GD.Print($"Local endpoint: {udpState.udpClient.Client.LocalEndPoint}");
		GD.Print($"Remote endpoint: {udpState.endPoint}");

		// GetNode<Label>("/root/Spatial/Label").Text = welcomeMessage;
	}
	#endregion
}

public class ClientController : Node
{
	public override void _Ready()
	{
		Client.ip = "127.0.0.1";
		Client.port = "24465";

		Client.StartClient();
	}

	public override async void _Process(float delta)
	{
		await Client.ReceivePacketAsync();
	}
}
