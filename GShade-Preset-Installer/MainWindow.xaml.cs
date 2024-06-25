using System.Windows;

namespace GShadePresetInstaller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool nextDown = false;

        public MainWindow()
        {
            InitializeComponent();
            this.Title = App._windowTitle;
            this.textHeader.Text = App._instName;
            this.textInstructions.Text = "Thanks for downloading the " + App._presetPathName + " presets! Press Start to begin installation.";
            App.InitLog();
        }
        private void Next_Progress(object sender, RoutedEventArgs e)
        {
            if (!nextDown)
            {
                nextDown = true;

                if (!App.BuildGamePaths())
                {
                    MessageBox.Show("No GShade installations found. The preset installer will now close.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    App.CloseLog();
                    this.Close();
                }
                else if (!App.ZipExtractionProcess())
                {
                    MessageBox.Show("Preset extraction failed. The preset installer will now close.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    App.CloseLog();
                    this.Close();
                }
                else
                {
                    App.FileDeploymentProcess();

                    MessageBox.Show("Preset installation complete!", "Success", MessageBoxButton.OK, MessageBoxImage.None);
                    App.CloseLog();
                    this.Close();
                }
            }
        }
        private void Cancel_Close(object sender, RoutedEventArgs e)
        {
            App.CloseLog();
            System.Windows.Application.Current.Shutdown();
        }
    }
}
