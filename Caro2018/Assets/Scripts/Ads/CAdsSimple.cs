using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.Events;

public class CAdsSimple : MonoBehaviour {

	#if UNITY_IOS
   	[SerializeField]	protected string gameId = "2663901";
    #elif UNITY_ANDROID
    [SerializeField]	protected string gameId = "2663901";
    #endif
	[SerializeField]	protected string placementId = "video"; // "rewardedVideo";

	[Header("Events")]
	public UnityEvent OnShow;
	public UnityEvent OnFisnish;
	public UnityEvent OnSkip;
	public UnityEvent OnFail;

	protected virtual void Start() {
		this.InitAds ();
	}

	public virtual void InitAds() {
		if (Advertisement.isSupported) {
            Advertisement.Initialize (this.gameId, true);
        }
	}

	public virtual void Show() {
		if (Advertisement.IsReady() == false)
			return;	
		ShowOptions options = new ShowOptions();
        options.resultCallback = this.HandleShowResult;
        Advertisement.Show(this.placementId, options);
		if (this.OnShow != null) {
			this.OnShow.Invoke ();
		}
	}

	public virtual void ShowRandomRate(float percentRate = 50.0f) {
		var random = Random.Range(0f, 100f);
		if (random <= percentRate) {
			this.Show();
		} else {
			if (this.OnFisnish != null) {
				this.OnFisnish.Invoke ();
			}
		}
	}

	protected void HandleShowResult (ShowResult result)
    {
        if(result == ShowResult.Finished) {
        	Debug.Log("Video completed - Offer a reward to the player");
			if (this.OnFisnish != null) {
				this.OnFisnish.Invoke ();
			}
        }else if(result == ShowResult.Skipped) {
            Debug.LogWarning("Video was skipped - Do NOT reward the player");
			if (this.OnSkip != null) {
				this.OnSkip.Invoke ();
			}
        }else if(result == ShowResult.Failed) {
            Debug.LogError("Video failed to show");
			if (this.OnFail != null) {
				this.OnFail.Invoke ();
			}
        }
    }

}
