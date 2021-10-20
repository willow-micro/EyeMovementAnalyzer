// Default
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Syncfusion.SfSkinManager;
// Additional (Microsoft)
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Configuration;
// Additional (Third party)
using Syncfusion.UI.Xaml.Charts;
using SimplifiedEyeTracker;


namespace EyeMovementAnalyzer
{
    /// <summary>
    /// Constants for MainWindow.xaml
    /// </summary>
    public class Constants
    {
        #region Fields
        public readonly double targetPointRadius;
        public readonly double primaryScreenWidth;
        public readonly double primaryScreenHeight;
        public readonly int timeSecForEachAttempt;
        public readonly int numberOfAttempts;
        #endregion

        /// <summary>
        /// Constructor. Define constants here.
        /// </summary>
        public Constants()
        {
            // Constants from appsettings.json
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .Build();
            IConfigurationSection settings = configuration.GetSection("Settings");
            this.targetPointRadius = settings.GetSection("TargetPointRadius").Get<double>();
            this.timeSecForEachAttempt = settings.GetSection("TimeSecForEachAttempt").Get<int>();
            this.numberOfAttempts = settings.GetSection("NumberOfAttempts").Get<int>();
            // Constants declared here
            this.primaryScreenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            this.primaryScreenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
        }
    }
    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
		#region Fields4SF
        private string currentVisualStyle;
		private string currentSizeMode;
        #endregion

        #region Properties4SF
        /// <summary>
        /// Gets or sets the current visual style.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public string CurrentVisualStyle
        {
            get
            {
                return currentVisualStyle;
            }
            set
            {
                currentVisualStyle = value;
                OnVisualStyleChanged();
            }
        }
		
		/// <summary>
        /// Gets or sets the current Size mode.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public string CurrentSizeMode
        {
            get
            {
                return currentSizeMode;
            }
            set
            {
                currentSizeMode = value;
                OnSizeModeChanged();
            }
        }
        #endregion

        #region Fields
        private readonly Constants constants;
        private readonly Ellipse targetPoint;
        private readonly Button startButton;
        private readonly System.Windows.Threading.DispatcherTimer dispatcherTimer;
        private readonly EyeTracker eyeTracker;
        private readonly SFChartViewModel chartVM;
        #endregion

        #region Properties
        private int CurrentAttemptNumber { get; set; }
        #endregion

        /// <summary>
        /// MainWindow Constructor.
        /// </summary>
        public MainWindow()
        {
            // Default
            InitializeComponent();
			this.Loaded += OnLoaded;
            // Initialize constants
            this.constants = new Constants();
            Debug.Print($"Width: {constants.primaryScreenWidth}");
            Debug.Print($"Height: {constants.primaryScreenHeight}");
            // Initialize attempt status
            this.CurrentAttemptNumber = 0;
            // Initialize canvas stuffs
            this.targetPoint = new Ellipse()
            {
                Fill = Brushes.Gray,
                Width = this.constants.targetPointRadius * 2.0,
                Height = this.constants.targetPointRadius * 2.0
            };
            // Initialize Button
            this.startButton = new Button()
            {
                Content = "Start",
                Name = "StartButton",
                Width = 50,
                Height = 20
            };
            this.startButton.Click += OnStartButton;
            // Initialize DispatcherTimer
            this.dispatcherTimer = new System.Windows.Threading.DispatcherTimer()
            {
                Interval = new TimeSpan(0, 0, this.constants.timeSecForEachAttempt)
            };
            this.dispatcherTimer.Tick += new EventHandler(this.UpdateTargetPointPosition);
            // Initialize Eye Tracker
            try
            {
                this.eyeTracker = new EyeTracker(constants.primaryScreenWidth, constants.primaryScreenHeight);
                Debug.Print($"Model: {this.eyeTracker.GetModel()}");
                Debug.Print($"SerialNumber: {this.eyeTracker.GetSerialNumber()}");
                Debug.Print($"Width: {constants.primaryScreenWidth}, Height: {constants.primaryScreenHeight}");
                Debug.Print($"WidthInMillimeters: {this.eyeTracker.CalcMillimetersFromPixels(constants.primaryScreenWidth)}, HeightInMillimeters: {this.eyeTracker.CalcMillimetersFromPixels(constants.primaryScreenHeight)}");
                Debug.Print($"PixelPitch: {this.eyeTracker.CalcPixelPitch()}");
                this.eyeTracker.OnGazeData += this.OnGazeData;
            }
            catch (ArgumentOutOfRangeException e)
            {
                Debug.Print(e.Message);
                this.Close();
            }
            catch (InvalidOperationException e)
            {
                if (MessageBox.Show(e.Message, "OK", MessageBoxButton.OK, MessageBoxImage.Error) == MessageBoxResult.OK)
                {
                    this.Close();
                }
            }
            // Initialize SFCharts dependencies
            this.chartVM = new SFChartViewModel();
        }
        
