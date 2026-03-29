using AppUser.ViewModels;

namespace AppUser.Pages
{
    public partial class POIListPage : ContentPage
    {
        private readonly POIListViewModel _vm;

        public POIListPage(POIListViewModel vm)
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
