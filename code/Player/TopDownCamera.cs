using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using static Sandbox.Input;

public class TopDownCamera : CameraMode
{
	private float topDownHeight = 375;

	public override void Update()
	{
		if ( Local.Pawn is not AnimatedEntity pawn )
			return;

		Position = pawn.Position;

		var center = pawn.Position + Vector3.Up * topDownHeight;

		Position = center;
		Rotation = Rotation.FromPitch( 90 );
		
		FieldOfView = 70;

		Viewer = null;
	}

	public override void BuildInput( InputBuilder input )
	{
		topDownHeight -= input.MouseWheel * 5;

		topDownHeight = topDownHeight.Clamp( 325, 500 );

		input.ViewAngles = Angles.Zero;

		base.BuildInput( input );
	}
}
