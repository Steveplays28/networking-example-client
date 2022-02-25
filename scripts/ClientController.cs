using Godot;
using SteveNetworking;

public class ClientController : Node
{
	public static ClientController instance;

	public override void _Ready()
	{
		// C# singleton instance initializer for strong typing support
		if (instance != null)
		{
			GD.PushWarning("ClientController instance is already set, overriding!");
		}
		instance = this;

		Client.InitializeClient();
	}

	public override void _Process(float delta)
	{
		if (Input.IsActionJustReleased("ui_cancel"))
		{
			Client.Disconnect();
		}
	}

	public override void _ExitTree()
	{
		Client.CloseUdpClient();
	}
}
