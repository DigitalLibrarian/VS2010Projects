using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

namespace Forever.Physics.Collide
{
    /// <summary>
    /// This type is here for debugging purposes only
    /// </summary>
    public enum ContactDiscoveryHint
    {
        None,
        BoxOnBox_Edge_Edge,
        BoxOnBox_Point_Face,
        
        BoxOnPlane_Corner
    }


  public class Contact
  {
      public ContactDiscoveryHint DiscoveryHint { get; set; }

    public Vector3 Point { get; set; }
    
    private Vector3 _privNormal = Vector3.Zero;

    public Vector3 Normal {
        get { return _privNormal; }
        set
        {
            Vector3 incoming = value;
            Debug.IsSaneNormal(incoming);
            _privNormal = incoming;
        }
    }
      
    public float Penetration { get; set; }
    public float Restitution { get; set; }
    public float Friction { get; set;  }
    public IRigidBody[] Bodies { get; set; }


    Matrix contactWorld;
    public Matrix ContactWorld { get { return contactWorld; } }

    Vector3 contactVelocity;
    public Vector3 ContactVelocity { 
        get { return contactVelocity;}
        set { contactVelocity = value; }
    }

    float desiredDeltaVelocity;
    public float DesiredDeltaVelocity { get { return desiredDeltaVelocity; } set { desiredDeltaVelocity = value; } }

    Vector3[] relativeContactPositions;
    public Vector3[] RelativeContactPositions { get { return relativeContactPositions; } }

    public Contact() 
    {
      Bodies = new IRigidBody[2];
      Restitution = 1f;
      Friction = 0f;
      DiscoveryHint = ContactDiscoveryHint.None;
    }

    public void ReCalc(float duration)
    {
        CalcContactBasis();

        relativeContactPositions = new Vector3[2];
        relativeContactPositions[0] = Point - Bodies[0].Position;

        contactVelocity = CalcLocalVelocity(0, duration);
        if (Bodies[1] != null)
        {
            relativeContactPositions[1] = Point - Bodies[1].Position;
            contactVelocity -= CalcLocalVelocity(1, duration);
        }

        CalcDesiredDeltaVelocity(duration);
    }

    private void CalcContactBasis()
    {
        Vector3[] contactTangent = new Vector3[2];
        float sWas;
        bool top = false;
        if (TrickyMath.Abs(Normal.X) > TrickyMath.Abs(Normal.Y))
        {
            float s = 1f / TrickyMath.Sqrt(Normal.Z * Normal.Z + Normal.X * Normal.X);
            sWas = s;
            top = true;
            contactTangent[0] = new Vector3(
                Normal.Z * s,
                0f, 
                -Normal.X * s

                );
            
            contactTangent[1] = new Vector3(
                Normal.Y * contactTangent[0].X,
                Normal.Z * contactTangent[0].X - Normal.X * contactTangent[0].Z,
                -Normal.Y * contactTangent[0].X
                );
        }
        else
        {
            float s = 1f / TrickyMath.Sqrt(Normal.Z * Normal.Z + Normal.Y * Normal.Y);
            sWas = s;
            contactTangent[0] = new Vector3(
                0f,
                -Normal.Z * s,
                Normal.Y * s
                );

            contactTangent[1] = new Vector3(
                Normal.Y * contactTangent[0].Z - Normal.Z * contactTangent[0].Y,
                -Normal.X * contactTangent[0].Z,
                Normal.X * contactTangent[0].Y
                );
        }
        
        contactWorld = new Matrix(
            Normal.X, contactTangent[0].X, contactTangent[1].X, 0f,
            Normal.Y, contactTangent[0].Y, contactTangent[1].Y, 0f,
            Normal.Z, contactTangent[0].Z, contactTangent[1].Z, 0f, 
            0f, 0f, 0f, 1f
            );
        contactWorld = Matrix.Transpose(contactWorld);
        Debug.Sanity(contactWorld);
        Debug.Sanity(contactTangent[0]);
        Debug.Sanity(contactTangent[1]);
    }

