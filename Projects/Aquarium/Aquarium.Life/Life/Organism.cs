﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using Forever.Physics;
using Forever.Neural;
using Aquarium.Life.Bodies;
using Aquarium.Life.Environments;
using Aquarium.Life.Phenotypes;
using Aquarium.Life.Genomes;
using Aquarium.Life.Spec;

using Forever.Extensions;


namespace Aquarium.Life
{
    public class Organism : IFood
    {
        public LifeForce LifeForce { get; private set; }
        public Body Body { get; private set; }
        public IRigidBody RigidBody { get; private set; }

        public NervousSystem NervousSystem { get; private set; }
        public ISurroundings Surroundings { get; set; }

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


        public int TotalOrgans { get; private set; }

        public static Organism CreateFromGenome(BodyGenome g, OrganismSpecParser specParser)
        {
            var organismSpec = specParser.ReadOrganismSpec(g);
            var pheno = new BodyPhenotype(organismSpec);

            PhenotypeReader gR = new PhenotypeReader(); 
            var body = gR.ProduceBody(pheno);
            if (body != null)
            {
                return new Organism(body);
            }
            return null;
        }

        public Organism(Body b)
        {

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
                    TotalOrgans++;
                    var fg = organ.ForceGenerator;
                    if (fg != null)
                    {
                        OrganForces.Add(fg);
                    }
                }
                UpdateBoundingGeometryToContain(part);
            }

            LifeForce = new LifeForce(LifeForce.Data.MaxEnergy);
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

        
        long Tick = 0;
        public void Update(float duration)
        {
            if (!IsDead && ((Tick++ % 2) == 0))
            {
                LifeForce.Update(duration, this);
                NervousSystem.Update(duration);
            }

            UpdatePhysics(duration);
        }

        public bool IsDead { get { return LifeForce.IsDead; } }
        public float Energy { get { return LifeForce.Energy; } }
        public float EdibleEnergy { get { return LifeForce.Energy; } }
        public float MaxEnergy { get { return LifeForce.MaxEnergy; } }
        public long Age { get { return LifeForce.Age; } }
       
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

        #region Food
        public void Consume(IFood food)
        {
            var BiteSize = 75;
            float energyEfficiency = .8f;

            LifeForce.AddEnergy(food.BeConsumed(BiteSize) * energyEfficiency);
        }

        public float ConsumableEnergy
        {
            get { return this.EdibleEnergy; }
        }

        public float BeConsumed(float biteSize)
        {
            var energy = LifeForce.Energy;
            var remaining = energy - biteSize;
            LifeForce.PayEnergyCost(biteSize);
            if(remaining >= 0)
            {
                return biteSize;
            }
            else
            {
                return biteSize - energy;
            }
        }
        #endregion

    }




    
    
    
}
