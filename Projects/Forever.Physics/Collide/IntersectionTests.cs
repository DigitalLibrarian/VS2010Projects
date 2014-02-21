using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace Forever.Physics.Collide
{
    // Needs some work...
    public static class IntersectionTests
    {

        //dispatch
        public static int primAndPrim(Primitive one, Primitive two, CollisionData data)
        {

            if (one.PrimType == CollideType.Sphere
             && two.PrimType == CollideType.Sphere)
            {
                return sphereAndSphere((Sphere)one, (Sphere)two, data);
            }



            if (one.PrimType == CollideType.Sphere
             && two.PrimType == CollideType.Plane)
            {
                return sphereAndHalfSpace((Sphere)one, (Plane)two, data);
            }

            if (one.PrimType == CollideType.Plane
             && two.PrimType == CollideType.Sphere)
            {
                return sphereAndHalfSpace((Sphere)two, (Plane)one, data);
            }

            if (one.PrimType == CollideType.Box
             && two.PrimType == CollideType.Sphere)
            {
                return boxAndSphere((Box)one, (Sphere)two, data);
            }


            if (one.PrimType == CollideType.Sphere
             && two.PrimType == CollideType.Box)
            {
                return boxAndSphere((Box)two, (Sphere)one, data);
            }


            if (one.PrimType == CollideType.Box
             && two.PrimType == CollideType.Plane)
            {
                return boxAndPlane((Box)one, (Plane)two, data);
            }


            if (one.PrimType == CollideType.Plane
             && two.PrimType == CollideType.Box)
            {
                return boxAndPlane((Box)two, (Plane)one, data);
            }

            if (one.PrimType == CollideType.Box && two.PrimType == CollideType.Box)
            {
                return boxAndBox((Box)one, (Box)two, data);
            }

            throw new Exception("I don't know wtf that is!");
        }

        //TODO - write unit tests!!!!!!



        public static int boxAndPoint(Box box, Vector3 pointWorld, CollisionData data)
        {

            Vector3 pointLocal = Vector3.Transform(pointWorld, Matrix.Invert(box.Body.World * box.OffsetMatrix));

            Vector3 halfSizes = box.HalfSizes;

            Vector3 absPoint = new Vector3(
                TrickyMath.Abs(pointLocal.X),
                TrickyMath.Abs(pointLocal.Y),
                TrickyMath.Abs(pointLocal.Z)
                );

            Vector3 check = halfSizes - absPoint;

            if (check.X < 0 || check.Y < 0 || check.Z < 0)
            {
                return 0;
            }

            Vector3 depth = new Vector3(
                TrickyMath.Abs(absPoint.X - halfSizes.X),
                TrickyMath.Abs(absPoint.Y - halfSizes.Y),
                TrickyMath.Abs(absPoint.Z - halfSizes.Z)
                );

            depth = new Vector3(
                depth.X != 0f ? depth.X : float.MaxValue,
                depth.Y != 0f ? depth.Y : float.MaxValue,
                depth.Z != 0f ? depth.Z : float.MaxValue

                );

            if (depth.X <= depth.Y && depth.X <= depth.Z)
            {
                Contact contact = new Contact();
                contact.DiscoveryHint = ContactDiscoveryHint.BoxOnBox_Point_Face;
                Vector3 boxAxis = box.getAxis(0);
                contact.Normal = boxAxis * (pointLocal.X > 0 ? -1f : 1f);
                contact.Point = pointWorld;
                contact.Penetration = TrickyMath.Abs(absPoint.X - halfSizes.X);

                contact.Bodies[0] = box.Body;
                contact.Bodies[1] = null;

                contact.Restitution = data.restitution;
                contact.Friction = data.friction;
                data.contacts.Add(contact);
                return 1;

            }
            else if (depth.Y <= depth.X && depth.Y <= depth.Z)
            {

                Contact contact = new Contact();
                contact.DiscoveryHint = ContactDiscoveryHint.BoxOnBox_Point_Face;
                Vector3 boxAxis = box.getAxis(1);
                contact.Normal = boxAxis * (pointLocal.Y > 0 ? -1f : 1f);
                contact.Point = pointWorld;
                contact.Penetration = TrickyMath.Abs(absPoint.Y - halfSizes.Y);

                contact.Bodies[0] = box.Body;
                contact.Bodies[1] = null;

                contact.Restitution = data.restitution;
                contact.Friction = data.friction;
                data.contacts.Add(contact);
                return 1;
            }
            else if (depth.Z <= depth.X && depth.Z <= depth.Y)
            {

                Contact contact = new Contact();
                contact.DiscoveryHint = ContactDiscoveryHint.BoxOnBox_Point_Face;
                Vector3 boxAxis = box.getAxis(2);
                contact.Normal = boxAxis * (pointLocal.Z > 0 ? -1f : 1f);
                contact.Point = pointWorld;
                contact.Penetration = TrickyMath.Abs(absPoint.Z - halfSizes.Z);

                contact.Bodies[0] = box.Body;
                contact.Bodies[1] = null;

                contact.Restitution = data.restitution;
                contact.Friction = data.friction;
                data.contacts.Add(contact);
                return 1;
            }
            else
            {
                return 0;
            }


           

        }


        public static Contact checkEdgeAndEdge(
            Box left, Vector3 leftEdgeOne, Vector3 leftEdgeTwo,
            Box right, Vector3 rightEdgeOne, Vector3 rightEdgeTwo, float restitution, float friction)
        {

            return null;
            CollisionData pointData = new CollisionData();
            pointData.restitution = restitution;
            pointData.friction = friction;

            int pointContacts = IntersectionTests.edgeAndEdge(left, leftEdgeOne, leftEdgeTwo, right, rightEdgeOne, rightEdgeTwo, pointData);


            if (pointContacts > 0)
            {
                return pointData.contacts[0];
            }
            return null;
        }


        public static int edgeAndEdge(
            Box left, Vector3 leftEdgeOne, Vector3 leftEdgeTwo,
            Box right, Vector3 rightEdgeOne, Vector3 rightEdgeTwo,
            CollisionData data)
        {
            //return 0; //TODO
            
            Vector3 leftCenter = left.Body.Position;
            Vector3 rightCenter = right.Body.Position;
            Vector3 midLineRight = rightCenter - leftCenter;

            float distCenters = midLineRight.Length();

            Vector3 projectedLeftEdge = TrickyMath.Project(leftEdgeOne - leftCenter, midLineRight);
            float distLeftCenterToEdge = projectedLeftEdge.Length();
            Vector3 projectedRightEdge = TrickyMath.Project(rightEdgeOne - rightCenter, -midLineRight);
            float distRightCenterToEdge = projectedRightEdge.Length();

            

            if (distCenters > (distLeftCenterToEdge + distRightCenterToEdge))
            {
                return 0;
            }


            float distEdges = (distLeftCenterToEdge + distRightCenterToEdge) - distCenters;
            Contact contact = new Contact();
            contact.DiscoveryHint = ContactDiscoveryHint.BoxOnBox_Edge_Edge;
            contact.Restitution = data.restitution;
            contact.Friction = data.friction;
            contact.Bodies[0] = left.Body;
            contact.Bodies[1] = right.Body;
            contact.Penetration = (distLeftCenterToEdge + distRightCenterToEdge) - distCenters;
            contact.Point = leftCenter + (midLineRight * 0.5f);

            Vector3 x = leftEdgeOne - leftEdgeTwo; ;
            x.Normalize();
            Vector3 y, z;
            TrickyMath.MakeOrthonormalBasis(x, out y, out z);
            Vector3 realY = y;


            Vector3 normal = TrickyMath.VectorProduct(x, realY);
            normal.Normalize();
            contact.Normal = normal;
            data.contacts.Add(contact);
            return 1;


        }

        private static float transformToAxis(Box box, Vector3 axis)
        {
            return box.HalfSizes.X * TrickyMath.Abs(Vector3.Dot(axis, box.getAxis(0)))
                + box.HalfSizes.Y * TrickyMath.Abs(Vector3.Dot(axis, box.getAxis(1)))
                + box.HalfSizes.Z * TrickyMath.Abs(Vector3.Dot(axis, box.getAxis(2)));

        }

        private static float penetrationOnAxis(Box one, Box two, Vector3 axis, Vector3 toCenter)
        {
            float oneProject = transformToAxis(one, axis);
            float twoProject = transformToAxis(two, axis);

            float dist = TrickyMath.Abs(TrickyMath.ScalarProduct(toCenter, axis));
            return oneProject + twoProject - dist;
        }

        private static bool tryAxis(Box one, Box two, Vector3 axis, Vector3 toCenter,
            int index, ref float smallestPenetration, ref int smallestCase)
        {
            if (axis.LengthSquared() < 0.0001f) return true;

            axis.Normalize();

            float penetration = penetrationOnAxis(one, two, axis, toCenter);

            if (penetration < 0)
            {
                return false;
            }
            else if (penetration < smallestPenetration)
            {
                smallestPenetration = penetration;
                smallestCase = index;
            }
            return true;
        }


        public static int boxAndBox(Box one, Box two, CollisionData data)
        {
            Vector3 toCenter = two.Body.Position - one.Body.Position;

            float pen = float.MaxValue;
            int best = int.MaxValue;

            if (!tryAxis(one, two, one.getAxis(0), toCenter, 0, ref pen, ref best)) return 0;
            if (!tryAxis(one, two, one.getAxis(1), toCenter, 1, ref pen, ref best)) return 0;
            if (!tryAxis(one, two, one.getAxis(2), toCenter, 2, ref pen, ref best)) return 0;

            if (!tryAxis(one, two, one.getAxis(0), toCenter, 3, ref pen, ref best)) return 0;
            if (!tryAxis(one, two, one.getAxis(1), toCenter, 4, ref pen, ref best)) return 0;
            if (!tryAxis(one, two, one.getAxis(2), toCenter, 5, ref pen, ref best)) return 0;

            int bestSingleAxis = best;

            if (!tryAxis(one, two, TrickyMath.VectorProduct(one.getAxis(0), two.getAxis(0)), toCenter, 6, ref pen, ref best)) return 0;
            if (!tryAxis(one, two, TrickyMath.VectorProduct(one.getAxis(0), two.getAxis(1)), toCenter, 7, ref pen, ref best)) return 0;
            if (!tryAxis(one, two, TrickyMath.VectorProduct(one.getAxis(0), two.getAxis(2)), toCenter, 8, ref pen, ref best)) return 0;
            if (!tryAxis(one, two, TrickyMath.VectorProduct(one.getAxis(1), two.getAxis(0)), toCenter, 9, ref pen, ref best)) return 0;
            if (!tryAxis(one, two, TrickyMath.VectorProduct(one.getAxis(1), two.getAxis(1)), toCenter, 10, ref pen, ref best)) return 0;
            if (!tryAxis(one, two, TrickyMath.VectorProduct(one.getAxis(1), two.getAxis(2)), toCenter, 11, ref pen, ref best)) return 0;
            if (!tryAxis(one, two, TrickyMath.VectorProduct(one.getAxis(2), two.getAxis(0)), toCenter, 12, ref pen, ref best)) return 0;
            if (!tryAxis(one, two, TrickyMath.VectorProduct(one.getAxis(2), two.getAxis(1)), toCenter, 13, ref pen, ref best)) return 0;
            if (!tryAxis(one, two, TrickyMath.VectorProduct(one.getAxis(2), two.getAxis(2)), toCenter, 14, ref pen, ref best)) return 0;



            if (best == int.MaxValue)
            {
                throw new Exception("Found no good axis"); 
            }

            if (best < 3)
            {
                fillPointFaceBoxBox(one, two, toCenter, data, best, pen);

                return 1;
            }
            else if (best < 6)
            {

                
                // We've got a vertex of box one on a face of box two.
                // We use the same algorithm as above, but swap around
                // one and two (and therefore also the vector between their
                // centres).
                fillPointFaceBoxBox(two, one, toCenter * -1.0f, data, best - 3, pen);

                return 1;
            }
            else
            {
                best -= 6; //move this back to talk about a cardinal axis
                int oneAxisIndex = best / 3;
                int twoAxisIndex = best % 3;
                Vector3 oneAxis = one.getAxis(oneAxisIndex);
                Vector3 twoAxis = two.getAxis(twoAxisIndex);
                Vector3 axis = TrickyMath.VectorProduct(oneAxis, twoAxis);
                axis.Normalize();
                if (Vector3.Dot(axis, toCenter) > 0)
                {
                    axis *= -1f;
                }

                float[] ptOnOneEdge = TrickyMath.Vector3ToFloatArray(one.HalfSizes);
                float[] ptOnTwoEdge = TrickyMath.Vector3ToFloatArray(two.HalfSizes);

                for (int i = 0; i < 3; i++)
                {
                    if (i == oneAxisIndex)
                    {
                        ptOnOneEdge[i] = 0f;
                    }
                    else if (Vector3.Dot(one.getAxis(i), axis) > 0)
                    {
                        ptOnOneEdge[i] *= -1f;
                    }

                    if (i == twoAxisIndex)
                    {
                        ptOnTwoEdge[i] = 0;
                    }
                    else if (Vector3.Dot(two.getAxis(i), axis) < 0)
                    {
                        ptOnTwoEdge[i] *= -1f;
                    }
                }

                Vector3 vertexOnOneEdge = Vector3.Transform(
                                               TrickyMath.FloatArrayToVector3(ptOnOneEdge),
                                               one.Transform
                                               );

                Vector3 vertexOnTwoEdge = Vector3.Transform(
                                               TrickyMath.FloatArrayToVector3(ptOnTwoEdge),
                                               two.Transform
                                               );

                float[] oneHalfSizes = TrickyMath.Vector3ToFloatArray(one.HalfSizes);
                float[] twoHalfSizes = TrickyMath.Vector3ToFloatArray(two.HalfSizes);
                Vector3 vertex = contactPoint(
                        vertexOnOneEdge, oneAxis, oneHalfSizes[oneAxisIndex],
                        vertexOnTwoEdge, twoAxis, twoHalfSizes[twoAxisIndex],
                        bestSingleAxis > 2
                    );

                Contact contact = new Contact();
                contact.DiscoveryHint = ContactDiscoveryHint.BoxOnBox_Edge_Edge;
                contact.Penetration = pen;
                contact.Normal = axis;
                contact.Point = vertex;
                contact.Bodies[0] = one.Body;
                contact.Bodies[1] = two.Body;
                contact.Restitution = data.restitution;
                contact.Friction = data.friction;


                data.contacts.Add(contact);


                return 1;
            }


            return 0;

        }

        private static Vector3 contactPoint(Vector3 pOne, Vector3 dOne, float oneSize, Vector3 pTwo, Vector3 dTwo, float twoSize, bool useOne){

            Vector3 toSt, cOne, cTwo;
            float dpStaOne, dpStaTwo, dpOneTwo, smOne, smTwo;
            float denom, mua, mub;

            smOne = dOne.LengthSquared();
            smTwo = dTwo.LengthSquared();
            dpOneTwo = Vector3.Dot(dTwo, dOne);


            toSt = pOne - pTwo;
            dpStaOne = Vector3.Dot(dOne, toSt);
            dpStaTwo = Vector3.Dot(dTwo, toSt);

            denom = smOne * smTwo - dpOneTwo * dpOneTwo;

            
            // Zero denominator indicates parrallel lines
            if (TrickyMath.Abs(denom) < 0.0001f) {
                return useOne?pOne:pTwo;
            }

            mua = (dpOneTwo * dpStaTwo - smTwo * dpStaOne) / denom;
            mub = (smOne * dpStaTwo - dpOneTwo * dpStaOne) / denom;


            if (mua > oneSize ||
                mua < -oneSize ||
                mub > twoSize ||
                mub < -twoSize)
            {
                return useOne?pOne:pTwo;
            }
            else
            {
                cOne = pOne + dOne * mua;
                cTwo = pTwo + dTwo * mub;

                return cOne * 0.5f + cTwo * 0.5f;
            }
        }

        private static void fillPointFaceBoxBox(Box one, Box two, Vector3 toCenter, CollisionData data, int best, float penetration)
        {
            Contact contact = new Contact();
            Vector3 normal = one.getAxis(best);
            if (Vector3.Dot(normal, toCenter) > 0)
            {
                normal *= -1f;
            }


            Vector3 vertex = two.HalfSizes;
            float oneDot = Vector3.Dot(two.getAxis(0), normal);
            if (oneDot < 0)
            {
                vertex *= new Vector3(-1f, 0f, 0f);
            }

            float twoDot = Vector3.Dot(two.getAxis(1), normal);
            if (twoDot < 0)
            {
                vertex *= new Vector3(0f, -1f, 0f);
            }

            float threeDot = Vector3.Dot(two.getAxis(2), normal);
            if (threeDot < 0)
            {
                vertex *= new Vector3(0f, 0f, -1f);
            }
            contact.DiscoveryHint = ContactDiscoveryHint.BoxOnBox_Point_Face;
            contact.Normal = normal;
            contact.Penetration = penetration;
            contact.Point = Vector3.Transform(vertex, two.Transform);
            contact.Bodies[0] = one.Body;
            contact.Bodies[1] = two.Body;
            contact.Restitution = data.restitution;
            contact.Friction = data.friction;

            data.contacts.Add(contact);
            
        }


        public static int boxAndBoxBetter(Box left, Box right, CollisionData data)
        {
            Vector3[] leftWorldVerts = left.WorldVerts();
            Vector3[] rightWorldVerts = right.WorldVerts();



            foreach (Vector3 leftWorldVert in leftWorldVerts)
            {

                Contact contact = IntersectionTests.checkBoxAndPoint(right, leftWorldVert, data.restitution, data.friction);
                if (contact != null)
                {
                    contact.Bodies[1] = left.Body;
                    data.contacts.Add(contact);
                }
         
            }
            
            if (data.contacts.Count > 0)
            {
                return data.contacts.Count;
            }
             

            
            foreach (Vector3 rightWorldVert in rightWorldVerts)
            {
                Contact contact = IntersectionTests.checkBoxAndPoint(left, rightWorldVert, data.restitution, data.friction);
                if (contact != null)
                {
                    contact.Bodies[1] = right.Body;
                    data.contacts.Add(contact);


                }
               
            }
            
            // If any point/face contacts get precedent over edge/edge
            if (data.contacts.Count > 0)
            {
                return data.contacts.Count;
            }
            

            List<Vector3[]> leftEdges = left.WorldEdges();
            List<Vector3[]> rightEdges = right.WorldEdges();
            foreach (Vector3[] leftEdge in leftEdges)
            {
                foreach (Vector3[] rightEdge in rightEdges)
                {
                    Contact contact = checkEdgeAndEdge(left, leftEdge[0], leftEdge[1], right, rightEdge[0], rightEdge[1], data.restitution, data.friction);

                    if (contact != null)
                    {
                        data.contacts.Add(contact);
                    }
                }

            }



            return data.contacts.Count;
        }

        private static Contact checkBoxAndPoint(Box box, Vector3 point, float restitution, float friction)
        {

            CollisionData pointData = new CollisionData();
            pointData.restitution = restitution;
            pointData.friction = friction;

            int pointContacts = IntersectionTests.boxAndPoint(box, point, pointData);


            if (pointContacts > 0)
            {
                return pointData.contacts[0];
            }
            return null;
        }

        public static int sphereAndSphere(Sphere one, Sphere two, CollisionData data)
        {
            //make sure we have contacts
            if (data.contactsLeft <= 0) return 0;

            // Cache the sphere positions
            Vector3 positionOne = one.Body.Position;
            Vector3 positionTwo = two.Body.Position;


            if (positionOne == positionTwo)
            {
                throw new ArgumentException("Two bodies with the same position are unsupported....TODO");
            }

            // find the vector between the objects
            Vector3 midline = positionOne - positionTwo;
            float size = midline.Length();

            //see if it is large enough
            if (size <= 0.0f || size > one.Radius + two.Radius)
            {
                return 0;
            }

            // we manually create the normal, because we have the size at hand;
            Vector3 normal = midline * (1f / size);

            Contact contact = new Contact();
            contact.Normal = normal;
            contact.Point = positionTwo + midline * 0.5f;
            contact.Penetration = (one.Radius + two.Radius - size);

            //write the appropriate data
            contact.Bodies[0] = one.Body;
            contact.Bodies[1] = two.Body;
            contact.Restitution = data.restitution;
            contact.Friction = data.friction;
            data.contacts.Add(contact);

            return 1;
        }


        public static int sphereAndHalfSpace(Sphere sphere, Plane plane, CollisionData data)
        {
            
            Vector3 pos = sphere.Body.Position;
            

            //d = p * L - l
            float ballDist = Vector3.Dot(plane.Normal, pos) - (sphere.Radius - plane.Offset);

            if (ballDist > 0) return 0;

            Contact contact = new Contact();
            contact.Normal = plane.Normal;
            contact.Penetration = -ballDist;
            contact.Point = pos - (plane.Normal * (ballDist + sphere.Radius));

            contact.Bodies[0] = sphere.Body;
            contact.Bodies[1] = null;

            contact.Restitution = data.restitution;
            contact.Friction = data.friction;

            data.contacts.Add(contact);

            return 1;
        }

        /// <summary>
        /// danger! This works for ContactDemo, but there is one hang up.  The call to Math.Abs() should not
        /// be needed.  It isn't in the orange book text.  However, we only get negative numbers from that Dot.
        /// </summary>
        /// <param name="box"></param>
        /// <param name="plane"></param>
        /// <param name="data"></param>
        /// <returns></returns>

        public static int boxAndPlane(Box box, Plane plane, CollisionData data)
        {
            if (data.contactsLeft <= 0) return 0;

            int contacts_found = 0;

            // p dot L < l

            Vector3[] verts = box.WorldVerts();
            foreach (Vector3 vert in verts)
            {
                //float vertexDist = (float)Math.Abs( (double)Vector3.Dot(plane.Normal, vert));
                float vertexDist = Vector3.Dot(-plane.Normal, vert);

                if (vertexDist >= plane.Offset + data.tolerance)
                {
                    // the contact point is halfway between the vertex and the plane
                    // we multiply the direction by half the spearation distance
                    // and add the vertex location
                    Contact contact = new Contact();
                    contact.DiscoveryHint = ContactDiscoveryHint.BoxOnPlane_Corner;
                    contact.Bodies[0] = box.Body;
                    contact.Bodies[1] = null;
                    contact.Point = plane.ClosestPoint(vert);
                    contact.Normal = plane.Normal;
                    contact.Penetration = vertexDist - plane.Offset;//TrickyMathHelper.Abs(plane.Offset - vertexDist);
                    contact.Restitution = data.restitution;
                    contact.Friction = data.friction;
                    data.contacts.Add(contact);
                    contacts_found++;
                }
            }

            return contacts_found;
        }


        public static int boxAndSphere(Box box, Sphere sph, CollisionData data)
        {
            Vector3 center = sph.Body.Position;

            Vector3 relCenter = Vector3.Transform(center, Matrix.Invert(box.Transform));

            double x = relCenter.X;
            double y = relCenter.Y;
            double z = relCenter.Z;

            if (Math.Abs(x) - sph.Radius > box.HalfSizes.X
              || Math.Abs(y) - sph.Radius > box.HalfSizes.Y
              || Math.Abs(z) - sph.Radius > box.HalfSizes.Z)
            {
                return 0;
            }

            Vector3 closest = Vector3.Zero;
            float dist;

            dist = relCenter.X;
            if (dist > box.HalfSizes.X) dist = box.HalfSizes.X;
            if (dist < -box.HalfSizes.X) dist = -box.HalfSizes.X;
            closest.X = dist;


            dist = relCenter.Y;
            if (dist > box.HalfSizes.Y) dist = box.HalfSizes.Y;
            if (dist < -box.HalfSizes.Y) dist = -box.HalfSizes.Y;
            closest.Y = dist;


            dist = relCenter.Z;
            if (dist > box.HalfSizes.Z) dist = box.HalfSizes.Z;
            if (dist < -box.HalfSizes.Z) dist = -box.HalfSizes.Z;
            closest.Z = dist;


            dist = (closest - relCenter).Length();
            
            if (dist > sph.Radius && !TrickyMath.AlmostEquals(0f, dist - sph.Radius)) return 0;


            Vector3 closestWorld = Vector3.Transform(closest, box.Transform);

            Contact contact = new Contact();
            Vector3 normal = closestWorld - center;// (center - closestWorld);
            normal.Normalize();
            contact.Normal = normal;
            Debug.Sanity(contact.Normal);

            contact.Point = closestWorld;
            contact.Penetration = sph.Radius - dist;// (float)Math.Sqrt((float)dist);

            contact.Bodies[0] = box.Body;
            contact.Bodies[1] = sph.Body;
            contact.Restitution = data.restitution;
            contact.Friction = data.friction;

            data.contacts.Add(contact);

            return 1;
        }
    }
}
