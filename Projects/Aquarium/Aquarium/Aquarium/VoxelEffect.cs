﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Forever.Voxel.World;
using Forever.Voxel.SVO;

namespace Aquarium
{
    class VoxelEffect : IVoxelEffect
    {
        public Matrix WVP
        {
            get { return _effect.Parameters["WVP"].GetValueMatrix(); }
            set { _effect.Parameters["WVP"].SetValue(value); }
        }

        public Vector3 CameraPosition
        {
            get { return _effect.Parameters["CameraPos"].GetValueVector3(); }
            set { _effect.Parameters["CameraPos"].SetValue(value); }
        }

        public Vector3 LightPos
        {
            get { return _effect.Parameters["LightPosition"].GetValueVector3(); }
            set { _effect.Parameters["LightPosition"].SetValue(value); }
        }

        public float LightDistanceSquared
        {
            get { return _effect.Parameters["LightDistanceSquared"].GetValueSingle(); }
            set { _effect.Parameters["LightDistanceSquared"].SetValue(value); }
        }

        public Color LightDiffuseColorIntensity
        {
            get { return new Color( _effect.Parameters["LightDiffuseColorIntensity"].GetValueVector3()); }
            set { _effect.Parameters["LightDiffuseColorIntensity"].SetValue(value.ToVector3()); }
        }
        public Color DiffuseColor
        {
            get { return new Color(_effect.Parameters["DiffuseColor"].GetValueVector3()); }
            set { _effect.Parameters["DiffuseColor"].SetValue(value.ToVector3()); }
        }



        public float VoxelScale
        {
            // these don't do anything...yet
            get;
            set;
        }

        public Microsoft.Xna.Framework.Graphics.EffectTechnique CurrentTechnique { get { return _effect.CurrentTechnique; } }

        private Effect _effect;
        public VoxelEffect(Effect effect)
        {
            _effect = effect;
        }
    }
}
