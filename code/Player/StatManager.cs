using System;
using System.Collections.Generic;
using Sandbox;

public partial class CDPawn : IPlayerData
{
	//OldEXP is used to display animations for the client
	//The rest is self-explanatory 
	[Net] public int OldEXP { get; private set; }
	[Net] public int Cash { get; private set; }
	[Net] public int EXP { get; set; }
	[Net] public int ReqEXP { get; set; }
	[Net] public int Level { get; set; }
	public string[] TowerSlots { get; set; }

	public void NewPlayerStats()
	{
		Level = 1;
		EXP = 0;
		OldEXP = 0;
		ReqEXP = 1000;

		TowerSlots = new string[]
		{
			"Pistol",
			"Sniper",
			"RadioactiveEmitter",
			"Lightning"
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

	public void SetUpPlayer()
	{
		Cash = 50;
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
			//Incase the EXP goes over the next EXP requirements
			while ( EXP >= ReqEXP )
			{
				Level++;
				EXP -= ReqEXP;
				ReqEXP *= (int)2.5f;
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

