using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleSingleton;
using SocketIO;

public class CGameManager : CMonoSingleton<CGameManager> {

	#region Fields

	[SerializeField]	protected bool m_IsLocal;
	public bool isLocal { 
		get { return this.m_IsLocal; }
		set { this.m_IsLocal = value; }
	}

	// TURN INDEX.
	// TRUE is RED. FALSE is BLUE.
	[SerializeField]	protected bool m_TurnIndex = false;
	public bool turnIndex { 
		get { return this.m_TurnIndex; } 
		set { this.m_TurnIndex = value; } 
	}

	[SerializeField]	protected int m_CheckValue = 5;
	[SerializeField]	protected int m_MapColumn = 7;
	[SerializeField]	protected CChess[] m_ListChesses;
	public CChess[] listChesses {
		get { return this.m_ListChesses; }
	}

	protected CChess[,] m_MapChesses;
	public CChess[,] mapChesses {
		get { return this.m_MapChesses; }
	}

	protected CPlayer m_Player;
	protected int m_ChessPlayedCount = 0;

	#endregion

	#region MonoBehaviour Implementation

	protected override void Awake()
	{
		base.Awake();
	}

	protected virtual void Start() {
		if (this.m_IsLocal == false) {
			this.m_Player = CPlayer.GetInstance();
			this.m_Player.socket.On("receiveChessPosition", this.OnReceiveChessPosition);
		}
		this.InitGame ();
		this.OnStartGame ();
	}

	#endregion

	#region Main methods

	public virtual void InitGame() {
		this.m_TurnIndex = false;
		this.m_IsLocal = false;
		this.m_MapChesses = new CChess[this.m_MapColumn, this.m_MapColumn];
		for (int y = 0; y < this.m_MapColumn; y++)
		{
			for (int x = 0; x < this.m_MapColumn; x++)
			{
				var index = (y * this.m_MapColumn) + x;
				var cell = this.m_ListChesses[index];
				cell.posX = x;
				cell.posY = y;
				this.m_MapChesses[x, y] = cell;
			}
		}
		this.m_ChessPlayedCount = 0;
	}

	#endregion

	#region State Game

	public virtual void OnStartGame() {

	}

	public virtual void OnUpdateGame(int x, int y) {
		if (this.m_IsLocal == false) {
			this.m_Player.SendChessPosition(x, y);
		} else {
			var chess =	this.m_MapChesses[x, y];
			chess.SetState(this.m_TurnIndex ? CChess.EChessState.RED : CChess.EChessState.BLUE);
			this.CheckTurn(chess);
			this.ChangeTurn();
		}
	}

	protected virtual void OnReceiveChessPosition(SocketIOEvent e) {
		var currentPos = e.data.GetField("currentPos");
		var x = int.Parse (currentPos.GetField("x").ToString());
		var y = int.Parse (currentPos.GetField("y").ToString());
		var turnIndex = int.Parse (e.data.GetField("turnIndex").ToString());
		var chess =	this.m_MapChesses[x, y];
		this.m_TurnIndex = turnIndex == 1;
		chess.SetState(this.m_TurnIndex ? CChess.EChessState.RED : CChess.EChessState.BLUE);
		this.CheckTurn(chess);
		this.ChangeTurn();
		this.m_ChessPlayedCount++;
	}

	public virtual void OnEndGame() {
		Debug.Log ("AAAAAA WINNER IS " + (this.m_TurnIndex ? "RED" : "BLUE"));
		if (this.m_IsLocal == false) {
			var winnerName = this.m_Player.room.roomPlayes[this.m_TurnIndex ? 1 : 0].name;
			if (winnerName == this.m_Player.playerData.name) {
				this.m_Player.ShowMessage ("...YOU WIN...", this.OnResetGame);
			} else {
				this.m_Player.ShowMessage ("...YOU LOSE...", this.OnResetGame);
			}
		}
	}

	public virtual void OnResetGame() {
		if (this.m_IsLocal == false) {
			this.m_Player.LeaveRoom();
		} 
	}

	#endregion

	#region Logics game

	public virtual void ChangeTurn() {
		this.m_TurnIndex = !this.m_TurnIndex;
	}

