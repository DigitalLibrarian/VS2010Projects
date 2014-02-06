using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;

namespace Aquarium.GA.Genomes
{
    public class Genome<T>
    {
        // ((1 1) (2 1) (3 0) (4 0) (5 0) (6 1) (7 1))
        public List<Gene<T>> Genes { get; private set; }

        public int Size { get { return Genes.Count(); } }

        public Genome(List<Gene<T>> genes)
        {
            Genes = genes;
        }

        public Gene<T> ByName(int name)
        {
            // positional ordering wins
            return Genes.FirstOrDefault(g => g.Name == name);
        }


        public Gene<T> ByName(int name, GenomeTemplate<T> template)
        {
            // positional ordering wins
            var gene = Genes.FirstOrDefault(g => g.Name == name);
            if (gene == null) gene = template.ByName(name);
            return gene;
        }

        public List<T> ReadDataSequence(int index, int num, GenomeTemplate<T> template)
        {
            return CondenseSequence(index, num, template).Select(g => g.Value).ToList();
        }

        /// <summary>
        /// Removes duplicates, adds in missing info from template.  List returned is same length
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        internal List<Gene<T>> Condense(GenomeTemplate<T> template)
        {
            var result = new List<Gene<T>>();
            int visited = 0;
            int index = 0;
            while(visited < Genes.Count())
            {
                var gene = ByName(index);
                result.Add(gene);

                index++;
                visited++;
            }
            return result;
        }


        /// <summary>
        /// Removes duplicates, adds in missing info from template.  List returned is same length
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        public List<Gene<T>> CondenseSequence(int index, int num, GenomeTemplate<T> template)
        {
            var result = new List<Gene<T>>();
            int i = 0;
            while (i < num)
            {
                var gene = ByName(index + i, template);
                result.Add(gene);

                i++;
            }
            return result;
        }

        /// <summary>
        /// Mutator mutator
        /// </summary>
        /// <param name="random"></param>
        public virtual void Mutate(Random random) { }
    }

}