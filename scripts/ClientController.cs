using Godot;

public class ClientController : Node
{
	public static ClientController instance;

	[Signal]
	public delegate void OnConnected(int clientId, string messageOfTheDay);

	public override void _Ready()
	{
		if (instance != null)
		{
			GD.PushWarning("ClientController instance is already set, overriding!");
		}
		instance = this;

		// Set the endpoint to connect the client to
		Client.ip = "127.0.0.1";
		Client.port = "24476";

		// Start client
		Client.StartClient();
	}

	public override void _Process(float delta)
	{
		if (Input.IsActionJustReleased("ui_cancel"))
		{
			GetTree().Quit();
		}
	}

	public override void _ExitTree()
	{
		Client.CloseUdpClient();
	}
}
