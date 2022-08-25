﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class PlayerLoadout : Panel
{
	public static Panel Slots;

	static int lastSlot = -1;

	public PlayerLoadout()
	{
		StyleSheet.Load( "UI/PlayerLoadout.scss" );
		Slots = Add.Panel( "Slots" );

		ConsoleSystem.Run( "cd_get_towerslots" );
	}

	public static void AddSlot( Slot s )
	{
		Slots.AddChild( s );
	}

	public partial class Slot : Panel
	{
		public Slot( string slotName, int slotNum )
		{
			Panel Slot = Add.Panel( $"slot" );
			Slot.SetClass( slotName, true );
			Slot.Add.Label( $" {slotNum} ", "slot-num" );
		}
	}

	public static void SetSlot( int slotNum )
	{
		if ( CDGame.Instance.GameStatus == CDGame.GameEnum.MapChange )
			return;

		if ( lastSlot != -1 )
			Slots.GetChild( lastSlot ).GetChild( 0 ).SetClass( "selected", false );

		if( slotNum >= 0 )
			Slots.GetChild( slotNum ).GetChild( 0 ).SetClass( "selected", true );

		lastSlot = slotNum;
	}
}