	public virtual void CheckTurn(CChess value) {
		for (int i = 0; i < this.m_ListChesses.Length; i++)
		{
			var chess = this.m_ListChesses[i];
			if (chess.chessState == CChess.EChessState.None)
				continue;
			var results = this.CheckChess (chess);
			if (results != null && results.Count > 0) {	// WIN or LOSE or IS DRAW
				this.OnEndGame ();
				break;
			} 
		}
	}

	public virtual bool CheckChess (CChess value, out List<CChess> results) {
		results = new List<CChess> ();
		if (value.chessState == CChess.EChessState.None)
			return false;
		results = this.CheckChess (value);
		return results != null && results.Count > 0;
	}

	public virtual List<CChess> CheckChess (CChess value) {
		if (value == null)
			return null;
		if (value.chessState == CChess.EChessState.None)
			return null;
		var results = new List<CChess> ();
		var checkX = value.posX;
		var checkY = value.posY;
		var dimension = 1;
		while (dimension < 9) {
			switch (dimension)
			{
				// TOP == 1
				case 1:
				if (checkY - 1 >= 0) {
					var cell = this.m_MapChesses[checkX, checkY - 1];
					if (cell.chessState == value.chessState) {
						results.Add (cell);
						if (results.Count >= this.m_CheckValue - 1)
							return results;
						checkX = cell.posX;
						checkY = cell.posY;
					} else {
						results.Clear ();
						dimension += 1;
						checkX = value.posX;
						checkY = value.posY;
					}
				} else {
					results.Clear ();
					dimension += 1;
					checkX = value.posX;
					checkY = value.posY;
				}
				break;
				// TOP RIGHT == 2
				case 2:
				if (checkX + 1 < this.m_MapColumn && checkY - 1 >= 0) {
					var cell = this.m_MapChesses[checkX + 1, checkY - 1];
					if (cell.chessState == value.chessState) {
						results.Add (cell);
						if (results.Count >= this.m_CheckValue - 1)
							return results;
						checkX = cell.posX;
						checkY = cell.posY;
					} else {
						results.Clear ();
						dimension += 1;
						checkX = value.posX;
						checkY = value.posY;
					}
				} else {
					results.Clear ();
					dimension += 1;
					checkX = value.posX;
					checkY = value.posY;
				}
				break;
				// RIGHT == 3
				case 3:
				if (checkX + 1 < this.m_MapColumn) {
					var cell = this.m_MapChesses[checkX + 1, checkY];
					if (cell.chessState == value.chessState) {
						results.Add (cell);
						if (results.Count >= this.m_CheckValue - 1)
							return results;
						checkX = cell.posX;
						checkY = cell.posY;
					} else {
						results.Clear ();
						dimension += 1;
						checkX = value.posX;
						checkY = value.posY;
					} 
				} else {
					results.Clear ();
					dimension += 1;
					checkX = value.posX;
					checkY = value.posY;
				}
				break;
				// DOWN RIGHT == 4
				case 4:
				if (checkX + 1 < this.m_MapColumn && checkY + 1 < this.m_MapColumn) {
					var cell = this.m_MapChesses[checkX + 1, checkY + 1];
					if (cell.chessState == value.chessState) {
						results.Add (cell);
						if (results.Count >= this.m_CheckValue - 1)
							return results;
						checkX = cell.posX;
						checkY = cell.posY;
					} else {
						results.Clear ();
						dimension += 1;
						checkX = value.posX;
						checkY = value.posY;
					}
				} else {
					results.Clear ();
					dimension += 1;
					checkX = value.posX;
					checkY = value.posY;
				}
				break;
				// DOWN == 5
				case 5:
				if (checkY + 1 < this.m_MapColumn) {
					var cell = this.m_MapChesses[checkX, checkY + 1];
					if (cell.chessState == value.chessState) {
						results.Add (cell);
						if (results.Count >= this.m_CheckValue - 1)
							return results;
						checkX = cell.posX;
						checkY = cell.posY;
					} else {
						results.Clear ();
						dimension += 1;
						checkX = value.posX;
						checkY = value.posY;
					}
				} else {
					results.Clear ();
					dimension += 1;
					checkX = value.posX;
					checkY = value.posY;
				}
				break;
				// LEFT DOWN == 6
				case 6:
				if (checkX - 1 >= 0 && checkY + 1 < this.m_MapColumn) {
					var cell = this.m_MapChesses[checkX - 1, checkY + 1];
					if (cell.chessState == value.chessState) {
						results.Add (cell);
						if (results.Count >= this.m_CheckValue - 1)
							return results;
						checkX = cell.posX;
						checkY = cell.posY;
					} else {
						results.Clear ();
						dimension += 1;
						checkX = value.posX;
						checkY = value.posY;
					}
				} else {
					results.Clear ();
					dimension += 1;
					checkX = value.posX;
					checkY = value.posY;
				}
				break;
				// LEFT == 7
				case 7:
				if (checkX - 1 >= 0) {
					var cell = this.m_MapChesses[checkX - 1, checkY];
					if (cell.chessState == value.chessState) {
						results.Add (cell);
						if (results.Count >= this.m_CheckValue - 1)
							return results;
						checkX = cell.posX;
						checkY = cell.posY;
					} else {
						results.Clear ();
						dimension += 1;
						checkX = value.posX;
						checkY = value.posY;
					}
				} else {
					results.Clear ();
					dimension += 1;
					checkX = value.posX;
					checkY = value.posY;
				}
				break;
				// LEFT TOP == 8
				case 8:
				if (checkX - 1 >= 0 && checkY - 1 >= 0) {
					var cell = this.m_MapChesses[checkX - 1, checkY - 1];
					if (cell.chessState == value.chessState) {
						results.Add (cell);
						if (results.Count >= this.m_CheckValue - 1)
							return results;
						checkX = cell.posX;
						checkY = cell.posY;
					} else {
						results.Clear ();
						dimension += 1;
						checkX = value.posX;
						checkY = value.posY;
					}
				} else {
					results.Clear ();
					dimension += 1;
					checkX = value.posX;
					checkY = value.posY;
				}
				break;
			}
		}
		return results;
	}

