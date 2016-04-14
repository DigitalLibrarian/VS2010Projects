using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.UI.Controls;
using Forever.Voxel.SVO;

namespace Aquarium
{
    class RayCastScreen : FlyAroundGameScreen
    {
        const int VoxelsPerDimension = 8;

        #region Content
        public override void LoadContent()
        {
            base.LoadContent();

            SetupGui();
        }
        #endregion

        #region UI
        RayCastWindowControl RayCastWindowControl { get; set; }
        void SetupGui()
        {
            RayCastWindowControl = new RayCastWindowControl(1500, 200);
            RayCastWindowControl.OnClear += new EventHandler(RayCastWindowControl_OnClear);
            RayCastWindowControl.OnNextLeaf += new EventHandler(RayCastWindowControl_OnNextLeaf);


            this.GuiManager.Screen.Desktop.Children.Add(RayCastWindowControl);
        }

        void RayCastWindowControl_OnNextLeaf(object sender, EventArgs e)
        {
            NextLeaf();
        }

        void RayCastWindowControl_OnClear(object sender, EventArgs e)
        {
            Clear();
        }
        #endregion

        // probably better to tie these to the action bar, and use a window to explain controls.....
        void Clear()
        {
        }

        void NextLeaf()
        {

        }
    }
}
