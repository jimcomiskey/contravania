//using Microsoft.DirectX.DirectInput;
using Microsoft.Xna.Framework.Input;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        //private static ButtonState[] bs = new ButtonState[12]; //8 Buttons + 4 DPads
        //private static Vector2[] ts = new Vector2[3];      //3 Thumbsticks with x/y
        static void Main(string[] args)
        {

            // Initialize DirectInput
            var directInput = new DirectInput();

            // Find a Joystick Guid
            var joystickGuid = Guid.Empty;

            foreach (var deviceInstance in directInput.GetDevices(DeviceType.Gamepad,
                        DeviceEnumerationFlags.AllDevices))
                joystickGuid = deviceInstance.InstanceGuid;

            // If Gamepad not found, look for a Joystick
            if (joystickGuid == Guid.Empty)
            {

                foreach (var deviceInstance in directInput.GetDevices(DeviceType.Joystick,
                        DeviceEnumerationFlags.AllDevices))
                {
                    Console.WriteLine("{0} Instance Name: {1} ProductName: {2} SubType: {3} Type: {4} Usage: {5} UsagePage: {6}",
                        deviceInstance.InstanceGuid, deviceInstance.InstanceName, deviceInstance.ProductName, deviceInstance.Subtype, deviceInstance.Type, deviceInstance.Usage, deviceInstance.UsagePage);
                    Console.WriteLine(new string('-', 60));
                    joystickGuid = deviceInstance.InstanceGuid;
                    break;
                }
            }

            // If Joystick not found, throws an error
            if (joystickGuid == Guid.Empty)
            {
                Console.WriteLine("No joystick/Gamepad found.");
                Console.ReadKey();
                Environment.Exit(1);
            }

            // Instantiate the joystick
            var joystick = new Joystick(directInput, joystickGuid);

            Console.WriteLine("Found Joystick/Gamepad with GUID: {0}", joystickGuid);

            // Query all suported ForceFeedback effects
            var allEffects = joystick.GetEffects();
            foreach (var effectInfo in allEffects)
                Console.WriteLine("Effect available {0}", effectInfo.Name);

            // Set BufferSize in order to use buffered data.
            joystick.Properties.BufferSize = 128;

            // Acquire the joystick
            joystick.Acquire();

            // Poll events from joystick
            while (true)
            {
                joystick.Poll();
                var datas = joystick.GetBufferedData();
                foreach (var state in datas)
                    Console.WriteLine(state);
                
            }
        }
    }
}
