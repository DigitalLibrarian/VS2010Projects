using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Mumbler
{
    class Program
    {
        static void Main(string[] args)
        {

            var mumbler = new Mumbler();

            string[] files = Directory.GetFiles(@"C:\Users\Valued Customer\Desktop\txtBooks\");

            foreach (var filename in files)
            {
                if (!filename.EndsWith(".txt")) continue;
                Console.WriteLine("Ingesting {0}", filename);
                // Read the file as one string.
                var myFile = new System.IO.StreamReader(filename);
                string contents = myFile.ReadToEnd();
                myFile.Close();
                mumbler.DigestMaterial(contents);
            }

            Console.WriteLine("Trained.");
            while (Console.ReadKey().KeyChar != 'q')
            {
                Console.WriteLine(">>" + mumbler.NextSentence());
            }

        }


    }
}
