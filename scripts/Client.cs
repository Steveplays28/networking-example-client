using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Godot;

// test
public class Client : Node
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
		{ 0, Welcome }
	};
	#endregion

	public override void _Ready()
	{
		StartClient();
	}

	#region Receiving packets
	private void StartClient()
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

		// Start receiving packets
		udpState.udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), udpState);
		GD.Print("Started listening for messages");
	}
	private void ReceiveCallback(IAsyncResult asyncResult)
	{
		// Called when a packet is received
		byte[] receiveBytes = udpState.udpClient.EndReceive(asyncResult, ref udpState.endPoint);
		udpState.packetCount += 1;

		// Continue listening for packets
		udpState.udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), udpState.udpClient);

		// Handle received packet
		using (Packet packet = new Packet(receiveBytes))
		{
			// Debug, lol
			GD.Print(string.Join(",", receiveBytes));

			// Invoke the packet's connected function
			packetFunctions[packet.connectedFunction].Invoke(packet.clientId, packet);

			// TODO: check for dropped packets using packetNumber, and let the server resend a list of packets if needed
			// TODO: use checksum to check for data corruption/data loss in the packet
		}
	}
	#endregion

	#region Packet callback functions
	// Packet callback functions must be static, else they cannot be stored in the packetFunctions dictionary
	private static void Welcome(int clientId, Packet packet)
	{
		// Do stuff with the packet's data here :D
		string welcomeMessage = packet.ReadString();

		GD.Print($"Welcome message received from server ({udpState.endPoint}): {welcomeMessage}");
		GD.Print($"Local endpoint: {udpState.udpClient.Client.LocalEndPoint}");
		GD.Print($"Remote endpoint: {udpState.endPoint}");
		// GetNode<Label>("/root/Spatial/Label").Text = welcomeMessage;
	}
	#endregion
}
