using System.Windows;
using System.Windows.Media;

public static class VisualTreeHelperExtensions
{
    // Method to get all Image controls in the visual tree of a parent element
    public static List<Image> FindVisualChildren<Image>(DependencyObject parent) where Image : DependencyObject
    {
        var images = new List<Image>();

        // Recursive search of the visual tree to find all Image controls
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is not null and Image)
            {
                images.Add((Image)child);
            }

            images.AddRange(FindVisualChildren<Image>(child));
        }

        return images;
    }
}
