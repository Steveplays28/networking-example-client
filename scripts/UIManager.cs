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
	}

	public void SetLabelText(string text)
	{
		GetNode<Label>("/root/Spatial/Label").Text = text;
	}
}
