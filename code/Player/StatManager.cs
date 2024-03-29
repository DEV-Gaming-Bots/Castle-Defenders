﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using static PlayerLoadout;

public  partial class CDPawn : IPlayerData
{
	[Net] public int Cash { get; private set; }
	[Net] public int EXP { get; set; }
	[Net] public int ReqEXP { get; set; }
	[Net] public int Level { get; set; }
	[Net] public int TotalDamage { get; set; }

	[Net] public IDictionary<int, string> TowerSlots { get; set; }

	[ClientRpc]
	public void UpdateSlots(string item, int slot)
	{
		AddSlot( new Slot( item, slot ) );
	}

	[ClientRpc]
	public void ClearTowerSlots()
	{
		ClearSlots();
	}

	[ClientRpc]
	public void ChangeSlot(string name, int slot)
	{
		ReplaceSlot( slot, name );
	}

	public void NewPlayerStats()
	{
		Level = 1;
		EXP = 0;
		ReqEXP = 500;

		TowerSlots.Clear();

		TowerSlots.Add( 0, "Pistol" );
		TowerSlots.Add( 1, "SMG" );
		TowerSlots.Add( 2, "Shotgun" );

		if(CDGame.DataConfig == CDGame.DataCFGEnum.Host)
			CDGame.Instance.SaveData( this );
		else if (CDGame.DataConfig == CDGame.DataCFGEnum.Web)
			CDGame.Instance.SaveToOnlineDatabase( this );

		UpdateTowerSlots();
	}

	public void AddTowerSlots(string newTower)
	{
		foreach ( var item in TowerSlots )
		{
			if ( item.Value == newTower )
				return;
		}

		TowerSlots.Add( TowerSlots.Count, newTower );
		UpdateTowerSlots();
	}
	public void UpdateTowerSlots()
	{
		ClearTowerSlots(To.Single(this));
		var slotNum = 1;

		foreach ( var item in TowerSlots )
		{
			UpdateSlots( To.Single( this ), item.Value, item.Key + 1 );
			slotNum++;
		}

		UpdateSlots( To.Single( this ), "Hands", 0 );
	}

	public void LoadStats(IPlayerData playerData)
	{
		Level = playerData.Level;
		EXP = playerData.EXP;
		ReqEXP = playerData.ReqEXP;
		TowerSlots = playerData.TowerSlots;

		UpdateTowerSlots();
	}

	[Net]
	public string GetSlotNet { get; private set; }

	public string GetSlotIndex(int index)
	{
		return TowerSlots[index];
	}

	public void SetUpPlayer()
	{
		if ( CDGame.Instance.LoopedTimes > 1 )
			return;

		Cash = 60 + (Level - 1) * 10;

		SelectedTower?.Delete();
		SelectedTower = null;

		if ( CDGame.Instance.Competitive )
		{
			while(CurTeam == TeamEnum.Unknown)
			{
				var blueCount = All.OfType<CDPawn>().Count( x => x.CurTeam == TeamEnum.Blue );
				var redCount = All.OfType<CDPawn>().Count( x => x.CurTeam == TeamEnum.Red );
				
				switch ( Game.Random.Int(1, 2))
				{
					case 1:
						if( blueCount > redCount )
						{
							CurTeam = TeamEnum.Red;
							break;
						}
						CurTeam = TeamEnum.Blue;
						break;

					case 2:
						if ( redCount > blueCount )
						{
							CurTeam = TeamEnum.Blue;
							break;
						}

						CurTeam = TeamEnum.Red;
						break;
				}
			}

			OnOtherTeamSide = false;

			if ( CurTeam == TeamEnum.Red )
				Transform = All.OfType<OpposingSpawnpoint>().OrderBy( x => Guid.NewGuid() ).FirstOrDefault().Transform;
		}
	}

	public void AddCash(int addCash)
	{
		Cash += addCash;
		Cash = Cash.Clamp( 0, 5000 );
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
				ReqEXP += (int)(4 * Math.Pow( Level + 3, 2 ) );
				UnlockTowerCheck();
			}
		}
	}

	public void UnlockTowerCheck()
	{
		foreach ( var tower in TypeLibrary.GetTypes<BaseTower>() )
		{
			if ( tower.ClassName.Contains( "TemplateTower" )
				|| tower.ClassName.Contains( "BaseTower" )
				|| tower.ClassName.Contains( "BaseSuperTower" )
				)
				continue;

			if( Level >= BaseTower.GetUnlockLevel(tower.ClassName) )
			{
				AddTowerSlots( tower.ClassName );
			}
		}
	}

	public int GetDamage()
	{
		return TotalDamage;
	}
	public int GetCash()
	{
		return Cash;
	}

	public int GetEXP()
	{
		return EXP;
	}

	public int GetLevel()
	{
		return Level;
	}

	public int GetReqEXP()
	{
		return ReqEXP;
	}
}

