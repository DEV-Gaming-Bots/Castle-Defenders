using Sandbox;

public class DuckController 
{
	public StandardController Controller { get; private set; }
	public CDPawn Player => Controller.Owner;
	public bool IsActive { get; private set; }

	public DuckController( StandardController controller )
	{
		Controller = controller;
	}

	public void PreTick()
	{
		bool wants = Input.Down( InputButton.Duck );

		if ( wants != IsActive )
		{
			if ( wants )
				TryDuck();
			else
				TryUnDuck();
		}

		if ( IsActive )
		{
			Controller.SetTag( "ducked" );
			Player.EyeLocalPosition *= 0.5f;
		}
	}

	private void TryDuck()
	{
		IsActive = true;
	}

	private void TryUnDuck()
	{
		var pm = Controller.TraceBBox( Player.Position, Player.Position, OriginalMins, OriginalMaxs );

		if ( pm.StartedSolid )
			return;

		IsActive = false;
	}

	private Vector3 OriginalMins { get; set; }
	private Vector3 OriginalMaxs { get; set; }

	internal void UpdateBBox( ref Vector3 mins, ref Vector3 maxs )
	{
		OriginalMins = mins;
		OriginalMaxs = maxs;

		if ( IsActive )
		{
			maxs = maxs.WithZ( 36f );
		}
	}

	public float GetWishSpeed()
	{
		if ( !IsActive )
			return -1f;

		return 64f;
	}
}
