﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using System.Windows;
using System.Windows.Controls;
using CsvHelper;
using CsvHelper.Configuration;
using OxyPlot.Legends;
using System.Windows.Input;

namespace Redysim_trial
{
    public partial class MainWindow : Window
    {
        private string selectedCsvFilePath;
        public ObservableCollection<GravityData> GravityDataList { get; set; }
        public ObservableCollection<ForceData> ForceDataList { get; set; }
        public ObservableCollection<KinematicData> KinematicDataList { get; set; }
        public ObservableCollection<DynamicData> DynamicDataList { get; set; }

        private PlotModel plotModel;

        private Dictionary<string, LineSeries> seriesDictionary;

        private Dictionary<string, List<DataRecord>> dataDictionary;
        public MainWindow()
        {
            InitializeComponent();
            InitializePlot();

            seriesDictionary = new Dictionary<string, LineSeries>();
            dataDictionary = new Dictionary<string, List<DataRecord>>();

            GravityDataList = new ObservableCollection<GravityData>
            {
                new GravityData { gx = 0, gy = 0, gz = 0 }
            };
            GravityTable.ItemsSource = GravityDataList;

            ForceDataList = new ObservableCollection<ForceData>
            {
                new ForceData { Force = 0 }
            };
            Force.ItemsSource = ForceDataList;

            //Adding blank rows to the datagrids

            KinematicDataList = new ObservableCollection<KinematicData>
            {
                new KinematicData { JointNo = "", JointType = "", JointOffset = "", JointAngle = "", LinkLength = "", TwistAngle = "", JVInitial = "", JVFinal = "" }
            };

            DynamicDataList = new ObservableCollection<DynamicData>
            {
                new DynamicData { Mass = "", LinkLength1 = "",ActuatedJoint = "",bt = "", dx = "", dy = "", dz = "", Ixx = "", Iyy = "", Izz = "", Ixy = "", Iyz = "", Izx = "" }
            };

            // Bind collections to DataGrids
            UITablekin.ItemsSource = KinematicDataList;
            UITabledyn.ItemsSource = DynamicDataList;
        }

