using Sandbox;

partial class MovementAbility : BaseNetworkable
{
	public virtual string HudName { get; protected set; }
	public virtual string HudDescription { get; protected set; }

	public bool IsActive { get; protected set; }
	public virtual bool AlwaysSimulate { get; private set; }
	public virtual bool TakesOverControl { get; }
	public TimeSince TimeSinceActivate { get; set; }

	public virtual float EyePosMultiplier => 1f;

	protected DeathmatchController ctrl;

	public MovementAbility( DeathmatchController controller )
	{
		ctrl = controller;
	}

	public bool Try()
	{
		IsActive = TryActivate();
		if ( IsActive )
		{
			TimeSinceActivate = 0;

			if ( BasePlayerController.Debug )
			{
				Log.Info( "ACTIVATED: " + GetType().Name );
			}
		}

		return IsActive;
	}

	public virtual void PreSimulate() { }
	public virtual void PostSimulate() { }
	public virtual void Simulate() { }
	public virtual void UpdateBBox( ref Vector3 mins, ref Vector3 maxs, float scale = 1f ) { }
	public virtual float GetWishSpeed() { return -1f; }
	protected virtual bool TryActivate() { return false; }
}
