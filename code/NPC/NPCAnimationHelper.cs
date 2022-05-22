using System;

namespace Sandbox
{
	/// <summary>
	/// A struct to help you set up your citizen based animations
	/// </summary>
	public struct NPCAnimationHelper
	{
		AnimatedEntity Owner;

		public NPCAnimationHelper( AnimatedEntity entity )
		{
			Owner = entity;
		}

		/// <summary>
		/// Have the player look at this point in the world
		/// </summary>
		public void WithLookAt( Vector3 look, float eyesWeight = 1.0f, float headWeight = 1.0f, float bodyWeight = 1.0f )
		{
			Owner.SetAnimLookAt( "aim_eyes", look );
			Owner.SetAnimLookAt( "aim_head", look );
			Owner.SetAnimLookAt( "aim_body", look );
			
			Owner.SetAnimParameter( "aim_head_weight", headWeight );
			Owner.SetAnimParameter( "aim_body_weight", bodyWeight );
		}

		public void WithVelocity( Vector3 Velocity )
		{
			var dir = Velocity;
			var forward = Owner.Rotation.Forward.Dot( dir );
			var sideward = Owner.Rotation.Right.Dot( dir );

			var angle = MathF.Atan2( sideward, forward ).RadianToDegree().NormalizeDegrees();

			Owner.SetAnimParameter( "move_direction", angle );
			Owner.SetAnimParameter( "move_speed", Velocity.Length );
			Owner.SetAnimParameter( "move_groundspeed", Velocity.WithZ( 0 ).Length );
			Owner.SetAnimParameter( "move_y", sideward );
			Owner.SetAnimParameter( "move_x", forward );
			Owner.SetAnimParameter( "move_z", Velocity.z );
		}

		public void WithWishVelocity( Vector3 Velocity )
		{
			var dir = Velocity;
			var forward = Owner.Rotation.Forward.Dot( dir );
			var sideward = Owner.Rotation.Right.Dot( dir );

			var angle = MathF.Atan2( sideward, forward ).RadianToDegree().NormalizeDegrees();

			Owner.SetAnimParameter( "wish_direction", angle );
			Owner.SetAnimParameter( "wish_speed", Velocity.Length );
			Owner.SetAnimParameter( "wish_groundspeed", Velocity.WithZ( 0 ).Length );
			Owner.SetAnimParameter( "wish_y", sideward );
			Owner.SetAnimParameter( "wish_x", forward );
			Owner.SetAnimParameter( "wish_z", Velocity.z );
		}
	}
}