        private void LoadCSVData(string csvFilePath)
        {
            try
            {
                var combinedData = GetCombinedDataFromCsv(csvFilePath);

                // Split combined data into kinematic and dynamic data
                var kinematicData = combinedData.Select(cd => new KinematicData
                {
                    JointNo = cd.JointNo,
                    JointType = cd.JointType,
                    JointOffset = cd.JointOffset,
                    JointAngle = cd.JointAngle,
                    LinkLength = cd.LinkLength,
                    TwistAngle = cd.TwistAngle,
                    JVInitial = cd.JVInitial,
                    JVFinal = cd.JVFinal
                }).ToList();

                var dynamicData = combinedData.Select(cd => new DynamicData
                {
                    Mass = cd.Mass,
                    LinkLength1 = cd.LinkLength1,
                    ActuatedJoint = cd.ActuatedJoint,
                    bt = cd.bt,
                    dx = cd.dx,
                    dy = cd.dy,
                    dz = cd.dz,
                    Ixx = cd.Ixx,
                    Iyy = cd.Iyy,
                    Izz = cd.Izz,
                    Ixy = cd.Ixy,
                    Iyz = cd.Iyz,
                    Izx = cd.Izx
                }).ToList();

                UITablekin.ItemsSource = kinematicData;
                UITabledyn.ItemsSource = dynamicData;

                Save_Button.IsEnabled = true;
                ClearSelection_Button.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading CSV file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private List<CombinedData> GetCombinedDataFromCsv(string path)
        {
            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                return csv.GetRecords<CombinedData>().ToList();
            }
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            GravityDataList.Clear();
            GravityDataList.Add(new GravityData { gx = 0, gy = 0, gz = 0 });

            ForceDataList.Clear();
            ForceDataList.Add(new ForceData { Force = 0 });

            Time_spinner.Value = 0;
            Step_spinner.Value = 0;

            plotModel.Series.Clear();
            plotModel.InvalidatePlot(true);

            j1_position.IsChecked = false;
            j1_velocity.IsChecked = false;
            j1_acceleration.IsChecked = false;
            j1_force.IsChecked = false;

            // Clear the checkboxes for Joint 2
            j2_position.IsChecked = false;
            j2_velocity.IsChecked = false;
            j2_acceleration.IsChecked = false;
            j2_force.IsChecked = false;

            // Clear the checkboxes for Joint 3
            j3_position.IsChecked = false;
            j3_velocity.IsChecked = false;
            j3_acceleration.IsChecked = false;
            j3_force.IsChecked = false;

            // Clear the checkboxes for Joint 4
            j4_position.IsChecked = false;
            j4_velocity.IsChecked = false;
            j4_acceleration.IsChecked = false;
            j4_force.IsChecked = false;

            // Clear the checkboxes for Joint 5
            j5_position.IsChecked = false;
            j5_velocity.IsChecked = false;
            j5_acceleration.IsChecked = false;
            j5_force.IsChecked = false;

            // Clear the checkboxes for Joint 6
            j6_position.IsChecked = false;
            j6_velocity.IsChecked = false;
            j6_acceleration.IsChecked = false;
            j6_force.IsChecked = false;

            if (sender is MenuItem menuItem)
            {
                selectedCsvFilePath = menuItem.Tag as string;

                if (selectedCsvFilePath != null && parentmenuitem != null)
                {
                    parentmenuitem.Header = menuItem.Header;

                    // Determine visibility based on selected CSV file
                    if (selectedCsvFilePath == "RR.csv" || selectedCsvFilePath == "RP.csv")
                    {
                        J1.Visibility = Visibility.Visible;
                        J2.Visibility = Visibility.Visible;
                        J3.Visibility = Visibility.Collapsed;
                        J4.Visibility = Visibility.Collapsed;
                        J5.Visibility = Visibility.Collapsed;
                        J6.Visibility = Visibility.Collapsed;
                        L1.Visibility = Visibility.Visible;
                        L2.Visibility = Visibility.Visible;
                        L3.Visibility = Visibility.Collapsed;
                        L4.Visibility = Visibility.Collapsed;
                        L5.Visibility = Visibility.Collapsed;
                        L6.Visibility = Visibility.Collapsed;
                    }
                    else if (selectedCsvFilePath == "RRR.csv")
                    {
                        J1.Visibility = Visibility.Visible;
                        J2.Visibility = Visibility.Visible;
                        J3.Visibility = Visibility.Visible;
                        J4.Visibility = Visibility.Collapsed;
                        J5.Visibility = Visibility.Collapsed;
                        J6.Visibility = Visibility.Collapsed;
                        L1.Visibility = Visibility.Visible;
                        L2.Visibility = Visibility.Visible;
                        L3.Visibility = Visibility.Visible;
                        L4.Visibility = Visibility.Collapsed;
                        L5.Visibility = Visibility.Collapsed;
                        L6.Visibility = Visibility.Collapsed;
                    }
                    else if (selectedCsvFilePath == "KUKA.csv" || selectedCsvFilePath == "Stanford_arm.csv")
                    {
                        J1.Visibility = Visibility.Visible;
                        J2.Visibility = Visibility.Visible;
                        J3.Visibility = Visibility.Visible;
                        J4.Visibility = Visibility.Visible;
                        J5.Visibility = Visibility.Visible;
                        J6.Visibility = Visibility.Visible;
                        L1.Visibility = Visibility.Visible;
                        L2.Visibility = Visibility.Visible;
                        L3.Visibility = Visibility.Visible;
                        L4.Visibility = Visibility.Visible;
                        L5.Visibility = Visibility.Visible;
                        L6.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        // Default visibility settings when no specific file is selected
                        J1.Visibility = Visibility.Collapsed;
                        J2.Visibility = Visibility.Collapsed;
                        J3.Visibility = Visibility.Collapsed;
                        J4.Visibility = Visibility.Collapsed;
                        J5.Visibility = Visibility.Collapsed;
                        J6.Visibility = Visibility.Collapsed;
                        L1.Visibility = Visibility.Collapsed;
                        L2.Visibility = Visibility.Collapsed;
                        L3.Visibility = Visibility.Collapsed;
                        L4.Visibility = Visibility.Collapsed;
                        L5.Visibility = Visibility.Collapsed;
                        L6.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }
        private string lastSavedFilePath;

        private bool SaveButton_Clicked = false;
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {

            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;

            string defaultFileName = @"ExportedData.csv";

            string filePath = Path.Combine(appDirectory, defaultFileName);

            SaveAllData(filePath);

            lastSavedFilePath = filePath;

            SaveButton_Clicked = true;

            plotModel.Series.Clear();
            plotModel.InvalidatePlot(true);

            MessageBox.Show($"Data exported to file:\n{filePath}", "Export Successful", MessageBoxButton.OK);
        }

        private void SaveAllData(string filePath)
        {
            try
            {
                var kinData = UITablekin.ItemsSource as List<KinematicData>;
                var dynData = UITabledyn.ItemsSource as List<DynamicData>;

                string time = Time_spinner.Value != null ? Time_spinner.Value.ToString() : "0";
                string step = Step_spinner.Value != null ? Step_spinner.Value.ToString() : "0";

                var gravityData = GravityDataList.FirstOrDefault();
                string gx = gravityData != null ? gravityData.gx.ToString() : "0";
                string gy = gravityData != null ? gravityData.gy.ToString() : "0";
                string gz = gravityData != null ? gravityData.gz.ToString() : "0";

                var forceData = ForceDataList.FirstOrDefault();
                string force = forceData != null ? forceData.Force.ToString() : "0";

                using (var writer = new StreamWriter(filePath))
                using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
                {
                    // Write header
                    csv.WriteHeader<CombinedData>();
                    csv.NextRecord();

                    // Combine kinematic and dynamic data into CombinedData objects
                    var combinedData = kinData.Zip(dynData, (kin, dyn) => new CombinedData
                    {
                        // Kinematic Data properties
                        JointNo = kin.JointNo,
                        JointType = kin.JointType,
                        JointOffset = kin.JointOffset,
                        JointAngle = kin.JointAngle,
                        LinkLength = kin.LinkLength,
                        TwistAngle = kin.TwistAngle,
                        JVInitial = kin.JVInitial,
                        JVFinal = kin.JVFinal,

                        // Dynamic Data properties
                        Mass = dyn.Mass,
                        LinkLength1 = dyn.LinkLength1,
                        ActuatedJoint = dyn.ActuatedJoint,
                        bt = dyn.bt,
                        dx = dyn.dx,
                        dy = dyn.dy,
                        dz = dyn.dz,
                        Ixx = dyn.Ixx,
                        Iyy = dyn.Iyy,
                        Izz = dyn.Izz,
                        Ixy = dyn.Ixy,
                        Iyz = dyn.Iyz,
                        Izx = dyn.Izx,
                    }).ToList();

                    // Add additional data only to the first combined data record
                    if (combinedData.Any())
                    {
                        combinedData[0].Time = time;
                        combinedData[0].Step = step;
                        combinedData[0].gx = gx;
                        combinedData[0].gy = gy;
                        combinedData[0].gz = gz;
                        combinedData[0].Force = force;
                    }

                    // Write combined data
                    csv.WriteRecords(combinedData);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving data to CSV file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(selectedCsvFilePath))
            {
                LoadCSVData(selectedCsvFilePath);
            }
            else
            {
                MessageBox.Show("Please select a robot from the menu first.", "No Robot Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void ClearSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            selectedCsvFilePath = null;
            parentmenuitem.Header = "DOF";

            UITablekin.ItemsSource = null;
            UITabledyn.ItemsSource = null;

            Save_Button.IsEnabled = false;
            ClearSelection_Button.IsEnabled = false;

            J1.Visibility = Visibility.Collapsed;
            J2.Visibility = Visibility.Collapsed;
            J3.Visibility = Visibility.Collapsed;
            J4.Visibility = Visibility.Collapsed;
            J5.Visibility = Visibility.Collapsed;
            J6.Visibility = Visibility.Collapsed;
            L1.Visibility = Visibility.Collapsed;
            L2.Visibility = Visibility.Collapsed;
            L3.Visibility = Visibility.Collapsed;
            L4.Visibility = Visibility.Collapsed;
            L5.Visibility = Visibility.Collapsed;
            L6.Visibility = Visibility.Collapsed;

            GravityDataList.Clear();
            GravityDataList.Add(new GravityData { gx = 0, gy = 0, gz = 0 });

            ForceDataList.Clear();
            ForceDataList.Add(new ForceData { Force = 0 });

            Time_spinner.Value = 0;
            Step_spinner.Value = 0;

            UITablekin.ItemsSource = KinematicDataList;
            UITabledyn.ItemsSource = DynamicDataList;

            plotModel.Series.Clear();
            plotModel.InvalidatePlot(true);

            j1_position.IsChecked = false;
            j1_velocity.IsChecked = false;
            j1_acceleration.IsChecked = false;
            j1_force.IsChecked = false;

            // Clear the checkboxes for Joint 2
            j2_position.IsChecked = false;
            j2_velocity.IsChecked = false;
            j2_acceleration.IsChecked = false;
            j2_force.IsChecked = false;

            // Clear the checkboxes for Joint 3
            j3_position.IsChecked = false;
            j3_velocity.IsChecked = false;
            j3_acceleration.IsChecked = false;
            j3_force.IsChecked = false;

            // Clear the checkboxes for Joint 4
            j4_position.IsChecked = false;
            j4_velocity.IsChecked = false;
            j4_acceleration.IsChecked = false;
            j4_force.IsChecked = false;

            // Clear the checkboxes for Joint 5
            j5_position.IsChecked = false;
            j5_velocity.IsChecked = false;
            j5_acceleration.IsChecked = false;
            j5_force.IsChecked = false;

            // Clear the checkboxes for Joint 6
            j6_position.IsChecked = false;
            j6_velocity.IsChecked = false;
            j6_acceleration.IsChecked = false;
            j6_force.IsChecked = false;

            MessageBox.Show("Selection cleared.", "Clear Selection", MessageBoxButton.OK);
        }

        //private bool invDynButtonPressed = false;
        private void InvDyn_Click(object sender, RoutedEventArgs e)
        {
            if (!SaveButton_Clicked)
            {
                MessageBox.Show("Please press the Save button to Export the data first.", "Error",MessageBoxButton.OK);
                return;
            }
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;

            string defaultFileName = @"ExportedData.csv";

            string filePath = Path.Combine(appDirectory, defaultFileName);

            // Read all lines from the CSV file
            string[] lines = File.ReadAllLines(filePath);

            if (lines.Length < 2)
            {
                return;
            }

            // Extract the header and data rows
            string[] headers = lines[0].Split(',');
            string[][] data = lines.Skip(1).Select(line => line.Split(',')).ToArray();

            int numberOfColumns = data.Length;

            // Initialize matrices with the dynamic size based on the number of rows in the CSV file
            double Ti = 0;
            double Tp = 0;
            double step = 0;
            int dof = numberOfColumns;

            if (!double.TryParse(data[0][Array.IndexOf(headers, "Time")], out Tp) || !double.TryParse(data[0][Array.IndexOf(headers, "Step")], out step))
            {
                return;
            }

            double[] qin = new double[numberOfColumns];
            double[] qf = new double[numberOfColumns];
            for (int i = 0; i < numberOfColumns; i++)
            {
                if (!double.TryParse(data[i][Array.IndexOf(headers, "JVInitial")], out qin[i]))
                {
                    return;
                }

                if (!double.TryParse(data[i][Array.IndexOf(headers, "JVFinal")], out qf[i]))
                {
                    return;
                }
            }

            double[] time = new double[(int)((Tp - Ti) / step) + 1];
            //Console.WriteLine("Length of time array: " + time.Length);
            for (int i = 0; i < time.Length; i++)
            {
                time[i] = Ti + i * step;
            }

            double[,] theta = new double[time.Length, dof];
            double[,] dth = new double[time.Length, dof];
            double[,] ddth = new double[time.Length, dof];
            double[,] tor = new double[time.Length, dof];
            double[,] B1X = new double[time.Length, dof + 2];
            double[,] B1Y = new double[time.Length, dof + 2];
            double[,] B1Z = new double[time.Length, dof + 2];

            double[] aj = new double[numberOfColumns];
            double[] r = new double[numberOfColumns];
            double[] al = new double[numberOfColumns];
            double[] alp = new double[numberOfColumns];
            double[] a = new double[numberOfColumns];
            double[] b = new double[numberOfColumns];
            double[] th = new double[numberOfColumns];
            double[] bt = new double[numberOfColumns];
            double[] dx = new double[numberOfColumns];
            double[] dy = new double[numberOfColumns];
            double[] dz = new double[numberOfColumns];
            double[] m = new double[numberOfColumns];
            double[] g = new double[3];
            double[] Icxx = new double[numberOfColumns];
            double[] Icyy = new double[numberOfColumns];
            double[] Iczz = new double[numberOfColumns];
            double[] Icxy = new double[numberOfColumns];
            double[] Icyz = new double[numberOfColumns];
            double[] Iczx = new double[numberOfColumns];

            int n = numberOfColumns;
            // Populate the matrices and parse values as double
            for (int i = 0; i < numberOfColumns; i++)
            {
                if (!double.TryParse(data[i][Array.IndexOf(headers, "ActuatedJoint")], out aj[i]))
                {
                    return;
                }

                if (!double.TryParse(data[i][Array.IndexOf(headers, "JointType")], out r[i]))
                {
                    return;
                }

                if (!double.TryParse(data[i][Array.IndexOf(headers, "LinkLength1")], out al[i]))
                {
                    return;
                }

                if (!double.TryParse(data[i][Array.IndexOf(headers, "TwistAngle")], out alp[i]))
                {
                    return;
                }

                if (!double.TryParse(data[i][Array.IndexOf(headers, "LinkLength")], out a[i]))
                {
                    return;
                }

                if (!double.TryParse(data[i][Array.IndexOf(headers, "JointOffset")], out b[i]))
                {
                    return;
                }

                if (!double.TryParse(data[i][Array.IndexOf(headers, "JointAngle")], out th[i]))
                {
                    return;
                }

                if (!double.TryParse(data[i][Array.IndexOf(headers, "bt")], out bt[i]))
                {
                    return;
                }

                if (!double.TryParse(data[i][Array.IndexOf(headers, "dx")], out dx[i]))
                {
                    return;
                }

                if (!double.TryParse(data[i][Array.IndexOf(headers, "dy")], out dy[i]))
                {
                    return;
                }

                if (!double.TryParse(data[i][Array.IndexOf(headers, "dz")], out dz[i]))
                {
                    return;
                }

                if (!double.TryParse(data[i][Array.IndexOf(headers, "Mass")], out m[i]))
                {
                    return;
                }

                if (!double.TryParse(data[i][Array.IndexOf(headers, "Ixx")], out Icxx[i]))
                {
                    return;
                }

                if (!double.TryParse(data[i][Array.IndexOf(headers, "Iyy")], out Icyy[i]))
                {
                    return;
                }

                if (!double.TryParse(data[i][Array.IndexOf(headers, "Izz")], out Iczz[i]))
                {
                    return;
                }

                if (!double.TryParse(data[i][Array.IndexOf(headers, "Ixy")], out Icxy[i]))
                {
                    return;
                }

                if (!double.TryParse(data[i][Array.IndexOf(headers, "Iyz")], out Icyz[i]))
                {
                    return;
                }

                if (!double.TryParse(data[i][Array.IndexOf(headers, "Izx")], out Iczx[i]))
                {
                    return;
                }
            }

            if (!double.TryParse(data[0][Array.IndexOf(headers, "gx")], out g[0]) ||
              !double.TryParse(data[0][Array.IndexOf(headers, "gy")], out g[1]) ||
              !double.TryParse(data[0][Array.IndexOf(headers, "gz")], out g[2]))
            {
                return;
            }

            //double tim = Ti; // Directly setting time to 0

            for (int j = 0; j < time.Length; j++)
            {
                double tim = time[j];
                double[] q = new double[dof];
                double[] dq = new double[dof];
                double[] ddq = new double[dof];

                trajectory_try(tim, dof, qin, qf, Tp, out q, out dq, out ddq);
                //Console.WriteLine("Calling invdyn_tree_eff at time t=0");

                for (int i = 0; i < dof; i++)
                {
                    theta[j, i] = q[i];
                    dth[j, i] = dq[i];
                    ddth[j, i] = ddq[i];
                }

                double[] tu = invdyn_tree_eff(q, dq, ddq, n, alp, a, b, th, bt, r, dx, dy, dz, m, g, Icxx, Icyy, Iczz, Icxy, Icyz, Iczx);

                for (int i = 0; i < dof; i++)
                {
                    tor[j, i] = tu[i];
                }

                // Call for_kine function and print the results
                var (B1XValues, B1YValues, B1ZValues) = for_kine(q, dq, n, alp, a, b, th, bt, r, dx, dy, dz);

                for (int i = 0; i < dof + 2; i++)
                {
                    B1X[j, i] = B1XValues[i];
                    B1Y[j, i] = B1YValues[i];
                    B1Z[j, i] = B1ZValues[i];
                }

                // Save the specific row of the tor matrix to a CSV file
                SaveToCSV(time, theta, "theta_matrix.csv", "Position_J");
                SaveToCSV(time, dth, "dth_matrix.csv", "Velocity_J");
                SaveToCSV(time, ddth, "ddth_matrix.csv", "Acceleration_J");
                SaveToCSV(time, tor, "tor_matrix.csv", "Force_J");
                SaveToCSV3D(B1X, "B1X_matrix.csv");
                SaveToCSV3D(B1Y, "B1Y_matrix.csv");
                SaveToCSV3D(B1Z, "B1Z_matrix.csv");
            }
            MessageBox.Show("Data processing and saving completed.");
            LoadData("position", @"theta_matrix.csv");
            LoadData("velocity", @"dth_matrix.csv");
            LoadData("acceleration", @"ddth_matrix.csv");
            LoadData("force", @"tor_matrix.csv");
            Console.ReadLine();
        }
        static void SaveToCSV3D(double[,] data, string filename)
        {
            using (StreamWriter writer = new StreamWriter(filename))
            {
                // Write column headers
                for (int i = 0; i < data.GetLength(1); i++)
                {
                    writer.Write("Joint" + (i + 1) + (i == data.GetLength(1) - 1 ? "\n" : ","));
                }

                // Write data
                for (int i = 0; i < data.GetLength(0); i++)
                {
                    for (int j = 0; j < data.GetLength(1); j++)
                    {
                        writer.Write(data[i, j] + (j == data.GetLength(1) - 1 ? "\n" : ","));
                    }
                }
            }
        }
        public static void SaveToCSV(double[] time, double[,] matrix, string filePath, string prefix)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                int rows = matrix.GetLength(0);
                int cols = matrix.GetLength(1);

                // Write the header
                writer.Write("Time");
                for (int col = 0; col < cols; col++)
                {
                    writer.Write($",{prefix}{col + 1}");
                }
                writer.WriteLine();

                // Write the data
                for (int row = 0; row < rows; row++)
                {
                    writer.Write(time[row]); // Write time value
                    for (int col = 0; col < cols; col++)
                    {
                        writer.Write($",{matrix[row, col]}"); // Write matrix values
                    }
                    writer.WriteLine(); // Add a newline after each row
                }
            }
        }
        static void trajectory_try(double tim, int dof, double[] qin, double[] qf, double Tp, out double[] qi, out double[] dqi, out double[] ddqi)
        {
            qi = new double[dof];
            dqi = new double[dof];
            ddqi = new double[dof];
            for (int i = 0; i < dof; i++)
            {
                qi[i] = qin[i] + ((qf[i] - qin[i]) / Tp) * (tim - (Tp / (2 * Math.PI)) * Math.Sin((2 * Math.PI / Tp) * tim));
                dqi[i] = ((qf[i] - qin[i]) / Tp) * (1 - Math.Cos((2 * Math.PI / Tp) * tim));
                ddqi[i] = (2 * Math.PI * (qf[i] - qin[i]) / (Tp * Tp)) * Math.Sin((2 * Math.PI / Tp) * tim);
            }
        }
        static double[] invdyn_tree_eff(double[] q, double[] dq, double[] ddq, int n, double[] alp, double[] a, double[] b, double[] th, double[] bt, double[] r, double[] dx, double[] dy, double[] dz, double[] m, double[] g, double[] Icxx, double[] Icyy, double[] Iczz, double[] Icxy, double[] Icyz, double[] Iczx)
        {
            //g = -g;
            double[,] tt = new double[3, n];
            double[,] dtt = new double[3, n];
            double[,] dtb = new double[3, n];
            double[] tu = new double[n];
            double[,] twt = new double[3, n];
            double[,] twb = new double[3, n];
            double[,,] tW = new double[3, 3, n];
            double[] cth = new double[n];
            double[] sth = new double[n];
            double[] cal = new double[n];
            double[] sal = new double[n];
            double[] Dxx = new double[n];
            double[] Dyy = new double[n];
            double[] Dzz = new double[n];
            double[] Dxy = new double[n];
            double[] Dyz = new double[n];
            double[] Dzx = new double[n];
            double[] Ixx = new double[n];
            double[] Iyy = new double[n];
            double[] Izz = new double[n];
            double[] Ixy = new double[n];
            double[] Iyz = new double[n];
            double[] Izx = new double[n];
            double[] p = new double[n];

            for (int i = 0; i < n; i++)
            {
                p[i] = 1 - r[i];
                // Console.WriteLine("p[" + i + "] = " + p[i]);

                th[i] = th[i] * p[i] + q[i] * r[i];
                b[i] = b[i] * r[i] + q[i] * p[i];
                if (bt[i] == 0) // When parent of the link is ground link
                {
                    tt[0, i] = 0;
                    tt[1, i] = 0;
                    tt[2, i] = r[i] * dq[i];
                    dtt[0, i] = 0;
                    dtt[1, i] = 0;
                    dtt[2, i] = r[i] * ddq[i];
                    double om3 = tt[2, i]; double dom3 = dtt[2, i]; double om3s = om3 * om3;
                    tW[0, 0, i] = -om3s;
                    tW[0, 1, i] = -dom3;
                    tW[0, 2, i] = 0;
                    tW[1, 0, i] = dom3;
                    tW[1, 1, i] = -om3s;
                    tW[1, 2, i] = 0;
                    tW[2, 0, i] = 0;
                    tW[2, 1, i] = 0;
                    tW[2, 2, i] = 0;
                    cal[i] = Math.Cos(alp[i]);
                    cth[i] = Math.Cos(th[i]);
                    sal[i] = Math.Sin(alp[i]);
                    sth[i] = Math.Sin(th[i]);

                    // Linear acceleration
                    double bx = g[0];
                    double by = g[1] * cal[i] + g[2] * sal[i];
                    double bz = -g[1] * sal[i] + g[2] * cal[i];
                    dtb[0, i] = bx * cth[i] + by * sth[i];
                    dtb[1, i] = -bx * sth[i] + by * cth[i];
                    dtb[2, i] = bz + p[i] * ddq[i];
                }
                else // Calculation for the links other than those attached with ground
                {
                    cal[i] = Math.Cos(alp[i]);
                    cth[i] = Math.Cos(th[i]);
                    sal[i] = Math.Sin(alp[i]);
                    sth[i] = Math.Sin(th[i]);

                    // position vector from origin of link to origin of next link
                    double[] ai = new double[3];
                    ai[0] = a[i];
                    ai[1] = -b[i] * sal[i];
                    ai[2] = b[i] * cal[i];

                    // Angular velocity
                    double bx = tt[0, (int)bt[i - 1]];
                    double by = tt[1, (int)bt[i - 1]] * cal[i] + tt[2, (int)bt[i - 1]] * sal[i];
                    double bz = -tt[1, (int)bt[i - 1]] * sal[i] + tt[2, (int)bt[i - 1]] * cal[i];
                    double[,] ttb = new double[3, 1] { { bx * cth[i] + by * sth[i] }, { -bx * sth[i] + by * cth[i] }, { bz } };
                    tt[0, i] = ttb[0, 0];
                    tt[1, i] = ttb[1, 0];
                    tt[2, i] = ttb[2, 0] + r[i] * dq[i];

                    // Angular acceleration
                    bx = dtt[0, (int)bt[i - 1]];
                    by = dtt[1, (int)bt[i - 1]] * cal[i] + dtt[2, (int)bt[i - 1]] * sal[i];
                    bz = -dtt[1, (int)bt[i - 1]] * sal[i] + dtt[2, (int)bt[i - 1]] * cal[i];
                    double[] dttb = new double[] { bx * cth[i] + by * sth[i], -bx * sth[i] + by * cth[i], bz };
                    dtt[0, i] = dttb[0] + r[i] * tt[1, i] * dq[i];
                    dtt[1, i] = dttb[1] - r[i] * tt[0, i] * dq[i];
                    dtt[2, i] = dttb[2] + r[i] * ddq[i];

                    double om1 = tt[0, i];
                    double dom1 = dtt[0, i];
                    double om1s = om1 * om1;
                    double om2 = tt[1, i];
                    double dom2 = dtt[1, i];
                    double om2s = om2 * om2;
                    double om3 = tt[2, i];
                    double dom3 = dtt[2, i];
                    double om3s = om3 * om3;
                    double om12 = om1 * om2;
                    double om23 = om2 * om3;
                    double om13 = om1 * om3;

                    tW[0, 0, i] = -om3s - om2s;
                    tW[0, 1, i] = -dom3 + om12;
                    tW[0, 2, i] = dom2 + om13;
                    tW[1, 0, i] = dom3 + om12;
                    tW[1, 1, i] = -om3s - om1s;
                    tW[1, 2, i] = -dom1 + om23;
                    tW[2, 0, i] = -dom2 + om13;
                    tW[2, 1, i] = dom1 + om23;
                    tW[2, 2, i] = -om2s - om1s;

                    // Linear velocity
                    double[,] dtbb = new double[1, 3];
                    dtbb[0, 0] = dtb[0, (int)bt[i - 1]] + tW[0, 0, (int)bt[i - 1]] * ai[0] + tW[0, 1, (int)bt[i - 1]] * ai[1] + tW[0, 2, (int)bt[i - 1]] * ai[2];
                    dtbb[0, 1] = dtb[1, (int)bt[i - 1]] + tW[1, 0, (int)bt[i - 1]] * ai[0] + tW[1, 1, (int)bt[i - 1]] * ai[1] + tW[1, 2, (int)bt[i - 1]] * ai[2];
                    dtbb[0, 2] = dtb[2, (int)bt[i - 1]] + tW[2, 0, (int)bt[i - 1]] * ai[0] + tW[2, 1, (int)bt[i - 1]] * ai[1] + tW[2, 2, (int)bt[i - 1]] * ai[2];

                    bx = dtbb[0, 0];
                    by = dtbb[0, 1] * cal[i] + dtbb[0, 2] * sal[i];
                    bz = -dtbb[0, 1] * sal[i] + dtbb[0, 2] * cal[i];
                    dtb[0, i] = bx * cth[i] + by * sth[i] + 2 * p[i] * tt[1, i] * dq[i];
                    dtb[1, i] = -bx * sth[i] + by * cth[i] - 2 * p[i] * tt[0, i] * dq[i];
                    dtb[2, i] = bz + p[i] * ddq[i];
                }
                // Transfer of inertia tensor from Center of mass to link origin
                double dxxs = dx[i] * dx[i];
                double dyys = dy[i] * dy[i];
                double dzzs = dz[i] * dz[i];
                double dxy = dx[i] * dy[i];
                double dyz = dy[i] * dz[i];
                double dzx = dz[i] * dx[i];
                Dxx[i] = -m[i] * (dzzs + dyys);
                Dyy[i] = -m[i] * (dzzs + dxxs);
                Dzz[i] = -m[i] * (dxxs + dyys);
                Dxy[i] = m[i] * dxy;
                Dyz[i] = m[i] * dyz;
                Dzx[i] = m[i] * dzx;
                Ixx[i] = Icxx[i] - Dxx[i];
                Iyy[i] = Icyy[i] - Dyy[i];
                Izz[i] = Iczz[i] - Dzz[i];
                Ixy[i] = Icxy[i] - Dxy[i];
                Iyz[i] = Icyz[i] - Dyz[i];
                Izx[i] = Iczx[i] - Dzx[i];

                double[] mdi = new double[3] { m[i] * dx[i], m[i] * dy[i], m[i] * dz[i] };
                double[] dtbi = new double[] { dtb[0, i], dtb[1, i], dtb[2, i] };
                double[] mdixdtbi = new double[] { mdi[1] * dtbi[2] - dtbi[1] * mdi[2], -(mdi[0] * dtbi[2] - dtbi[0] * mdi[2]), mdi[0] * dtbi[1] - dtbi[0] * mdi[1] };

                double dsum = 0.5 * (Ixx[i] + Iyy[i] + Izz[i]);
                double cI1 = Ixx[i] - dsum;
                double cI2 = Iyy[i] - dsum;
                double cI3 = Izz[i] - dsum;
                double[] ui = new double[3];
                ui[0] = (Izx[i] * tW[1, 0, i]) - (Ixy[i] * tW[2, 0, i]) + (Iyz[i] * (tW[1, 1, i] - tW[2, 2, i])) + (cI3 * tW[1, 2, i]) - (cI2 * tW[2, 1, i]);
                ui[1] = Ixy[i] * tW[2, 1, i] - Iyz[i] * tW[0, 1, i] + Izx[i] * (tW[2, 2, i] - tW[0, 0, i]) + cI1 * tW[2, 0, i] - cI3 * tW[0, 2, i];
                ui[2] = Iyz[i] * tW[0, 2, i] - Izx[i] * tW[1, 2, i] + Ixy[i] * (tW[0, 0, i] - tW[1, 1, i]) + cI2 * tW[0, 1, i] - cI1 * tW[1, 0, i];
                double[] Wit = new double[] { ui[0] + mdixdtbi[0], ui[1] + mdixdtbi[1], ui[2] + mdixdtbi[2] };
                twt[0, i] = Wit[0];
                twt[1, i] = Wit[1];
                twt[2, i] = Wit[2];

                double[,] Wib = new double[1, 3]
                {{
                m[i] * dtb[0, i] + tW[0, 0, i] * mdi[0] + tW[0, 1, i] * mdi[1] + tW[0, 2, i] * mdi[2],
                m[i] * dtb[1, i] + tW[1, 0, i] * mdi[0] + tW[1, 1, i] * mdi[1] + tW[1, 2, i] * mdi[2],
                m[i] * dtb[2, i] + tW[2, 0, i] * mdi[0] + tW[2, 1, i] * mdi[1] + tW[2, 2, i] * mdi[2]
                } };

                twb[0, i] = Wib[0, 0];
                twb[1, i] = Wib[0, 1];
                twb[2, i] = Wib[0, 2];
                //Console.WriteLine(twb);
            }

            // BACKWARD RECURSION_FINDING JOINT TORQUE
            for (int i = n - 1; i >= 0; i--)
            {
                // Calculation of the generalized forces
                tu[i] = r[i] * twt[2, i] + p[i] * twb[2, i];
                if (bt[i] != 0) // When parent of the link is not ground link
                {
                    double[] ai = new double[3] { a[i], -b[i] * sal[i], b[i] * cal[i] };

                    double bx = twb[0, i] * cth[i] - twb[1, i] * sth[i];
                    double by = twb[0, i] * sth[i] + twb[1, i] * cth[i];
                    double bz = twb[2, i];
                    double[] twbi = new double[] { bx, by * cal[i] - bz * sal[i], by * sal[i] + bz * cal[i] };

                    bx = twt[0, i] * cth[i] - twt[1, i] * sth[i];
                    by = twt[0, i] * sth[i] + twt[1, i] * cth[i];
                    bz = twt[2, i];
                    double[] twti = new double[] { bx, by * cal[i] - bz * sal[i], by * sal[i] + bz * cal[i] };

                    double[] aixtwbi = new double[] {
                    ai[1] * twbi[2] - twbi[1] * ai[2],
                    -(ai[0] * twbi[2] - twbi[0] * ai[2]),
                    ai[0] * twbi[1] - twbi[0] * ai[1]
                    };

                    twt[0, (int)bt[i - 1]] += twti[0] + aixtwbi[0];
                    twt[1, (int)bt[i - 1]] += twti[1] + aixtwbi[1];
                    twt[2, (int)bt[i - 1]] += twti[2] + aixtwbi[2];

                    twb[0, (int)bt[i - 1]] += twbi[0];
                    twb[1, (int)bt[i - 1]] += twbi[1];
                    twb[2, (int)bt[i - 1]] += twbi[2];
                }
            }
            return tu;
        }

        public static (double[], double[], double[]) for_kine(double[] q, double[] dq, int n, double[] alp, double[] a, double[] b, double[] th, double[] bt, double[] r, double[] dx, double[] dy, double[] dz)
        {
            // Initialization
            double[] e = { 0, 0, 1 };
            double[,] tt = new double[3, n];
            double[,] tb = new double[3, n];
            double[,] so = new double[3, n];
            double[,] sc = new double[3, n];
            double[,] st = new double[3, n];
            double[,] vc = new double[3, n];
            double[,,] Qf = new double[3, 3, n];
            double[] p = new double[n];

            for (int i = 0; i < n; i++)
            {
                p[i] = 1 - r[i];
                th[i] = th[i] * p[i] + q[i] * r[i];
                b[i] = b[i] * r[i] + q[i] * p[i];
                if (bt[i] == 0)// When parent of the link is ground link
                {
                    double cosTheta = Math.Cos(th[i]);
                    double sinTheta = Math.Sin(th[i]);
                    double cosAlpha = Math.Cos(alp[i]);
                    double sinAlpha = Math.Sin(alp[i]);

                    double[,] Qi = new double[3, 3]
                    {
                        { cosTheta,               -sinTheta,                0             },
                        { cosAlpha * sinTheta,    cosAlpha * cosTheta,     -sinAlpha     },
                        { sinAlpha * sinTheta,    sinAlpha * cosTheta,      cosAlpha     }
                    };

                    for (int row = 0; row < 3; row++)
                    {
                        for (int col = 0; col < 3; col++)
                        {
                            Qf[row, col, i] = Qi[row, col];
                        }
                    }

                    double[] di = { dx[i], dy[i], dz[i] };

                    double[] aim = { a[i], -b[i] * Math.Sin(alp[i]), b[i] * Math.Cos(alp[i]) };
                    // Matrix multiplication and addition for sc
                    for (int row = 0; row < 3; row++)
                    {
                        sc[row, i] = so[row, i];
                        for (int col = 0; col < 3; col++)
                        {
                            sc[row, i] += Qf[row, col, i] * di[col];
                        }
                    }

                    // Matrix multiplication and addition for st
                    for (int row = 0; row < 3; row++)
                    {
                        st[row, i] = so[row, i];
                        for (int col = 0; col < 3; col++)
                        {
                            st[row, i] += Qf[row, col, i] * 2 * di[col];
                        }
                    }

                    // Angular velocity
                    double[] edq = new double[3];
                    for (int row = 0; row < 3; row++)
                    {
                        edq[row] = e[row] * dq[i];
                    }

                    for (int row = 0; row < 3; row++)
                    {
                        tt[row, i] = r[i] * edq[row];
                    }

                    double[] tti = new double[3];
                    for (int row = 0; row < 3; row++)
                    {
                        tti[row] = tt[row, i];
                    }

                    // tb = p * edq
                    for (int row = 0; row < 3; row++)
                    {
                        tb[row, i] = p[i] * edq[row];
                    }

                    // ttixdi = [tti(2)*di(3)-di(2)*tti(3); -(tti(1)*di(3)-di(1)*tti(3)); tti(1)*di(2)-di(1)*tti(2)]
                    double[] ttixdi = {
                        tti[1] * di[2] - di[1] * tti[2],
                        -(tti[0] * di[2] - di[0] * tti[2]),
                        tti[0] * di[1] - di[0] * tti[1]
                    };

                    // vc = tb + ttixdi
                    for (int row = 0; row < 3; row++)
                    {
                        vc[row, i] = tb[row, i] + ttixdi[row];
                    }
                }

                else // Calculation for the links other than those attached with ground
                {
                    double[,] Qi = {
                        { Math.Cos(th[i]), -Math.Sin(th[i]), 0 },
                        { Math.Cos(alp[i]) * Math.Sin(th[i]), Math.Cos(alp[i]) * Math.Cos(th[i]), -Math.Sin(alp[i]) },
                        { Math.Sin(alp[i]) * Math.Sin(th[i]), Math.Sin(alp[i]) * Math.Cos(th[i]), Math.Cos(alp[i]) }
                    };

                    // Qf(:,:,i) = Qf(:,:,bt(i)) * Qi;
                    for (int row = 0; row < 3; row++)
                    {
                        for (int col = 0; col < 3; col++)
                        {
                            Qf[row, col, i] = 0;
                            for (int k = 0; k < 3; k++)
                            {
                                Qf[row, col, i] += Qf[row, k, (int)bt[i - 1]] * Qi[k, col];
                            }
                        }
                    }

                    // Position vector from origin of link to origin of next link
                    double[] aim = { a[i], -b[i] * Math.Sin(alp[i]), b[i] * Math.Cos(alp[i]) };
                    double[] di = { dx[i], dy[i], dz[i] };

                    // Positions
                    for (int row = 0; row < 3; row++)
                    {
                        so[row, i] = 0;
                        for (int col = 0; col < 3; col++)
                        {
                            so[row, i] += Qf[row, col, (int)bt[i - 1]] * aim[col];
                        }
                        so[row, i] += so[row, (int)bt[i - 1]];

                        sc[row, i] = so[row, i];
                        for (int col = 0; col < 3; col++)
                        {
                            sc[row, i] += Qf[row, col, i] * di[col];
                        }

                        st[row, i] = so[row, i];
                        for (int col = 0; col < 3; col++)
                        {
                            st[row, i] += Qf[row, col, i] * 2 * di[col];
                        }
                    }

                    // Angular velocity
                    double[] ttbi = new double[3];
                    for (int row = 0; row < 3; row++)
                    {
                        ttbi[row] = tt[row, (int)bt[i - 1]];
                    }

                    double[] edq = new double[3];
                    for (int row = 0; row < 3; row++)
                    {
                        edq[row] = e[row] * dq[i];
                    }

                    for (int row = 0; row < 3; row++)
                    {
                        tt[row, i] = 0;
                        for (int col = 0; col < 3; col++)
                        {
                            tt[row, i] += Qi[col, row] * ttbi[col];
                        }
                        tt[row, i] += r[i] * edq[row];
                    }

                    double[] tti = new double[3];
                    for (int row = 0; row < 3; row++)
                    {
                        tti[row] = tt[row, i];
                    }

                    // Linear velocity
                    double[] ttbixaim = {
                        ttbi[1] * aim[2] - aim[1] * ttbi[2],
                        -(ttbi[0] * aim[2] - aim[0] * ttbi[2]),
                        ttbi[0] * aim[1] - aim[0] * ttbi[1]
                    };

                    for (int row = 0; row < 3; row++)
                    {
                        tb[row, i] = 0;
                        for (int col = 0; col < 3; col++)
                        {
                            tb[row, i] += Qi[col, row] * (tb[col, (int)bt[i - 1]] + ttbixaim[col]);
                        }
                        tb[row, i] += p[i] * edq[row];
                    }

                    double[] ttixdi = {
                        tti[1] * di[2] - di[1] * tti[2],
                        -(tti[0] * di[2] - di[0] * tti[2]),
                        tti[0] * di[1] - di[0] * tti[1]
                    };

                    for (int row = 0; row < 3; row++)
                    {
                        vc[row, i] = tb[row, i] + ttixdi[row];
                    }
                }
            }
            // Compute B1X, B1Y, B1Z
            double[] B1X = new double[n + 2]; // n + 2 = 4 elements
            double[] B1Y = new double[n + 2]; // n + 2 = 4 elements
            double[] B1Z = new double[n + 2]; // n + 2 = 4 elements

            B1X[0] = 0;
            B1Y[0] = 0;
            B1Z[0] = 0;

            for (int i = 0; i < n; i++)
            {
                B1X[i + 1] = so[0, i];
                B1Y[i + 1] = so[1, i];
                B1Z[i + 1] = so[2, i];
            }

            B1X[n + 1] = st[0, n - 1];
            B1Y[n + 1] = st[1, n - 1];
            B1Z[n + 1] = st[2, n - 1];

            return (B1X, B1Y, B1Z);
            // return (so, sc, vc, tt, st);
        }
        public class DataRecord
        {
            public double Time { get; set; }
            public List<double> Values { get; set; }
        }
        private void LoadData(string dataType, string filePath)
        {   

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true }))
            {
                var records = new List<DataRecord>();
                csv.Read();
                csv.ReadHeader();
                var headerRow = csv.Context.Reader.HeaderRecord.ToList();

                while (csv.Read())
                {
                    var record = new DataRecord
                    {
                        Time = csv.GetField<double>(0),
                        Values = new List<double>()
                    };

                    for (int i = 1; i < headerRow.Count; i++)
                    {
                        record.Values.Add(csv.GetField<double>(i));
                    }

                    records.Add(record);
                }

                for (int i = 0; i < headerRow.Count - 1; i++)
                {
                    string key = $"j{i + 1}_{dataType}";
                    dataDictionary[key] = records.Select(r => new DataRecord { Time = r.Time, Values = new List<double> { r.Values[i] } }).ToList();

                    var series = new LineSeries
                    {
                        Title = $"J{i + 1} {dataType}",
                        StrokeThickness = 3
                    };
                    seriesDictionary[key] = series;
                }

                foreach (var kvp in dataDictionary)
                {
                    Console.WriteLine($"Key: {kvp.Key}, Count: {kvp.Value.Count}");
                }
            }
        }
        private void InitializePlot()
        {
            plotModel = new PlotModel {  };

            // Create X Axis and set its title
            var xAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Time"
            };
            plotModel.Axes.Add(xAxis);

            // Create Y Axis and set its title
            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Values"
            };
            plotModel.Axes.Add(yAxis);

            // Set the legend properties
            var legend = new Legend
            {
                //LegendTitle = "Variables",
                LegendPosition = LegendPosition.TopCenter,
                LegendPlacement = LegendPlacement.Outside,
                LegendOrientation = LegendOrientation.Horizontal,
                LegendBackground = OxyColors.White,
                LegendBorder = OxyColors.Black
            };

            plotModel.Legends.Add(legend);

            plotView.Model = plotModel;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UpdatePlot();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdatePlot();
        }

