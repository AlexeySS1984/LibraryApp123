using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LibraryApp123;

namespace libraryapp.Pages
{
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) => Reload();

        private void Reload()
        {
            Root.Children.Clear();
            var u = Core.Context.AppUsers.Include(x => x.Roles).First(x => x.UserId == AppSession.CurrentUser.UserId);

            Root.Children.Add(new TextBlock 
            { 
                Text = "Профиль", 
                FontSize = 26, 
                FontWeight = FontWeights.Bold,
                Foreground = Application.Current.Resources["InkBrush"] as Brush
            });
            Root.Children.Add(new TextBlock 
            { 
                Text = "Ваша информация и статус в системе", 
                FontSize = 13,
                Foreground = Application.Current.Resources["MutedBrush"] as Brush,
                Margin = new Thickness(0, 4, 0, 16)
            });

            var card = new Border
            {
                Background = Application.Current.Resources["CardBrush"] as Brush,
                BorderBrush = Application.Current.Resources["BorderSubtleBrush"] as Brush,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(16),
                Margin = new Thickness(0, 0, 0, 20),
                CornerRadius = new System.Windows.CornerRadius(8)
            };

            var cardSp = new StackPanel();
            cardSp.Children.Add(new TextBlock 
            { 
                Text = "Имя: " + u.DisplayName, 
                FontSize = 12,
                Margin = new Thickness(0, 0, 0, 8)
            });
            cardSp.Children.Add(new TextBlock 
            { 
                Text = "Логин: " + u.Login, 
                FontSize = 12,
                Margin = new Thickness(0, 0, 0, 8)
            });
            cardSp.Children.Add(new TextBlock 
            { 
                Text = "Электронная почта: " + u.Email, 
                FontSize = 12,
                Margin = new Thickness(0, 0, 0, 8)
            });
            cardSp.Children.Add(new TextBlock 
            { 
                Text = "Роль: " + (u.Roles?.Name ?? ""), 
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = Application.Current.Resources["BrandAccentBrush"] as Brush
            });
            card.Child = cardSp;
            Root.Children.Add(card);

            if (u.IsFrozen)
            {
                var warn = new Border
                {
                    Background = new SolidColorBrush(Color.FromArgb(255, 255, 242, 242)),
                    BorderBrush = new SolidColorBrush(Color.FromArgb(255, 220, 38, 38)),
                    BorderThickness = new Thickness(1),
                    Padding = new Thickness(16),
                    Margin = new Thickness(0, 0, 0, 20),
                    CornerRadius = new System.Windows.CornerRadius(8)
                };
                var sp = new StackPanel();
                sp.Children.Add(new TextBlock 
                { 
                    Text = "⚠️ Аккаунт заморожен", 
                    FontWeight = FontWeights.Bold,
                    FontSize = 14,
                    Foreground = new SolidColorBrush(Color.FromArgb(255, 220, 38, 38))
                });
                sp.Children.Add(new TextBlock 
                { 
                    Text = "Причина: " + (u.FreezeReason ?? "—"), 
                    TextWrapping = TextWrapping.Wrap, 
                    Margin = new Thickness(0, 8, 0, 12),
                    FontSize = 12
                });
                var tb = new TextBox 
                { 
                    MinHeight = 80, 
                    AcceptsReturn = true, 
                    TextWrapping = TextWrapping.Wrap, 
                    Margin = new Thickness(0, 0, 0, 12),
                    ToolTip = "Опишите причину обращения" 
                };
                sp.Children.Add(tb);
                var btn = new Button 
                { 
                    Content = "⚖️ Оспорить заморозку", 
                    Style = Application.Current.Resources["PrimaryActionButton"] as Style,
                    HorizontalAlignment = HorizontalAlignment.Left 
                };
                btn.Click += (_, __) =>
                {
                    if (string.IsNullOrWhiteSpace(tb.Text))
                    {
                        MessageBox.Show("Введите текст обращения.");
                        return;
                    }
                    var fd = new FreezeDisputes
                    {
                        DisputeKind = DisputeKinds.Account,
                        TargetUserId = u.UserId,
                        RequesterUserId = u.UserId,
                        Message = tb.Text.Trim(),
                        Status = RequestStatus.Pending,
                        CreatedUtc = DateTime.UtcNow
                    };
                    Core.Context.FreezeDisputes.Add(fd);
                    Core.Context.SaveChanges();
                    MessageBox.Show("Заявка отправлена администратору.");
                };
                sp.Children.Add(btn);
                warn.Child = sp;
                Root.Children.Add(warn);
            }

            Root.Children.Add(new TextBlock 
            { 
                Text = "Мои отзывы", 
                FontSize = 16, 
                FontWeight = FontWeights.SemiBold,
                Foreground = Application.Current.Resources["InkBrush"] as Brush,
                Margin = new Thickness(0, 20, 0, 12)
            });

            var reviews = Core.Context.Reviews
                .Include(r => r.Books)
                .Where(r => r.UserId == u.UserId)
                .OrderByDescending(r => r.ReviewId)
                .ToList();

            if (!reviews.Any())
            {
                Root.Children.Add(new TextBlock 
                { 
                    Text = "Вы пока не оставили ни одного отзыва.",
                    Foreground = Application.Current.Resources["MutedBrush"] as Brush,
                    FontSize = 12
                });
            }
            else
            {
                var dg = new DataGrid
                {
                    ItemsSource = reviews,
                    AutoGenerateColumns = false,
                    IsReadOnly = true,
                    Height = 220,
                    Margin = new Thickness(0, 0, 0, 0)
                };
                dg.Columns.Add(new DataGridTextColumn { Header = "Книга", Binding = new System.Windows.Data.Binding("Books.Title"), Width = new DataGridLength(2, DataGridLengthUnitType.Star) });
                dg.Columns.Add(new DataGridTextColumn { Header = "Оценка", Binding = new System.Windows.Data.Binding("Rating"), Width = 60 });
                dg.Columns.Add(new DataGridTextColumn { Header = "Комментарий", Binding = new System.Windows.Data.Binding("Comment"), Width = new DataGridLength(3, DataGridLengthUnitType.Star) });
                Root.Children.Add(dg);
            }

            if (u.RoleId == RoleIds.Reader)
            {
                Root.Children.Add(new TextBlock 
                { 
                    Text = "Заявка на роль автора", 
                    FontSize = 16, 
                    FontWeight = FontWeights.SemiBold,
                    Foreground = Application.Current.Resources["InkBrush"] as Brush,
                    Margin = new Thickness(0, 28, 0, 12)
                });
                var hasPending = Core.Context.AuthorRoleRequests.Any(r => r.UserId == u.UserId && r.Status == RequestStatus.Pending);
                if (hasPending)
                {
                    var pending = new Border
                    {
                        Background = new SolidColorBrush(Color.FromArgb(255, 254, 243, 224)),
                        BorderBrush = new SolidColorBrush(Color.FromArgb(255, 180, 83, 9)),
                        BorderThickness = new Thickness(1),
                        Padding = new Thickness(16),
                        CornerRadius = new System.Windows.CornerRadius(8)
                    };
                    pending.Child = new TextBlock 
                    { 
                        Text = "⏳ Ваша заявка на рассмотрении администратором", 
                        Foreground = new SolidColorBrush(Color.FromArgb(255, 180, 83, 9)),
                        FontWeight = FontWeights.SemiBold
                    };
                    Root.Children.Add(pending);
                }
                else
                {
                    var msg = new TextBox 
                    { 
                        MinHeight = 80, 
                        AcceptsReturn = true, 
                        TextWrapping = TextWrapping.Wrap, 
                        Margin = new Thickness(0, 0, 0, 12)
                    };
                    Root.Children.Add(msg);
                    var apply = new Button 
                    { 
                        Content = "📝 Подать заявку", 
                        Style = Application.Current.Resources["PrimaryActionButton"] as Style,
                        HorizontalAlignment = HorizontalAlignment.Left 
                    };
                    apply.Click += (_, __) =>
                    {
                        if (string.IsNullOrWhiteSpace(msg.Text))
                        {
                            MessageBox.Show("Пожалуйста, опишите вашу мотивацию.");
                            return;
                        }
                        var req = new AuthorRoleRequests
                        {
                            UserId = u.UserId,
                            Message = msg.Text,
                            Status = RequestStatus.Pending,
                            CreatedUtc = DateTime.UtcNow
                        };
                        Core.Context.AuthorRoleRequests.Add(req);
                        Core.Context.SaveChanges();
                        MessageBox.Show("Заявка отправлена администратору. Спасибо!");
                        Reload();
                    };
                    Root.Children.Add(apply);
                }
            }
        }
    }
}
