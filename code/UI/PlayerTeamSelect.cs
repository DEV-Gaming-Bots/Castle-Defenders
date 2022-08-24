using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class PlayerTeamSelect : Panel
{
	public Panel BLUETEAM;
	public Panel REDTEAM;
	public Panel GameInfoPanel;
	public Label DifficultyText;

	public PlayerTeamSelect( Action OnBlueClick, Action OnRedClick )
	{
		StyleSheet.Load( "UI/PlayerTeamSelect.scss" );

		// gameinfo
		GameInfoPanel = Add.Panel( "gameinfo" );
		GameInfoPanel.Add.Label( "TEAM SELECT", "big text" );
		DifficultyText = GameInfoPanel.Add.Label( "Difficulty: -", "text" );
		// gameinfo end

		Panel centerPanel = Add.Panel( "centerPanel" );
		BLUETEAM = centerPanel.Add.Panel( "blue" );
		BLUETEAM.Add.Label( "BLUE", "text" );
		BLUETEAM.AddEventListener( "onClick", () =>
		{
			OnBlueClick();
		} );
		///////// ADD GAMEINFO PANEL //////////
		centerPanel.AddChild( GameInfoPanel );
		///////////////////////////////////////
		REDTEAM = centerPanel.Add.Panel( "red" );
		REDTEAM.Add.Label( "RED", "text" );
		REDTEAM.AddEventListener( "onClick", () =>
		{
			OnRedClick();
		} );


	}
}
