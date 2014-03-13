using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using Forever.Physics;
using Forever.Neural;
using Aquarium.GA.Bodies;
using Aquarium.GA.Environments;


namespace Aquarium.GA
{
    public class Organism : IFood
    {
        public Body Body { get; private set; }
        public IRigidBody RigidBody { get; private set; }

        public NervousSystem NervousSystem { get; private set; }
        public ISurroundings Surroundings { get; set; }

        private const float EnergyBleed = 0.99f;
        private const float EnergyFlatline = 1f;
        /// <summary>
        /// Gets or sets the position of the organism.
        /// 
        /// Setting this also recaluates the rigid body data.
        /// </summary>
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
                RigidBody.calculateDerivedData();
            }
        }


        public BoundingBox LocalBB { get; private set; }
        public BoundingBox WorldBB
        {
            get
            {
                var corners = LocalBB.GetCorners();
                var newCorners = new List<Vector3>();
                foreach (var corner in corners)
                {
                    newCorners.Add(Vector3.Transform(corner, RigidBody.World));
                }
                return BoundingBox.CreateFromPoints(newCorners);
            }
        }

        public long Age { get; private set; }


        public float MaxEnergy { get; private set; }
        public float Energy { get; private set; }
        public float EdibleEnergy { get; private set; }
        public bool IsDead
        {
            get
            {
                return Energy <= EnergyFlatline;
            }
        }

        public Organism(Body b)
        {
            ConfigureSpawnLevels();

            Body = b;
            RigidBody = new RigidBody(b.Position);

            RigidBody.Awake = true;
            RigidBody.CanSleep = true;
            RigidBody.LinearDamping = 0.999f;
            RigidBody.AngularDamping = 0.999f;
            RigidBody.Mass = 1f;
            RigidBody.InertiaTensor = InertiaTensorFactory.Sphere(RigidBody.Mass, 1f);

            NervousSystem = new NervousSystem(this);

            OrganForces = new List<IForceGenerator>();
            foreach (var part in Body.Parts)
            {
                foreach (var organ in part.Organs)
                {
                    var fg = organ.ForceGenerator;
                    if (fg != null)
                    {
                        OrganForces.Add(fg);
                    }
                }
                UpdateBoundingGeometryToContain(part);
            }
        }

        private void UpdateBoundingGeometryToContain(BodyPart part)
        {
            if (LocalBB == null)
            {
                LocalBB = part.BodyBB();
            }
            else
            {
                LocalBB = LocalBB.ExtendToContain(part.BodyBB());
            }
        }

        private void ConfigureSpawnLevels()
        {
            Age = 0;
            MaxEnergy = 100;
            Energy = MaxEnergy;
            EdibleEnergy = Energy;
        }

        long Tick = 0;
        public void Update(float duration)
        {
            if (!IsDead && ((Tick++ % 10) == 0))
            {
                UpdateMetabolism(duration);
                NervousSystem.Update();
            }

            UpdatePhysics(duration);
        }

        protected void UpdateMetabolism(float duration)
        {
            Energy *= EnergyBleed;
            Age++;
        }



        public void Consume(IFood food)
        {
            //TODO - this should be moved to organ
            var BiteSize = 75;
            float energyEfficiency = .65f;
            Energy += food.BeConsumed(BiteSize) * energyEfficiency;
        }

        List<IForceGenerator> OrganForces { get; set; }
        protected void UpdatePhysics(float duration)
        {
            foreach (var fg in OrganForces)
            {
                fg.updateForce(RigidBody, duration);
            }
            RigidBody.integrate(duration);
            Body.Position = RigidBody.Position;
            Body.World = RigidBody.World;
        }

        public float ConsumableEnergy
        {
            get { return this.EdibleEnergy; }
        }

        public float BeConsumed(float biteSize)
        {
            var energy = Energy - biteSize;

            if (energy > 0)
            {
                Energy = energy;
                EdibleEnergy = Energy;
                return biteSize;
            }
            else
            {
                // im dead
                Energy = 0;
                EdibleEnergy = 0;
                return Math.Min(ConsumableEnergy, biteSize);
            }
        }

    }




    
    
    
}