    public Vector3 CalcLocalVelocity(int bodyIndex, float duration)
    {
        IRigidBody body = Bodies[bodyIndex];
        Vector3 velocity = TrickyMath.VectorProduct(body.Rotation, relativeContactPositions[bodyIndex]);
        velocity += body.Velocity;

        Matrix inverseWorld = Matrix.Transpose(ContactWorld);
        Vector3 contactVelo = Vector3.Transform(velocity, inverseWorld);

        Vector3 accVelo = body.LastAccel * duration;
        accVelo = Vector3.Transform(accVelo, inverseWorld);

        accVelo = new Vector3(
            accVelo.X, 0f, 0f // only interested velocity in the direction of the contact normal in local space (forward on x)
           );
        contactVelo += accVelo;
        Debug.Sanity(contactVelo);
        return contactVelo;
    }

    
    public void CalcDesiredDeltaVelocity(float duration)
    {
        float veloLimit = 0.0001f;

        float veloFromAcc = 0f;

        if (Bodies[0].Awake)
        {
            veloFromAcc += (Bodies[0].LastAccel * duration * Normal).Length();
        }

        if (Bodies[1] != null && Bodies[1].Awake)
        {
            veloFromAcc -= (Bodies[1].LastAccel * duration * Normal).Length();
        }

        float effectRet = Restitution;

        if (TrickyMath.Abs(ContactVelocity.X) < veloLimit)
        {
            effectRet = 0f;
        }

        // Combine the bounce velocity with the removed
        // acceleration velocity.
        desiredDeltaVelocity =
            -veloFromAcc
            - ((1f + effectRet) * (ContactVelocity.X - veloFromAcc));

    }


    public void ApplyPositionChange(ref Vector3[] linearChange, ref Vector3[] angularChange, float penetration){
        float angularLimit = 0.025f;

        float[] angularMove = new float[2];
        float[] linearMove = new float[2];
        
        float totalInertia = 0f;
        float[] linearInertia = new float[2];
        float[] angularInertia = new float[2];
        
        for(int bodyIndex = 0; bodyIndex < 2; bodyIndex++)
        {
            IRigidBody body = Bodies[bodyIndex];
            if (body != null)
            {
                Matrix inverseInertiaTensor = body.InverseInertiaTensor;

                Vector3 angularInertiaWorld = TrickyMath.VectorProduct(RelativeContactPositions[bodyIndex], Point);
                angularInertiaWorld = Vector3.Transform(angularInertiaWorld, inverseInertiaTensor);
                angularInertiaWorld = TrickyMath.VectorProduct(angularInertiaWorld, RelativeContactPositions[bodyIndex]);
                angularInertia[bodyIndex] = TrickyMath.ScalarProduct(angularInertiaWorld, Point);

                linearInertia[bodyIndex] = body.InverseMass;
                totalInertia += linearInertia[bodyIndex] + angularInertia[bodyIndex];
            }
       }

        for(int bodyIndex = 0; bodyIndex < 2; bodyIndex++)
        {
            IRigidBody body = Bodies[bodyIndex];
            if (body != null)
            {
                float sign = bodyIndex == 0 ? 1f : -1f;
                angularMove[bodyIndex] = sign * penetration * (angularInertia[bodyIndex] / totalInertia);
                linearMove[bodyIndex] = sign * penetration * (linearInertia[bodyIndex] / totalInertia);


                Vector3 projection = RelativeContactPositions[bodyIndex];
                projection += TrickyMath.ScalarProduct(RelativeContactPositions[bodyIndex], Normal) * -Normal;

                float maxMag = angularLimit * projection.Length();
                if (angularMove[bodyIndex] < -maxMag)
                {
                    float totalMove = angularMove[bodyIndex] + linearMove[bodyIndex];
                    angularMove[bodyIndex] = -maxMag;
                    linearMove[bodyIndex] = totalMove - angularMove[bodyIndex];
                }
                else if (angularMove[bodyIndex] > maxMag)
                {
                    float totalMove = angularMove[bodyIndex] + linearMove[bodyIndex];
                    angularMove[bodyIndex] = maxMag;
                    linearMove[bodyIndex] = totalMove - angularMove[bodyIndex];
                }

                if (angularMove[bodyIndex] == 0)
                {
                    angularChange[bodyIndex] = Vector3.Zero;
                }
                else
                {
                    Vector3 targetAngularDirection = TrickyMath.VectorProduct(RelativeContactPositions[bodyIndex], Point);
                    Matrix inverseInertiaTensor = Matrix.Transpose(body.InertiaTensorWorld);
                    angularChange[bodyIndex] =
                        Vector3.Transform(targetAngularDirection, inverseInertiaTensor) * (angularMove[bodyIndex] / angularInertia[bodyIndex]);
                }

                linearChange[bodyIndex] = Normal * linearMove[bodyIndex];


                Vector3 pos = body.Position;
                Vector3 tran = linearMove[bodyIndex] * Normal;
                body.Position = pos + tran;

                Vector3 rotation = angularChange[bodyIndex] * Normal;

                
                Quaternion q = body.Orientation;
                q = TrickyMath.AddVector(q, rotation);
                body.Orientation = q;

                

                
                if (!body.Awake)
                {

                    // We need to calculate the derived data for any body that is
                    // asleep, so that the changes are reflected in the object's
                    // data. Otherwise the resolution will not change the position
                    // of the object, and the next collision detection round will
                    // have the same penetration.
                    RigidBody bodyB = body as RigidBody;
                    bodyB.calculateDerivedData();
                }
            }
        }

    }
    private Matrix getTensorFromBody(IRigidBody body)
    {
        return body.InverseInertiaTensorWorld;
    }

