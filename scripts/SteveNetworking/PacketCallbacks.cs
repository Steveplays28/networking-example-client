using System;
using System.Collections.Generic;

public static class PacketCallbacks
{
	public static Dictionary<int, Action<Packet>> packetFunctions = new Dictionary<int, Action<Packet>>()
	{
		{ 0, OnConnected }
	};

	public delegate void OnConnected();
}
