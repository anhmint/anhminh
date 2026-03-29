using AppUser.ViewModels;

namespace AppUser.Pages
{
    public partial class AudioPlayerPage : ContentPage
    {
        private readonly AudioPlayerViewModel _vm;

        public AudioPlayerPage(AudioPlayerViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            BindingContext = vm;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            UpdatePlayPauseButton();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        private void UpdatePlayPauseButton()
        {
            // Update play/pause icon based on state
            PlayPauseBtn.Text = _vm.IsPlaying ? "⏸" : "▶";
        }

        // Called when TogglePlayPause changes
        private void OnPlayStateChanged(object? sender, EventArgs e)
        {
            PlayPauseBtn.Text = _vm.IsPlaying ? "⏸" : "▶";
        }
    }
}
