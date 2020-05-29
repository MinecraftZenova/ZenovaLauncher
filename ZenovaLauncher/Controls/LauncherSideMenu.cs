using MahApps.Metro.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace ZenovaLauncher.Controls
{
    public class LauncherSideMenuListBox : ListBox
    {
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DependencyObject obj = this.ContainerFromElement((Visual)e.OriginalSource);
                if (obj != null)
                {
                    FrameworkElement element = obj as FrameworkElement;
                    if (element != null)
                    {
                        ListBoxItem item = element as ListBoxItem;
                        if (item != null)
                        {
                            HamburgerMenuItem menuItem = item.Content as HamburgerMenuItem;
                            if (menuItem != null && this.Items.Contains(menuItem))
                            {
                                this.SelectedItem = menuItem;
                            }
                        }
                    }
                }
            }
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }

    class LauncherSideMenu : HamburgerMenu
    {
        private static readonly PropertyPath _scaleYPath = new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)");
        private static readonly BitmapCache _bitmapCacheMode = new BitmapCache();

        private UIElement _paneGrid;
        private ListBox _buttonsListView;
        private ListBox _optionsListView;

        private UIElement _nextIndicator;

        static LauncherSideMenu()
        {
            DisplayModeProperty.OverrideMetadata(typeof(LauncherSideMenu), new FrameworkPropertyMetadata(OnDisplayModePropertyChanged));
            IsPaneOpenProperty.OverrideMetadata(typeof(LauncherSideMenu), new FrameworkPropertyMetadata(OnIsPaneOpenPropertyChanged));
            CompactPaneLengthProperty.OverrideMetadata(typeof(LauncherSideMenu), new FrameworkPropertyMetadata(OnPaneLengthPropertyChanged));
            OpenPaneLengthProperty.OverrideMetadata(typeof(LauncherSideMenu), new FrameworkPropertyMetadata(OnPaneLengthPropertyChanged));
            SelectedItemProperty.OverrideMetadata(typeof(LauncherSideMenu), new FrameworkPropertyMetadata(OnSelectedItemPropertyChanged));
            SelectedOptionsItemProperty.OverrideMetadata(typeof(LauncherSideMenu), new FrameworkPropertyMetadata(OnSelectedItemPropertyChanged));
        }

        /// <summary>
        /// Initializes a new instance of the LauncherSideMenu class.
        /// </summary>
        public LauncherSideMenu()
        {
            DefaultStyleKey = typeof(LauncherSideMenu);

            SetResourceReference(DefaultItemFocusVisualStyleProperty, SystemParameters.FocusVisualStyleKey);
            SetResourceReference(ClientAreaAnimationProperty, SystemParameters.ClientAreaAnimationKey);
        }

        #region PaneLength

        private static readonly DependencyPropertyKey PaneLengthPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(PaneLength),
                typeof(double),
                typeof(LauncherSideMenu),
                new PropertyMetadata(0.0));

        public static readonly DependencyProperty PaneLengthProperty =
            PaneLengthPropertyKey.DependencyProperty;

        public double PaneLength
        {
            get => (double)GetValue(PaneLengthProperty);
            private set => SetValue(PaneLengthPropertyKey, value);
        }

        private void UpdatePaneLength()
        {
            PaneLength = IsPaneOpen ? OpenPaneLength : CompactPaneLength;
        }

        #endregion

        #region Header

        /// <summary>
        /// Identifies the Header dependency property.
        /// </summary>
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(
                nameof(Header),
                typeof(object),
                typeof(LauncherSideMenu));

        /// <summary>
        /// Gets or sets the header content.
        /// </summary>
        /// <returns>The header content.</returns>
        public object Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        #endregion

        #region HeaderTemplate

        /// <summary>
        /// Identifies the HeaderTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty HeaderTemplateProperty =
            DependencyProperty.Register(
                nameof(HeaderTemplate),
                typeof(DataTemplate),
                typeof(LauncherSideMenu));

        /// <summary>
        /// Gets or sets the DataTemplate used to display the control's header.
        /// </summary>
        /// <returns>The DataTemplate used to display the control's header.</returns>
        public DataTemplate HeaderTemplate
        {
            get => (DataTemplate)GetValue(HeaderTemplateProperty);
            set => SetValue(HeaderTemplateProperty, value);
        }

        #endregion

        #region DefaultItemFocusVisualStyle

        private static readonly DependencyProperty DefaultItemFocusVisualStyleProperty =
            DependencyProperty.Register(
                nameof(DefaultItemFocusVisualStyle),
                typeof(Style),
                typeof(LauncherSideMenu),
                new PropertyMetadata(OnDefaultItemFocusVisualStyleChanged));

        private Style DefaultItemFocusVisualStyle
        {
            get => (Style)GetValue(DefaultItemFocusVisualStyleProperty);
            set => SetValue(DefaultItemFocusVisualStyleProperty, value);
        }

        private static void OnDefaultItemFocusVisualStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LauncherSideMenu)d).ChangeItemFocusVisualStyle();
        }

        #endregion

        #region PaneTitle

        /// <summary>
        /// Identifies the PaneTitle dependency property.
        /// </summary>
        public static readonly DependencyProperty PaneTitleProperty =
            DependencyProperty.Register(
                nameof(PaneTitle),
                typeof(string),
                typeof(LauncherSideMenu),
                new PropertyMetadata(OnPaneTitleChanged));

        /// <summary>
        /// Gets or sets the label adjacent to the menu icon when the pane is open.
        /// </summary>
        /// <returns>
        /// The label adjacent to the menu icon when the pane is open. The default is an
        /// empty string.
        /// </returns>
        public string PaneTitle
        {
            get => (string)GetValue(PaneTitleProperty);
            set => SetValue(PaneTitleProperty, value);
        }

        private static void OnPaneTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LauncherSideMenu)d).OnPaneTitleChanged(e);
        }

        private void OnPaneTitleChanged(DependencyPropertyChangedEventArgs e)
        {
            HasPaneTitle = !string.IsNullOrEmpty((string)e.NewValue);
        }

        #endregion

        #region HasPaneTitle

        private static readonly DependencyPropertyKey HasPaneTitlePropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(HasPaneTitle),
                typeof(bool),
                typeof(LauncherSideMenu),
                new PropertyMetadata(false));

        public static readonly DependencyProperty HasPaneTitleProperty =
            HasPaneTitlePropertyKey.DependencyProperty;

        public bool HasPaneTitle
        {
            get => (bool)GetValue(HasPaneTitleProperty);
            private set => SetValue(HasPaneTitlePropertyKey, value);
        }

        #endregion

        #region SelectedMenuItem

        private static readonly DependencyProperty SelectedMenuItemProperty =
            DependencyProperty.Register(
                nameof(SelectedMenuItem),
                typeof(object),
                typeof(LauncherSideMenu),
                new PropertyMetadata(OnSelectedMenuItemChanged));

        private object SelectedMenuItem
        {
            get => GetValue(SelectedMenuItemProperty);
            set => SetValue(SelectedMenuItemProperty, value);
        }

        private static void OnSelectedMenuItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LauncherSideMenu)d).OnSelectedMenuItemChanged(e.OldValue, e.NewValue);
        }

        private void OnSelectedMenuItemChanged(object oldValue, object newValue)
        {
            SelectionChanged?.Invoke(this, new LauncherSideMenuSelectionChangedEventArgs { SelectedItem = newValue });

            AnimateSelectionChanged(oldValue, newValue);
        }

        private void UpdateSelectedMenuItem()
        {
            SelectedMenuItem = SelectedItem ?? SelectedOptionsItem;
        }

        #endregion

        #region ClientAreaAnimation

        private static readonly DependencyProperty ClientAreaAnimationProperty =
            DependencyProperty.Register(
                nameof(ClientAreaAnimation),
                typeof(bool),
                typeof(LauncherSideMenu),
                new PropertyMetadata(SystemParameters.ClientAreaAnimation));

        private bool ClientAreaAnimation
        {
            get => (bool)GetValue(ClientAreaAnimationProperty);
            set => SetValue(ClientAreaAnimationProperty, value);
        }

        #endregion

        #region ElementAnimation

        private static readonly DependencyProperty ElementAnimationProperty =
            DependencyProperty.RegisterAttached(
                "ElementAnimation",
                typeof(Storyboard),
                typeof(LauncherSideMenu));

        private static Storyboard GetElementAnimation(FrameworkElement element)
        {
            return (Storyboard)element.GetValue(ElementAnimationProperty);
        }

        private static void SetElementAnimation(FrameworkElement element, Storyboard value)
        {
            element.SetValue(ElementAnimationProperty, value);
        }

        #endregion

        /// <summary>
        /// Occurs when the DisplayMode property changes.
        /// </summary>
        public event EventHandler<LauncherSideMenuDisplayModeChangedEventArgs> DisplayModeChanged;

        /// <summary>
        /// Occurs when the pane is opened.
        /// </summary>
        public event EventHandler PaneOpened;

        /// <summary>
        /// Occurs when the pane is closed.
        /// </summary>
        public event EventHandler PaneClosed;

        /// <summary>
        /// Occurs when the currently selected item changes.
        /// </summary>
        public event EventHandler<LauncherSideMenuSelectionChangedEventArgs> SelectionChanged;

        /// <summary>
        /// Called when the Template's tree has been generated.
        /// </summary>
        public override void OnApplyTemplate()
        {
            if (_buttonsListView != null && _optionsListView != null)
            {
                _buttonsListView.ItemContainerGenerator.StatusChanged -= OnListViewItemContainerGeneratorStatusChanged;
                _optionsListView.ItemContainerGenerator.StatusChanged -= OnListViewItemContainerGeneratorStatusChanged;
            }

            base.OnApplyTemplate();

            _paneGrid = GetTemplateChild("PaneGrid") as UIElement;
            _buttonsListView = GetTemplateChild("ButtonsListView") as ListBox;
            _optionsListView = GetTemplateChild("OptionsListView") as ListBox;

            if (_buttonsListView != null && _optionsListView != null)
            {
                _buttonsListView.ItemContainerGenerator.StatusChanged += OnListViewItemContainerGeneratorStatusChanged;
                _optionsListView.ItemContainerGenerator.StatusChanged += OnListViewItemContainerGeneratorStatusChanged;
            }

            ChangeItemFocusVisualStyle();
        }

        private static void OnDisplayModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LauncherSideMenu)d).OnDisplayModeChanged(e);
        }

        private static void OnIsPaneOpenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LauncherSideMenu)d).OnIsPaneOpenChanged(e);
        }

        private static void OnPaneLengthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LauncherSideMenu)d).OnPaneLengthPropertyChanged(e);
        }

        private static void OnSelectedItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LauncherSideMenu)d).UpdateSelectedMenuItem();
        }

        private void OnListViewItemContainerGeneratorStatusChanged(object sender, EventArgs e)
        {
            if ((_buttonsListView.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated || !_buttonsListView.HasItems) &&
                (_optionsListView.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated || !_optionsListView.HasItems))
            {
                _buttonsListView.ItemContainerGenerator.StatusChanged -= OnListViewItemContainerGeneratorStatusChanged;
                _optionsListView.ItemContainerGenerator.StatusChanged -= OnListViewItemContainerGeneratorStatusChanged;

                var item = SelectedItem;
                if (item != null)
                {
                    AnimateSelectionChanged(null, item);
                }
            }
        }

        private void OnDisplayModeChanged(DependencyPropertyChangedEventArgs e)
        {
            DisplayModeChanged?.Invoke(this, new LauncherSideMenuDisplayModeChangedEventArgs((SplitViewDisplayMode)e.NewValue));
        }

        private void OnIsPaneOpenChanged(DependencyPropertyChangedEventArgs e)
        {
            UpdatePaneLength();
            ChangeItemFocusVisualStyle();

            if ((bool)e.NewValue)
            {
                PaneOpened?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                PaneClosed?.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnPaneLengthPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            UpdatePaneLength();
            ChangeItemFocusVisualStyle();
        }

        private void ChangeItemFocusVisualStyle()
        {
            if (DefaultItemFocusVisualStyle != null)
            {
                var focusVisualStyle = new Style(typeof(Control), DefaultItemFocusVisualStyle);
                focusVisualStyle.Setters.Add(new Setter(Control.WidthProperty, IsPaneOpen ? OpenPaneLength : CompactPaneLength));
                focusVisualStyle.Setters.Add(new Setter(Control.HorizontalAlignmentProperty, HorizontalAlignment.Left));
                focusVisualStyle.Seal();

                SetValue(ItemFocusVisualStylePropertyKey, focusVisualStyle);
            }
        }

        private void AnimateSelectionChanged(object prevItem, object nextItem)
        {
            UIElement prevIndicator = FindSelectionIndicator(prevItem);
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
                if ((prevIndicator != nextIndicator) && prevIndicator != null && nextIndicator != null && ClientAreaAnimation && RenderCapability.Tier > 0)
                {
                    // Make sure both indicators are visible and in their original locations
                    ResetElementAnimationProperties(prevIndicator, 0.0);
                    ResetElementAnimationProperties(nextIndicator, 1.0);

                    // Play the animation on both the previous and next indicators
                    PlayIndicatorAnimations(nextIndicator);

                    _nextIndicator = nextIndicator;
                }
                else
                {
                    ResetElementAnimationProperties(prevIndicator, 0.0);
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
            Storyboard.SetTargetProperty(scaleAnim, _scaleYPath);
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
                var container = _buttonsListView?.ItemContainerGenerator.ContainerFromItem(item) ??
                                _optionsListView?.ItemContainerGenerator.ContainerFromItem(item);
                if (container is Control control)
                {
                    return control.Template?.FindName("SelectionIndicator", control) as UIElement;
                }
            }

            return null;
        }
    }
}