        /// <summary>
        /// [SFDefault] Called when [loaded].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            CurrentVisualStyle = "FluentLight";
	        CurrentSizeMode = "Default";
        }
        /// <summary>
        /// [SFDefault] On Visual Style Changed.
        /// </summary>
        /// <remarks></remarks>
        private void OnVisualStyleChanged()
        {
            VisualStyles visualStyle = VisualStyles.Default;
            Enum.TryParse(CurrentVisualStyle, out visualStyle);            
            if (visualStyle != VisualStyles.Default)
            {
                SfSkinManager.ApplyStylesOnApplication = true;
                SfSkinManager.SetVisualStyle(this, visualStyle);
                SfSkinManager.ApplyStylesOnApplication = false;
            }
        }
        /// <summary>
        /// [SFDefault] On Size Mode Changed event.
        /// </summary>
        /// <remarks></remarks>
        private void OnSizeModeChanged()
        {
            SizeMode sizeMode = SizeMode.Default;
            Enum.TryParse(CurrentSizeMode, out sizeMode);
            if (sizeMode != SizeMode.Default)
            {
                SfSkinManager.ApplyStylesOnApplication = true;
                SfSkinManager.SetSizeMode(this, sizeMode);
                SfSkinManager.ApplyStylesOnApplication = false;
            }
        }

        /// <summary>
        /// Ask user to close or not when the Esc key was pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AppCloseEvent(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure to close?", "Close", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                this.Close();
            }
        }

        /// <summary>
        /// Called when [contents were rendered].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The instance containing the event data.</param>
        private void OnContentRendered(object sender, EventArgs e)
        {
            Grid.SetRow(this.startButton, 0);
            Grid.SetColumn(this.startButton, 0);
            this.mainGrid.Children.Add(this.startButton);
        }

        /// <summary>
        /// Called when [start button was clicked].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The instance containing the event data.</param>
        private void OnStartButton(object sender, RoutedEventArgs e)
        {
            this.dispatcherTimer.Start();
            this.mainGrid.Children.Remove(this.startButton);            
        }

        private void UpdateTargetPointPosition(object sender, EventArgs e)
        {
            // Add the point if not exists
            if (!this.mainCanvas.Children.Contains(this.targetPoint)) {
                this.mainCanvas.Children.Add(this.targetPoint);
                this.eyeTracker.StartReceivingGazeData();
            }
            // Update position
            if (this.CurrentAttemptNumber < this.constants.numberOfAttempts)
            {
                Debug.Print($"Attempt: {this.CurrentAttemptNumber}");
                double halfWidth = this.constants.primaryScreenWidth / 2.0;
                double factor = (double)this.CurrentAttemptNumber++ / (double)(this.constants.numberOfAttempts - 1);
                double offset = (this.constants.primaryScreenWidth / 4.0) - this.constants.targetPointRadius;
                double xPos = halfWidth * factor + offset;
                Debug.Print($"X: {xPos}");
                Canvas.SetLeft(this.targetPoint, xPos);
                Canvas.SetTop(this.targetPoint, constants.primaryScreenHeight / 2 - constants.targetPointRadius);
            }
            else
            {
                this.eyeTracker.StopReceivingGazeData();
                this.dispatcherTimer.Stop();
                this.mainCanvas.Children.Remove(this.targetPoint);
                this.ShowChart();
            }
        }

        private void OnGazeData(object sender, SimplifiedGazeDataEventArgs e)
        {
            if (e.IsLeftValid &&
                e.LeftX >= 0.0 && e.LeftX <= this.eyeTracker.GetScreenWidthInPixels() &&
                e.LeftY >= 0.0 && e.LeftY <= this.eyeTracker.GetScreenHeightInPixels())
            {
                Debug.Print($"X: {e.LeftX}, Y: {e.LeftY}");
                //Debug.Print($"DeviceTimeStamp: {e.DeviceTimeStamp}, SystemTimeStamp: {e.SystemTimeStamp}");
                //Debug.Print($"Interval: {e.SystemTimeStampInterval / 1000} [ms]");
                Debug.Print($"AngularVelocity: {e.LeftGazeAngularVelocity} [deg/s]");
                Debug.Print((e.LeftEyeMovementType == EyeMovementType.Fixation) ? "Fixation" : "Not");
                EyeDataForChart newData = new EyeDataForChart()
                {
                    TimestampInMs = e.DeviceTimeStamp,
                    XCoordinate = e.LeftX,
                    AngularVelocity = e.LeftGazeAngularVelocity
                };
                this.chartVM.Data.Add(newData);
            }
        }

        private void ShowChart()
        {
            // Initialize a chart
            this.DataContext = this.chartVM;
            SfChart chart = new SfChart();

            // Initialize axes
            // Timestamp
            NumericalAxis timestampAxis = new NumericalAxis()
            {
                Header = "Timestamp[ms]"
            };
            // X Coordinate
            NumericalAxis xCoordinateAxis = new NumericalAxis()
            {
                Header = "X[px]",
                Interval = 250
            };
            // Angular Velocity
            NumericalAxis angularVelocityAxis = new NumericalAxis()
            {
                Header = "Velocity[rad/s]",
                OpposedPosition = true,
                Maximum = 500,
                Minimum = 0,
                Interval = 25
            };

            // Initialize series
            LineSeries xCoordinateSeries = new LineSeries()
            {
                ItemsSource = this.chartVM.Data,
                XBindingPath = "TimestampInMs",
                YBindingPath = "XCoordinate",
                Label = "X Coordinate",
                XAxis = timestampAxis,
                YAxis = xCoordinateAxis
            };
            LineSeries angularVelocitySeries = new LineSeries()
            {
                ItemsSource = this.chartVM.Data,
                XBindingPath = "TimestampInMs",
                YBindingPath = "AngularVelocity",
                Label = "Angular Velocity",
                XAxis = timestampAxis,
                YAxis = angularVelocityAxis
            };

            // Add series to the chart series collection
            chart.Series.Add(xCoordinateSeries);
            chart.Series.Add(angularVelocitySeries);

            // Set chart options
            chart.Header = "Gaze Data";
            chart.Legend = new ChartLegend();

            // Add chart to the grid for drawing
            Grid.SetRow(chart, 0);
            Grid.SetColumn(chart, 0);
            this.mainGrid.Children.Add(chart);
        }
    }
}
