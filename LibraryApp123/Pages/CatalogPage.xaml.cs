using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using LibraryApp123;
using System.Windows.Threading;

namespace libraryapp.Pages
{
    public partial class CatalogPage : Page
    {
        public CatalogPage()
        {
            InitializeComponent();
            SortBox.SelectionChanged += Refresh_Click;
            GenreBox.SelectionChanged += Refresh_Click;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (GenreBox.Items.Count == 0)
            {
                GenreBox.Items.Add(new ComboBoxItem { Content = "Все жанры", Tag = null });
                foreach (var g in Core.Context.Genres.OrderBy(x => x.Name).ToList())
                    GenreBox.Items.Add(new ComboBoxItem { Content = g.Name, Tag = g.GenreId });
                GenreBox.SelectedIndex = 0;
            }
            Dispatcher.BeginInvoke(new Action(() => Refresh_Click(sender, e)), DispatcherPriority.Loaded);
        }

        private void Refresh_Click(object sender, System.EventArgs e)
        {
            if (BooksHost == null) return;

            var search = (SearchBox.Text ?? string.Empty).Trim().ToLowerInvariant();
            int? genreId = null;
            if (GenreBox?.SelectedItem is ComboBoxItem gi && gi.Tag is int gid) genreId = gid;

            var sortRating = (SortBox.SelectedItem as ComboBoxItem)?.Tag as string == "rating";

            var books = Core.Context.Books
                .Include(b => b.AppUsers)
                .Include(b => b.Genres)
                .Include(b => b.Reviews)
                .Where(b => !b.IsFrozen)  // ✅ Скрываем замороженные книги
                .ToList();

            IEnumerable<Books> q = books;
            if (!string.IsNullOrEmpty(search))
            {
                q = q.Where(b =>
                    (b.Title ?? "").ToLowerInvariant().Contains(search) ||
                    ((b.AppUsers?.DisplayName ?? "") + (b.AppUsers?.Login ?? "")).ToLowerInvariant().Contains(search));
            }

            if (genreId.HasValue)
                q = q.Where(b => b.Genres.Any(g => g.GenreId == genreId.Value));

            var list = q.Select(b => new
            {
                Book = b,
                Avg = b.Reviews.Any(r => !r.IsFrozen)
                    ? b.Reviews.Where(r => !r.IsFrozen).Average(r => (double)r.Rating)
                    : (double?)null
            }).ToList();

            if (sortRating)
                list = list.OrderByDescending(x => x.Avg ?? 0).ThenBy(x => x.Book.Title).ToList();
            else
                list = list.OrderBy(x => x.Book.Title).ToList();

            BooksHost.Items.Clear();
            foreach (var x in list)
                BooksHost.Items.Add(CreateCard(x.Book, x.Avg));
        }

        private UIElement CreateCard(Books b, double? avg)
        {
            var root = new Border
            {
                Width = 176,
                Margin = new Thickness(6),
                BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12),
                Background = Brushes.White,
                Cursor = Cursors.Hand,
                Tag = b.BookId,
                Effect = new DropShadowEffect { BlurRadius = 12, ShadowDepth = 2, Opacity = 0.08, Direction = 270, Color = Color.FromRgb(30, 41, 59) }
            };
            root.MouseLeftButtonUp += (s, ev) =>
            {
                if (s is Border bd && bd.Tag is int id)
                    NavigationService?.Navigate(new BookDetailPage(id));
            };

            var sp = new StackPanel { Margin = new Thickness(10, 10, 10, 12) };
            var bi = ImageHelper.ToBitmapImage(b.CoverImage);
            if (bi != null)
            {
                sp.Children.Add(new Image { Height = 120, Stretch = Stretch.UniformToFill, Source = bi });
            }
            else
            {
                sp.Children.Add(new Border
                {
                    Height = 120,
                    CornerRadius = new CornerRadius(8),
                    ClipToBounds = true,
                    Background = new SolidColorBrush(Color.FromRgb(241, 245, 249)),
                    Child = new TextBlock
                    {
                        Text = "Нет обложки",
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                        FontSize = 11
                    }
                });
            }

            sp.Children.Add(new TextBlock
            {
                Text = b.Title,
                TextWrapping = TextWrapping.Wrap,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(30, 41, 59)),
                Margin = new Thickness(0, 8, 0, 0),
                FontSize = 12,
                MaxHeight = 40
            });

            sp.Children.Add(new TextBlock
            {
                Text = b.AppUsers?.DisplayName ?? "",
                Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                FontSize = 11,
                TextTrimming = TextTrimming.CharacterEllipsis
            });

            sp.Children.Add(new TextBlock
            {
                Text = avg.HasValue ? $"Оценка: {avg:0.0}" : "Нет оценок",
                Margin = new Thickness(0, 4, 0, 0),
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(71, 85, 105))
            });

            var shelfPanel = new StackPanel { Margin = new Thickness(0, 8, 0, 0) };
            var uid = AppSession.CurrentUser.UserId;
            var bookId = b.BookId;
            var existingShelf = Core.Context.UserBookShelves.FirstOrDefault(x => x.UserId == uid && x.BookId == bookId);

            var cb = new ComboBox
            {
                Height = 30,
                FontSize = 11,
                ToolTip = "Выберите полку"
            };
            cb.Items.Add(new ComboBoxItem { Content = "На полку…", Tag = null });
            cb.Items.Add(new ComboBoxItem { Content = "Заброшено", Tag = ShelfTypes.Abandoned });
            cb.Items.Add(new ComboBoxItem { Content = "В планах", Tag = ShelfTypes.Planned });
            cb.Items.Add(new ComboBoxItem { Content = "Читаю", Tag = ShelfTypes.Reading });
            cb.Items.Add(new ComboBoxItem { Content = "Прочитано", Tag = ShelfTypes.Read });
            cb.SelectedIndex = existingShelf != null
                ? 1 + Math.Min(3, Math.Max(0, (int)existingShelf.ShelfType))
                : 0;

            cb.SelectionChanged += (_, __) =>
            {
                var ci = cb.SelectedItem as ComboBoxItem;
                if (ci == null || !(ci.Tag is byte shelf))
                    return;
                var row = Core.Context.UserBookShelves.FirstOrDefault(x => x.UserId == uid && x.BookId == bookId);
                if (row == null)
                {
                    Core.Context.UserBookShelves.Add(new UserBookShelves { UserId = uid, BookId = bookId, ShelfType = shelf });
                    Core.Context.SaveChanges();
                    MessageBox.Show("Книга добавлена в список.", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (row.ShelfType != shelf)
                {
                    row.ShelfType = shelf;
                    Core.Context.SaveChanges();
                    MessageBox.Show("Книга перенесена в список.", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            };

            shelfPanel.Children.Add(cb);
            sp.Children.Add(shelfPanel);

            root.MouseLeftButtonUp += (_, ev) =>
            {
                if (IsInteractiveNavigationSource(ev.OriginalSource as DependencyObject))
                    return;
                NavigationService?.Navigate(new BookDetailPage(b.BookId));
            };

            root.Child = sp;
            return root;
        }

        private static bool IsInteractiveNavigationSource(DependencyObject src)
        {
            while (src != null)
            {
                if (src is System.Windows.Controls.Primitives.ButtonBase || src is ComboBox || src is ComboBoxItem || src is TextBox)
                    return true;
                src = VisualTreeHelper.GetParent(src);
            }
            return false;
        }
    }
}