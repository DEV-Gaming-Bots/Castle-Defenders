using System;
using System.Security.Authentication;

namespace Sandbox
{
	/// <summary>
	/// A struct to help you set up your citizen based animations
	/// </summary>
	public struct NPCAnimationHelper
	{
		private readonly AnimatedEntity _owner;


		public NPCAnimationHelper( AnimatedEntity entity )
		{
			_owner = entity;
		}

		public void DoFlying()
		{
			_owner.SetAnimParameter( "b_noclip", true );
		}

		/// <summary>
		/// Have the player look at this point in the world
		/// </summary>
		public void WithLookAt( Vector3 look, float eyesWeight = 1.0f, float headWeight = 1.0f, float bodyWeight = 1.0f )
		{
			_owner.SetAnimLookAt( "aim_eyes", look, _owner.Position );
			_owner.SetAnimLookAt( "aim_head", look, _owner.Position );
			_owner.SetAnimLookAt( "aim_body", look, _owner.Position );
			
			_owner.SetAnimParameter( "aim_head_weight", headWeight );
			_owner.SetAnimParameter( "aim_body_weight", bodyWeight );
		}

		public void WithVelocity( Vector3 Velocity )
		{
			var forward = _owner.Rotation.Forward.Dot( Velocity );
			var sideward = _owner.Rotation.Right.Dot( Velocity );

			var angle = MathF.Atan2( sideward, forward ).RadianToDegree().NormalizeDegrees();

			_owner.SetAnimParameter( "move_direction", angle );
			_owner.SetAnimParameter( "move_speed", Velocity.Length );
			_owner.SetAnimParameter( "move_groundspeed", Velocity.WithZ( 0 ).Length );
			_owner.SetAnimParameter( "move_y", sideward );
			_owner.SetAnimParameter( "move_x", forward );
			_owner.SetAnimParameter( "move_z", Velocity.z );
		}

		public void WithWishVelocity( Vector3 Velocity )
		{
			var forward = _owner.Rotation.Forward.Dot( Velocity );
			var sideward = _owner.Rotation.Right.Dot( Velocity );

			var angle = MathF.Atan2( sideward, forward ).RadianToDegree().NormalizeDegrees();

			_owner.SetAnimParameter( "wish_direction", angle );
			_owner.SetAnimParameter( "wish_speed", Velocity.Length );
			_owner.SetAnimParameter( "wish_groundspeed", Velocity.WithZ( 0 ).Length );
			_owner.SetAnimParameter( "wish_y", sideward );
			_owner.SetAnimParameter( "wish_x", forward );
			_owner.SetAnimParameter( "wish_z", Velocity.z );
		}
	}
}