	protected List<CChess> CheckNeighbours (CChess value) {
		var results = new List<CChess> ();
		if (value == null)
			return results;
		var checkX = value.posX;
		var checkY = value.posY;
		// TOP == 1
		if (checkY - 1 >= 0) {
			var cell = this.m_MapChesses[checkX, checkY - 1];
			if (cell.chessState != CChess.EChessState.None) {
				results.Add (cell);				
			}
		}
		// TOP RIGHT == 2
		if (checkX + 1 < this.m_MapColumn && checkY - 1 >= 0) {
			var cell = this.m_MapChesses[checkX + 1, checkY - 1];
			if (cell.chessState != CChess.EChessState.None) {
				results.Add (cell);				
			}
		}
		// RIGHT == 3
		if (checkX + 1 < this.m_MapColumn) {
			var cell = this.m_MapChesses[checkX + 1, checkY];
			if (cell.chessState != CChess.EChessState.None) {
				results.Add (cell);				
			}
		}
		// DOWN RIGHT == 4
		if (checkX + 1 < this.m_MapColumn && checkY + 1 < this.m_MapColumn) {
			var cell = this.m_MapChesses[checkX + 1, checkY + 1];
			if (cell.chessState != CChess.EChessState.None) {
				results.Add (cell);				
			}
		}
		// DOWN == 5
		if (checkY + 1 < this.m_MapColumn) {
			var cell = this.m_MapChesses[checkX, checkY + 1];
			if (cell.chessState != CChess.EChessState.None) {
				results.Add (cell);				
			}
		}
		// LEFT DOWN == 6
		if (checkX - 1 >= 0 && checkY + 1 < this.m_MapColumn) {
			var cell = this.m_MapChesses[checkX - 1, checkY + 1];
			if (cell.chessState != CChess.EChessState.None) {
				results.Add (cell);				
			}
		}
		// LEFT == 7
		if (checkX - 1 >= 0) {
			var cell = this.m_MapChesses[checkX - 1, checkY];
			if (cell.chessState != CChess.EChessState.None) {
				results.Add (cell);				
			}
		}
		// LEFT TOP == 8
		if (checkX - 1 >= 0 && checkY - 1 >= 0) {
			var cell = this.m_MapChesses[checkX - 1, checkY - 1];
			if (cell.chessState != CChess.EChessState.None) {
				results.Add (cell);				
			}
		}
		return results;
	}

	#endregion

	#region Getter && Setter

	public virtual void SetTurn (bool value) {
		this.m_TurnIndex = value;
	}

	public virtual bool IsRed() {
		return this.m_TurnIndex == true;
	}

	public virtual bool IsBlue() {
		return this.m_TurnIndex == false;
	}

	#endregion

}
