using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using Forever.Physics;
using Forever.Neural;
using Aquarium.GA.Bodies;


namespace Aquarium.GA
{
    public class Organism
    {
        public Body Body { get; private set; }
        public IRigidBody RigidBody { get; private set; }

        public NervousSystem NervousSystem { get; private set; }

        public Vector3 Position
        {
            get
            {
                return Body.Position;
            }
            set
            {
                var pos = value;
                Body.Position = pos;
                RigidBody.Position = pos;
            }
        }


        public float MaxEnergy { get; private set; }
        public float Energy { get; private set; }

        public Organism(Body b)
        {
            ConfigureSpawnLevels();
            Body = b;
            RigidBody = new RigidBody(b.Position);

            RigidBody.Awake = true;
            RigidBody.LinearDamping = 0.9f;
            RigidBody.AngularDamping = 0.7f;
            RigidBody.Mass = 0.1f;
            RigidBody.InertiaTensor = InertiaTensorFactory.Sphere(RigidBody.Mass, 1f);

            NervousSystem = new NervousSystem(this);
        }
        private void ConfigureSpawnLevels()
        {
            MaxEnergy = 100;
            Energy = MaxEnergy;
        }

        public void Update(float duration)
        {
            NervousSystem.Update();
            Body.Update(duration);

            UpdatePhysics(duration);
        }

        protected void UpdatePhysics(float duration)
        {
            RigidBody.integrate(duration);
            Body.Position = RigidBody.Position;

            Body.World = RigidBody.World;
        }
    }




    
    
    
}
