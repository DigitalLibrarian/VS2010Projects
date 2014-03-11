using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.GA.Genomes;
using Aquarium.GA.Phenotypes;
using Aquarium.GA;
using Aquarium.GA.Codons;
using Microsoft.Xna.Framework;

namespace Aquarium.Sim.Agents
{
    public class SpawnerAgent : IAgent
    {
        Vector3 Position { get; set; }
        Random Random = new Random();
        GenomeSplicer Splicer = new GenomeSplicer();

        public SpawnerAgent(Vector3 pos)
        {
            Position = pos;
        }



        int SpawnThreadFreq = 500;
        int MaxPerSpawnPump = 50;
        int DefaultParts = 10;
        int BirthsPerUpdate = 1;
        int MaxBirthQueueSize = 200;
        int minPopSize = 50;
        int maxPopSize = 100;
        int spawnRange = 25;
        int geneCap = 10000;
        int DefaultOrgans = 30;
        int DefaultNN = 5;
        int DefaultJunk = 1;


        private List<BodyGenome> GetGenePool()
        {
            throw new NotImplementedException();
        }

        #region Generation Tools

        private bool TryBirth(BodyGenome off)
        {
            Mutate(off);

            var spawn = SpawnFromGenome(off);
            if (spawn != null)
            {
                spawn.Position = Position + ( Random.NextVector() * spawnRange );
                spawn.RigidBody.Orientation = Quaternion.CreateFromYawPitchRoll((float)Random.NextDouble(), (float)Random.NextDouble(), (float)Random.NextDouble());
                
                //now we have organism;


                return true;
            }
            return false;
        }

        private void Mutate(BodyGenome off)
        {
            throw new NotImplementedException();
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

            return TryBirth(genome);
        }

        private bool TryMeiosis()
        {
            List<BodyGenome> genomes = GetGenePool();

            if (genomes.Any())
            {
                var p1 = Random.NextElement(genomes);
                var p2 = Random.NextElement(genomes);

                int count = 0;
                foreach (var offspring in Splicer.Meiosis(p1, p2))
                {
                    if (TryBirth(offspring)) count++;
                }
                return count > 0;
            }

            return false;
        }

        #endregion

        public void Draw(float duration, Forever.Render.RenderContext renderContext)
        {
            //TODO
        }

        public void Update(float duration)
        {
            //TODO
        }
    }
}
