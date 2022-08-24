using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using static Sandbox.Input;

public class TopDownCamera : CameraMode
{
	private Angles orbitAngles;
	private float orbitDistance = 150;
	private float orbitHeight = 0;

	public override void Update()
	{
		if ( Local.Pawn is not AnimatedEntity pawn )
			return;

		Position = pawn.Position;

		var center = pawn.Position + Vector3.Up * 375;

		Position = center;
		Rotation = Rotation.FromPitch( 90 );
		
		FieldOfView = 70;

		Viewer = null;
	}

	public override void BuildInput( InputBuilder input )
	{
		input.ViewAngles = Angles.Zero;

		base.BuildInput( input );
	}
}
