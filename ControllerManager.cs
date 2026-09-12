using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SharpDX.XInput;

namespace XboxControllerRemapper
{
    public class ControllerManager
    {
        private Controller[] controllers;
        private CancellationTokenSource cancellationTokenSource;
        private bool isListening = false;

        public ControllerManager()
        {
            controllers = new Controller[4];
            for (int i = 0; i < 4; i++)
            {
                controllers[i] = new Controller((UserIndex)i);
            }
        }

        public List<string> GetAvailableControllers()
        {
            List<string> availableControllers = new List<string>();
            
            for (int i = 0; i < 4; i++)
            {
                if (controllers[i].IsConnected)
                {
                    availableControllers.Add($"Controller {i + 1}");
                }
            }
            
            return availableControllers;
        }

        public void StartListening(string selectedController, Dictionary<string, VirtualKeyCode> buttonMapping, KeyboardSimulator keyboardSimulator, Action<string> statusCallback)
        {
            if (isListening)
                return;

            int controllerIndex = int.Parse(selectedController.Split(' ')[1]) - 1;
            Controller selectedCtrl = controllers[controllerIndex];

            if (!selectedCtrl.IsConnected)
            {
                statusCallback("Selected controller is not connected.");
                return;
            }

            isListening = true;
            cancellationTokenSource = new CancellationTokenSource();

            Task.Run(() =>
            {
                GamepadButtonState previousState = new GamepadButtonState();

                while (isListening && !cancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        if (!selectedCtrl.IsConnected)
                        {
                            statusCallback("Controller disconnected.");
                            isListening = false;
                            break;
                        }

                        State state = selectedCtrl.GetState();

                        if (state.Gamepad.Buttons != previousState.Buttons)
                        {
                            CheckAndProcessButton(state.Gamepad, previousState, buttonMapping, keyboardSimulator, statusCallback);
                            previousState = state.Gamepad;
                        }

                        Thread.Sleep(10);
                    }
                    catch (Exception ex)
                    {
                        statusCallback($"Error: {ex.Message}");
                    }
                }

                isListening = false;
            }, cancellationTokenSource.Token);
        }

        public void StopListening()
        {
            isListening = false;
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
        }

        private void CheckAndProcessButton(Gamepad gamepad, GamepadButtonState previousState, Dictionary<string, VirtualKeyCode> buttonMapping, KeyboardSimulator keyboardSimulator, Action<string> statusCallback)
        {
            var buttons = new Dictionary<string, GamepadButtonFlags>
            {
                { "A Button", GamepadButtonFlags.A },
                { "B Button", GamepadButtonFlags.B },
                { "X Button", GamepadButtonFlags.X },
                { "Y Button", GamepadButtonFlags.Y },
                { "LB (Left Bumper)", GamepadButtonFlags.LeftShoulder },
                { "RB (Right Bumper)", GamepadButtonFlags.RightShoulder },
                { "Start Button", GamepadButtonFlags.Start },
                { "Back Button", GamepadButtonFlags.Back },
                { "Left Stick Click", GamepadButtonFlags.LeftThumb },
                { "Right Stick Click", GamepadButtonFlags.RightThumb }
            };

            foreach (var mapping in buttonMapping)
            {
                if (buttons.ContainsKey(mapping.Key))
                {
                    GamepadButtonFlags flag = buttons[mapping.Key];
                    
                    bool isCurrentlyPressed = (gamepad.Buttons & flag) == flag;
                    bool wasPreviouslyPressed = (previousState.Buttons & flag) == flag;
                    
                    if (isCurrentlyPressed && !wasPreviouslyPressed)
                    {
                        // Button pressed
                        keyboardSimulator.PressKey(mapping.Value);
                        statusCallback($"{mapping.Key} pressed → Sending {mapping.Value}");
                    }
                    else if (!isCurrentlyPressed && wasPreviouslyPressed)
                    {
                        // Button released
                        keyboardSimulator.ReleaseKey(mapping.Value);
                        statusCallback($"{mapping.Key} released");
                    }
                }
            }
        }
    }

    public struct GamepadButtonState
    {
        public GamepadButtonFlags Buttons { get; set; }
    }
}
