using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppUser.Models;
using AppUser.Services;

namespace AppUser.ViewModels
{
    [QueryProperty(nameof(POI), "POI")]
    public partial class POIDetailViewModel : ObservableObject
    {
        private readonly AudioService _audioService;

        [ObservableProperty]
        private POIDto? pOI;

        [ObservableProperty]
        private AudioGuideDto? currentAudioGuide;

        [ObservableProperty]
        private string currentLanguage = "vi";

        [ObservableProperty]
        private bool hasAudio = false;

        public string DisplayName => POI?.DisplayName(CurrentLanguage) ?? string.Empty;
        public string DisplayDescription => POI?.DisplayDescription(CurrentLanguage) ?? string.Empty;

        public POIDetailViewModel(AudioService audio)
        {
            _audioService = audio;
            CurrentLanguage = _audioService.CurrentLanguage;
        }

        partial void OnPOIChanged(POIDto? value)
        {
            if (value != null)
            {
                CurrentAudioGuide = _audioService.GetGuideForPOI(value);
                HasAudio = CurrentAudioGuide != null;
                OnPropertyChanged(nameof(DisplayName));
                OnPropertyChanged(nameof(DisplayDescription));
            }
        }

        [RelayCommand]
        private async Task PlayAudioAsync()
        {
            if (POI == null || CurrentAudioGuide == null) return;

            _audioService.LoadGuide(CurrentAudioGuide, POI);

            await Shell.Current.GoToAsync("audioPlayer",
                new Dictionary<string, object>
                {
                    ["AudioGuide"] = CurrentAudioGuide,
                    ["POI"] = POI
                });
        }

        [RelayCommand]
        private void ToggleLanguage()
        {
            CurrentLanguage = CurrentLanguage == "vi" ? "en" : "vi";
            if (POI != null)
            {
                CurrentAudioGuide = _audioService.GetGuideForPOI(POI);
                HasAudio = CurrentAudioGuide != null;
            }
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(DisplayDescription));
        }

        [RelayCommand]
        private async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
