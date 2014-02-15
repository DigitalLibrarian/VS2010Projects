using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquarium.GA.Genomes;
using Aquarium.GA.Phenotypes;
using Aquarium.GA.Headers;

namespace Aquarium.GA.Codons
{
    public abstract class AqParser : CodonParser<int>
    {
        public List<int> Extract(BodyGenome genome, GenomeTemplate<int> template, int startIndex, Codon<int> recognizer, Codon<int>[] terminals)
        {
            var iterator = startIndex;
            int maxRead = genome.Size;
            List<int> clump = new List<int>();

            bool endRecognized = false;
            do
            {
                var scan = iterator % maxRead;
                var seq = genome.CondenseSequence(scan, recognizer.FrameSize, template);
                var data = seq.Select(gene => gene.Value).ToList();
                if (recognizer.Recognize(data))
                {
                    iterator += recognizer.FrameSize;
                    scan = iterator % maxRead;

                    clump = ReadUntilOrEnd(genome, template, terminals, scan);

                    return clump;
                }
                else
                {
                    foreach (var term in terminals)
                    {
                        if (term.Recognize(data))
                        {
                            return clump;
                        }
                    }
                    iterator++;
                }
            } while (iterator < maxRead && !endRecognized);

            return clump;
        }

        public int VisitSequence(BodyGenome genome, GenomeTemplate<int> template, int startIndex, Codon<int> recognizer, Codon<int>[] terminals, Action<List<int>>  visitor)
        {
            {
                var iterator = startIndex;
                int maxRead = genome.Size;
                List<int> clump = new List<int>();

                bool endRecognized = false;
                do
                {
                    var scan = iterator % maxRead;
                    var seq = genome.CondenseSequence(scan, recognizer.FrameSize, template);
                    var data = seq.Select(gene => gene.Value).ToList();
                    if (recognizer.Recognize(data))
                    {
                        iterator += recognizer.FrameSize;
                        scan = iterator % maxRead;

                        clump = ReadUntilOrEnd(genome, template, terminals, scan);

                        visitor(clump);


                        iterator += clump.Count();
                    }
                    else
                    {
                        foreach (var term in terminals)
                        {
                            if (term.Recognize(data))
                            {
                                return scan;
                            }
                        }
                        iterator++;
                    }
                } while (iterator < maxRead && !endRecognized);
                return iterator % maxRead;
            }
        }
    }

    public class BodyCodonParser : AqParser
    {

        Dictionary<Codon<int>, Func<BodyGenome, GenomeTemplate<int>, int, int>> ClumpProcessors { get; set; }
   
        public IBodyPhenotype Pheno { get; private set; }
        public bool EndRecognized { get; private set; }

        public BodyCodonParser()
        {
            EndRecognized = false;
            Pheno = new BodyPhenotype();
            ClumpProcessors =new Dictionary<Codon<int>, Func<BodyGenome, GenomeTemplate<int>, int, int>>();
            RegisterClumpProcessors();


        }

        public IBodyPhenotype ParseBodyPhenotype(BodyGenome g, GenomeTemplate<int> t)
        {
            var iterator = 0;
            int maxRead = g.Size;
            do
            {
                var scan = iterator % maxRead;
                bool recog = false;
                foreach (var codon in ClumpProcessors.Keys)
                {
                    var seq = g.CondenseSequence(scan, codon.FrameSize, t);
                    var data = seq.Select(gene => gene.Value).ToList();
                    if (codon.Recognize(data))
                    {
                        iterator += codon.FrameSize;
                        scan = iterator % maxRead;

                        var numRead = ClumpProcessors[codon](g, t, scan);

                        if (numRead > 0)
                        {
                            iterator += numRead;
                            scan = iterator % maxRead;
                            recog = true;

                            break;
                        }
                    }
                }

                if (!recog)
                {
                    iterator++;
                }

            } while (iterator < maxRead && !EndRecognized);

            return Pheno;
        }


        public void RegisterClumpProcessors()
        {
            var partStart = new BodyPartStartCodon();
            ClumpProcessors.Add(partStart, ProcessBodyPartClump);
            ClumpProcessors.Add(new OrganStartCodon(), ProcessOrganClump);
            ClumpProcessors.Add(new BodyEndCodon(), ProcessBodyEndClump);
            ClumpProcessors.Add(new NeuralNetworkStartCodon(), ProcessNeuralNetworkClump);
        }

        private int ProcessBodyPartClump(BodyGenome g, GenomeTemplate<int> t, int iterator)
        {
            var clump = ReadUntilOrEnd(
                            g, t,
                            new Codon<int>[] 
                            { 
                                new BodyPartStartCodon(),
                                new BodyPartEndCodon(),
                                new BodyEndCodon()
                            },
                            iterator);


            if (clump.Count() >= BodyPartHeader.Size)
            {
                var header = BodyPartHeader.FromGenes(clump);
                Pheno.BodyPartPhenos.Add(new BodyPartPhenotype(header));

                return clump.Count();
            }
            
            return 0;
        }


        private int ProcessOrganClump(BodyGenome g, GenomeTemplate<int> t, int iterator)
        {
            var clump = ReadUntilOrEnd(
                            g, t,
                            new Codon<int>[] 
                            { 
                                new OrganStartCodon(),
                                new OrganEndCodon(),
                                new BodyEndCodon()
                            },
                            iterator);

            if (clump.Count() >= OrganHeader.Size)
            {
                var header = OrganHeader.FromGenes(clump);
                var pheno = new OrganPhenotype(header);
                Pheno.OrganPhenos.Add(pheno);

                return  clump.Count();
            }

            return 0;
        }

        private int ProcessNeuralNetworkClump(BodyGenome g, GenomeTemplate<int> t, int iterator)
        {

            var clump = ReadUntilOrEnd(
                            g, t,
                            new Codon<int>[] 
                            { 
                                new NeuralNetworkStartCodon(),
                                new NeuralNetworkEndCodon(),
                                new BodyEndCodon()
                            },
                            iterator);

            if (clump.Count() >= NeuralNetworkHeader.Size)
            {
                var header = NeuralNetworkHeader.FromGenes(clump);
                if (header.Weights == null)
                {
                    // this happens when the there wasn't enough data to make the weight array,
                    // we could pad it
                    // we also need to make this affect the genome score, maybe scaled by the padding required

                }
                else
                {
                    var pheno = new NeuralNetworkPhenotype(header);
                    Pheno.NeuralNetworkPhenos.Add(pheno);

                }

                return  clump.Count();
            }

            return 0;
        }

        private int ProcessBodyEndClump(BodyGenome g, GenomeTemplate<int> t, int iterator)
        {
            EndRecognized = true;
            return 0;
        }
    }

}
