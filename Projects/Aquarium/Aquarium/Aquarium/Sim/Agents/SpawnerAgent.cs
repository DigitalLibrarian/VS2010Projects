using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.Life.Genomes;
using Aquarium.Life.Phenotypes;
using Aquarium.Life;
using Aquarium.Life.Codons;
using Microsoft.Xna.Framework;
using System.Collections.Concurrent;
using Microsoft.Xna.Framework.Graphics;
using Forever.Render;
using Aquarium.UI.Targets;
using System.Threading;
using Aquarium.Life.Organs.OrganAbilities;

namespace Aquarium.Sim.Agents
{
    public interface IOrganismAgentPool
    {
        ICollection<OrganismAgent> OrganismAgents { get; }
        
        void Birth(OrganismAgent agent);
        void Death(OrganismAgent agent);
    }

    public class SpawnerAgent : IAgent,  IRayPickable, ITarget, IMatingManager
    {
        Vector3 Position { get; set; }
        Random Random = new Random();
        GenomeSplicer Splicer = new GenomeSplicer();

        IOrganismAgentPool Pool;

        public ConcurrentQueue<OrganismAgent> Births { get; private set; }

        BoundingBox Box { get; set; }
        Model Model { get; set; }

        public Thread Thread { get; private set; }

        public SpawnerAgent(Vector3 pos, IOrganismAgentPool pool, Model model, BoundingBox box)
        {
            Position = pos;
            Pool = pool;
            Model = model;
            Box = box;
            Births = new ConcurrentQueue<OrganismAgent>();

            Thread = new Thread(BackgroundThreadFunc);
        }
        
        public int MaxPerSpawnPump = 50;
        public int BirthsPerUpdate = 1;
        public int MaxBirthQueueSize = 20;
        public int MaxPopSize = 10;
        public int SpawnRange = 25;
        public int GeneCap = 10000;
        public int DefaultParts = 25;
        public int DefaultOrgans = 30;
        public int DefaultNN = 15;
        public int DefaultJunk = 0;
        public bool UseMeiosis = false;
        public bool UseRandom = false;
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

           // gR.AbilityFactories.Add((a, b) => new MatingOrganAbility(this, a));

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
            if (!UseRandom) return false;
            BodyGenome genome;
            var list = Pool.OrganismAgents.ToList();

            if (list.Count() < 5)
            {

                genome = BodyGenome.Random(
                    Random,
                    numParts: DefaultParts,
                    numOrgans: DefaultOrgans,
                    numNN: DefaultNN,
                    sizeJunk: DefaultJunk
                    );
            }
            else
            {
                genome = Random.NextElement(list).Genome;
            }

            return TryEnqueue(genome);
        }

        private int TryMeiosis()
        {
            return 0;
            if (!UseMeiosis) return 0;
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
            while (true)
            {
                Thread.Sleep(500);

                if (QueueFull) continue;
                if (!Producing) continue;
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

        IAgent ITarget.Agent
        {
            get { return this; }
        }

        BoundingBox ITarget.TargetBB
        {
            get { return Box; }
        }

        public bool TryMate(OrganismAgent me, OrganismAgent other)
        {
            if (QueueFull) return false;
            var p1 = me.Genome;
            var p2 = other.Genome;

            int count = 0;
            foreach (var offspring in Splicer.Meiosis(p1, p2))
            {
                if (TryEnqueue(offspring)) count++;
            }
            return count > 0;
        }
    }
}
