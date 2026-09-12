using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using SharpDX.XInput;

namespace XboxControllerRemapper
{
    public partial class MainWindow : Window
    {
        private ControllerManager controllerManager;
        private KeyboardSimulator keyboardSimulator;
        private Dictionary<string, VirtualKeyCode> buttonMapping;
        private bool isListening = false;

        public MainWindow()
        {
            InitializeComponent();
            controllerManager = new ControllerManager();
            keyboardSimulator = new KeyboardSimulator();
            buttonMapping = new Dictionary<string, VirtualKeyCode>();
            
            InitializeButtonComboBox();
            InitializeKeyComboBox();
            RefreshControllers();
        }

        private void InitializeButtonComboBox()
        {
            ControllerButtonComboBox.Items.Add("A Button");
            ControllerButtonComboBox.Items.Add("B Button");
            ControllerButtonComboBox.Items.Add("X Button");
            ControllerButtonComboBox.Items.Add("Y Button");
            ControllerButtonComboBox.Items.Add("LB (Left Bumper)");
            ControllerButtonComboBox.Items.Add("RB (Right Bumper)");
            ControllerButtonComboBox.Items.Add("Start Button");
            ControllerButtonComboBox.Items.Add("Back Button");
            ControllerButtonComboBox.Items.Add("Left Stick Click");
            ControllerButtonComboBox.Items.Add("Right Stick Click");
            ControllerButtonComboBox.SelectedIndex = 6; // Default to Start button
        }

        private void InitializeKeyComboBox()
        {
            var keys = new Dictionary<string, VirtualKeyCode>
            {
                { "A", VirtualKeyCode.A },
                { "B", VirtualKeyCode.B },
                { "C", VirtualKeyCode.C },
                { "D", VirtualKeyCode.D },
                { "E", VirtualKeyCode.E },
                { "F", VirtualKeyCode.F },
                { "G", VirtualKeyCode.G },
                { "H", VirtualKeyCode.H },
                { "I", VirtualKeyCode.I },
                { "J", VirtualKeyCode.J },
                { "K", VirtualKeyCode.K },
                { "L", VirtualKeyCode.L },
                { "M", VirtualKeyCode.M },
                { "N", VirtualKeyCode.N },
                { "O", VirtualKeyCode.O },
                { "P", VirtualKeyCode.P },
                { "Q", VirtualKeyCode.Q },
                { "R", VirtualKeyCode.R },
                { "S", VirtualKeyCode.S },
                { "T", VirtualKeyCode.T },
                { "U", VirtualKeyCode.U },
                { "V", VirtualKeyCode.V },
                { "W", VirtualKeyCode.W },
                { "X", VirtualKeyCode.X },
                { "Y", VirtualKeyCode.Y },
                { "Z", VirtualKeyCode.Z },
                { "0", VirtualKeyCode.NumPad0 },
                { "1", VirtualKeyCode.NumPad1 },
                { "2", VirtualKeyCode.NumPad2 },
                { "3", VirtualKeyCode.NumPad3 },
                { "4", VirtualKeyCode.NumPad4 },
                { "5", VirtualKeyCode.NumPad5 },
                { "6", VirtualKeyCode.NumPad6 },
                { "7", VirtualKeyCode.NumPad7 },
                { "8", VirtualKeyCode.NumPad8 },
                { "9", VirtualKeyCode.NumPad9 },
                { "Space", VirtualKeyCode.Space },
                { "Enter", VirtualKeyCode.Return },
                { "Tab", VirtualKeyCode.Tab },
                { "Escape", VirtualKeyCode.Escape },
                { "Backspace", VirtualKeyCode.Back },
                { "Delete", VirtualKeyCode.Delete },
                { "Shift", VirtualKeyCode.LShift },
                { "Ctrl", VirtualKeyCode.LControl },
                { "Alt", VirtualKeyCode.LAlt },
                { "F1", VirtualKeyCode.F1 },
                { "F2", VirtualKeyCode.F2 },
                { "F3", VirtualKeyCode.F3 },
                { "F4", VirtualKeyCode.F4 },
                { "F5", VirtualKeyCode.F5 },
                { "F6", VirtualKeyCode.F6 },
                { "F7", VirtualKeyCode.F7 },
                { "F8", VirtualKeyCode.F8 },
                { "F9", VirtualKeyCode.F9 },
                { "F10", VirtualKeyCode.F10 },
                { "F11", VirtualKeyCode.F11 },
                { "F12", VirtualKeyCode.F12 },
                { "Left Arrow", VirtualKeyCode.Left },
                { "Right Arrow", VirtualKeyCode.Right },
                { "Up Arrow", VirtualKeyCode.Up },
                { "Down Arrow", VirtualKeyCode.Down },
                { "Insert", VirtualKeyCode.Insert },
                { "Home", VirtualKeyCode.Home },
                { "End", VirtualKeyCode.End },
                { "Page Up", VirtualKeyCode.Prior },
                { "Page Down", VirtualKeyCode.Next },
                { "+", VirtualKeyCode.Add },
                { "-", VirtualKeyCode.Subtract },
                { "*", VirtualKeyCode.Multiply },
                { "/", VirtualKeyCode.Divide },
                { ";", VirtualKeyCode.Oem1 },
                { "'", VirtualKeyCode.Oem7 },
                { ",", VirtualKeyCode.Oemcomma },
                { ".", VirtualKeyCode.OemPeriod },
                { "/", VirtualKeyCode.Oem2 },
                { "[", VirtualKeyCode.Oem4 },
                { "]", VirtualKeyCode.Oem6 },
                { "\\", VirtualKeyCode.Oem5 }
            };

            foreach (var key in keys)
            {
                KeyComboBox.Items.Add(key.Key);
            }
            
            // Set default to G key
            KeyComboBox.SelectedItem = "G";
        }

