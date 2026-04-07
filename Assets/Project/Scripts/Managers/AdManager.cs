using System;
using UnityEngine;
using GoogleMobileAds.Api;
using System.Collections;

public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance { get; private set; }

    [Header("Ad Unit IDs")]
    private readonly string bannerId = "ca-app-pub-1945244255127558/2361559821";
    private readonly string interId = "ca-app-pub-1945244255127558/9815160974";
    private readonly string rewardId = "ca-app-pub-1945244255127558/1375781213";

    private BannerView _bannerView;
    private InterstitialAd _interAd;
    private RewardedAd _rewardedAd;

    private bool _isInitialized = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        MobileAds.Initialize(initStatus =>
        {
            _isInitialized = true;

            LoadAllAds();
        });
    }

    private void LoadAllAds()
    {
        LoadBannerAd();
        LoadInterstitialAd();
        LoadRewardedAd();
    }

    private AdRequest CreateAdRequest() => new AdRequest();

    public void LoadBannerAd()
    {
        DestroyBanner();
        _bannerView = new BannerView(bannerId, AdSize.Banner, AdPosition.Bottom);

        _bannerView.OnBannerAdLoadFailed += (error) =>
        {
            StartCoroutine(RetryLoad(LoadBannerAd, 15f));
        };

        _bannerView.LoadAd(CreateAdRequest());
    }

    public void ShowBanner() => _bannerView?.Show();
    public void HideBanner() => _bannerView?.Hide();

    public void DestroyBanner()
    {
        _bannerView?.Destroy();
        _bannerView = null;
    }

    public void LoadInterstitialAd()
    {
        DestroyInterstitial();
        InterstitialAd.Load(interId, CreateAdRequest(), (ad, error) =>
        {
            if (error != null || ad == null)
            {
                StartCoroutine(RetryLoad(LoadInterstitialAd, 15f));
                return;
            }
            _interAd = ad;
            _interAd.OnAdFullScreenContentClosed += LoadInterstitialAd;
            _interAd.OnAdFullScreenContentFailed += (e) => LoadInterstitialAd();
        });
    }

    public void ShowInterstitial()
    {
        if (_interAd != null && _interAd.CanShowAd())
            _interAd.Show();
        else
            LoadInterstitialAd();
    }

    private void DestroyInterstitial()
    {
        _interAd?.Destroy();
        _interAd = null;
    }

    public void LoadRewardedAd()
    {
        DestroyRewarded();
        RewardedAd.Load(rewardId, CreateAdRequest(), (ad, error) =>
        {
            if (error != null || ad == null)
            {
                StartCoroutine(RetryLoad(LoadRewardedAd, 15f));
                return;
            }
            _rewardedAd = ad;
            _rewardedAd.OnAdFullScreenContentClosed += LoadRewardedAd;
            _rewardedAd.OnAdFullScreenContentFailed += (e) => LoadRewardedAd();
        });
    }

    //public void ShowRewarded()
    //{
    //    if (_rewardedAd != null && _rewardedAd.CanShowAd())
    //    {
    //        _rewardedAd.Show(reward =>
    //        {

    //            if (ScoreManager.Instance != null)
    //            {
    //                ScoreManager.Instance.AddAppleScore(50);
    //            }
    //        });
    //    }
    //    else
    //    {
    //        LoadRewardedAd();
    //    }
    //}
    // Bắt buộc phải có chữ Action ở trong ngoặc
    public void ShowRewarded(Action onRewardEarned)
    {
        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            _rewardedAd.Show(reward =>
            {
                // Lệnh này có nghĩa là: Chạy xong video thì kích hoạt phần thưởng
                onRewardEarned?.Invoke();
            });
        }
        else
        {
            Debug.Log("Video chưa tải xong, đang tải lại...");
            LoadRewardedAd();
        }
    }

    private void DestroyRewarded()
    {
        _rewardedAd?.Destroy();
        _rewardedAd = null;
    }

    private IEnumerator RetryLoad(Action reloadAction, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (_isInitialized) reloadAction?.Invoke();
    }

    private void OnDestroy()
    {
        DestroyBanner();
        DestroyInterstitial();
        DestroyRewarded();
    }
}