        private void UpdatePlot()
        {
            plotModel.Series.Clear();

            PlotJointData("j1", "position", j1_position.IsChecked);
            PlotJointData("j1", "velocity", j1_velocity.IsChecked);
            PlotJointData("j1", "acceleration", j1_acceleration.IsChecked);
            PlotJointData("j1", "force", j1_force.IsChecked);

            PlotJointData("j2", "position", j2_position.IsChecked);
            PlotJointData("j2", "velocity", j2_velocity.IsChecked);
            PlotJointData("j2", "acceleration", j2_acceleration.IsChecked);
            PlotJointData("j2", "force", j2_force.IsChecked);

            PlotJointData("j3", "position", j3_position.IsChecked);
            PlotJointData("j3", "velocity", j3_velocity.IsChecked);
            PlotJointData("j3", "acceleration", j3_acceleration.IsChecked);
            PlotJointData("j3", "force", j3_force.IsChecked);

            PlotJointData("j4", "position", j4_position.IsChecked);
            PlotJointData("j4", "velocity", j4_velocity.IsChecked);
            PlotJointData("j4", "acceleration", j4_acceleration.IsChecked);
            PlotJointData("j4", "force", j4_force.IsChecked);

            PlotJointData("j5", "position", j5_position.IsChecked);
            PlotJointData("j5", "velocity", j5_velocity.IsChecked);
            PlotJointData("j5", "acceleration", j5_acceleration.IsChecked);
            PlotJointData("j5", "force", j5_force.IsChecked);

            PlotJointData("j6", "position", j6_position.IsChecked);
            PlotJointData("j6", "velocity", j6_velocity.IsChecked);
            PlotJointData("j6", "acceleration", j6_acceleration.IsChecked);
            PlotJointData("j6", "force", j6_force.IsChecked);

            plotModel.InvalidatePlot(true); // Refresh the plot
        }
        private void PlotJointData(string joint, string dataType, bool? isChecked)
        {
            if (isChecked == true)
            {
                string key = $"{joint}_{dataType}";
                var series = seriesDictionary[key];
                series.Points.Clear();

                foreach (var record in dataDictionary[key])
                {
                    series.Points.Add(new DataPoint(record.Time, record.Values[0]));
                }

                // Capitalize the first letter of the joint identifier
                string capitalizedJoint = char.ToUpper(joint[0]) + joint.Substring(1);

                // Customize the series title for force variables
                if (dataType == "force")
                {
                    series.Title = $"{capitalizedJoint} force/torque";
                }
                else
                {
                    series.Title = $"{capitalizedJoint} {dataType}";
                }

                plotModel.Series.Add(series);
            }
        }
        private OxyColor GetColorForJointData(string joint, string dataType)
        {
            // Define a color for each joint and data type combination
            switch (joint)
            {
                case "j1":
                    switch (dataType)
                    {
                        case "position": return OxyColors.Red;
                        case "velocity": return OxyColors.Green;
                        case "acceleration": return OxyColors.Blue;
                        case "force": return OxyColors.Orange;
                    }
                    break;
                case "j2":
                    switch (dataType)
                    {
                        case "position": return OxyColors.Magenta;
                        case "velocity": return OxyColors.Yellow;
                        case "acceleration": return OxyColors.Cyan;
                        case "force": return OxyColors.Brown;
                    }
                    break;
                case "j3":
                    switch (dataType)
                    {
                        case "position": return OxyColors.Purple;
                        case "velocity": return OxyColors.Lime;
                        case "acceleration": return OxyColors.SkyBlue;
                        case "force": return OxyColors.Pink;
                    }
                    break;
                case "j4":
                    switch (dataType)
                    {
                        case "position": return OxyColors.Indigo;
                        case "velocity": return OxyColors.DarkOrange;
                        case "acceleration": return OxyColors.DarkBlue;
                        case "force": return OxyColors.DarkRed;
                    }
                    break;
                case "j5":
                    switch (dataType)
                    {
                        case "position": return OxyColors.DarkGreen;
                        case "velocity": return OxyColors.DarkMagenta;
                        case "acceleration": return OxyColors.DarkCyan;
                        case "force": return OxyColors.DarkGoldenrod;
                    }
                    break;
                case "j6":
                    switch (dataType)
                    {
                        case "position": return OxyColors.LightGreen;
                        case "velocity": return OxyColors.LightBlue;
                        case "acceleration": return OxyColors.LightCoral;
                        case "force": return OxyColors.LightPink;
                    }
                    break;
            }

            // Fallback color
            return OxyColors.Black;
        }
        private void PlotView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ExportDataToCSV();
        }

