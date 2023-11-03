using Godot;
using System;

public partial class CameraControlller : Node3D
{
	private float camrot_h = 0;
	private float camrot_v = 0;

	[Export]
	public int cam_v_max = 75; // 75 recomendado

	[Export]
	public int cam_v_min = -55; // -55 recomendado

	private float h_sensitivity = 0.01f;
	private float v_sensitivity = 0.01f;
	private float h_acceleration = 10;
	private float v_acceleration = 10;

	private Node3D h;
	public Node3D v;

	[Export]
	public Camera3D camera;

	[Export]
	private Node3D player;

	[Export]
	private float followSpeed = 5.0f;

	public override void _Ready()
	{
		h = GetNode<Node3D>("h");
		v = GetNode<Node3D>("h/v");
	}
	public override void _Input(InputEvent @event)
	{
		// Mouse in viewport coordinates.
		if (@event is InputEventMouseMotion eventMouseMotion)
		{
			InputEventMouseMotion mouseMotion = eventMouseMotion;
			camrot_h += -mouseMotion.Relative.X * h_sensitivity;
			camrot_v += mouseMotion.Relative.Y * v_sensitivity;
		}
	}

	public override void _Process(double delta)
	{
		if (player != null)
		{
			// Obtener la posición actual de la cámara
			var cameraPosition = GlobalTransform.Origin;

			// Obtener la posición actual del jugador
			var playerPosition = player.GlobalTransform.Origin;

			// Interpolar suavemente la posición de la cámara hacia la posición del jugador
			cameraPosition = cameraPosition.Lerp(playerPosition, followSpeed * (float)delta);

			// Establecer la nueva posición de la cámara
			GlobalTransform = new Transform3D(Basis.Identity, cameraPosition);
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		camrot_v = Mathf.Clamp(camrot_v, Mathf.DegToRad(cam_v_min), Mathf.DegToRad(cam_v_max));

		h.Rotation = new Vector3(h.Rotation.X, Mathf.Lerp(h.Rotation.Y, camrot_h, (float)delta * h_acceleration), h.Rotation.Z);
		v.Rotation = new Vector3(Mathf.Lerp(v.Rotation.X, camrot_v, (float)delta * v_acceleration), v.Rotation.Y, v.Rotation.Z);
	}
	public void SetPlayer(Player target)
	{
		player = target;
	}
}
