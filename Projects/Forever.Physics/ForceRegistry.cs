using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace Forever.Physics
{


    public class ForceRegistry
    {
        protected Dictionary<IPhysicsObject, List<IForceGenerator>> registry;

        public ForceRegistry()
        {
            registry = new Dictionary<IPhysicsObject, List<IForceGenerator>>();
        }
        public void Add(IPhysicsObject p, IForceGenerator forceGen)
        {
            List<IForceGenerator> list;
            if (registry.ContainsKey(p))
            {
                list = registry[p];
            }
            else
            {
                registry[p] = list = new List<IForceGenerator>();
            }
            list.Add(forceGen);
        }

        public void Remove(IPhysicsObject p, IForceGenerator forceGen)
        {
            List<IForceGenerator> list;
            if (registry.ContainsKey(p))
            {
                list = registry[p];
                list.Remove(forceGen);
            }
        }

        public void ClearParticle(IPhysicsObject p)
        {
            if (registry.ContainsKey(p))
            {
                registry.Remove(p);
            }
        }


        public void Clear()
        {
            IPhysicsObject[] keys = new IPhysicsObject[registry.Keys.Count];
            registry.Keys.CopyTo(keys, 0);
            for (int i = 0; i < registry.Keys.Count; i++)
            {
                registry[keys[i]].Clear();
            }
            registry.Clear();
        }


        public void UpdateForceGenerators(IPhysicsObject p, float duration)
        {
            if (registry.ContainsKey(p))
            {
                int size = registry[p].Count;
                IForceGenerator[] list = new IForceGenerator[size];
                registry[p].CopyTo(list, 0);
                for (int i = 0; i < size; i++)
                {
                    list[i].updateForce(p, duration);
                }

            }
         
        }
    }


}
