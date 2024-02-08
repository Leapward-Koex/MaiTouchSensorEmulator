using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

public static class VisualTreeHelperExtensions
{
    // Method to get all Image controls in the visual tree of a parent element
    public static List<Image> FindVisualChildren<Image>(DependencyObject parent) where Image : DependencyObject
    {
        var images = new List<Image>();

        // Recursive search of the visual tree to find all Image controls
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child != null && child is Image)
            {
                images.Add((Image)child);
            }

            images.AddRange(FindVisualChildren<Image>(child));
        }

        return images;
    }
}