        // Method to export data to CSV
        private void ExportDataToCSV()
        {
            // Retrieve the data from your plot model
            var plotModel = plotView.Model;
            if (plotModel == null) return;

            var seriesList = plotModel.Series;
            if (seriesList.Count == 0) return;

            // StringBuilder to hold CSV data
            var csvData = new StringBuilder();

            // Iterate through each series and add its data to the CSV
            foreach (var series in seriesList)
            {
                if (series is OxyPlot.Series.LineSeries lineSeries)
                {
                    // Add series name as a header
                    csvData.AppendLine(lineSeries.Title);
                    csvData.AppendLine("X,Y");

                    // Add data points
                    foreach (var point in lineSeries.Points)
                    {
                        csvData.AppendLine($"{point.X},{point.Y}");
                    }

                    // Add an empty line for better readability between series
                    csvData.AppendLine();
                }
            }

            // Prompt user to save the CSV file
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "CSV file (*.csv)|*.csv",
                FileName = "plot_data.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, csvData.ToString());
                MessageBox.Show("Data exported successfully!", "Export to CSV", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
    public class KinematicData
    {
        public string JointNo { get; set; }
        public string JointType { get; set; }
        public string JointOffset { get; set; }
        public string JointAngle { get; set; }
        public string LinkLength { get; set; }
        public string TwistAngle { get; set; }
        public string JVInitial { get; set; }
        public string JVFinal { get; set; }
    }
    public class DynamicData
    {
        public string Mass { get; set; }
        public string LinkLength1 { get; set; }
        public string ActuatedJoint { get; set; }
        public string bt { get; set; }
        public string dx { get; set; }
        public string dy { get; set; }
        public string dz { get; set; }
        public string Ixx { get; set; }
        public string Iyy { get; set; }
        public string Izz { get; set; }
        public string Ixy { get; set; }
        public string Iyz { get; set; }
        public string Izx { get; set; }
    }
    public class GravityData
    {
        public double gx { get; set; }
        public double gy { get; set; }
        public double gz { get; set; }
    }
    public class ForceData
    {
        public double Force { get; set; }
    }
    public class CombinedData
    {
        // Kinematic Data properties
        public string JointNo { get; set; }
        public string JointType { get; set; }
        public string JointOffset { get; set; }
        public string JointAngle { get; set; }
        public string LinkLength { get; set; }
        public string TwistAngle { get; set; }
        public string JVInitial { get; set; }
        public string JVFinal { get; set; }

        // Dynamic Data properties
        public string Mass { get; set; }
        public string LinkLength1 { get; set; }
        public string ActuatedJoint { get; set; }
        public string bt { get; set; }
        public string dx { get; set; }
        public string dy { get; set; }
        public string dz { get; set; }
        public string Ixx { get; set; }
        public string Iyy { get; set; }
        public string Izz { get; set; }
        public string Ixy { get; set; }
        public string Iyz { get; set; }
        public string Izx { get; set; }

        //Analysis Data Properties
        public string Time { get; set; }
        public string Step { get; set; }
        public string gx { get; set; }
        public string gy { get; set; }
        public string gz { get; set; }
        public string Force { get; set; }
    }
}