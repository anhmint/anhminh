using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppUser.Models;
using AppUser.Services;
using System.Collections.ObjectModel;

namespace AppUser.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        private readonly POIService _poiService;
        private readonly AuthService _authService;
        private readonly AudioService _audioService;

        [ObservableProperty]
        private ObservableCollection<POIDto> featuredPOIs = new();

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private string greetingText = "Chào buổi tối!";

        [ObservableProperty]
        private string userEmail = string.Empty;

        [ObservableProperty]
        private string currentLanguage = "vi";

        public HomeViewModel(POIService poi, AuthService auth, AudioService audio)
        {
            _poiService = poi;
            _authService = auth;
            _audioService = audio;
        }

        public async Task InitializeAsync()
        {
            if (_authService.IsLoggedIn)
            {
                // Keep local user state in sync with backend (admin can disable accounts).
                var (success, _) = await _authService.RefreshMeAsync();
                if (!success)
                {
                    await _authService.LogoutAsync();
                    await Shell.Current.GoToAsync("//login");
                    return;
                }
            }

            UpdateGreeting();
            UserEmail = _authService.CurrentUser?.Email ?? string.Empty;
            await LoadFeaturedPOIsAsync();
        }

        [RelayCommand]
        private async Task LoadFeaturedPOIsAsync()
        {
            IsLoading = true;
            try
            {
                var pois = await _poiService.GetFeaturedPOIsAsync(5, CurrentLanguage);
                FeaturedPOIs.Clear();
                foreach (var p in pois)
                    FeaturedPOIs.Add(p);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task NavigateToPOIAsync(POIDto poi)
        {
            await Shell.Current.GoToAsync("poiDetail",
                new Dictionary<string, object> { ["POI"] = poi });
        }

        [RelayCommand]
        private async Task NavigateToAllPOIsAsync()
        {
            await Shell.Current.GoToAsync("//poiList");
        }

        [RelayCommand]
        private async Task PlayAudioDirect(POIDto poi)
        {
            if (poi == null) return;

            // Fetch full detail to get audio URL
            var fullPoi = await _poiService.GetPOIByIdAsync(poi.Id, CurrentLanguage);
            if (fullPoi == null || !fullPoi.AudioGuides.Any())
            {
                await Shell.Current.DisplayAlert("Không có audio", "Điểm ẩm thực này chưa có thuyết minh audio.", "OK");
                return;
            }

            var guide = _audioService.GetGuideForPOI(fullPoi) ?? fullPoi.AudioGuides.First();
            _audioService.LoadGuide(guide, fullPoi);

            await Shell.Current.GoToAsync("audioPlayer",
                new Dictionary<string, object>
                {
                    ["AudioGuide"] = guide,
                    ["POI"] = fullPoi
                });
        }

        [RelayCommand]
        private async Task ToggleLanguageAsync()
        {
            CurrentLanguage = CurrentLanguage switch
            {
                "vi" => "en",
                "en" => "zh",
                _ => "vi"
            };
            
            _audioService.SetLanguage(CurrentLanguage);
            UpdateGreeting();
            await LoadFeaturedPOIsAsync();
        }

        private void UpdateGreeting()
        {
            var hour = DateTime.Now.Hour;
            if (CurrentLanguage == "vi")
            {
                GreetingText = hour switch
                {
                    >= 5 and < 12 => "Chào buổi sáng!",
                    >= 12 and < 18 => "Chào buổi chiều!",
                    _ => "Chào buổi tối!"
                };
            }
            else if (CurrentLanguage == "en")
            {
                GreetingText = hour switch
                {
                    >= 5 and < 12 => "Good Morning!",
                    >= 12 and < 18 => "Good Afternoon!",
                    _ => "Good Evening!"
                };
            }
            else if (CurrentLanguage == "zh")
            {
                GreetingText = hour switch
                {
                    >= 5 and < 12 => "早上好!",
                    >= 12 and < 18 => "下午好!",
                    _ => "晚上好!"
                };
            }
        }
    }
}
