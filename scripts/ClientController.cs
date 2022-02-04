using Godot;

public class ClientController : Node
{
	public override void _Ready()
	{
		Client.ip = "127.0.0.1";
		Client.port = "24476";

		Client.StartClient();
	}

	public override void _Process(float delta)
	{
		Client.ReceivePacket();

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
