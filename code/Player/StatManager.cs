using System;
using System.Collections.Generic;
using Sandbox;

public partial class CDPawn : IPlayerData
{
	//OldEXP is used to display animations for the client
	//The rest is self-explanatory 
	[Net] protected int OldEXP { get; private set; }
	[Net] protected int EXP { get; private set; }
	[Net] protected int ReqEXP { get; private set; }
	[Net] protected int Level { get; private set; }
	[Net] protected int Cash { get; private set; }

	int IPlayerData.EXP { get => EXP; set => EXP = value; }
	int IPlayerData.ReqEXP { get => ReqEXP; set => ReqEXP = value; }
	int IPlayerData.Level { get => Level; set => Level = value; }
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

