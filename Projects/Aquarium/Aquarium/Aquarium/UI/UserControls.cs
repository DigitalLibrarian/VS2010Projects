using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Forever.Screens;

namespace Aquarium.UI
{
    // TODO - i wanna be able to support x3 controls

    public enum UserControlKeys
    {
        MoveFoward,
        MoveBackward,
        MoveLeft,
        MoveRight,
        MoveUp,
        MoveDown,
        TurnLeft,
        TurnRight,
        LookUp,
        LookDown,
        RollRight,
        RollLeft,

        Shifter,
        DoubleShifter,

        PrimaryFire,
        SecondaryFire
    };

    public class UserControls
    {
        // Computed deltas 
        Vector3 force = Vector3.Zero;
        public Vector3 LocalForce { get { return force; } }
        Vector3 torque;

        public Vector3 LocalTorque { get { return torque; } }

        public float ForceMag { get; set; }
        public float TorqueMag { get; set; }

        public float ForceShiftMag { get; set; }
        public float TorqueShiftMag { get; set; }

        public bool PrimaryFire { get; set; }
        public bool SecondaryFire { get; set; }

        public bool ControlSchemeToggle { get; set; }

        public PlayerIndex playerIndex;
        public Dictionary<Keys, UserControlKeys> KeyMappings = new Dictionary<Keys, UserControlKeys>();

        public UserControls(PlayerIndex playerIndex, float forceMag, float torqueMag, float forceShiftMag, float torqueShiftMag)
        {
            this.ForceMag = forceMag;
            this.TorqueMag = torqueMag;
            this.ForceShiftMag = forceShiftMag;
            this.TorqueShiftMag = torqueShiftMag;
            this.playerIndex = playerIndex;

            //default mappings
            KeyMappings[Keys.W] = UserControlKeys.MoveFoward;
            KeyMappings[Keys.S] = UserControlKeys.MoveBackward;
            KeyMappings[Keys.A] = UserControlKeys.MoveLeft;
            KeyMappings[Keys.D] = UserControlKeys.MoveRight;
            KeyMappings[Keys.Q] = UserControlKeys.RollLeft;
            KeyMappings[Keys.Left] = UserControlKeys.TurnLeft;
            KeyMappings[Keys.E] = UserControlKeys.RollRight;
            KeyMappings[Keys.Right] = UserControlKeys.TurnRight;
            KeyMappings[Keys.X] = UserControlKeys.MoveUp;
            KeyMappings[Keys.Z] = UserControlKeys.MoveDown;

            KeyMappings[Keys.Down] = UserControlKeys.LookUp;
            KeyMappings[Keys.Up] = UserControlKeys.LookDown;

            KeyMappings[Keys.T] = UserControlKeys.TurnRight;
            KeyMappings[Keys.G] = UserControlKeys.TurnLeft;

            KeyMappings[Keys.LeftShift] = UserControlKeys.Shifter;
            KeyMappings[Keys.RightShift] = UserControlKeys.DoubleShifter;

            KeyMappings[Keys.LeftAlt] = UserControlKeys.PrimaryFire;
            KeyMappings[Keys.RightAlt] = UserControlKeys.SecondaryFire;

        }


        public void HandleInput(InputState inputState)
        {
            Vector3 newTrans = Vector3.Zero;
            Vector3 newRot = Vector3.Zero;
            bool shift = false;
            bool doubleShift = false;
            bool primaryFire = false;
            bool secondaryFire = false;
            foreach (KeyValuePair<Keys, UserControlKeys> pair in KeyMappings)
            {
                Keys key = pair.Key;
                if (inputState.IsKeyDown(key, playerIndex))
                {
                    UserControlKeys flyKey = pair.Value;
                    switch (flyKey)
                    {
                        case UserControlKeys.MoveFoward:
                            newTrans += Vector3.Forward;
                            break;
                        case UserControlKeys.MoveBackward:
                            newTrans += Vector3.Backward;
                            break;
                        case UserControlKeys.MoveRight:
                            newTrans += Vector3.Right;
                            break;
                        case UserControlKeys.MoveLeft:
                            newTrans += Vector3.Left;
                            break;
                        case UserControlKeys.MoveUp:
                            newTrans += Vector3.Up;
                            break;
                        case UserControlKeys.MoveDown:
                            newTrans += Vector3.Down;
                            break;
                        case UserControlKeys.TurnLeft:
                            newRot += Vector3.Up;
                            break;
                        case UserControlKeys.TurnRight:
                            newRot += Vector3.Down;
                            break;
                        case UserControlKeys.LookUp:
                            newRot += Vector3.Right;
                            break;
                        case UserControlKeys.LookDown:
                            newRot += Vector3.Left;
                            break;
                        case UserControlKeys.RollRight:
                            newRot += Vector3.Forward;
                            break;
                        case UserControlKeys.RollLeft:
                            newRot += Vector3.Backward;
                            break;
                        case UserControlKeys.Shifter:
                            shift = true;
                            break;
                        case UserControlKeys.DoubleShifter:
                            doubleShift = true;
                            break;
                        case UserControlKeys.PrimaryFire:
                            primaryFire = true;
                            break;
                        case UserControlKeys.SecondaryFire:
                            secondaryFire = true;
                            break;
                    }

                   

                }

            }

            var gamePad = inputState.CurrentGamePadStates[(int)playerIndex];
            var leftStick = gamePad.ThumbSticks.Left;
            var rightStick = gamePad.ThumbSticks.Right;

            newTrans += new Vector3(leftStick.X, 0, -leftStick.Y);
            newRot += new Vector3(rightStick.Y, -rightStick.X, 0f);

            if (gamePad.IsButtonDown(Buttons.LeftShoulder))
            {
                newRot += Vector3.Backward;
            }

            if (gamePad.IsButtonDown(Buttons.RightShoulder))
            {
                newRot += Vector3.Forward;
            }

            if (gamePad.IsButtonDown(Buttons.LeftTrigger))
            {
                shift = true;
            }

            if (gamePad.IsButtonDown(Buttons.RightTrigger))
            {
                doubleShift = true;
            }

            force = newTrans;
            if (force.Length() > 0)
            {
                force *= shift ? ForceShiftMag : ForceMag;
                if (shift && doubleShift)
                {

                    force *= 10;
                }
            }

            torque = newRot;
            if (torque.Length() > 0)
            {
                torque *= shift ? TorqueShiftMag : TorqueMag;
            }
            PrimaryFire = primaryFire;
            SecondaryFire = secondaryFire;

            ControlSchemeToggle = inputState.IsNewKeyPress(Keys.Space);
        }


    }
}
