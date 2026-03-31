using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppUser.Services;
using System.Text.RegularExpressions;

namespace AppUser.ViewModels
{
    public partial class EditProfileViewModel : ObservableObject
    {
        private readonly AuthService _authService;

        [ObservableProperty]
        private string fullName = string.Empty;

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string currentPassword = string.Empty;

        [ObservableProperty]
        private string newPassword = string.Empty;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        public EditProfileViewModel(AuthService authService)
        {
            _authService = authService;
            var user = _authService.GetCurrentUser();
            if (user != null)
            {
                FullName = user.FullName;
                Email = user.Email;
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (IsBusy) return;

            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(FullName))
            {
                ErrorMessage = "Vui lòng nhập họ và tên.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Email) || !Regex.IsMatch(Email, @"^[\w-\.]+@example\.com$"))
            {
                ErrorMessage = "Email không hợp lệ. Bắt buộc phải có đuôi @example.com";
                return;
            }

            if (!string.IsNullOrEmpty(NewPassword) && string.IsNullOrEmpty(CurrentPassword))
            {
                ErrorMessage = "Vui lòng nhập mật khẩu hiện tại để đổi mật khẩu mới.";
                return;
            }

            IsBusy = true;

            var (success, msg) = await _authService.UpdateProfileAsync(Email, FullName, CurrentPassword, NewPassword);

            IsBusy = false;

            if (success)
            {
                await Shell.Current.DisplayAlert("Thành công", msg, "OK");
                await Shell.Current.GoToAsync(".."); // Go back to profile
            }
            else
            {
                ErrorMessage = msg;
            }
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
