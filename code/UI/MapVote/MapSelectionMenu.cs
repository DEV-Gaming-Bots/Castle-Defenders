using System;
using System.Linq;
using Sandbox;
using Sandbox.UI;

public partial class MapSelectionMenu : Panel
{
	private Panel Maps { get; set; }

	public MapSelectionMenu()
	{
		
	}

	public override void Tick()
	{
		if ( MapVoteEntity.Current == null )
			return;

		// We are looping quite a lot in this code. Maybe we can use razor to make this less painful?
		var maps = Maps.ChildrenOfType<MapIcon>();
		foreach ( var icon in maps )
			icon.Votes = 0;

		foreach ( var group in MapVoteEntity.Current.Votes.GroupBy( x => x.Value ).OrderByDescending( x => x.Count() ) )
		{
			foreach ( var map in maps )
			{
				if ( group.Key == map.Ident )
					map.Votes = group.Count();
			}
		}
	}

	protected override int BuildHash() => HashCode.Combine( MapVoteEntity.Current.Votes.GetHashCode() );
}
