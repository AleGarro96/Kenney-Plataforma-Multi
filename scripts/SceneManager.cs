using Godot;
using System;
using System.Data.Common;
using System.Diagnostics;


public partial class SceneManager : Node3D
{
	[Export]
	private PackedScene playerScene;

	public override void _Ready()
	{
		int index = 0;
		foreach (var item in GameManager.Players)
		{
			Node3D currentNodePlayer = playerScene.Instantiate<Node3D>();
			Player currentPlayer = currentNodePlayer.GetNode<Player>("Player");

			currentPlayer.Name = item.Id.ToString();
			currentPlayer.SetUpPlayer(item.Name);
			AddChild(currentNodePlayer);
			foreach (Node3D spawnPoint in GetTree().GetNodesInGroup("PlayerSpawnPoints"))
			{
				if (int.Parse(spawnPoint.Name) == index)
				{
					currentPlayer.GlobalPosition = spawnPoint.GlobalPosition;
				}
			}
			index++;
		}
	}

	public override void _Process(double delta)
	{
	}
}
