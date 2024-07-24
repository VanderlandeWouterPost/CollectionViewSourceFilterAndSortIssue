using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace CollectionViewSourceIssue
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly object sync = new object();
        private volatile bool runLoop;

        public MainWindow()
        {
            Items = new ObservableCollection<Item>();
            BindingOperations.EnableCollectionSynchronization(Items, sync);
            DataContext = this;

            InitializeComponent();

            // start the loop.
            StartBackgroundLoop(this, null);
        }

        public ObservableCollection<Item> Items { get; }

        /// <summary>
        /// Filter items when they're added to the view.
        /// </summary>
        private void FilteringEvent(object sender, System.Windows.Data.FilterEventArgs e)
        {
            e.Accepted = (e.Item as IFilterable)?.IsVisibleThroughFilter == true;
        }

        private void BackgroundTask()
        {
            while (runLoop)
            {
                // Clear the list to start fresh every time.
                lock (sync)
                {
                    Items.Clear();
                }

                // Sleep a bit for the UI to catch up.
                Thread.Sleep(100);

                // Add items.
                for (int i = 1000; i >= 0; i--)
                {
                    lock (sync)
                    {
                        Items.Add(new Item(i));
                    }
                }

                // Mark items as visible.
                foreach (var item in Items)
                {
                    item.IsVisibleThroughFilter = true;
                }

                // Do a low priority check through the Dispatcher (after the UI updated the view)
                // When counts don't match, stop the loop.
                Dispatcher.Invoke(() =>
                {
                    if ((MyDataGrid.ItemsSource as ListCollectionView).Count != Items.Count)
                    {
                        MessageBox.Show("The view does not hold the same amount of items as the collection");
                        runLoop = false;
                    }
                }, DispatcherPriority.ApplicationIdle);
            }
        }

        private void StartBackgroundLoop(object sender, RoutedEventArgs e)
        {
            runLoop = true;
            Task.Run(BackgroundTask);
        }
    }
}