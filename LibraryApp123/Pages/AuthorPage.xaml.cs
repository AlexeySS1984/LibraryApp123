using System;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LibraryApp123;

namespace libraryapp.Pages
{
    public partial class AuthorPage : Page
    {
        public AuthorPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) => Reload();

        private void Reload()
        {
            Root.Children.Clear();
            var uid = AppSession.CurrentUser.UserId;

            Root.Children.Add(new TextBlock 
            { 
                Text = "Кабинет автора", 
                FontSize = 26, 
                FontWeight = FontWeights.Bold,
                Foreground = Application.Current.Resources["InkBrush"] as Brush,
                Margin = new Thickness(0, 0, 0, 8)
            });
            Root.Children.Add(new TextBlock 
            { 
                Text = "Управляйте своими книгами и отслеживайте их статус", 
                FontSize = 13,
                Foreground = Application.Current.Resources["MutedBrush"] as Brush,
                Margin = new Thickness(0, 0, 0, 20)
            });

            var add = new Button 
            { 
                Content = "➕ Добавить новую книгу", 
                Style = Application.Current.Resources["PrimaryActionButton"] as Style,
                HorizontalAlignment = HorizontalAlignment.Left, 
                Margin = new Thickness(0, 0, 0, 24) 
            };
            add.Click += (_, __) => NavigationService?.Navigate(new BookEditPage(0));
            Root.Children.Add(add);

            Root.Children.Add(new TextBlock 
            { 
                Text = "Опубликованные книги", 
                FontSize = 16, 
                FontWeight = FontWeights.SemiBold,
                Foreground = Application.Current.Resources["InkBrush"] as Brush,
                Margin = new Thickness(0, 0, 0, 12) 
            });

            var books = Core.Context.Books.Where(b => b.AuthorUserId == uid && !b.IsFrozen).OrderBy(b => b.Title).ToList();

            if (!books.Any())
            {
                Root.Children.Add(new TextBlock 
                { 
                    Text = "У вас пока нет опубликованных книг.", 
                    Foreground = Application.Current.Resources["MutedBrush"] as Brush,
                    FontSize = 12,
                    Margin = new Thickness(0, 8, 0, 0)
                });
            }
            else
            {
                foreach (var b in books)
                {
                    var bookId = b.BookId;
                    var card = new Border
                    {
                        Background = Application.Current.Resources["CardBrush"] as Brush,
                        BorderBrush = Application.Current.Resources["BorderSubtleBrush"] as Brush,
                        BorderThickness = new Thickness(1),
                        Padding = new Thickness(16),
                        Margin = new Thickness(0, 0, 0, 12),
                        CornerRadius = new System.Windows.CornerRadius(8)
                    };

                    var sp = new StackPanel();
                    sp.Children.Add(new TextBlock 
                    { 
                        Text = b.Title, 
                        FontSize = 14,
                        FontWeight = FontWeights.SemiBold,
                        Foreground = Application.Current.Resources["InkBrush"] as Brush,
                        TextWrapping = TextWrapping.Wrap
                    });

                    var btnRow = new StackPanel 
                    { 
                        Orientation = Orientation.Horizontal, 
                        Margin = new Thickness(0, 12, 0, 0) 
                    };

                    var edit = new Button 
                    { 
                        Content = "✏️ Редактировать", 
                        Style = Application.Current.Resources["PrimaryActionButton"] as Style,
                        Margin = new Thickness(0, 0, 8, 0)
                    };
                    edit.Click += (_, __) => NavigationService?.Navigate(new BookEditPage(bookId));
                    btnRow.Children.Add(edit);

                    sp.Children.Add(btnRow);
                    card.Child = sp;
                    Root.Children.Add(card);
                }
            }

            Root.Children.Add(new TextBlock 
            { 
                Text = "Замороженные книги", 
                FontSize = 16, 
                FontWeight = FontWeights.SemiBold,
                Foreground = Application.Current.Resources["InkBrush"] as Brush,
                Margin = new Thickness(0, 28, 0, 12) 
            });

            var frozen = Core.Context.Books.Where(b => b.AuthorUserId == uid && b.IsFrozen).ToList();
            if (!frozen.Any())
            {
                Root.Children.Add(new TextBlock 
                { 
                    Text = "Нет замороженных книг.", 
                    Foreground = Application.Current.Resources["MutedBrush"] as Brush,
                    FontSize = 12,
                    Margin = new Thickness(0, 8, 0, 0)
                });
            }
            else
            {
                foreach (var b in frozen)
                {
                    var bookId = b.BookId;
                    var card = new Border
                    {
                        Background = new SolidColorBrush(Color.FromArgb(255, 255, 242, 242)),
                        BorderBrush = new SolidColorBrush(Color.FromArgb(255, 220, 38, 38)),
                        BorderThickness = new Thickness(1),
                        Padding = new Thickness(16),
                        Margin = new Thickness(0, 0, 0, 12),
                        CornerRadius = new System.Windows.CornerRadius(8)
                    };

                    var sp = new StackPanel();
                    sp.Children.Add(new TextBlock 
                    { 
                        Text = b.Title, 
                        FontSize = 14,
                        FontWeight = FontWeights.SemiBold,
                        Foreground = Application.Current.Resources["InkBrush"] as Brush,
                        TextWrapping = TextWrapping.Wrap
                    });

                    sp.Children.Add(new TextBlock
                    {
                        Text = $"Причина: {b.FreezeReason ?? "—"}",
                        FontSize = 12,
                        Foreground = Application.Current.Resources["MutedBrush"] as Brush,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 8, 0, 0)
                    });

                    var btnRow = new StackPanel 
                    { 
                        Orientation = Orientation.Horizontal, 
                        Margin = new Thickness(0, 12, 0, 0) 
                    };

                    var dispute = new Button 
                    { 
                        Content = "⚖️ Оспорить", 
                        Style = Application.Current.Resources["PrimaryActionButton"] as Style,
                        Margin = new Thickness(0, 0, 0, 0)
                    };
                    dispute.Click += (_, __) =>
                    {
                        var text = UiPrompts.AskMultiline("Оспаривание заморозки книги", "Опишите причину обращения");
                        if (string.IsNullOrWhiteSpace(text)) return;
                        var fd = new FreezeDisputes
                        {
                            DisputeKind = DisputeKinds.Book,
                            TargetBookId = bookId,
                            RequesterUserId = uid,
                            Message = text.Trim(),
                            Status = RequestStatus.Pending,
                            CreatedUtc = DateTime.UtcNow
                        };
                        Core.Context.FreezeDisputes.Add(fd);
                        Core.Context.SaveChanges();
                        MessageBox.Show("Заявка отправлена администратору.");
                        Reload();
                    };
                    btnRow.Children.Add(dispute);
                    sp.Children.Add(btnRow);
                    card.Child = sp;
                    Root.Children.Add(card);
                }
            }
        }
    }
}
