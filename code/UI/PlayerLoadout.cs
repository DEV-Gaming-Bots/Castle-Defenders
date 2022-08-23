using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class PlayerLoadout : Panel
{
	public Panel Slots;

	public PlayerLoadout()
	{
		StyleSheet.Load( "UI/PlayerLoadout.scss" );
		Slots = Add.Panel( "Slots" );
		//for( int i = 0; i < 7; i++ )
		//{
		//	Panel Slot = Add.Panel($"slot");
		//	Slot.Add.Label( $" { i+1 } ", "slot-num" );
		//	//if (i == 4)
		//	//{
		//	//	Slot.AddClass( "selected" );
		//	//}
		//	Slots.AddChild( Slot );
		//}
		Slots.AddChild( new Slot( 1 ));
		Slots.AddChild( new Slot( 2 ));
		Slots.AddChild( new Slot( 3 ));
	}

	public partial class Slot : Panel
	{
		public Slot(int slotNum )
		{
			Panel Slot = Add.Panel( $"slot" );
			Slot.Add.Label( $" {slotNum} ", "slot-num" );
			//if (i == 4)
			//{
			//	Slot.AddClass( "selected" );
			//}
		}
	}
	/// <summary>
	/// Adds slots to the ui
	/// </summary>
	public void AddSlot( Slot s )
	{
		Slots.AddChild( s );
	}
}
