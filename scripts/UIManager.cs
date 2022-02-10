using Godot;

public class UIManager : Node
{
	public static UIManager instance;

	public override void _Ready()
	{
		// C# singleton instance initializer for strong typing support
		if (instance != null)
		{
			GD.Print($"UIManager instance was already set, overriding.");
		}
		instance = this;

		ClientController.instance.Connect(nameof(ClientController.OnConnected), this, "OnConnected");
		ClientController.instance.Connect(nameof(ClientController.OnDisconnected), this, "OnDisconnected");
	}

	public void SetLabelText(string text)
	{
		GetNode<Label>("/root/Spatial/Label").Text = text;
	}

	private void OnConnected(int clientId, string messageOfTheDay)
	{
		SetLabelText(messageOfTheDay);
	}

	private void OnDisconnected()
	{
		SetLabelText("Disconnected from the server.");
	}
}
