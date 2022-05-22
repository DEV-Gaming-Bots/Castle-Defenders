using Sandbox;
using System;
using System.Buffers;
public class NPCPathSteer
{
	protected NPCNavigation Path { get; private set; }

	public NPCPathSteer()
	{
		Path = new NPCNavigation();
	}

	public virtual void Tick( Vector3 currentPosition )
	{
		Path.Update( currentPosition, Target );
		
		Output.Finished = Path.IsEmpty;

		if ( Output.Finished )
		{
			Output.Direction = Vector3.Zero;
			return;
		}

		Output.Direction = Path.GetDirection( currentPosition );
	}

	public Vector3 Target { get; set; }

	public NavSteerOutput Output;


	public struct NavSteerOutput
	{
		public bool Finished;
		public Vector3 Direction;
	}
}
