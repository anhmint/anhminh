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

        public string POIName => POI?.DisplayName() ?? string.Empty;
        public string GuideTitle => AudioGuide?.Title ?? string.Empty;
        public string ImageUrl => POI?.ImageUrl ?? string.Empty;

        // Available speed options
        public List<double> SpeedOptions = new() { 0.75, 1.0, 1.25, 1.5, 2.0 };

        public AudioPlayerViewModel(AudioService audio)
        {
            _audioService = audio;
        }

        partial void OnAudioGuideChanged(AudioGuideDto? value)
        {
            if (value != null)
            {
                DurationText = value.DurationDisplay;
                OnPropertyChanged(nameof(GuideTitle));
            }
        }

        partial void OnPOIChanged(POIDto? value)
        {
            OnPropertyChanged(nameof(POIName));
            OnPropertyChanged(nameof(ImageUrl));
        }

        [RelayCommand]
        private void TogglePlayPause()
        {
            IsPlaying = !IsPlaying;
            _audioService.SetPlayState(IsPlaying);
        }

        [RelayCommand]
        private void SeekBackward()
        {
            // Will trigger MediaElement seek via binding
            var newPos = Math.Max(0, Progress - 0.05);
            Progress = newPos;
        }

        [RelayCommand]
        private void SeekForward()
        {
            var newPos = Math.Min(1.0, Progress + 0.05);
            Progress = newPos;
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
            IsPlaying = false;
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
    }
}
