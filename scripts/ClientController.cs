using Godot;

public class ClientController : Node
{
	public override void _Ready()
	{
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