    public void ApplyVelocityChange(ref Vector3[] velocityChange, ref Vector3[] rotationChange)
    {
        Matrix[] inverseInertiaTensor = new Matrix[2];

        inverseInertiaTensor[0] = getTensorFromBody(Bodies[0]);
        if (Bodies[1] != null)
        {
            inverseInertiaTensor[1] = getTensorFromBody(Bodies[1]);
        }
        Vector3 impulseContactSpace;
        if (Friction == 0f)
        {
            impulseContactSpace = CalculateFrictionlessUnitImpulse(inverseInertiaTensor);
        }
        else
        {
            impulseContactSpace = CalculateFrictionUnitImpluse(inverseInertiaTensor);
        }

        Matrix contactSpaceToWorld = Matrix.Transpose(ContactWorld);

        Vector3 impulseWorld = Vector3.Transform(impulseContactSpace, ContactWorld);
        Vector3 impulsiveTorque = Vector3.Cross(this.RelativeContactPositions[0], impulseWorld);

        rotationChange[0] = Vector3.Transform(impulsiveTorque, inverseInertiaTensor[0]);
        velocityChange[0] = (impulseWorld * Bodies[0].InverseMass);
        
        Bodies[0].addVelocity(velocityChange[0]);
        Bodies[0].addRotation(rotationChange[0]);

        if (Bodies[1] != null)
        {
            impulsiveTorque = Vector3.Cross(impulseWorld, this.RelativeContactPositions[1]);

            rotationChange[1] = Vector3.Transform(impulsiveTorque, inverseInertiaTensor[1]);
            velocityChange[1] = impulseWorld * -Bodies[1].InverseMass;

            Bodies[1].addVelocity(velocityChange[1]);
            Bodies[1].addRotation(rotationChange[1]);

        }
    }



