using log4net;
using Sortit.al.aldi.sortit.control;
using Sortit.al.aldi.sortit.model;
using Sortit.al.aldi.sortit.views;
using Sortit.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sortit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // binding console for debug purposes
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        private static readonly ILog x = LogManager.GetLogger("MainWindow");

        // --------------------------------------------------------------------

        const String ALPHA_GRID_INSTANCE = "alphaGridInstance";
        const String DATE_GRID_INSTANCE = "dateGridInstance";

        AlphaGrid alphaGridInstance = null;
        DateGrid dateGridInstance = null;

        // --------------------------------------------------------------------

        private readonly BackgroundWorker workerPrepareSorting = new BackgroundWorker();
        private ISort Algorithm = null;

        private delegate void UpdateSortFilesDelegate();
        private event UpdateSortFilesDelegate UpdateSortFilesEvent;

        private IList<File2Sort> _sortFiles = null;
        private IList<File2Sort> SortFiles { get { return _sortFiles; } set { _sortFiles = value; UpdateSortFilesEvent(); } }

        public MainWindow()
        {
#if DEBUG
            AllocConsole();
#endif
            log4net.Config.XmlConfigurator.Configure();
            x.Info("Starting Application");

            InitializeComponent();

            UpdateSortFilesEvent = UpdateStartButton;
            UpdateSortFilesEvent();

            // these are instances of UI objects instantiated in XAML
            alphaGridInstance = FindResource(ALPHA_GRID_INSTANCE) as AlphaGrid;
            dateGridInstance = FindResource(DATE_GRID_INSTANCE) as DateGrid;

            // Registering background worker for intense computation
            workerPrepareSorting.DoWork += worker_DoWork;
            workerPrepareSorting.RunWorkerCompleted += worker_RunWorkerCompleted;

            // Setting the window Icon
            var iconStream = new MemoryStream();
            Icon icon = Properties.Resources.format_indent_more;
            icon.Save(iconStream);
            iconStream.Seek(0, SeekOrigin.Begin);
            Icon = BitmapFrame.Create(iconStream);

            mnItemExit.Click += mnItemExit_Click;

            // Console for debug purposes

        }

        /// <summary>
        /// Handles Worker's resolution.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                // The user canceled the operation.
            }
            else if (e.Error != null)
            {
                // There was an error during the operation.
                string msg = String.Format("An error occurred: {0}", e.Error.Message);
            }
            else
            {
                // The operation completed normally.
                string msg = String.Format("Result = {0}", e.Result);
            }
        }

        /// <summary>
        /// Executes a background worker for prepraring the files for sorting and adding them to the tree.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = sender as BackgroundWorker;

            // get the right object type here
            Tuple<System.Windows.Controls.TreeView, IList<File2Sort>, ISort> arg = e.Argument as Tuple<System.Windows.Controls.TreeView, IList<File2Sort>, ISort>;

            // prepare the files for sorting.
            // generates the desired path
            ISort algorithm = arg.Item3;
            if (null != algorithm)
            {
                algorithm.PrepareForSorting(arg.Item2);
            }

            AddItemsToTree(bw, arg.Item1, arg.Item2);

            // if uses decides to stop operation
            if (bw.CancellationPending)
            {
                e.Cancel = true;
            }

        }

        /// <summary>
        /// Clicking the start button starts the listing and sorting process.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            await Algorithm.SortAsync(SortFiles);
            // clean up

            if (GeneralSettings.chckCleanEmptyDir.IsChecked.Value)
            {
                // cleaning both source and destination folders for empty directories
                IOUtils.CleanEmptyDirs(txtDestinationFolder.Text);
                IOUtils.CleanEmptyDirs(txtSourceFolder.Text);
            }

        }

        /// <summary>
        /// Calculate the files to be sorted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnCalculate_Click(object sender, RoutedEventArgs e)
        {
            if(txtSourceFolder.Text.Equals(""))
            {
                System.Windows.MessageBox.Show("Cant crawl an empty dir!");
                return;
            }
            if (txtDestinationFolder.Text.Equals(""))
            {
                txtDestinationFolder.Text = txtSourceFolder.Text;
            }

            String filePath = txtSourceFolder.Text.EndsWith("\\") ? txtSourceFolder.Text : txtSourceFolder.Text + "\\";
            

            Algorithm = GetSelectedAlgorithm();

            Func<File2Sort, bool> checkConfig = GetCheckFileConfig;
            SortFiles = await IOUtils.GetAllFiles(filePath, txtPattern.Text, checkConfig); //todo adding checkConfig here prevents the sorting.
            //RegisterObserver(files);
      

            // Adding entries in the treeView
            Tuple<System.Windows.Controls.TreeView, IList<File2Sort>, ISort> arg = new Tuple<System.Windows.Controls.TreeView, IList<File2Sort>, ISort>(tvFilesTree, SortFiles, Algorithm);
            workerPrepareSorting.RunWorkerAsync(arg);

            // Status bar update 
            UpdateStatusBar(arg.Item2);
        }

        /// <summary>
        /// Adds items to the list view. Should be run in background and called by a background worker.
        /// </summary>
        /// <param name="bw"></param>
        /// <param name="files"></param>
        private void AddItemsToTree(BackgroundWorker bw, System.Windows.Controls.TreeView tree, IList<File2Sort> files)
        {
            //tree.Items.Clear();

            tree.Dispatcher.Invoke(new Action(delegate()
                {
                    tree.Items.Clear();
                }
            ));

            foreach (File2Sort file in files)
            {
                if (!bw.CancellationPending)
                {
                    AddItemToTree(tree, file);
                    // registering objserver before updating
                    Algorithm.RegisterObserver(file, UpdateFileChanged);
                }
                else
                {
                    break;
                }
            }
        }

        private void AddItemToTree(System.Windows.Controls.TreeView tree, File2Sort file)
        {

            //var retInt = tree.Items.Add(itemRoot);
            tree.Dispatcher.Invoke(new Action(delegate()
            {
                TreeViewItem itemRoot = new TreeViewItem();
                itemRoot.Tag = file;
                itemRoot.Header = file.FileName;
                itemRoot.Items.Add("Source :-> " + file.FilePath);
                itemRoot.Items.Add("Destination :-> " + file.FullDestination);
                itemRoot.Items.Add("Size :-> " + (file.RawSourceFile.Length / (1024 * 1024)) + " MB");
                itemRoot.Items.Add("Created :-> " + file.CreatedDateTime);

                tree.Items.Add(itemRoot);
            }
            ));
        }

        private bool GetCheckFileConfig(File2Sort file)
        {
            bool fileChecked = true;

            //compulsory checks

            // variable checks
            if (alphaGridInstance.chckIgnoreNonAlpha.IsChecked.Value)
            {
                fileChecked &= file.IsAlphaNumeric;
            }
            if (!GeneralSettings.chckShowSorted.IsChecked.Value)
            {
                // updating the destination path based on the algorithm
                file.SetDestinationFullPath(Algorithm.RenameFunc);
                fileChecked &= file.IsAlreadySorted;
            }

            return fileChecked;

        }

        private ISort GetSelectedAlgorithm()
        {
            ISort algorithm = null;

            switch (((ComboBoxItem)SortingType.SelectedItem).Name)
            {
                case "alpha":
                    algorithm = new SortFilesAlpha(txtDestinationFolder.Text, Int32.Parse(alphaGridInstance.txtDepth.Text), GeneralSettings.chckCopy.IsChecked.Value, GeneralSettings.chckOverwriteExisting.IsChecked.Value);
                    break;

                case "date":
                    algorithm = new SortFilesDate(txtDestinationFolder.Text, alphaGridInstance.txtDepth.Text, GeneralSettings.chckCopy.IsChecked.Value, ((ComboBoxItem)dateGridInstance.cmbSortType.SelectedItem).Name, GeneralSettings.chckOverwriteExisting.IsChecked.Value);
                    break;
            }

            return algorithm;
        }

        private void btnSourceFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();

            // Show open file dialog box
            DialogResult result = dlg.ShowDialog();
            if (!dlg.SelectedPath.Equals(""))
            {
                txtSourceFolder.Text = dlg.SelectedPath;

                Settings.Default[txtSourceFolder.Name] = txtSourceFolder.Text;
                Settings.Default.Save();
            }
        }

        private void btnDestinationFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();

            // Show open file dialog box
            DialogResult result = dlg.ShowDialog();
            if (!dlg.SelectedPath.Equals(""))
            {
                txtDestinationFolder.Text = dlg.SelectedPath;

                Settings.Default[txtDestinationFolder.Name] = txtDestinationFolder.Text;
                Settings.Default.Save();
            }
        }


        private void OnExit(object sender, ExitEventArgs e)
        {
            Properties.Settings.Default.Save();
        }
               
        private void saveControlChanges(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox)
            {
                System.Windows.Controls.TextBox box = sender as System.Windows.Controls.TextBox;
                Settings.Default[box.Name] = box.Text;
            }
            else if (sender is System.Windows.Controls.CheckBox)
            {
                System.Windows.Controls.CheckBox box = sender as System.Windows.Controls.CheckBox;
                Settings.Default[box.Name] = !box.IsChecked;
            }
            else if (sender is System.Windows.Controls.ComboBox)
            {
                System.Windows.Controls.ComboBox box = sender as System.Windows.Controls.ComboBox;
                Settings.Default[box.Name] = box.SelectedIndex;
            }

            Properties.Settings.Default.Save();
        }

        private void updateButtonClick(object sender, RoutedEventArgs e)
        {
            if (e.Source.GetType() == typeof(System.Windows.Controls.Button))
            {
                txtOperationValue.Text = ((System.Windows.Controls.Button)e.Source).Content.ToString();
            }
        }


        private void UpdateStartButton()
        {
            btnStart.IsEnabled = SortFiles != null && SortFiles.Count > 0;
        }

        private void UpdateFileChanged(File2Sort file, File2Sort.FileChangesType type)
        {
            TreeViewItem foundedItem = null;
            tvFilesTree.Dispatcher.BeginInvoke((Action)(() =>
            {
                foreach (TreeViewItem tvItem in tvFilesTree.Items)
                {
                    if (tvItem.Tag == file)
                    {
                        foundedItem = tvItem;
                        break;
                    }
                }

                if (null != foundedItem && File2Sort.FileChangesType.OPERATION_ENDED == type)
                {
                    foundedItem.FontWeight = FontWeights.Bold;
                    foreach (TreeViewItem tvi in tvFilesTree.Items)
                    {
                        tvi.FontWeight = FontWeights.Normal;
                    }
                }
            }));
        }


        private void UpdateStatusBar(IList<File2Sort> files)
        {
            long generalSize = (from f in files select f.RawSourceFile.Length).Sum() / (1024 * 1024);
            txtGeneralSizeValue.Text = generalSize.ToString() + " MB";
            txtItemsCount.Text = files.Count.ToString();

        }

        private void mnItemExit_Click(object o, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private sealed class AlgorithmDataTemplateSelector : DataTemplateSelector
        {
            public override DataTemplate SelectTemplate(object item, DependencyObject container)
            {
                return base.SelectTemplate(item, container);
            }

        }

        private void SortingType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            saveControlChanges(sender, e);

            if (null != alphaGridInstance && null != dateGridInstance)
            {
                String selectedAlgorithm = ((ComboBoxItem)SortingType.SelectedItem).Name;

                // switching will replace the context sensitive area. 3 ist the current order of the grid location
                switch (selectedAlgorithm)
                {
                    case "alpha":
                        this.mainPanel.Children.Remove(dateGridInstance);
                        if (!this.mainPanel.Children.Contains(alphaGridInstance))
                            this.mainPanel.Children.Insert(3, alphaGridInstance);
                        break;

                    case "date":
                        this.mainPanel.Children.Remove(alphaGridInstance);
                        if (!this.mainPanel.Children.Contains(dateGridInstance))
                            this.mainPanel.Children.Insert(3, dateGridInstance);
                        break;
                }
            }
        }
    }
}
