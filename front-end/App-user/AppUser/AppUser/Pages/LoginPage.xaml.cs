using AppUser.ViewModels;

namespace AppUser.Pages
{
    public partial class LoginPage : ContentPage
    {
        private readonly LoginViewModel _vm;

        public LoginPage(LoginViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            BindingContext = vm;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _vm.ClearErrorCommand.Execute(null);
        }
    }
}