    private Vector3 CalculateFrictionlessUnitImpulse(Matrix[] inverseInertiaTensor)
    {
        Vector3 deltaVelWorld = TrickyMath.VectorProduct(RelativeContactPositions[0], Normal);
        deltaVelWorld = Vector3.Transform(deltaVelWorld, inverseInertiaTensor[0]);
        deltaVelWorld = TrickyMath.VectorProduct(deltaVelWorld, RelativeContactPositions[0]);

        float deltaVelocity = TrickyMath.ScalarProduct(deltaVelWorld, Normal);
        deltaVelocity += Bodies[0].InverseMass;

        if (Bodies[1] != null)
        {
            deltaVelWorld = TrickyMath.VectorProduct(RelativeContactPositions[1], Normal);;
            deltaVelWorld = Vector3.Transform(deltaVelWorld, inverseInertiaTensor[1]);
            deltaVelWorld = TrickyMath.VectorProduct(deltaVelWorld, RelativeContactPositions[1]);

            deltaVelocity += TrickyMath.ScalarProduct(deltaVelWorld, Normal);
            deltaVelocity += Bodies[1].InverseMass;
        }
        Vector3 result;
        if (deltaVelocity != 0)
        {
            result = new Vector3(desiredDeltaVelocity / deltaVelocity, 0f, 0f);
        }
        else
        {
            result = new Vector3(0f, 0f, 0f);
        }
        Debug.Sanity(result);
        return result;
        
    }


    private Vector3 CalculateFrictionUnitImpluse(Matrix[] inverseInertiaTensor)
    {
        float inverseMass = Bodies[0].InverseMass;

        Matrix impulseToTorque = TrickyMath.CreateSkewSymmetric(RelativeContactPositions[0]);

        Matrix deltaVelWorld = impulseToTorque;
        deltaVelWorld *= inverseInertiaTensor[0];
        deltaVelWorld *= impulseToTorque;
        deltaVelWorld *= -1f;

        if (Bodies[1] != null)
        {
            impulseToTorque = TrickyMath.CreateSkewSymmetric(RelativeContactPositions[1]);
            Matrix deltaVelWorld2 = impulseToTorque;
            deltaVelWorld2 *= inverseInertiaTensor[1];
            deltaVelWorld2 *= impulseToTorque;
            deltaVelWorld2 *= -1f;

            deltaVelWorld += deltaVelWorld2;
            inverseMass += Bodies[1].InverseMass;
        }


        Matrix deltaVelocity = Matrix.Transpose(ContactWorld);
        deltaVelocity *= deltaVelWorld;
        deltaVelocity *= ContactWorld;


        deltaVelocity = new Matrix(

            deltaVelocity.M11 + inverseMass, deltaVelocity.M12, deltaVelocity.M13, deltaVelocity.M14,

            deltaVelocity.M21, deltaVelocity.M22 + inverseMass, deltaVelocity.M23, deltaVelocity.M24,

            deltaVelocity.M31, deltaVelocity.M32, deltaVelocity.M33 + inverseMass, deltaVelocity.M34,

            deltaVelocity.M41, deltaVelocity.M42, deltaVelocity.M43, 1f

            );


        Matrix impulseMatrix = Matrix.Invert(deltaVelocity);


        Vector3 velKill = new Vector3(
            DesiredDeltaVelocity, -ContactVelocity.Y, -ContactVelocity.Z);

        Vector3 impulseContact = Vector3.Transform(velKill, impulseMatrix);

        float planarImpulse = TrickyMath.Sqrt(
            impulseContact.Y * impulseContact.Y + 
            impulseContact.Z * impulseContact.Z
            );

        
        //dynamic friction
        if (planarImpulse > impulseContact.X * Friction)
        {
            float x = impulseContact.X;
            float y = impulseContact.Y;
            float z = impulseContact.Z;

            y /= planarImpulse;
            z /= planarImpulse;

            x = deltaVelocity.M11 
                + deltaVelocity.M21 * Friction * y 
                + deltaVelocity.M31 * Friction * z;

            x = DesiredDeltaVelocity / x;
            y *= Friction * x ;
            z *= Friction * x ;

            impulseContact = new Vector3(x, y, z);


        }

        return impulseContact;
    }

    public void MatchAwakeState()
    {
        if (Bodies[1] == null)
        {
            return;
        }

        if (Bodies[0].Awake ^ Bodies[1].Awake)
        {
            if (!Bodies[0].Awake)
            {
                Bodies[0].Awake = true;
            }
            else
            {
                Bodies[1].Awake = true;
            }
        }
    }

  }
}
