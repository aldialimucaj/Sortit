using Sortit.al.aldi.sortit.control;
using Sortit.al.aldi.sortit.model;
using Sortit.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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

        public MainWindow()
        {
            InitializeComponent();

            // Setting the window Icon
            var iconStream = new MemoryStream();
            Icon icon = Properties.Resources.format_indent_more;
            icon.Save(iconStream);
            iconStream.Seek(0, SeekOrigin.Begin);
            Icon = BitmapFrame.Create(iconStream);

            // Console for debug purposes
#if DEBUG
            AllocConsole();
#endif
        }

        /// <summary>
        /// Clicking the start button starts the listing and sorting process.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            String filePath = txtSourceFolder.Text.EndsWith("\\") ? txtSourceFolder.Text : txtSourceFolder.Text + "\\";
            IList<File2Sort> files = null;

            Func<File2Sort, bool> checkConfig = GetCheckFileConfig;
            files = IOUtils.GetAllFiles(filePath, txtPattern.Text, checkConfig).ToList();

            // Adding entries in the treeView
            AddItemsToTree(files);

            // Status bar update 
            UpdateStatusBar(files);

            ISort algorithm = GetSelectedAlgorithm();
            if (null != algorithm)
            {
                algorithm.PrepareForSorting(files);
                algorithm.Sort(files);
                if (chckCleanEmptyDir.IsChecked.Value)
                {
                    IOUtils.CleanEmptyDirs(txtDestinationFolder.Text);
                    IOUtils.CleanEmptyDirs(txtSourceFolder.Text);
                }
            }

        }

        private void btnCalculate_Click(object sender, RoutedEventArgs e)
        {
            String filePath = txtSourceFolder.Text.EndsWith("\\") ? txtSourceFolder.Text : txtSourceFolder.Text + "\\";
            IList<File2Sort> files = null;

            Func<File2Sort, bool> checkConfig = GetCheckFileConfig;
            files = IOUtils.GetAllFiles(filePath, txtPattern.Text, checkConfig).ToList(); //todo adding checkConfig here prevents the sorting.

            ISort algorithm = GetSelectedAlgorithm();
            if (null != algorithm)
            {
                algorithm.PrepareForSorting(files);
            }

            // Adding entries in the treeView
            AddItemsToTree(files);

            // Status bar update 
            UpdateStatusBar(files);
        }



        private void UpdateStatusBar(IList<File2Sort> files)
        {
            long generalSize = (from f in files select f.RawSourceFile.Length).Sum() / (1024 * 1024);
            txtGeneralSizeValue.Text = generalSize.ToString() + " MB";

        }


        private void AddItemsToTree(IList<File2Sort> files)
        {
            tvFilesTree.Items.Clear();
            foreach (File2Sort file in files)
            {
                TreeViewItem itemRoot = new TreeViewItem();
                itemRoot.Header = file.FileName;
                itemRoot.Items.Add("Source :-> " + file.FilePath);
                itemRoot.Items.Add("Destination :-> " + file.FullDestination);
                itemRoot.Items.Add("Size :-> " + (file.RawSourceFile.Length / (1024 * 1024)) + " MB");


                tvFilesTree.Items.Add(itemRoot);
            }
        }

        private bool GetCheckFileConfig(File2Sort file)
        {
            bool fileChecked = true;

            //compulsory checks


            // variable checks
            if (chckIgnoreNonAlpha.IsChecked.Value)
            {
                fileChecked &= file.IsAlphaNumeric;
            }
            if (!chckShowSorted.IsChecked.Value)
            {

                file.SetDestinationFullPath(GetSelectedAlgorithm().RenameFunc);
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
                    algorithm = new SortFilesAlpha(txtDestinationFolder.Text, Int32.Parse(txtDepth.Text), chckCopy.IsChecked.Value);
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

                Settings.Default["dir_source"] = txtSourceFolder.Text;
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

                Settings.Default["dir_destination"] = txtDestinationFolder.Text;
                Settings.Default.Save();
            }
        }


        private void OnExit(object sender, ExitEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void txtDestinationFolder_TextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.Default["dir_destination"] = txtDestinationFolder.Text;
            Properties.Settings.Default.Save();
        }

        private void txtSourceFolder_Textchanged(object sender, TextChangedEventArgs e)
        {
            Settings.Default["dir_source"] = txtSourceFolder.Text;
            Properties.Settings.Default.Save();
        }

        private void txtDepth_TextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.Default["value_depth"] = Int32.Parse(txtDepth.Text);
            Properties.Settings.Default.Save();
        }

        private void SortingType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.Default["sort_type"] = SortingType.SelectedIndex;
            Properties.Settings.Default.Save();
        }

        private void chckCleanEmptyDir_UnCheck(object sender, RoutedEventArgs e)
        {
            Settings.Default["chck_clean_dir"] = chckCleanEmptyDir.IsEnabled;
            Properties.Settings.Default.Save();
        }

        private void txtPattern_TextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.Default["crawl_pattern"] = txtPattern.Text;
            Settings.Default.Save();
        }

        private void chckIgnoreNonAlpha_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default["chck_ignore_non_alpha"] = true;
            Properties.Settings.Default.Save();
        }

        private void chckIgnoreNonAlpha_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default["chck_ignore_non_alpha"] = false;
            Properties.Settings.Default.Save();
        }

        private void chckCleanEmptyDir_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default["chck_clean_dir"] = false;
            Properties.Settings.Default.Save();
        }

        private void chckCleanEmptyDir_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default["chck_clean_dir"] = true;
            Properties.Settings.Default.Save();
        }

        private void chckCopy_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default["chck_copy"] = true;
            Properties.Settings.Default.Save();
        }

        private void chckCopy_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default["chck_copy"] = false;
            Properties.Settings.Default.Save();
        }

        private void chckShowSorted_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default["chck_show_sorted"] = false;
            Properties.Settings.Default.Save();
        }

        private void chckShowSorted_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default["chck_show_sorted"] = true;
            Properties.Settings.Default.Save();
        }

        private void updateButtonClick(object sender, RoutedEventArgs e)
        {
            if (e.Source.GetType() == typeof(System.Windows.Controls.Button))
            {
                txtOperationValue.Text = ((System.Windows.Controls.Button)e.Source).Content.ToString();
            }
        }


    }
}
