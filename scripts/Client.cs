using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Godot;

public class Client : Node
{
	public static string ip = "127.0.0.1";
	public static string port = "24465";

	public struct UdpState
	{
		public IPEndPoint endPoint;
		public UdpClient udpClient;
	}
	public static UdpState udpState = new UdpState();

	public override void _Ready()
	{
		StartClient();
	}

	public override void _Process(float delta)
	{

	}

	public void StartClient()
	{
		// Create udp client
		udpState.endPoint = new IPEndPoint(IPAddress.Parse(ip), port.ToInt());
		udpState.udpClient = new UdpClient(udpState.endPoint);

		udpState.udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), udpState);
		GD.Print("Started listening for messages");
	}
	public void ReceiveCallback(IAsyncResult asyncResult)
	{
		byte[] receiveBytes = udpState.udpClient.EndReceive(asyncResult, ref udpState.endPoint);
		udpState.udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), udpState.udpClient);

		int prefix = BitConverter.ToInt32(receiveBytes, 0);

		if (prefix == 1)
		{
			int receivedInt = BitConverter.ToInt32(receiveBytes, 4);
			GetNode<Label>("../Label").Text = receivedInt.ToString();
			GD.Print($"Received: {receivedInt.ToString()}");
		}
		if (prefix == 3)
		{
			string receivedString = Encoding.ASCII.GetString(receiveBytes, 4, receiveBytes.Length - 4);
			GetNode<Label>("../Label").Text = receivedString;
			GD.Print($"Received: {receivedString}");
		}
	}
}
