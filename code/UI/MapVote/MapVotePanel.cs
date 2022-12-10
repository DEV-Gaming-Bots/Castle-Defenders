using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

 class MapVotePanel : Panel
{
	public string TitleText { get; set; } = "Map Vote";
	public string SubtitleText { get; set; } = "Choose your next map";
	public string TimeText { get; set; } = "00:23";

	public Panel Body { get; set; }

	public List<MapIcon> MapIcons = new();

	public MapVotePanel()
	{
		_ = PopulateMaps();
	}

	public async Task PopulateMaps()
	{
		var query = await Package.FindAsync( "type:map order:user game:devbots.castletd, 8" );

		foreach ( var package in query.Packages )
		{
			AddMap( package.FullIdent );
		}
	}

	private MapIcon AddMap( string fullIdent )
	{
		var icon = MapIcons.FirstOrDefault( x => x.Ident == fullIdent );

		if ( icon != null )
			return icon;

		icon = new MapIcon( fullIdent );
		icon.AddEventListener( "onmousedown", () => MapVoteEntity.SetVote( fullIdent ) );
		Body.AddChild( icon );

		MapIcons.Add( icon );
		return icon;
	}

	internal void UpdateFromVotes( IDictionary<IClient, string> votes )
	{
		foreach ( var icon in MapIcons )
			icon.VoteCount = "0";

		foreach ( var group in votes.GroupBy( x => x.Value ).OrderByDescending( x => x.Count() ) )
		{
			var icon = AddMap( group.Key );
			icon.VoteCount = group.Count().ToString( "n0" );
		}
	}
}

