﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Forever.Physics;
using Forever.Render;

using Aquarium.Life;
using Aquarium.Life.Genomes;
using Aquarium.Life.Phenotypes;

using Aquarium.Sim;
using Aquarium.Ui.Targets;
using Aquarium.Life.Spec;
using Forever.Extensions;
using Aquarium.Targeting;


namespace Aquarium.Agent
{
    public class SpawnerAgent : IAgent, IRayPickable, ITarget
    {
        Vector3 _position;
        Vector3 Position { get { return _position; } set { _position = value; } }
        Random Random = new Random();

        GenomeSplicer Splicer = new GenomeSplicer();

        public OrganismSpecParser SpecParser = new OrganismSpecParser();

        IOrganismAgentGroup Pool;
        List<OrganismAgent> BreedingCandidates;

        public ConcurrentQueue<OrganismAgent> Births { get; private set; }

        BoundingBox Box { get; set; }
        Model Model { get; set; }

        Vector3 ISimObject.Position { get { return _position; } }

        public Thread Thread { get; private set; }

        public SpawnerAgent(Vector3 pos, IOrganismAgentGroup pool, Model model, BoundingBox box)
        {
            Position = pos;
            Pool = pool;
            Model = model;
            Box = box;
            Births = new ConcurrentQueue<OrganismAgent>();
            BreedingCandidates = new List<OrganismAgent>();

            Thread = new Thread(BackgroundThreadFunc);
        }
        
        public int MaxPerSpawnPump = 50;
        public int BirthsPerUpdate = 1;
        public int MaxBirthQueueSize = 20;
        public int MaxPopSize = 10;
        public int SpawnRange = 25;
        public int GeneCap = 10000;
        public bool UseMeiosis = false;
        public bool UseRandom = false;
        public float MutationChance = 0.000001f;
        #region Generation Tools

        private bool TryEnqueue(BodyGenome off)
        {
            Mutate(off);

            var spawn = Organism.CreateFromGenome(off, SpecParser);
            if (spawn != null)
            {
                spawn.Position = Position + ( Random.NextVector() * SpawnRange );
                Vector3 rot = Random.NextVector() * (float)Math.PI;
                spawn.RigidBody.Orientation = Quaternion.CreateFromYawPitchRoll(rot.X, rot.Y, rot.Z);
                
                var orgAgent = new OrganismAgent(off, spawn);
                orgAgent.OnDeath += new OrganismAgent.OnDeathEventHandler(orgAgent_OnDeath);
                Births.Enqueue(orgAgent);

                return true;
            }
            return false;
        }

        void orgAgent_OnDeath(OrganismAgent agent)
        {
            Pool.Death(agent);
        }

        private void Mutate(BodyGenome off)
        {
            if (Random.NextDouble() < MutationChance)
            {
                Splicer.Mutate(off);
            }
        }


       

        private bool TryGenerateRandom()
        {
           // if (!UseRandom) return false;

            BodyGenome genome = new BodyGenome(Enumerable.Range(0, GeneCap / 2).Select(x => Random.Next()).ToList());
            return TryEnqueue(genome);
        }

        private int TryMeiosis()
        {
            //if (!UseMeiosis) return 0;
            int count = 0;
            List<BodyGenome> genomes = GetNewParents();
            if (genomes.Any())
            {
                var p1 = Random.NextElement(genomes);
                var p2 = Random.NextElement(genomes);

                foreach (var offspring in Splicer.Meiosis(p1, p2))
                {
                    if (TryEnqueue(offspring)) count++;
                }
            }

            return count;
        }

        private List<BodyGenome> GetNewParents()
        {
            if (!BreedingCandidates.Any())
            {
                return new List<BodyGenome>();
            }
            return new List<BodyGenome>
            {
                Random.NextElement(BreedingCandidates).Genome,
                Random.NextElement(BreedingCandidates).Genome
            };
        }

        #endregion

        public void Draw(float duration, Forever.Render.RenderContext renderContext)
        {

            var world = Matrix.CreateTranslation(Position);
            Renderer.RenderModel(Model, world, renderContext);

            Renderer.Render(renderContext, Pool.Box, Color.Red);
        }

        public void Update(float duration)
        {

            if (Pool.TotalAssigned < MaxPopSize)
            {
                UpdateBirths();
            }

        }

        private void UpdateBirths()
        {
            int totalBirths = 0;
            OrganismAgent localValue;
            while (totalBirths < BirthsPerUpdate && Births.TryPeek(out localValue))
            {
                Births.TryDequeue(out localValue);

                if (localValue != null)
                {
                    Pool.Birth(localValue);
                    totalBirths++;
                }
            }

            var oldest = Pool.OrganismAgents.OrderByDescending(x => x.Organism.Age);
            var newCandidates = new List<OrganismAgent>();

            var oldestEnum = oldest.GetEnumerator();
            foreach (var topTen in oldest)
            {
                if (newCandidates.Count > 10 || !oldestEnum.MoveNext())
                {
                    break;
                }
                newCandidates.Add(oldestEnum.Current);
            }


            BreedingCandidates = newCandidates;
        }

        public bool QueueFull
        {
            get { return Births.Count() >= MaxBirthQueueSize; }
        }

        

        public void BackgroundThreadFunc()
        {

            while (true)
            {
                Thread.Sleep(500);

                if (QueueFull) continue;
                if (!Producing) continue;
                int num = 0;

                while (!QueueFull && num < MaxPerSpawnPump)
                {
                    if (UseMeiosis)
                    {
                        num += TryMeiosis();
                    }

                    if (UseRandom && TryGenerateRandom())
                    {
                        num++;
                    }
                }
            }
        }

        public bool Producing { get { return UseMeiosis || UseRandom; } } 


        public bool IsHit(Ray ray)
        {
            return ray.Intersects(Box) != null;
        }

        string ITarget.Label
        {
            get { return this.ToString(); }
        }

        BoundingBox ITarget.TargetBB
        {
            get { return Box; }
        }
    }
}
