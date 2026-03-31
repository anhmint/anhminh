using AppUser.Pages;

namespace AppUser
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register detail routes (not in tab bar)
            Routing.RegisterRoute("poiDetail", typeof(POIDetailPage));
            Routing.RegisterRoute("audioPlayer", typeof(AudioPlayerPage));
            Routing.RegisterRoute("register", typeof(RegisterPage));
            Routing.RegisterRoute("editProfile", typeof(EditProfilePage));
        }
    }
}
