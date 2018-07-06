using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Animator))]
public class CChess : MonoBehaviour {

	public enum EChessState: byte {
		None = 0,
		RED = 1,
		BLUE = 2
	}
	[SerializeField]	protected int m_X = 0;
	public int posX { 
		get { return this.m_X; } 
		set { this.m_X = value; }
	}
	[SerializeField]	protected int m_Y = 0;
	public int posY { 
		get { return this.m_Y; } 
		set { this.m_Y = value; }
	}
	[SerializeField]	protected EChessState m_ChessState = EChessState.None;
	public EChessState chessState { 
		get { return this.m_ChessState; } 
		set { this.m_ChessState = value; } 
	}
	[SerializeField]	protected GameObject m_RedObject;
	[SerializeField]	protected GameObject m_BlueObject;

	protected CGameManager m_GameManager;
	protected Button m_Button;
	protected Animator m_Animator;

	protected virtual void Awake()
	{
		this.m_Button = this.GetComponent<Button> ();
		this.m_Animator = this.GetComponent<Animator> ();
	}

	protected virtual void Start()
	{
		this.InitChess();
		this.m_GameManager = CGameManager.GetInstance ();
		this.m_Button.interactable = true;
		this.m_Button.onClick.RemoveListener (this.ChangeState);
		this.m_Button.onClick.AddListener (this.ChangeState);
	}

	public virtual void InitChess() {
		// UPDATE STATE
		this.m_RedObject.SetActive (false);
		this.m_BlueObject.SetActive (false);
	}

	public virtual void ChangeState() {
		// UPDATE GAMEMANAGER 
		this.m_GameManager.OnUpdateGame (this.posX, this.posY);
	}

	public virtual void PlayAnimation() {
		if (this.m_Animator != null) {
			this.m_Animator.SetTrigger("Pop");
		}
	}

	public virtual void SetState(EChessState value) {
		// CHANGE STATE
		this.m_ChessState = value;
		// UPDATE STATE
		if (this.m_RedObject != null)
			this.m_RedObject.SetActive (this.m_ChessState == EChessState.RED);
		if (this.m_BlueObject != null)
			this.m_BlueObject.SetActive (this.m_ChessState == EChessState.BLUE);
		// END UPDATE STATE
		this.m_Button.interactable = false;
		// PLAY ANIMATION
		this.PlayAnimation();
	}

}
