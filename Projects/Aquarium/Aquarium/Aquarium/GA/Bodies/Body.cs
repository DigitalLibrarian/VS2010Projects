using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Aquarium.GA.Organs;
using Aquarium.GA.Genomes;
using Microsoft.Xna.Framework;
using Forever.Render;

namespace Aquarium.GA.Bodies
{

    public class Body
    {
        public int Age { get; protected set; }
        public int Energy { get; protected set; }

        public BodyGenome Genome { get; private set; }

        public List<BodyPart> Parts { get; private set; }

        public Vector3 Position { get; set; }
        public Matrix World { get; set; }

        public Body()
        {
            Parts = new List<BodyPart>();
        }

        public void Update(float duration)
        {

        }

        public void Render(RenderContext renderContext)
        {
            Parts.ForEach(part => part.Render(this, renderContext));

        }

        public bool WillFit(BodyPartSocket connectedSocket, BodyPartSocket foreignSocket)
        {
            var fPart = foreignSocket.Part;


            foreach (var part in Parts)
            {

                if (part == connectedSocket.Part) continue;

                var pBox = part.BodyBB();
                var fBox = fPart.BodyBB();



                
                var f = fBox;
                var p = pBox;
                if(f.Intersects(p)) return false;

                var corners = fPart.BodySpaceCorners().ToList();
                corners.Add(fPart.LocalPosition);
                foreach (var point in corners)
                {
                    var pointTest = p.Contains(point); if (pointTest != ContainmentType.Disjoint)
                    {
                        return false;
                    }
                }

               
            }


            return true;
        }

       
    }
}
