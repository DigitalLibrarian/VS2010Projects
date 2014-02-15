using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace Forever.Screens
{
  // This is the method that will be called
  // when the requested event is fired.
  public delegate void FunctionDelegate();

  public struct InputCallback
  {
    public Buttons? button;
    public PlayerIndex index;

    // The method is called if a button press event 
    // is detected for the button OR the key.
    public FunctionDelegate callback;
  }

  public class ControllerEvents  : InputState
  {
    List<InputCallback> callbacks = new List<InputCallback>();


    public override void  Update()
    {
      // update base state
      base.Update();

      _handleEvents();

    }

    public Vector2 LeftThumbstick(PlayerIndex playerIndex)
    {
      return GamePad.GetState(playerIndex).ThumbSticks.Left;
    }

    public Vector2 RightThumbstick(PlayerIndex playerIndex)
    {
      return GamePad.GetState(playerIndex).ThumbSticks.Right;
    }

    public void AddButtonPressCallback(Buttons button, PlayerIndex playerIndex, FunctionDelegate callback)
    {
      InputCallback c = new InputCallback();
      c.button = button;
      c.index = playerIndex;
      c.callback = callback;
      callbacks.Add(c);
    }



    private void _handleEvents()
    {
      foreach (InputCallback c in callbacks)
      {
        if (c.button != null && IsNewButtonPress((Buttons)c.button, c.index))
        {
          c.callback(); 
        }
      }
    }

  }
}
