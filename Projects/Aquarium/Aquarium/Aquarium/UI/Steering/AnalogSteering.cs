using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Forever.Screens;
using Forever.Physics;

namespace Aquarium.UI.Steering
{
    public enum AnalogKeys
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

    public class AnalogSteering : ISteering
    {
        // Computed deltas 
        public Vector3 LocalForce { get; private set; }

        public Vector3 LocalTorque { get; private set; }

        public IRigidBody Body { get; private set; }

        public float ForceMag { get; set; }
        public float TorqueMag { get; set; }

        public float ForceShiftMag { get; set; }
        public float TorqueShiftMag { get; set; }

        public bool PrimaryFire { get; set; }
        public bool SecondaryFire { get; set; }

        public bool ControlSchemeToggle { get; set; }

        public PlayerIndex playerIndex;
        public Dictionary<Keys, AnalogKeys> KeyMappings = new Dictionary<Keys, AnalogKeys>();

        public AnalogSteering(PlayerIndex playerIndex, float forceMag, float torqueMag, float forceShiftMag, float torqueShiftMag,
            IRigidBody body)
        {
            Body = body;

            this.ForceMag = forceMag;
            this.TorqueMag = torqueMag;
            this.ForceShiftMag = forceShiftMag;
            this.TorqueShiftMag = torqueShiftMag;
            this.playerIndex = playerIndex;

            //default mappings
            KeyMappings[Keys.W] = AnalogKeys.MoveFoward;
            KeyMappings[Keys.S] = AnalogKeys.MoveBackward;
            KeyMappings[Keys.A] = AnalogKeys.MoveLeft;
            KeyMappings[Keys.D] = AnalogKeys.MoveRight;
            KeyMappings[Keys.Q] = AnalogKeys.RollLeft;
            KeyMappings[Keys.Left] = AnalogKeys.TurnLeft;
            KeyMappings[Keys.E] = AnalogKeys.RollRight;
            KeyMappings[Keys.Right] = AnalogKeys.TurnRight;
            KeyMappings[Keys.X] = AnalogKeys.MoveUp;
            KeyMappings[Keys.Z] = AnalogKeys.MoveDown;

            KeyMappings[Keys.Down] = AnalogKeys.LookUp;
            KeyMappings[Keys.Up] = AnalogKeys.LookDown;

            KeyMappings[Keys.T] = AnalogKeys.TurnRight;
            KeyMappings[Keys.G] = AnalogKeys.TurnLeft;

            KeyMappings[Keys.LeftShift] = AnalogKeys.Shifter;
            KeyMappings[Keys.RightShift] = AnalogKeys.DoubleShifter;

            KeyMappings[Keys.LeftAlt] = AnalogKeys.PrimaryFire;
            KeyMappings[Keys.RightAlt] = AnalogKeys.SecondaryFire;

        }


        public void HandleInput(InputState inputState)
        {
            Vector3 newTrans = Vector3.Zero;
            Vector3 newRot = Vector3.Zero;
            bool shift = false;
            bool doubleShift = false;
            bool primaryFire = false;
            bool secondaryFire = false;
            foreach (KeyValuePair<Keys, AnalogKeys> pair in KeyMappings)
            {
                Keys key = pair.Key;
                if (inputState.IsKeyDown(key, playerIndex))
                {
                    AnalogKeys flyKey = pair.Value;
                    switch (flyKey)
                    {
                        case AnalogKeys.MoveFoward:
                            newTrans += Vector3.Forward;
                            break;
                        case AnalogKeys.MoveBackward:
                            newTrans += Vector3.Backward;
                            break;
                        case AnalogKeys.MoveRight:
                            newTrans += Vector3.Right;
                            break;
                        case AnalogKeys.MoveLeft:
                            newTrans += Vector3.Left;
                            break;
                        case AnalogKeys.MoveUp:
                            newTrans += Vector3.Up;
                            break;
                        case AnalogKeys.MoveDown:
                            newTrans += Vector3.Down;
                            break;
                        case AnalogKeys.TurnLeft:
                            newRot += Vector3.Up;
                            break;
                        case AnalogKeys.TurnRight:
                            newRot += Vector3.Down;
                            break;
                        case AnalogKeys.LookUp:
                            newRot += Vector3.Right;
                            break;
                        case AnalogKeys.LookDown:
                            newRot += Vector3.Left;
                            break;
                        case AnalogKeys.RollRight:
                            newRot += Vector3.Forward;
                            break;
                        case AnalogKeys.RollLeft:
                            newRot += Vector3.Backward;
                            break;
                        case AnalogKeys.Shifter:
                            shift = true;
                            break;
                        case AnalogKeys.DoubleShifter:
                            doubleShift = true;
                            break;
                        case AnalogKeys.PrimaryFire:
                            primaryFire = true;
                            break;
                        case AnalogKeys.SecondaryFire:
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

            LocalForce = newTrans;
            if (LocalForce.Length() > 0)
            {
                LocalForce *= shift ? ForceShiftMag : ForceMag;
                if (shift && doubleShift)
                {

                    LocalForce *= 10;
                }
            }

            LocalTorque = newRot;
            if (LocalTorque.Length() > 0)
            {
                LocalTorque *= shift ? TorqueShiftMag : TorqueMag;
            }


            
            Vector3 actuatorTrans = LocalForce;
            Vector3 actuatorRot = LocalTorque;

            float forwardForceMag = -actuatorTrans.Z;
            float rightForceMag = actuatorTrans.X;
            float upForceMag = actuatorTrans.Y;

            var forward = Vector3.Transform(Vector3.Forward, Body.Orientation);
            var right = Vector3.Transform(Vector3.Right, Body.Orientation);
            var up = Vector3.Transform(Vector3.Up, Body.Orientation);

            Vector3 force =
                (forward * forwardForceMag) +
                (right * rightForceMag) +
                (up * upForceMag);

            LocalForce = force;
            LocalTorque = Vector3.Transform(actuatorRot, Body.Orientation);
            
            PrimaryFire = primaryFire;
            SecondaryFire = secondaryFire;
            ControlSchemeToggle = inputState.IsNewKeyPress(Keys.Space);
        }



        Vector3 ISteering.Force
        {
            get { return LocalForce; }
        }

        Vector3 ISteering.Torque
        {
            get { return LocalTorque; }
        }
    }
}
