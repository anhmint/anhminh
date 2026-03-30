using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppUser.Services;

namespace AppUser.ViewModels
{
    public partial class RegisterViewModel : ObservableObject
    {
        private readonly AuthService _authService;

        [ObservableProperty]
        private string fullName = string.Empty;

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private string confirmPassword = string.Empty;

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private bool hasError = false;

        [ObservableProperty]
        private bool isPasswordVisible = false;

        public RegisterViewModel(AuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        private async Task RegisterAsync()
        {
            if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ShowError("Vui lòng điền đầy đủ các thông tin bắt buộc.");
                return;
            }

            if (Password != ConfirmPassword)
            {
                ShowError("Mật khẩu xác nhận không khớp.");
                return;
            }

            if (Password.Length < 6)
            {
                ShowError("Mật khẩu phải có ít nhất 6 ký tự.");
                return;
            }

            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;

            try
            {
                var (success, message) = await _authService.RegisterAsync(Email.Trim(), Password, FullName.Trim());

                if (success)
                {
                    // Đăng ký thành công, thông báo và về trang login
                    await Shell.Current.DisplayAlert("Thành công", "Đăng ký tài khoản thành công! Vui lòng đăng nhập.", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    ShowError(message);
                }
            }
            catch (Exception ex)
            {
                ShowError("Đã xảy ra lỗi kết nối. Vui lòng thử lại sau.");
                System.Diagnostics.Debug.WriteLine($"Register error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task GoBackToLoginAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        private void TogglePasswordVisibility()
        {
            IsPasswordVisible = !IsPasswordVisible;
        }

        private void ShowError(string message)
        {
            ErrorMessage = message;
            HasError = true;
        }
    }
}
