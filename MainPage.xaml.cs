using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Save_the_Humans.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Animation;
using Windows.Security.Cryptography.Core;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SaveHumans
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Random random = new Random();
        DispatcherTimer enemyTimer = new DispatcherTimer();
        DispatcherTimer targetTimer = new DispatcherTimer();
        bool caught = false;

        private NavigationHelper navHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        public ObservableDictionary DefaultViewModel => defaultViewModel;

        public NavigationHelper NavigationHelper => navHelper;

        public MainPage()
        {
            this.InitializeComponent();
            this.navHelper = new NavigationHelper(this);
            this.navHelper.LoadState += navigationHelper_LoadState;
            this.navHelper.SaveState += navigationHelper_SaveState;

            enemyTimer.Tick += enemyTimer_Tick;
            enemyTimer.Interval = TimeSpan.FromSeconds(2);

            targetTimer.Tick += targetTimer_Tick;
            targetTimer.Interval = TimeSpan.FromSeconds(.2);
        }
        
        private void Start()
        {
            human.IsHitTestVisible = true;
            caught = false;
            progressBar.Value = 0;
            startButton.Visibility = Visibility.Collapsed;
            playArea.Children.Clear();
            playArea.Children.Add(target);
            playArea.Children.Add(human);
            enemyTimer.Start();
            targetTimer.Start();
        }


        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            Start();
        }

        private void enemyTimer_Tick(object sender, object e)
        {
            AddEnemy();
        }

        private void End()
        {
            if (playArea.Children.Contains(gameOverText)) return;
            enemyTimer.Stop();
            targetTimer.Stop();
            caught = false;
            startButton.Visibility = Visibility.Visible;
            playArea.Children.Add(gameOverText);
        }

        private void targetTimer_Tick(object sender, object e)
        {
            progressBar.Value += 1;
            if (progressBar.Value >= progressBar.Maximum) End();
        }
        
        private void AnimateEnemy(ContentControl enemy, double from, double to, string propertyToAnimate)
        {
            var storyboard = new Storyboard() { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever };
            var animation = new DoubleAnimation()
            {
                From = from,
                To = to,
                Duration = new Duration(TimeSpan.FromSeconds(random.Next(4, 6)))
            };
            Storyboard.SetTarget(animation, enemy);
            Storyboard.SetTargetProperty(animation, propertyToAnimate);
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }

        private void AddEnemy()
        {
            ContentControl enemy = new ContentControl();
            enemy.Template = Resources["EnemyTemplate"] as ControlTemplate;
            AnimateEnemy(enemy, 0, playArea.ActualWidth - 100, "(Canvas.Left)");
            AnimateEnemy(enemy, random.Next((int)playArea.ActualHeight - 100),
                random.Next((int)playArea.ActualHeight - 100), "(Canvas.Top)");
            playArea.Children.Add(enemy);

            enemy.PointerEntered += enemy_PointerEntered;
        }

        private void enemy_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (caught) End();
        }

        private void human_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (enemyTimer.IsEnabled)
            {
                caught = true;
                human.IsHitTestVisible = false;
            }
        }

        private void target_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (targetTimer.IsEnabled && caught)
            {
                progressBar.Value = 0;
                Canvas.SetLeft(target, random.Next(100, (int)playArea.ActualWidth - 100));
                Canvas.SetTop(target, random.Next(100, (int)playArea.ActualHeight - 100));
                Canvas.SetLeft(human, random.Next(100, (int)playArea.ActualWidth - 100));
                Canvas.SetTop(human, random.Next(100, (int)playArea.ActualHeight - 100));
                caught = false;
                human.IsHitTestVisible = true;
            }
        }

        private void playArea_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (caught)
            {
                Point pointerPosition = e.GetCurrentPoint(null).Position;
                Point relativePosition = grid.TransformToVisual(playArea).TransformPoint(pointerPosition);
                if ((Math.Abs(relativePosition.X - Canvas.GetLeft(human)) > human.ActualWidth * 3)
                    || (Math.Abs(relativePosition.Y - Canvas.GetTop(human)) > human.ActualHeight * 3))
                {
                    caught = false;
                    human.IsHitTestVisible = true;
                }
                else
                {
                    Canvas.SetLeft(human, relativePosition.X - human.ActualWidth / 2);
                    Canvas.SetTop(human, relativePosition.Y - human.ActualHeight / 2);
                }
            }
        }

        private void playArea_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (caught)
                End();
        }

        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e) { }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e) { }
    }
}
