using Sandbox;
using System.Collections.Generic;

public class NPCNavigation
{
	public Vector3 TargetPosition;
	public List<Vector3> Points = new List<Vector3>();

	public bool IsEmpty => Points.Count <= 1;

	public void Update( Vector3 from, Vector3 to )
	{
		bool needsBuild = false;

		if ( !TargetPosition.AlmostEqual( to, 5 ) )
		{
			TargetPosition = to;
			needsBuild = true;
		}

		if ( needsBuild )
		{
			var from_fixed = NavMesh.GetClosestPoint( from );
			var tofixed = NavMesh.GetClosestPoint( to );

			Points.Clear();
			NavMesh.GetClosestPoint( from );
			NavMesh.BuildPath( from_fixed.Value, tofixed.Value, Points );
			//Points.Add( NavMesh.GetClosestPoint( to ) );
		}

		if ( Points.Count <= 1 )
		{
			return;
		}

		var deltaToCurrent = from - Points[0];
		var deltaToNext = from - Points[1];
		var delta = Points[1] - Points[0];
		var deltaNormal = delta.Normal;

		if ( deltaToNext.WithZ( 0 ).Length < 20 )
		{
			Points.RemoveAt( 0 );
			return;
		}

		// If we're in front of this line then
		// remove it and move on to next one
		if ( deltaToNext.Normal.Dot( deltaNormal ) >= 1.0f )
		{
			Points.RemoveAt( 0 );
		}
	}

	public float Distance( int point, Vector3 from )
	{
		if ( Points.Count <= point ) return float.MaxValue;

		return Points[point].WithZ( from.z ).Distance( from );
	}

	public Vector3 GetDirection( Vector3 position )
	{
		if ( Points.Count == 1 )
		{
			return (Points[0] - position).WithZ( 0 ).Normal;
		}

		return (Points[1] - position).WithZ( 0 ).Normal;
	}
}
