using AppUser.ViewModels;

namespace AppUser.Pages
{
    public partial class POIDetailPage : ContentPage
    {
        private readonly POIDetailViewModel _vm;

        public POIDetailPage(POIDetailViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            BindingContext = vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _vm.InitializeAsync();
        }
    }
}
