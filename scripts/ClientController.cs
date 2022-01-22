using Godot;

public class ClientController : Node
{
	public override async void _Ready()
	{
		Client.ip = "127.0.0.1";
		Client.port = "24465";

		await Client.StartClient();
	}

	public override async void _Process(float delta)
	{
		await Client.ReceivePacketAsync();
	}
}
