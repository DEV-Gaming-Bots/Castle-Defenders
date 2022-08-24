﻿using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using static PlayerLoadout;

public partial class CDPawn : IPlayerData
{
	//OldEXP is used to display animations for the client
	//The rest is self-explanatory 
	[Net] public int OldEXP { get; private set; }
	[Net] public int Cash { get; private set; }
	[Net] public int EXP { get; set; }
	[Net] public int ReqEXP { get; set; }
	[Net] public int Level { get; set; }

	[Net, Local] public string[] TowerSlots { get; set; }

	[ConCmd.Server("cd_get_towerslots")]
	public static void GetSlots()
	{
		var player = ConsoleSystem.Caller.Pawn as CDPawn;
		int slotNum = 1;

		foreach ( var item in player.TowerSlots )
		{
			player.TestNetwork( To.Single( player ), item, slotNum );
			slotNum++;
		}
	}

	[ClientRpc]
	public void TestNetwork(string item, int slot)
	{
		AddSlot( new Slot( item, slot ) );
	}

	public void NewPlayerStats()
	{
		Level = 1;
		EXP = 0;
		OldEXP = 0;
		ReqEXP = 250;

		TowerSlots = new string[]
		{
			"Pistol",
			"Sniper",
			"RadioactiveEmitter",
			"Lightning",
			"TimeDisplacer" 
		};

		CDGame.Instance.SaveData( this );
	}

	public void LoadStats(IPlayerData playerData)
	{
		Level = playerData.Level;
		EXP = playerData.EXP;
		ReqEXP = playerData.ReqEXP;
		TowerSlots = playerData.TowerSlots;
	}

	[Net]
	public string GetSlotNet { get; private set; }

	public string GetSlotIndex(int index)
	{
		return "Pistol";
	}

	public void SetUpPlayer()
	{
		Cash = 60 + ((Level - 1) * 5);
		SelectedTower?.Delete();
		SelectedTower = null;
	}

	public void AddCash(int addCash)
	{
		Cash += addCash;
	}

	public void TakeCash( int subCash )
	{
		Cash -= subCash;
	}

	public void AddEXP(int addEXP)
	{
		EXP += addEXP;

		if(EXP >= ReqEXP)
		{
			PlaySoundOnClient( To.Single( this ), "level_up" );

			//Incase the EXP goes over the next EXP requirements
			while ( EXP >= ReqEXP )
			{
				Level++;
				EXP -= ReqEXP;
				ReqEXP += Level * 500;
			}
		}
	}
	public int GetCash()
	{
		return Cash;
	}

	public int GetEXP()
	{
		return EXP;
	}
	public int GetOldEXP()
	{
		return OldEXP;
	}

	public int GetReqEXP()
	{
		return ReqEXP;
	}
}

