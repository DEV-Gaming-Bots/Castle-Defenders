using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public sealed partial class Jetpacker : BaseNPC
{
	public override string NPCName => "Jetpacker";
	public override float BaseHealth => 45;
	public override float BaseSpeed { get; set; } = 15.0f;
	public override string BaseModel => "models/citizen/citizen.vmdl";
	public override int[] MinMaxCashReward => new[] { 7, 25 };
	public override int[] MinMaxEXPReward => new[] { 6, 15 };
	public override float NPCScale => 0.5f;
	public override SpecialType NPCType => SpecialType.Airborne;
	public override float Damage => 7.5f;

	private Vector3 _inputVelocity;

	public override void Spawn()
	{
		base.Spawn();
	}

	public override void SetUpPanel()
	{
		Panel = new NPCInfo( NPCName, Health, "Use anti-airborne towers to attack!" );
	}

	[Event.Tick.Server]
	public override void Tick()
	{
		base.Tick();

		var animHelper = new NPCAnimationHelper( this );
		animHelper.DoFlying();
	}


	protected override void Move( float timeDelta )
	{
		var bbox = BBox.FromHeightAndRadius( 16, 4 );

		MoveHelper move = new( Position, Velocity ) { MaxStandableAngle = 50 };
		move.Trace = move.Trace.Ignore( this ).Size( bbox );

		_inputVelocity = Steer.Output.Direction.Normal;

		if ( !Velocity.IsNearlyZero( 0.001f ) )
		{
			move.TryUnstuck();
			move.TryMoveWithStep( timeDelta, 30 );
		}

		var tr = move.TraceDirection( Vector3.Down * 64.0f );

		if ( move.IsFloor( tr ) )
		{
			GroundEntity = tr.Entity;

			if ( !tr.StartedSolid )
			{
				move.Position = tr.EndPosition;
			}

			if ( _inputVelocity.Length > 0 )
			{
				var movement = move.Velocity.Dot( _inputVelocity.Normal );
				move.Velocity -= movement * _inputVelocity.Normal;
				move.ApplyFriction( tr.Surface.Friction * 10.0f, timeDelta );
				move.Velocity += movement * _inputVelocity.Normal;
			}
			else
			{
				move.ApplyFriction( tr.Surface.Friction * 10.0f, timeDelta );
			}
		}
		else
		{
			GroundEntity = null;
			move.Velocity += Vector3.Down * 900 * timeDelta;
		}

		Position = move.Position + Vector3.Up * 32;
		Velocity = move.Velocity;
	}
}
