using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.UI;
using Sandbox.UI.Construct;
using CastleDefenders.UI.Components;

public class LoadoutSelect : Panel
{
	public UserSelectUI.LoadoutSelectPanel LoadoutPanel = new();

	public LoadoutSelect()
	{
		Style.Set( "position: absolute; width: 100%; height: 100%;" );
		StyleSheet.Load( "UI/Components/UserLoadout.scss" );
		AddChild( LoadoutPanel );
	}
}
