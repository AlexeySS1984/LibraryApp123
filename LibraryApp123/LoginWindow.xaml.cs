using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using LibraryApp123;

namespace libraryapp
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void DoLogin(object sender, RoutedEventArgs e)
        {
            var login = LoginLogin.Text.Trim();
            var password = LoginPassword.Password;
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль.", "Вход", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var user = Core.Context.AppUsers.FirstOrDefault(u => u.Login == login);
            if (user == null || !PasswordHelper.Verify(password, user.PasswordHash))
            {
                MessageBox.Show("Неверный логин или пароль.", "Вход", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            AppSession.SetUser(user);
            var main = new MainWindow();
            main.Show();
            Close();
        }

        private void DoRegister(object sender, RoutedEventArgs e)
        {
            var login = RegLogin.Text.Trim();
            var password = RegPassword.Password;
            var email = RegEmail.Text.Trim();
            var name = RegName.Text.Trim();
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Заполните все поля.", "Регистрация", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Core.Context.AppUsers.Any(u => u.Login == login))
            {
                MessageBox.Show("Пользователь с таким логином уже существует.", "Регистрация", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var user = new AppUsers
            {
                Login = login,
                PasswordHash = PasswordHelper.Hash(password),
                Email = email,
                DisplayName = name,
                RoleId = RoleIds.Reader,
                IsFrozen = false
            };
            Core.Context.AppUsers.Add(user);
            Core.Context.SaveChanges();

            MessageBox.Show("Регистрация выполнена. Перейдите на вкладку «Вход».", "Регистрация", MessageBoxButton.OK, MessageBoxImage.Information);
            ClearRegisterFields();
        }

        private void LinkToRegister_Click(object sender, RequestNavigateEventArgs e)
        {
            var tabControl = this.FindName("TabControl") as System.Windows.Controls.TabControl;
            if (tabControl != null)
            {
                tabControl.SelectedIndex = 1;
            }
            e.Handled = true;
        }

        private void LinkToLogin_Click(object sender, RequestNavigateEventArgs e)
        {
            var tabControl = this.FindName("TabControl") as System.Windows.Controls.TabControl;
            if (tabControl != null)
            {
                tabControl.SelectedIndex = 0;
            }
            e.Handled = true;
        }

        private void ClearRegisterFields()
        {
            RegLogin.Clear();
            RegEmail.Clear();
            RegName.Clear();
            RegPassword.Clear();
        }

        private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
}
