using AppUser.ViewModels;

namespace AppUser.Pages
{
    public partial class EditProfilePage : ContentPage
    {
        public EditProfilePage(EditProfileViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
