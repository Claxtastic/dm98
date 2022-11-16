using Sandbox;
using System.Collections.Generic;
using System.Linq;

partial class DeathmatchController : BasePlayerController
{

	[Net] public bool IsSliding { get; set; }

	private List<MovementAbility> abilities = new();

	private MovementAbility activeAbility => abilities.FirstOrDefault( x => x.IsActive );

	public IReadOnlyList<MovementAbility> Abilities => abilities;
	public Vector3 Mins { get; private set; }
	public Vector3 Maxs { get; private set; }

	public DeathmatchController()
	{
		abilities.Add( new Slide( this ) );
	}

	public T GetAbility<T>() where T : MovementAbility
	{
		return abilities.FirstOrDefault( x => x is T ) as T;
	}

	public override void FrameSimulate()
	{
		base.FrameSimulate();

		EyeRotation = Input.Rotation;
	}

	public override void Simulate()
	{
		// This is confusing and needs review:
		//		PreSimulate and PostSimulate are always called if 
		//		the mechanic is active or AlwaysSimulate=true

		//		Simulate is only called if the mechanic is active, 
		//		AlwaysSimulates, AND there's no other mechanic in control

		//		The control is for things like vaulting, it stops 
		//		all other mechanics until its finished with the vault

		// Pros: modular, easy to edit/add movement mechanics

		foreach ( var m in abilities )
		{
			if ( !m.IsActive && !m.AlwaysSimulate ) continue;
			m.PreSimulate();
		}

		var control = activeAbility;

		if ( control == null )
		{
			foreach ( var m in abilities )
			{
				// try to activate, i.e. vault looks for a ledge in front of the player
				if ( !m.Try() ) continue;
				control = m;
				break;
			}
		}

		if ( control != null && control.TakesOverControl )
		{
			control.Simulate();
		}
		else
		{
			foreach ( var m in abilities )
			{
				if ( !m.IsActive && !m.AlwaysSimulate ) continue;
				m.Simulate();
			}
		}

		foreach ( var m in abilities )
		{
			if ( !m.IsActive && !m.AlwaysSimulate ) continue;
			m.PostSimulate();
		}

		var startOnGround = GroundEntity != null;
	}

	//public void Move( float groundAngle = 46f )
	//{
	//	MoveHelper mover = new MoveHelper( Position, Velocity );
	//	mover.Trace = mover.Trace.Size( Mins, Maxs ).Ignore( Pawn );
	//	mover.MaxStandableAngle = groundAngle;

	//	mover.TryMove( Time.Delta );

	//	Position = mover.Position;
	//	Velocity = mover.Velocity;
	//}

	public void ApplyFriction( float stopSpeed, float frictionAmount = 1.0f )
	{
		var speed = Velocity.Length;
		if ( speed < 0.1f ) return;

		var control = (speed < stopSpeed) ? stopSpeed : speed;
		var drop = control * Time.Delta * frictionAmount;

		// scale the velocity
		float newspeed = speed - drop;
		if ( newspeed < 0 ) newspeed = 0;

		if ( newspeed != speed )
		{
			newspeed /= speed;
			Velocity *= newspeed;
		}
	}
}
