using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Godot;

public class Client : Node
{
	public static string ip = "127.0.0.1";
	public static string port = "24465";

	public struct UdpState
	{
		public IPEndPoint endPoint;
		public UdpClient udpClient;
		public int clientId;
		public int packetCount;
	}
	public static UdpState udpState = new UdpState();

	public static Dictionary<int, Action<int, Packet>> packetFunctions = new Dictionary<int, Action<int, Packet>>()
	{
		{ 0, Welcome }
	};

	public override void _Ready()
	{
		StartClient();
	}

	public override void _Process(float delta)
	{

	}

	private void StartClient()
	{
		// Creates the UDP client and initializes the UDP state struct
		udpState.endPoint = new IPEndPoint(IPAddress.Parse(ip), port.ToInt());
		udpState.udpClient = new UdpClient(udpState.endPoint);
		udpState.clientId = 0;
		udpState.packetCount = 0;

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

			// Invoke connected function from packet
			packetFunctions[packet.connectedFunction].Invoke(packet.clientId, packet);

			// TODO: check for dropped packets using packetNumber, and let the server resend a list of packets if needed
			// TODO: use checksum to check for data corruption/data loss in the packet
		}
	}

	#region Packet callbacks
	// Packet callback functions must be static, else they cannot be stored in the packetFunctions dictionary.
	private static void Welcome(int clientId, Packet packet)
	{
		// Do stuff with the packet's data here :D
		string welcomeMessage = packet.ReadString();

		GD.Print($"Welcome message received from server: {welcomeMessage}");
		// GetNode<Label>("/root/Spatial/Label").Text = welcomeMessage;
	}
	#endregion
}
