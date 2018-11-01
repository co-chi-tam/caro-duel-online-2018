using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleSingleton;
using GoogleMobileAds.Api;

public class CAdmobManager {

#if UNITY_ANDROID
	public static string appId 	= "ca-app-pub-9073576735501135~2065653383";
	public static string adUnitId 	= "ca-app-pub-9073576735501135/4103114624";
#elif UNITY_IPHONE
	public static string appId 	= "ca-app-pub-9073576735501135~2065653383";
	public static string adUnitId 	= "ca-app-pub-9073576735501135/4103114624";
#else
	public static string appId 	= "ca-app-pub-9073576735501135~2065653383";
	public static string adUnitId 	= "ca-app-pub-9073576735501135/4103114624";
#endif

	protected static BannerView bannerView;

	public static void Init()
    {
        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(appId);
    }

	public static void InitBanner()
    {
        // Create a 320x50 banner at the bottom of the screen.
        bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);
		// Called when an ad request has successfully loaded.
        bannerView.OnAdLoaded -= HandleOnAdBannerLoaded;
        bannerView.OnAdLoaded += HandleOnAdBannerLoaded;
        // Called when an ad request failed to load.
        bannerView.OnAdFailedToLoad -= HandleOnAdBannerFailedToLoad;
        bannerView.OnAdFailedToLoad += HandleOnAdBannerFailedToLoad;
        // Called when an ad is clicked.
        bannerView.OnAdOpening -= HandleOnAdBannerOpening;
        bannerView.OnAdOpening += HandleOnAdBannerOpening;
        // Called when the user returned from the app after an ad click.
        bannerView.OnAdClosed -= HandleOnAdBannerClosed;
        bannerView.OnAdClosed += HandleOnAdBannerClosed;
        // Called when the ad click caused the user to leave the application.
        bannerView.OnAdLeavingApplication -= HandleOnAdBannerLeavingApplication;
        bannerView.OnAdLeavingApplication += HandleOnAdBannerLeavingApplication;
    }

    public static void LoadBanner()
    {
        if (bannerView != null)
        {
            // Create an empty ad request.
            AdRequest request = new AdRequest.Builder().Build();
            // Load the banner with the request.
            bannerView.LoadAd(request);
        }
    }

    public static void ShowHideBanner(bool value)
    {
        if (bannerView != null)
        {
            if (value)
            {
                bannerView.Show();
            }
            else
            {
                bannerView.Hide();
            }
        }
    }

    public static void DestroyBanner()
    {
        if (bannerView != null)
        {
            bannerView.OnAdLoaded -= HandleOnAdBannerLoaded;
            bannerView.OnAdFailedToLoad -= HandleOnAdBannerFailedToLoad;
            bannerView.OnAdOpening -= HandleOnAdBannerOpening;
            bannerView.OnAdClosed -= HandleOnAdBannerClosed;
            bannerView.OnAdLeavingApplication -= HandleOnAdBannerLeavingApplication;
            bannerView.Destroy();
        }
    }

	protected static void HandleOnAdBannerLoaded(object sender, EventArgs args)
    {
        Debug.LogWarning("HandleAdLoaded event received");
        ShowHideBanner(true);
    }

    protected static void HandleOnAdBannerFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        Debug.LogWarning("HandleFailedToReceiveAd event received with message: " + args.Message);
        LoadBanner();
    }

    protected static void HandleOnAdBannerOpening(object sender, EventArgs args)
    {
        Debug.LogWarning("HandleAdOpened event received");
    }

    protected static void HandleOnAdBannerClosed(object sender, EventArgs args)
    {
        Debug.LogWarning("HandleAdClosed event received");
    }

    protected static void HandleOnAdBannerLeavingApplication(object sender, EventArgs args)
    {
         Debug.LogWarning("HandleAdLeavingApplication event received");
    }

}
