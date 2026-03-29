using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppUser.Models;
using AppUser.Services;
using System.Collections.ObjectModel;

namespace AppUser.ViewModels
{
    public partial class POIListViewModel : ObservableObject
    {
        private readonly POIService _poiService;
        private readonly AudioService _audioService;
        private List<POIDto> _allPOIs = new();

        [ObservableProperty]
        private ObservableCollection<POIDto> filteredPOIs = new();

        [ObservableProperty]
        private string searchQuery = string.Empty;

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private bool isEmpty = false;

        [ObservableProperty]
        private string currentLanguage = "vi";

        public POIListViewModel(POIService poi, AudioService audio)
        {
            _poiService = poi;
            _audioService = audio;
            CurrentLanguage = _audioService.CurrentLanguage;
        }

        public async Task InitializeAsync()
        {
            await LoadAllPOIsAsync();
        }

        [RelayCommand]
        private async Task LoadAllPOIsAsync()
        {
            IsLoading = true;
            try
            {
                _allPOIs = await _poiService.GetAllPOIsAsync();
                ApplyFilter();
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task SearchAsync()
        {
            IsLoading = true;
            try
            {
                var results = await _poiService.SearchPOIsAsync(SearchQuery, CurrentLanguage);
                FilteredPOIs.Clear();
                foreach (var p in results)
                    FilteredPOIs.Add(p);
                IsEmpty = !FilteredPOIs.Any();
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
        private async Task RefreshAsync()
        {
            SearchQuery = string.Empty;
            await LoadAllPOIsAsync();
        }

        partial void OnSearchQueryChanged(string value)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            var query = SearchQuery.ToLower().Trim();
            var filtered = string.IsNullOrEmpty(query)
                ? _allPOIs
                : _allPOIs.Where(p =>
                    p.DisplayName(CurrentLanguage).ToLower().Contains(query) ||
                    (p.Location?.ToLower().Contains(query) ?? false) ||
                    (p.Shop?.Name.ToLower().Contains(query) ?? false)).ToList();

            FilteredPOIs.Clear();
            foreach (var p in filtered)
                FilteredPOIs.Add(p);
            IsEmpty = !FilteredPOIs.Any();
        }
    }
}
