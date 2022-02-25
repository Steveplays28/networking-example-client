using Godot;
using SteveNetworking;

public class UIManager : Node
{
	public static UIManager instance;

	public override void _Ready()
	{
		// C# singleton instance initializer for strong typing support
		if (instance != null)
		{
			GD.PushWarning($"UIManager instance was already set, overriding.");
		}
		instance = this;

		PacketCallbacksClient.OnConnected += OnConnected;
		PacketCallbacksClient.OnDisconnected += OnDisconnected;
	}

	public void SetLabelText(string text)
	{
		GetNode<Label>("/root/Spatial/Label").Text = text;
	}

	private void OnConnected(int clientId, string messageOfTheDay)
	{
		SetLabelText($"Connected to server {Client.udpState.serverEndPoint}, received client ID of {clientId}.\nMessage of the day received from server: {messageOfTheDay}");
	}

	private void OnDisconnected()
	{
		SetLabelText("Disconnected from the server.");
	}
}
