using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Aquarium.Life.Organs;
using Forever.Neural;
using Forever.Physics;

using Aquarium.Life.Signals;
using Aquarium.Life.Organs.OrganAbilities;

namespace Aquarium.Life.Bodies
{
    public class BodyPartGenerator
    {
        List<Func<BodyPart>> PartIndex { get; set; }
        public BodyPartGenerator()
        {
            PartIndex = ProduceLibraryOfParts();
        }


        public BodyPart NewPartFromIndex(int index)
        {
            var max = PartIndex.Count();

            return PartIndex[index % max]();
        }
      
        #region BodyPart strategies

        private  const float SCALE = 1f;

        public static BodyPart ChiefPart()
        {

            var part = new TestBodyPart();
            part.UCTransform = Matrix.CreateScale(SCALE * .5f);

            AddAllFaces(part);
            AddAllCorners(part);

            return part;
        }

        public static  BodyPart BarShape()
        {
            var part = new TestBodyPart();

            var scale = SCALE * 0.75f;
            part.UCTransform = Matrix.CreateScale(scale / 10f, scale * 2f, scale / 10f);

            part.Sockets.Add(new BodyPartSocket(part, Vector3.UnitY, Vector3.UnitY));
            part.Sockets.Add(new BodyPartSocket(part, -Vector3.UnitY, -Vector3.UnitY));

            return part;
        }


        public static  BodyPart FlatShape()
        {
            var part = new TestBodyPart();

            var scale = SCALE * 1f;
            part.UCTransform = Matrix.CreateScale(scale, scale / 10f, scale);
            var up = Vector3.Up;
            var down = Vector3.Down;

            var left = Vector3.Left;
            var right = Vector3.Right;

            part.Sockets.Add(new BodyPartSocket(part, left, left));
            part.Sockets.Add(new BodyPartSocket(part, right, right));
            
            return part;
        }

        public static  BodyPart BigBarShape()
        {
            var part = new TestBodyPart();

            var scale = SCALE * 2f;
            part.UCTransform = Matrix.CreateScale(scale / 10f, scale, scale / 10f);

            part.Sockets.Add(new BodyPartSocket(part, Vector3.UnitY, Vector3.UnitY));
            part.Sockets.Add(new BodyPartSocket(part, -Vector3.UnitY, -Vector3.UnitY));

            return part;
        }


        public static  BodyPart BigFlatShape()
        {
            var part = new TestBodyPart();

            var scale = SCALE * 2f;
            part.UCTransform = Matrix.CreateScale(scale, scale / 10f, scale);
            var up = Vector3.Up;
            var down = Vector3.Down;

            var left = Vector3.Left;
            var right = Vector3.Right;

            part.Sockets.Add(new BodyPartSocket(part, left, left));
            part.Sockets.Add(new BodyPartSocket(part, right, right));

            return part;
        }

        public static  BodyPart BoxWithEdges()
        {

            var part = new TestBodyPart();

            var scale = SCALE * 1f;
            part.UCTransform = Matrix.CreateScale(scale, scale, scale);
            var up = Vector3.Up;
            var down = Vector3.Down;

            var left = Vector3.Left;
            var right = Vector3.Right;


            var normal = up + left;
            part.Sockets.Add(new BodyPartSocket(part, up + left, normal));
            normal = up + right;
            part.Sockets.Add(new BodyPartSocket(part, up + right, normal));

            normal  = down + left;
            part.Sockets.Add(new BodyPartSocket(part, down + left, normal));
            normal = down + right;
            part.Sockets.Add(new BodyPartSocket(part, down + right, normal));

            left = Vector3.Forward;
            right = Vector3.Backward;

            normal = up + left;
            part.Sockets.Add(new BodyPartSocket(part, up + left, normal));
            normal = up + right;
            part.Sockets.Add(new BodyPartSocket(part, up + right, normal));

            normal = down + left;
            part.Sockets.Add(new BodyPartSocket(part, down + left, normal));
            normal = down + right;
            part.Sockets.Add(new BodyPartSocket(part, down + right, normal));


            return part;
        }


        public static  BodyPart OblongBlock()
        {
            var part = new TestBodyPart();

            var scale = SCALE * 0.75f;

            part.UCTransform = Matrix.CreateScale(scale / 4f, scale, scale / 4f);

            AddAllFaces(part);
            AddAllCorners(part);

            return part;
        }

        public static void AddAllFaces(TestBodyPart part)
        {
            part.Sockets.Add(new BodyPartSocket(part, Vector3.UnitX, Vector3.UnitX));
            part.Sockets.Add(new BodyPartSocket(part, -Vector3.UnitX, -Vector3.UnitX));

            part.Sockets.Add(new BodyPartSocket(part, Vector3.UnitY, Vector3.UnitY));
            part.Sockets.Add(new BodyPartSocket(part, -Vector3.UnitY, -Vector3.UnitY));

            part.Sockets.Add(new BodyPartSocket(part, Vector3.UnitZ, Vector3.UnitZ));
            part.Sockets.Add(new BodyPartSocket(part, -Vector3.UnitZ, -Vector3.UnitZ));
        }

        public static  void AddAllCorners(TestBodyPart part)
        {
            var corners = part.UnitCubeBB().GetCorners();
            foreach (var corner in corners)
            {
                var normal = corner;
                normal.Normalize();
                part.Sockets.Add(new BodyPartSocket(part, corner, normal));
            }
        }

        public static BodyPart BoxWithCorners()
        {
            var part = new TestBodyPart();
            var scale = SCALE * 0.5f;
            part.UCTransform = Matrix.Identity;

            AddAllCorners(part);
            part.UCTransform = Matrix.CreateScale(scale);
            return part;

        }

        private List<Func<BodyPart>> ProduceLibraryOfParts()
        {
            var lib = new List<Func<BodyPart>>();


            lib.Add(ChiefPart);
            
            lib.Add(BarShape);
            lib.Add(FlatShape);
            lib.Add(BigBarShape);
            lib.Add(BigFlatShape);
            
            
            lib.Add(BoxWithEdges);
            lib.Add(BoxWithCorners);
            
            lib.Add(OblongBlock);
            return lib;
        }
        #endregion



    }
}
