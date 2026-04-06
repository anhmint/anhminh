using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppUser.Models;
using AppUser.Services;

namespace AppUser.ViewModels
{
    [QueryProperty(nameof(AudioGuide), "AudioGuide")]
    [QueryProperty(nameof(POI), "POI")]
    public partial class AudioPlayerViewModel : ObservableObject
    {
        private readonly AudioService _audioService;

        [ObservableProperty]
        private AudioGuideDto? audioGuide;

        [ObservableProperty]
        private POIDto? pOI;

        [ObservableProperty]
        private bool isPlaying = false;

        [ObservableProperty]
        private double progress = 0;

        [ObservableProperty]
        private string positionText = "00:00";

        [ObservableProperty]
        private string durationText = "00:00";

        [ObservableProperty]
        private double playbackSpeed = 1.0;

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private string nowPlayingLabel = "ĐANG THƯỞNG THỨC";
        [ObservableProperty]
        private string subTitleLabel = "Thuyết minh văn hóa ẩm thực";

        [ObservableProperty]
        private string currentLangCode = "vi";

        [ObservableProperty]
        private bool isReviewDialogVisible = false;

        [ObservableProperty]
        private int reviewRating = 5;

        [ObservableProperty]
        private string reviewComment = string.Empty;

        private bool _hasTrackedListen = false;

        private readonly POIService _poiService;

        public string POIName => POI?.DisplayName(CurrentLangCode) ?? string.Empty;
        public string GuideTitle => AudioGuide?.Title ?? string.Empty;
        public string ImageUrl => POI?.ImageUrl ?? string.Empty;
        public List<AppLanguageDto> AvailableLanguages => POI?.AvailableLanguages ?? new();
        public bool HasMultipleLanguages => AvailableLanguages.Count > 1;

        // Audio URL resolved to absolute URL for playback
        public string AudioUrl
        {
            get
            {
                if (AudioGuide == null || string.IsNullOrWhiteSpace(AudioGuide.AudioUrl)) 
                    return string.Empty;
                
                return AppConfig.ResolveUrl(AudioGuide.AudioUrl);
            }
        }

        // Events for code-behind to control MediaElement
        public event EventHandler? PlayRequested;
        public event EventHandler? PauseRequested;
        public event EventHandler<double>? SeekRequested;
        public event EventHandler? SourceChanged;

        // Available speed options
        public List<double> SpeedOptions = new() { 0.75, 1.0, 1.25, 1.5, 2.0 };

        public AudioPlayerViewModel(AudioService audio, POIService poiService)
        {
            _audioService = audio;
            _poiService = poiService;
        }

        partial void OnAudioGuideChanged(AudioGuideDto? value)
        {
            if (value != null)
            {
                System.Diagnostics.Debug.WriteLine($"[AudioPlayerVM] AudioGuide changed: {value.Title}, URL: {value.AudioUrl}");
                DurationText = value.DurationDisplay;
                OnPropertyChanged(nameof(GuideTitle));
                OnPropertyChanged(nameof(AudioUrl));
                
                // Notify code-behind that source changed
                SourceChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        partial void OnPOIChanged(POIDto? value)
        {
            _hasTrackedListen = false; // Reset when POI changes
            if (value != null && value.AudioGuides.Any())
            {
                var guideByLang = value.AudioGuides.FirstOrDefault(g => g.LanguageCode == CurrentLangCode)
                    ?? value.AudioGuides.FirstOrDefault();
                if (guideByLang != null)
                {
                    AudioGuide = guideByLang;
                }
            }
            OnPropertyChanged(nameof(POIName));
            OnPropertyChanged(nameof(ImageUrl));
            OnPropertyChanged(nameof(AvailableLanguages));
            OnPropertyChanged(nameof(HasMultipleLanguages));
        }

        [RelayCommand]
        private async Task SwitchLanguage(AppLanguageDto lang)
        {
            if (lang == null || lang.Code == CurrentLangCode || !lang.HasAudio) return;

            IsLoading = true;
            try
            {
                PauseRequested?.Invoke(this, EventArgs.Empty);
                
                // Reload POI data with new language
                var updatedPoi = await _poiService.GetPOIByIdAsync(POI!.Id, lang.Code);
                if (updatedPoi != null)
                {
                    CurrentLangCode = lang.Code;
                    POI = updatedPoi;
                    
                    if (updatedPoi.AudioGuides.Any())
                    {
                        AudioGuide = updatedPoi.AudioGuides.First();
                    }
                }
            }
            finally
            {
                IsLoading = false;
                UpdateLocalization();
            }
        }

        private void UpdateLocalization()
        {
            if (CurrentLangCode == "vi")
            {
                NowPlayingLabel = "ĐANG THƯỞNG THỨC";
                SubTitleLabel = "Thuyết minh văn hóa ẩm thực";
            }
            else if (CurrentLangCode == "zh")
            {
                NowPlayingLabel = "正在播放";
                SubTitleLabel = "饮食文化语音讲解";
            }
            else // Default to English
            {
                NowPlayingLabel = "NOW PLAYING";
                SubTitleLabel = "Culinary Culture Guide";
            }
        }

        [RelayCommand]
        private void TogglePlayPause()
        {
            if (IsPlaying)
                PauseRequested?.Invoke(this, EventArgs.Empty);
            else
                PlayRequested?.Invoke(this, EventArgs.Empty);
        }

        public void CheckAndTrackListen()
        {
            if (!_hasTrackedListen && POI != null)
            {
                _hasTrackedListen = true;
                _ = _poiService.TrackListenAsync(POI.Id);
            }
        }

        [RelayCommand]
        private void SeekBackward()
        {
            var newPos = Math.Max(0, Progress - 0.05);
            SeekRequested?.Invoke(this, newPos);
        }

        [RelayCommand]
        private void SeekForward()
        {
            var newPos = Math.Min(1.0, Progress + 0.05);
            SeekRequested?.Invoke(this, newPos);
        }

        [RelayCommand]
        private void CycleSpeed()
        {
            var idx = SpeedOptions.IndexOf(PlaybackSpeed);
            PlaybackSpeed = SpeedOptions[(idx + 1) % SpeedOptions.Count];
        }

        [RelayCommand]
        private async Task GoBackAsync()
        {
            PauseRequested?.Invoke(this, EventArgs.Empty);
            _audioService.SetPlayState(false);
            await Shell.Current.GoToAsync("..");
        }

        /// <summary>Called from code-behind when MediaElement position changes</summary>
        public void UpdateProgress(TimeSpan position, TimeSpan duration)
        {
            _audioService.Position = position;
            _audioService.Duration = duration;
            Progress = _audioService.Progress;
            PositionText = $"{(int)position.TotalMinutes:D2}:{position.Seconds:D2}";
            if (duration != TimeSpan.Zero)
                DurationText = $"{(int)duration.TotalMinutes:D2}:{duration.Seconds:D2}";
        }

        [RelayCommand]
        private void ShowReview()
        {
            IsReviewDialogVisible = true;
        }

        [RelayCommand]
        private void CancelReview()
        {
            IsReviewDialogVisible = false;
        }

        [RelayCommand]
        private async Task SubmitReview()
        {
            if (POI != null)
            {
                var (success, message) = await _poiService.SubmitReviewWithResultAsync(POI.Id, ReviewRating, ReviewComment);
                if (!success)
                {
                    await Shell.Current.DisplayAlert("Không thể gửi đánh giá", message ?? "Vui lòng thử lại sau.", "OK");
                    return;
                }
            }
            IsReviewDialogVisible = false;
        }
    }
}
