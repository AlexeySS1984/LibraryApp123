using System.IO;
using System.Windows.Media.Imaging;

namespace libraryapp
{
    /// <summary>
    /// Вспомогательный класс для работы с изображениями в WPF.
    /// </summary>
    public static class ImageHelper
    {
        /// <summary>
        /// Преобразует массив байтов в BitmapImage.
        /// </summary>
        /// <param name="bytes">Исходные данные изображения.</param>
        /// <returns>BitmapImage или null при пустом входном массиве.</returns>
        public static BitmapImage ToBitmapImage(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return null;
            var image = new BitmapImage();
            using (var ms = new MemoryStream(bytes))
            {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
                image.Freeze();
            }
            return image;
        }
    }
}
