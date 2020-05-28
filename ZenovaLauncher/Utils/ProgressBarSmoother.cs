using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace ZenovaLauncher
{
    public class ProgressBarSmoother
    {
        public static double GetSmoothValue(DependencyObject obj)
        {
            return (double)obj.GetValue(SmoothValueProperty);
        }

        public static void SetSmoothValue(DependencyObject obj, double value)
        {
            obj.SetValue(SmoothValueProperty, value);
        }

        public static readonly DependencyProperty SmoothValueProperty =
            DependencyProperty.RegisterAttached("SmoothValue", typeof(double), typeof(ProgressBarSmoother), new PropertyMetadata(0.0, Changing));

        public static TimeSpan GetAnimateTime(DependencyObject obj)
        {
            return (TimeSpan)obj.GetValue(AnimateTimeProperty);
        }

        public static void SetAnimateTime(DependencyObject obj, TimeSpan value)
        {
            obj.SetValue(AnimateTimeProperty, value);
        }

        public static readonly DependencyProperty AnimateTimeProperty =
            DependencyProperty.RegisterAttached("AnimateTime", typeof(TimeSpan), typeof(ProgressBarSmoother), new PropertyMetadata(new TimeSpan(0,0,0,0,100)));

        private static void Changing(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d as ProgressBar).IsIndeterminate)
            {
                var anim = new DoubleAnimation((double)e.OldValue, (double)e.NewValue, GetAnimateTime(d));
                (d as ProgressBar).BeginAnimation(ProgressBar.ValueProperty, anim, HandoffBehavior.Compose);
            }
        }
    }
}