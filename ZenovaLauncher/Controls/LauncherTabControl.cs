using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace ZenovaLauncher.Controls
{
    public class MyTabItem : TabItem
    {
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (e.Source == this || !this.IsSelected)
                return;

            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (e.Source == this || !this.IsSelected)
                base.OnMouseLeftButtonDown(e);

            base.OnMouseLeftButtonUp(e);
        }
    }

    class LauncherTabControl : TabControl
    {
        private static readonly PropertyPath _scaleXPath = new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleX)");
        private static readonly BitmapCache _bitmapCacheMode = new BitmapCache();

        private UIElement _nextIndicator;

        #region ElementAnimation

        private static readonly DependencyProperty ElementAnimationProperty =
            DependencyProperty.RegisterAttached(
                "ElementAnimation",
                typeof(Storyboard),
                typeof(LauncherTabControl));

        private static Storyboard GetElementAnimation(FrameworkElement element)
        {
            return (Storyboard)element.GetValue(ElementAnimationProperty);
        }

        private static void SetElementAnimation(FrameworkElement element, Storyboard value)
        {
            element.SetValue(ElementAnimationProperty, value);
        }

        #endregion

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            AnimateSelectionChanged(SelectedItem);
        }

        private void AnimateSelectionChanged(object nextItem)
        {
            UIElement nextIndicator = FindSelectionIndicator(nextItem);

            bool haveValidAnimation = false;
            // It's possible that AnimateSelectionChanged is called multiple times before the first animation is complete.
            // To have better user experience, if the selected target is the same, keep the first animation
            // If the selected target is not the same, abort the first animation and launch another animation.
            if (_nextIndicator != null) // There is ongoing animation
            {
                if (nextIndicator != null && _nextIndicator == nextIndicator) // animate to the same target, just wait for animation complete
                {
                    haveValidAnimation = true;
                }
                else
                {
                    // If the last animation is still playing, force it to complete.
                    OnAnimationComplete();
                }
            }

            if (!haveValidAnimation)
            {
                if (nextIndicator != null && RenderCapability.Tier > 0)
                {
                    // Make sure both indicators are visible and in their original locations
                    ResetElementAnimationProperties(nextIndicator, 1.0);

                    // Play the animation on both the previous and next indicators
                    PlayIndicatorAnimations(nextIndicator);

                    _nextIndicator = nextIndicator;
                }
                else
                {
                    ResetElementAnimationProperties(nextIndicator, 1.0);
                }
            }
        }

        private void PlayIndicatorAnimations(UIElement indicator)
        {
            double beginScale = 0.0;
            double endScale = 1.0;

            var storyboard = new Storyboard { FillBehavior = FillBehavior.Stop };
            storyboard.Completed += delegate
            {
                OnAnimationComplete();
            };

            var scaleAnim = new DoubleAnimationUsingKeyFrames
            {
                KeyFrames =
                {
                    new DiscreteDoubleKeyFrame(beginScale, KeyTime.FromPercent(0.0)),
                    new SplineDoubleKeyFrame(endScale, KeyTime.FromPercent(1.0), new KeySpline(0.0, 0.0, 0.6, 1.0)),
                },
                Duration = TimeSpan.FromMilliseconds(150)
            };
            Storyboard.SetTargetProperty(scaleAnim, _scaleXPath);
            storyboard.Children.Add(scaleAnim);

            indicator.RenderTransform = new TransformGroup
            {
                Children =
                {
                    new ScaleTransform()
                }
            };

            if (indicator.CacheMode == null)
            {
                indicator.CacheMode = _bitmapCacheMode;
            }

            var indicatorAsFE = (FrameworkElement)indicator;
            SetElementAnimation(indicatorAsFE, storyboard);
            storyboard.Begin(indicatorAsFE, true);
            storyboard.Pause(indicatorAsFE);
            Dispatcher.BeginInvoke(() =>
            {
                var animation = GetElementAnimation(indicatorAsFE);
                if (animation == storyboard)
                {
                    animation.Resume(indicatorAsFE);
                }
            }, DispatcherPriority.Render);
        }

        private void OnAnimationComplete()
        {
            var indicator = _nextIndicator;
            ResetElementAnimationProperties(indicator, 1.0);
            _nextIndicator = null;
        }

        private void ResetElementAnimationProperties(UIElement element, double desiredOpacity)
        {
            if (element != null)
            {
                element.Opacity = desiredOpacity;
                element.RenderTransform = null;

                var elementAsFE = (FrameworkElement)element;
                var animation = GetElementAnimation(elementAsFE);
                if (animation != null)
                {
                    animation.Remove(elementAsFE);
                    elementAsFE.ClearValue(ElementAnimationProperty);
                }
            }
        }

        private UIElement FindSelectionIndicator(object item)
        {
            if (item != null)
            {
                if (item is Control control)
                {
                    return control.Template?.FindName("SelectedPipe", control) as UIElement;
                }
            }

            return null;
        }
    }
}
