using Godot;
using System;
using System.Transactions;

public partial class Player : CharacterBody3D
{
	[Signal]
	public delegate void CoinCollectedEventHandler();

	[Export(PropertyHint.None, "Components")]
	public CameraControlller View;

	[Export(PropertyHint.None, "Properties")]
	public double MovementSpeed = 250;
	[Export(PropertyHint.None, "Properties")]
	public double JumpStrength = 7;

	private Vector3 MovementVelocity;
	private double RotationDirection;
	private float Gravity = 0;

	private bool PreviouslyFloored = false;

	private bool JumpSingle = true;
	private bool JumpDouble = true;

	private int Coins = 0;

	private CpuParticles3D ParticleTrail;
	private AudioStreamPlayer SoundFootsteps;
	private Node3D Model;
	private AnimationPlayer Animation;
	private AudioPlayer AudioPlayer;
	private MultiplayerSynchronizer multiplayerSynchronizer;

	public override void _EnterTree()
	{
		multiplayerSynchronizer = GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer");
		multiplayerSynchronizer.SetMultiplayerAuthority(int.Parse(Name));
	}

	public override void _Ready()
	{
		if (!IsOwner()) return;

		AudioPlayer = GetNode<AudioPlayer>("/root/AudioPlayer");
		ParticleTrail = GetNode<CpuParticles3D>("ParticlesTrail");
		SoundFootsteps = GetNode<AudioStreamPlayer>("SoundFootsteps");
		Model = GetNode<Node3D>("Character");
		Animation = GetNode<AnimationPlayer>("Character/AnimationPlayer");

		View.camera.Current = true;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!IsOwner())
			return;

		HandleControls(delta);
		HandleGravity(delta);
		HandleEffects();

		var AppliedVelocity = Velocity.Lerp(MovementVelocity, (float)(delta * 10));
		AppliedVelocity.Y = -Gravity;

		Velocity = AppliedVelocity;
		MoveAndSlide();

		if (new Vector2(Velocity.Z, Velocity.X).Length() > 0)
		{
			RotationDirection = new Vector2(Velocity.Z, Velocity.X).Angle();
		}

		Rotation = new Vector3(0, (float)Mathf.LerpAngle(Rotation.Y, RotationDirection, delta * 10), 0);

		if (GlobalTransform.Origin.Y < -10)
		{
			GetTree().ReloadCurrentScene();
		}

		Model.Scale = Model.Scale.Lerp(new Vector3(1, 1, 1), (float)(delta * 10));

		if (IsOnFloor() && Gravity > 2 && !PreviouslyFloored)
		{
			Model.Scale = new Vector3(1.25f, 0.75f, 1.25f);
			AudioPlayer.Play("res://sounds/land.ogg");
		}

		PreviouslyFloored = IsOnFloor();
	}

	public bool IsOwner()
	{
		if (multiplayerSynchronizer.GetMultiplayerAuthority() == Multiplayer.GetUniqueId())
		{
			return true;
		}
		return false;
	}

	private void HandleEffects()
	{
		ParticleTrail.Emitting = false;
		SoundFootsteps.StreamPaused = true;

		if (IsOnFloor())
		{
			if (Math.Abs(Velocity.X) > 1 || Math.Abs(Velocity.Z) > 1)
			{
				Animation.Play("walk", -1, 0.5f, false);
				ParticleTrail.Emitting = true;
				SoundFootsteps.StreamPaused = false;
			}
			else
			{
				Animation.Play("idle", -1, 0.5f, false);
			}
		}
		else
		{
			Animation.Play("jump", -1, 0.5f, false);
		}
	}

	private void HandleControls(double delta)
	{
		Vector3 input = new Vector3();

		// Captura las entradas de movimiento (aquí se usa el teclado como ejemplo)
		float inputX = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
		float inputZ = Input.GetActionStrength("move_back") - Input.GetActionStrength("move_forward");

		// Calcula el vector de movimiento relativo a la cámara
		Vector3 cameraForward = View.camera.GlobalTransform.Basis.Z;
		Vector3 cameraRight = View.camera.GlobalTransform.Basis.X;

		input = (cameraForward * inputZ + cameraRight * inputX).Normalized();

		// Aplica el movimiento al nodo del personaje
		MovementVelocity = input * (float)MovementSpeed * (float)delta;

		if (Input.IsActionJustPressed("jump"))
		{
			if (JumpSingle || JumpDouble)
			{
				AudioPlayer.Play("res://sounds/jump.ogg");
			}

			if (JumpDouble)
			{
				Gravity = (float)-JumpStrength;
				JumpDouble = false;
				Model.Scale = new Vector3(0.5f, 1.5f, 0.5f);
			}

			if (JumpSingle)
			{
				Jump();
			}
		}
	}

	private void HandleGravity(double delta)
	{
		Gravity += 25 * (float)delta;

		if (Gravity > 0 && IsOnFloor())
		{
			JumpSingle = true;
			Gravity = 0;
		}
	}

	private void Jump()
	{
		Gravity = (float)-JumpStrength;
		Model.Scale = new Vector3(0.5f, 1.5f, 0.5f);
		JumpSingle = false;
		JumpDouble = true;
	}

	private void CollectCoin()
	{
		Coins += 1;
		EmitSignal("CoinCollected", Coins);
	}

	public void SetUpPlayer(string name)
	{
		GetNode<Label3D>("Label").Text = name;
	}
}
