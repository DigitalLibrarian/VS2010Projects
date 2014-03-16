using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.GA.Genomes;
using Aquarium.GA.Phenotypes;
using Aquarium.GA;
using Aquarium.GA.Codons;
using Microsoft.Xna.Framework;
using System.Collections.Concurrent;
using Microsoft.Xna.Framework.Graphics;
using Forever.Render;
using Aquarium.UI.Targets;

namespace Aquarium.Sim.Agents
{
    public interface IOrganismAgentPool
    {
        ICollection<OrganismAgent> OrganismAgents { get; }

        void Birth(OrganismAgent agent);
        void Death(OrganismAgent agent);
    }

    public class SpawnerAgent : IAgent,  IRayPickable, ITarget
    {
        Vector3 Position { get; set; }
        Random Random = new Random();
        GenomeSplicer Splicer = new GenomeSplicer();

        IOrganismAgentPool Pool;

        ConcurrentQueue<OrganismAgent> Births;

        BoundingBox Box { get; set; }
        Model Model { get; set; }

        public SpawnerAgent(Vector3 pos, IOrganismAgentPool pool, Model model, BoundingBox box)
        {
            Position = pos;
            Pool = pool;
            Model = model;
            Box = box;
            Births = new ConcurrentQueue<OrganismAgent>();
        }
        
        public int MaxPerSpawnPump = 50;
        public int BirthsPerUpdate = 1;
        public int MaxBirthQueueSize = 100;
        public int MaxPopSize = 100;
        public int SpawnRange = 25;
        public int GeneCap = 10000;
        public int DefaultParts = 7;
        public int DefaultOrgans = 30;
        public int DefaultNN = 15;
        public int DefaultJunk = 0;

        #region Generation Tools

        private bool TryEnqueue(BodyGenome off)
        {
            Mutate(off);

            var spawn = SpawnFromGenome(off);
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
            if (Random.Next(4) == 0)
            {
                Splicer.Mutate(off);
            }
        }


        public Organism SpawnFromGenome(BodyGenome g)
        {
            PhenotypeReader gR = new PhenotypeReader();

            var t = new RandomIntGenomeTemplate(Random);
            var parser = new BodyCodonParser();

            var pheno = parser.ParseBodyPhenotype(g, t);

            if (pheno != null)
            {
                var body = gR.ProduceBody(pheno);
                if (body != null)
                {
                    return new Organism(body);
                }
            }

            return null;
        }

        private bool TryGenerateRandom()
        {
            var genome = BodyGenome.Random(
                Random,
                numParts: DefaultParts,
                numOrgans: DefaultOrgans,
                numNN: DefaultNN,
                sizeJunk: DefaultJunk
                );

            return TryEnqueue(genome);
        }

        private int TryMeiosis()
        {
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
            if (!Pool.OrganismAgents.Any())
            {
                return new List<BodyGenome>();
            }

            return new List<BodyGenome>
            {
                Random.NextElement(Pool.OrganismAgents).Genome,
                Random.NextElement(Pool.OrganismAgents).Genome
            };
        }

        #endregion

        public void Draw(float duration, Forever.Render.RenderContext renderContext)
        {

            var world = Matrix.CreateTranslation(Position);
            Renderer.RenderModel(Model, world, renderContext);
        }

        public void Update(float duration)
        {

            if (Pool.OrganismAgents.Count() < MaxPopSize)
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

        }

        public bool QueueFull
        {
            get { return Births.Count() >= MaxBirthQueueSize; }
        }

        

        public void BackgroundThreadFunc()
        {
            if (QueueFull) return;
            int num = 0;

            while (num < MaxPerSpawnPump)
            {
                var mCount = TryMeiosis();
                num += mCount;
                if (mCount == 0 && TryGenerateRandom())
                {
                    num++;
                }
            }
            
        }



        public bool IsHit(Ray ray)
        {
            return ray.Intersects(Box) != null;
        }

        string ITarget.Label
        {
            get { return this.ToString(); }
        }

        IAgent ITarget.Agent
        {
            get { return this; }
        }

        BoundingBox ITarget.TargetBB
        {
            get { return Box; }
        }
    }
}
