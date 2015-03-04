using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RunAndGun
{
    public class InputState
    {
        public bool DirectionUp { get; set; }
        public bool DirectionDown { get; set; }
        public bool DirectionLeft { get; set; }
        public bool DirectionRight { get; set; }
        public float Movement { get; set; }

        public bool WeaponButtonPressed { get; set; }
        public bool JumpButtonPressed { get; set; }
        public bool StartButtonPressed { get; set; }
        public void CopyFrom(InputState inputState)
        {
            this.DirectionUp = inputState.DirectionUp;
            this.DirectionDown = inputState.DirectionDown;
            this.DirectionLeft = inputState.DirectionLeft;
            this.DirectionRight = inputState.DirectionRight;

            this.WeaponButtonPressed = inputState.WeaponButtonPressed;
            this.JumpButtonPressed = inputState.JumpButtonPressed;
            this.StartButtonPressed = inputState.StartButtonPressed;
        }
        public void Reset()
        {
            DirectionUp = false;
            DirectionDown = false;
            DirectionLeft = false;
            DirectionRight = false;
            Movement = 0.0f;

            WeaponButtonPressed = false;
            JumpButtonPressed = false;
            StartButtonPressed = false;

        }
    }
}
