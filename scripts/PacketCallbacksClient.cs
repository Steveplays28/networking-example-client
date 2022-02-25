using System;
using System.Collections.Generic;
using SteveNetworking;

public static class PacketCallbacksClient
{
	public static Dictionary<int, Action<Packet>> packetCallbacks = new Dictionary<int, Action<Packet>>()
	{
		{ 0, OnConnectedInvoker },
		{ 0, OnDisconnectedInvoker }
	};

	#region OnConnected
	public delegate void ConnectedDelegate(int clientId, string messageOfTheDay);
	public static event ConnectedDelegate OnConnected;

	private static void OnConnectedInvoker(Packet packet)
	{
		int clientId = packet.ReadInt32();
		string messageOfTheDay = packet.ReadString();

		OnConnected.Invoke(clientId, messageOfTheDay);
	}
	#endregion

	#region OnDisconnected
	public delegate void DisconnectedDelegate();
	public static event DisconnectedDelegate OnDisconnected;

	private static void OnDisconnectedInvoker(Packet packet)
	{
		OnDisconnected.Invoke();
	}
	#endregion
}
