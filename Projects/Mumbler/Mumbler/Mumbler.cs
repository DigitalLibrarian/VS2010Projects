using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Mumbler
{
    public class Mumbler
    {
        public Mumbler()
        {
            Random = new Random();
            WordMap = new Dictionary<string, Dictionary<string, int>>(StringComparer.OrdinalIgnoreCase);
            WordTotal = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            Terminals = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            Start = new Dictionary<string, int>();
        }
        
        Random Random { get; set; }
        Dictionary<string, Dictionary<string, int>>  WordMap { get; set; }
        Dictionary<string, int> WordTotal { get; set; }
        Dictionary<string, int> Terminals { get; set; }
        Dictionary<string, int> Start { get; set; }

        int TotalSentencesIngested { get; set; }

        public string NextWord(string firstWord)
        {
            return NextWord(Random, WordMap, WordTotal, firstWord);
        }
        public string NextWord(Random random, Dictionary<string, Dictionary<string, int>> bag, Dictionary<string, int> tots, string firstWord)
        {
            if (!bag.ContainsKey(firstWord)) return PickStartWord();
            var prob = bag[firstWord];
            var roll = Random.NextDouble();

            foreach (var key in prob.Keys.OrderBy(x => Random.Next())) 
            {
                var timesSeen = prob[key];
                
                if (tots.ContainsKey(key))
                {
                    var requiredRoll = timesSeen / tots[key];
                    if (requiredRoll < roll)
                    {
                        return key;
                    }
                }
            }
            return null;
        }


        public string NextSentence()
        {
            int deep = 0;
            string sentence = NextSentence(ref deep);
            //Console.WriteLine("Deep: {0}", deep);

            return sentence;
        }


        private string NextSentence(ref int deep)
        {
            deep++;
            var word = PickStartWord();
            string sentence = word;
            bool done = false;
            int totWords = 1;
            while (!done)
            {
                word = NextWord(word);
                if (word == null)
                {
                    //Console.WriteLine("RE: {0} - Out of words!", sentence);
                    return NextSentence(ref deep);
                }
                sentence += " " + word.ToLowerInvariant();
                done = EndSentence(word);
                totWords++;
                if (done && totWords < 8)
                {
                    //Console.WriteLine("RE: {0} - Too Simple!", sentence);
                    return NextSentence(ref deep);
                }
            }
            return sentence + ".\n";
        }

        private string PickStartWord()
        {
            string startWord;
            do
            {
                startWord = Start.Keys.ElementAt(Random.Next(Start.Keys.Count));
            }while(!WordMap.ContainsKey(startWord));

            return startWord;
        }

        private bool EndSentence(string word)
        {
            if (!Terminals.ContainsKey(word)) return false;
            if (Start.ContainsKey(word)) return false;
            double required = AverageTerminalWordFrequency();
            double shortness = 0.7f;
            return Random.NextDouble() * Terminals[word] * shortness > required;
        }

        private double AverageTerminalWordFrequency()
        {
            int totTerm = Terminals.Keys.Count;
            return (double) TotalSentencesIngested / (double)totTerm;
        }

        public string NormalizeMaterial(string material)
        {
            material = Regex.Replace(material, @"\s+'\s+", "'");
            material = Regex.Replace(material, @"[-:&<>',;]", " ");
            material = Regex.Replace(material, @"\s+", " ");
            return material;
        }

        public void DigestMaterial(string material)
        {
            
            material = NormalizeMaterial(material);
            var sentenceMatches = Regex.Matches(material, @"\p{Lu}((\w+[^\s]*\w+|\w)\s*)+|[.?!]");
            var sentences = new List<MatchCollection>();
            MatchCollection wordMatches;
            for (int i = 0; i < sentenceMatches.Count; i++)
            {
                if (IsDigestable(sentenceMatches[i].Value, out wordMatches))
                {
                    sentences.Add(wordMatches);
                }
            }

            foreach (var sentence in sentences)
            {
                TrainOnMaterial(sentence);
            }
        }


        private bool IsDigestable(string sentence, out MatchCollection wordMatches)
        {                
                wordMatches = Regex.Matches(sentence, @"\w+[^\s]*\w+|\w");
                if (wordMatches.Count < 3) return false;
                Match[] array = new Match[wordMatches.Count];

                wordMatches.CopyTo(array, 0);
                List<string> distinctWords = new List<string>();
                foreach (var wordMatch in array)
                {
                    if (!distinctWords.Contains(wordMatch.Value, StringComparer.OrdinalIgnoreCase)) distinctWords.Add(wordMatch.Value);
                }
                if(distinctWords.Count < 3) return false;
            
                return true;
        }
        
        private void TrainOnMaterial(MatchCollection material)
        {
            var dict = WordMap;
            var tots = WordTotal;
            int index = 0;
            StartWord(material[index].Value);
            while (index < material.Count - 1)
            {
                var key = material[index].Value;
                var nextKey = material[index + 1].Value;

                WordEncounter(key, nextKey);
                index++;
            }

            Terminal(material[material.Count - 1].Value);
            TotalSentencesIngested++;
        }

        private void WordEncounter(string firstWord, string secondWord)
        {
            var dict = WordMap;
            var tots = WordTotal;

            if (!dict.ContainsKey(firstWord)) dict[firstWord] = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            if (!dict.ContainsKey(secondWord)) dict[secondWord] = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            if (!dict[firstWord].ContainsKey(secondWord)) dict[firstWord][secondWord] = 0;
            if (!tots.ContainsKey(firstWord)) tots[firstWord] = 0;

            dict[firstWord][secondWord]++;
            tots[firstWord]++;
        }

        private void Terminal(string word)
        {
            if (!Terminals.ContainsKey(word)) Terminals[word] = 0;
            Terminals[word]++;
        }

        private void StartWord(string word)
        {
            if (!Start.ContainsKey(word)) Start[word] = 0;
            Start[word]++;
        }
    }

}