        private void RefreshControllers()
        {
            ControllerComboBox.Items.Clear();
            var controllers = controllerManager.GetAvailableControllers();
            
            if (controllers.Count == 0)
            {
                ControllerComboBox.Items.Add("No controllers found");
                ControllerComboBox.SelectedIndex = 0;
                StatusText.Text = "Status: No Xbox controllers detected. Please connect a controller.";
            }
            else
            {
                foreach (var controller in controllers)
                {
                    ControllerComboBox.Items.Add(controller);
                }
                ControllerComboBox.SelectedIndex = 0;
                StatusText.Text = "Status: Controller(s) detected. Select one to continue.";
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshControllers();
            StatusText.Text = "Status: Device list refreshed.";
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (ControllerComboBox.SelectedIndex == -1 || ControllerComboBox.Items.Count == 0 || 
                ControllerComboBox.SelectedItem.ToString() == "No controllers found")
            {
                MessageBox.Show("Please select a valid controller.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (ControllerButtonComboBox.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a controller button.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (KeyComboBox.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a keyboard key.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string buttonName = ControllerButtonComboBox.SelectedItem.ToString();
            string keyName = KeyComboBox.SelectedItem.ToString();
            VirtualKeyCode keyCode = GetVirtualKeyCode(keyName);

            buttonMapping.Clear();
            buttonMapping[buttonName] = keyCode;

            MappingStatusText.Text = $"Current Mapping: {buttonName} → {keyName}";
            StatusText.Text = $"Status: Mapping configured. Click 'Start Listening' to begin.";
        }

        private VirtualKeyCode GetVirtualKeyCode(string keyName)
        {
            var keyMap = new Dictionary<string, VirtualKeyCode>
            {
                { "A", VirtualKeyCode.A },
                { "B", VirtualKeyCode.B },
                { "C", VirtualKeyCode.C },
                { "D", VirtualKeyCode.D },
                { "E", VirtualKeyCode.E },
                { "F", VirtualKeyCode.F },
                { "G", VirtualKeyCode.G },
                { "H", VirtualKeyCode.H },
                { "I", VirtualKeyCode.I },
                { "J", VirtualKeyCode.J },
                { "K", VirtualKeyCode.K },
                { "L", VirtualKeyCode.L },
                { "M", VirtualKeyCode.M },
                { "N", VirtualKeyCode.N },
                { "O", VirtualKeyCode.O },
                { "P", VirtualKeyCode.P },
                { "Q", VirtualKeyCode.Q },
                { "R", VirtualKeyCode.R },
                { "S", VirtualKeyCode.S },
                { "T", VirtualKeyCode.T },
                { "U", VirtualKeyCode.U },
                { "V", VirtualKeyCode.V },
                { "W", VirtualKeyCode.W },
                { "X", VirtualKeyCode.X },
                { "Y", VirtualKeyCode.Y },
                { "Z", VirtualKeyCode.Z },
                { "0", VirtualKeyCode.NumPad0 },
                { "1", VirtualKeyCode.NumPad1 },
                { "2", VirtualKeyCode.NumPad2 },
                { "3", VirtualKeyCode.NumPad3 },
                { "4", VirtualKeyCode.NumPad4 },
                { "5", VirtualKeyCode.NumPad5 },
                { "6", VirtualKeyCode.NumPad6 },
                { "7", VirtualKeyCode.NumPad7 },
                { "8", VirtualKeyCode.NumPad8 },
                { "9", VirtualKeyCode.NumPad9 },
                { "Space", VirtualKeyCode.Space },
                { "Enter", VirtualKeyCode.Return },
                { "Tab", VirtualKeyCode.Tab },
                { "Escape", VirtualKeyCode.Escape },
                { "Backspace", VirtualKeyCode.Back },
                { "Delete", VirtualKeyCode.Delete },
                { "Shift", VirtualKeyCode.LShift },
                { "Ctrl", VirtualKeyCode.LControl },
                { "Alt", VirtualKeyCode.LAlt },
                { "F1", VirtualKeyCode.F1 },
                { "F2", VirtualKeyCode.F2 },
                { "F3", VirtualKeyCode.F3 },
                { "F4", VirtualKeyCode.F4 },
                { "F5", VirtualKeyCode.F5 },
                { "F6", VirtualKeyCode.F6 },
                { "F7", VirtualKeyCode.F7 },
                { "F8", VirtualKeyCode.F8 },
                { "F9", VirtualKeyCode.F9 },
                { "F10", VirtualKeyCode.F10 },
                { "F11", VirtualKeyCode.F11 },
                { "F12", VirtualKeyCode.F12 },
                { "Left Arrow", VirtualKeyCode.Left },
                { "Right Arrow", VirtualKeyCode.Right },
                { "Up Arrow", VirtualKeyCode.Up },
                { "Down Arrow", VirtualKeyCode.Down },
                { "Insert", VirtualKeyCode.Insert },
                { "Home", VirtualKeyCode.Home },
                { "End", VirtualKeyCode.End },
                { "Page Up", VirtualKeyCode.Prior },
                { "Page Down", VirtualKeyCode.Next },
                { "+", VirtualKeyCode.Add },
                { "-", VirtualKeyCode.Subtract },
                { "*", VirtualKeyCode.Multiply },
                { "/", VirtualKeyCode.Divide },
                { ";", VirtualKeyCode.Oem1 },
                { "'", VirtualKeyCode.Oem7 },
                { ",", VirtualKeyCode.Oemcomma },
                { ".", VirtualKeyCode.OemPeriod },
                { "[", VirtualKeyCode.Oem4 },
                { "]", VirtualKeyCode.Oem6 },
                { "\\", VirtualKeyCode.Oem5 }
            };

            return keyMap.ContainsKey(keyName) ? keyMap[keyName] : VirtualKeyCode.G;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (buttonMapping.Count == 0)
            {
                MessageBox.Show("Please configure a mapping first by clicking 'Apply Mapping'.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (ControllerComboBox.SelectedIndex == -1 || ControllerComboBox.Items.Count == 0 || 
                ControllerComboBox.SelectedItem.ToString() == "No controllers found")
            {
                MessageBox.Show("Please select a valid controller.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            isListening = true;
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            ApplyButton.IsEnabled = false;
            ControllerButtonComboBox.IsEnabled = false;
            KeyComboBox.IsEnabled = false;
            ControllerComboBox.IsEnabled = false;
            
            StatusText.Text = "Status: Listening for controller input...";
            
            string selectedController = ControllerComboBox.SelectedItem.ToString();
            controllerManager.StartListening(selectedController, buttonMapping, keyboardSimulator, UpdateStatus);
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            isListening = false;
            controllerManager.StopListening();
            
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
            ApplyButton.IsEnabled = true;
            ControllerButtonComboBox.IsEnabled = true;
            KeyComboBox.IsEnabled = true;
            ControllerComboBox.IsEnabled = true;
            
            StatusText.Text = "Status: Listening stopped.";
        }

        private void UpdateStatus(string message)
        {
            Dispatcher.Invoke(() =>
            {
                StatusText.Text = $"Status: {message}";
            });
        }
    }
}
