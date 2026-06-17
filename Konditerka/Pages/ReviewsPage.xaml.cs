using Konditerka.AppData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Konditerka.Pages
{
    public class ReviewViewModel
    {
        public string UserName { get; set; }
        public int Rating { get; set; }
        public string StarsDisplay => new string('★', Rating) + new string('☆', 5 - Rating);
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public partial class ReviewsPage : Page
    {
        private readonly Catalogs _product;
        private int _selectedRating = 0;

        private readonly Button[] _stars;

        public ReviewsPage(Catalogs product)
        {
            InitializeComponent();
            WindowSizeHelper.SetMinSize(500, 800);
            _product = product;
            _stars = new[] { Star1, Star2, Star3, Star4, Star5 };

            ProductTitleBlock.Text = $"Отзывы: {product.Product}";
            LoadReviews();

            // Если пользователь не авторизован — скрыть форму
            if (!BasketManager.IsUserAuthorized())
            {
                SubmitButton.IsEnabled = false;
                SubmitButton.ToolTip = "Войдите, чтобы оставить отзыв";
                CommentBox.IsEnabled = false;
                foreach (var s in _stars) s.IsEnabled = false;
            }
            else
            {
                // Проверить, оставлял ли пользователь уже отзыв
                int uid = CurrentUser.User.IdUser;
                bool alreadyReviewed = AppConnect.model0db.Reviews
                    .Any(r => r.IdUser == uid && r.IdCatalog == _product.IdCatalog);
                if (alreadyReviewed)
                {
                    SubmitButton.IsEnabled = false;
                    SubmitButton.Content = "Вы уже оставили отзыв";
                    CommentBox.IsEnabled = false;
                    foreach (var s in _stars) s.IsEnabled = false;
                }
            }
        }

        private void LoadReviews()
        {
            var reviews = AppConnect.model0db.Reviews
                .Where(r => r.IdCatalog == _product.IdCatalog)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewViewModel
                {
                    UserName = r.Users.NameUser,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })
                .ToList();

            ReviewsList.ItemsSource = reviews;

            if (reviews.Count > 0)
            {
                double avg = reviews.Average(r => r.Rating);
                AvgRatingBlock.Text = $"Средний рейтинг: {avg:F1} ★  ({reviews.Count} отзывов)";
            }
            else
            {
                AvgRatingBlock.Text = "Отзывов пока нет — будьте первым!";
            }
        }

        private void Star_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && int.TryParse(btn.Tag?.ToString(), out int rating))
            {
                _selectedRating = rating;
                UpdateStarDisplay();
                string[] labels = { "", "Плохо", "Так себе", "Нормально", "Хорошо", "Отлично" };
                RatingLabel.Text = labels[rating];
            }
        }

        private void UpdateStarDisplay()
        {
            for (int i = 0; i < _stars.Length; i++)
                _stars[i].Content = i < _selectedRating ? "★" : "☆";
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedRating == 0)
            {
                MessageBox.Show("Пожалуйста, выберите оценку (звёздочки).",
                    "Нет оценки", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var review = new Reviews
                {
                    IdUser = CurrentUser.User.IdUser,
                    IdCatalog = _product.IdCatalog,
                    Rating = (byte)_selectedRating,
                    Comment = CommentBox.Text.Trim(),
                    CreatedAt = DateTime.Now
                };

                AppConnect.model0db.Reviews.Add(review);
                AppConnect.model0db.SaveChanges();

                MessageBox.Show("Спасибо за ваш отзыв!", "Готово",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Сбросить форму и заблокировать повторный отзыв
                _selectedRating = 0;
                UpdateStarDisplay();
                RatingLabel.Text = string.Empty;
                CommentBox.Text = string.Empty;
                SubmitButton.IsEnabled = false;
                SubmitButton.Content = "Вы уже оставили отзыв";
                CommentBox.IsEnabled = false;
                foreach (var s in _stars) s.IsEnabled = false;

                LoadReviews();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении отзыва: " + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.framemain?.Navigate(new PageOutput());
        }
    }
}