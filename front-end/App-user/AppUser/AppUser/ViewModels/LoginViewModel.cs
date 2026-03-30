using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppUser.Services;

namespace AppUser.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly AuthService _authService;

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private bool hasError = false;

        [ObservableProperty]
        private bool isPasswordVisible = false;

        public LoginViewModel(AuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Vui lòng nhập đầy đủ email và mật khẩu.";
                HasError = true;
                return;
            }

            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;

            try
            {
                var (success, message) = await _authService.LoginAsync(Email.Trim(), Password);

                if (success)
                {
                    // Navigate to main app shell
                    await Shell.Current.GoToAsync("//home");
                }
                else
                {
                    ErrorMessage = message;
                    HasError = true;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Đã xảy ra lỗi. Vui lòng thử lại.";
                HasError = true;
                System.Diagnostics.Debug.WriteLine($"Login error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void TogglePasswordVisibility()
        {
            IsPasswordVisible = !IsPasswordVisible;
        }

        [RelayCommand]
        private void ClearError()
        {
            HasError = false;
            ErrorMessage = string.Empty;
        }

        [RelayCommand]
        private async Task NavigateToRegisterAsync()
        {
            await Shell.Current.GoToAsync("register");
        }
    }
}
