using AppUser.ViewModels;

namespace AppUser.Pages
{
    public partial class POIDetailPage : ContentPage
    {
        public POIDetailPage(POIDetailViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}
