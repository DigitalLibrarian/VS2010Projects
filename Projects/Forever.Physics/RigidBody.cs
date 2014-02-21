using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

using Forever.Physics;

namespace Forever.Physics
{
  public class RigidBody : IRigidBody
  {

      private const float SleepEpsilon = 0.0005f;
      private const double BaseOfMotionTrackingBias = 0.99f;

      #region Body Space Basis
      public Vector3 Forward
      {
          get
          {
              return Vector3.Transform(Vector3.Forward, Orientation);
          }
      }
      public Vector3 Up
      {
          get
          {
              return Vector3.Transform(Vector3.Up, Orientation);
          }
      }
      public Vector3 Right
      {
          get
          {
              return Vector3.Transform(Vector3.Right, Orientation);
          }
      }

      #endregion


      public string Label { get; set; }

    #region Cyclone privates

    float _inverse_mass;
    Matrix _inverse_inertia_tensor;

    float _linear_damping = 1f;
    float _angular_damping = 1f;

    Vector3 _position;
    Quaternion _orientation;
    Vector3 _velocity;
    //Vector3 angularMomentum;

    Matrix _inverse_inertia_tensor_world;

    public float Motion { get { return _motion; } }
    public float SmallestPositiveMotion { get; set; }
    float _motion;

    Matrix _transform_matrix;

    Vector3 _force_accum;
    Vector3 _torque_accum;

    Vector3 _acceleration;
    Vector3 _last_acceleration;

    bool _awake;

    #endregion

    #region X-or properties
    public float Mass
    {
      get
      {
        if (_inverse_mass == 0)
        {
          return float.MaxValue;
        }
        else
        {
          return 1f / _inverse_mass;
        }
      }

      set
      {
        float mass = value;
        if(mass == 0f){
          throw new Exception("Mass out of range.");
        }
        _inverse_mass = 1f / mass;
      }
    }


    public float InverseMass { get { return _inverse_mass; } set { _inverse_mass = value; } }
    public bool HasFiniteMass { get { return _inverse_mass > 0.0f; } }

    public Matrix InertiaTensor
    {
      get { return Matrix.Invert(_inverse_inertia_tensor); }
      set{
        
        Matrix IT = value;
        _inverse_inertia_tensor = Matrix.Invert(IT);
      }
    }

    public Matrix InertiaTensorWorld
    {
      get { return Matrix.Invert(_inverse_inertia_tensor_world);}
    }
    public Matrix InverseInertiaTensorWorld
    {
        get { return _inverse_inertia_tensor_world; }
    }
    public float LinearDamping { get { return _linear_damping; } set { _linear_damping = value; } }
    public float AngularDamping { get { return _angular_damping; } set { _angular_damping = value;} }


    public Vector3 Position { get { return _position; } set { _position = value; } }


    public Quaternion Orientation{
      get { return _orientation; } 
      set {
            _orientation = value;
            if (_orientation.Length() != 0)
            {
              _orientation.Normalize();
            }
        }
    }

    Vector3 _rotation;
    public Vector3 Rotation { get { return _rotation; } set { _rotation = value; } }
    public Vector3 Velocity { get { return _velocity; } set { _velocity = value; } }
    public Vector3 AngularMomentum { get { return _rotation * Mass; }  }
    public Vector3 LinearMomentum { get { return _velocity * Mass; } }

    public Vector3 LastAccel { get { return _last_acceleration; } }
     
    public Vector3 Acceler { get { return _acceleration; } set { _acceleration = value; } }

    public Matrix InverseInertiaTensor { get { return _inverse_inertia_tensor; } set { _inverse_inertia_tensor = value; } }

    public bool Awake { 
        get { 
            return _awake; 
        } 
        
        set {
            if (value)
            {
                _awake = true;
                primeMotion();
            }
            else 
            {
                _awake = false;
                Velocity = Vector3.Zero;
                _rotation = Vector3.Zero;
            }
        } 
    }

   

    public bool CanSleep { get; set; }

    public Matrix World { get { return _transform_matrix; } }


    private void primeMotion()
    {
        _motion = float.MaxValue;
    }


    public void addVelocity(Vector3 velo)
    {
        Debug.Sanity(velo);
        this._velocity += velo;
        
    }
    public void addRotation(Vector3 rot)
    {
        Debug.Sanity(rot);
        this._rotation += rot;
        
    }

#endregion


    public RigidBody(Vector3 pos)
    {
      this._awake = true;
      this.CanSleep = true;
      
      this.SmallestPositiveMotion = float.MaxValue;
      this._position = pos;
      this._velocity = Vector3.Zero;
      this._acceleration = Vector3.Zero;
      this._orientation = Quaternion.Identity;
      this._transform_matrix = Matrix.Identity;
      this._inverse_inertia_tensor_world = Matrix.Identity;
      this._inverse_inertia_tensor = Matrix.Identity;

      primeMotion();
      clearAccumulators();
      calculateDerivedData();
      
    }

    public void clearAccumulators()
    {
      _force_accum = Vector3.Zero;
      _torque_accum = Vector3.Zero;
    }

    public void calculateDerivedData() {

      Debug.Sanity(_orientation);
      Debug.Sanity(_transform_matrix);

      this._transform_matrix =  
          Matrix.CreateFromQuaternion(_orientation) * Matrix.CreateTranslation(this._position) ;

      Debug.Sanity(_transform_matrix);
        
      Matrix orientMatrix = Matrix.CreateFromQuaternion(_orientation);

      _inverse_inertia_tensor_world = 
          orientMatrix * _inverse_inertia_tensor * Matrix.Transpose(orientMatrix);
           


      Debug.Sanity(_inverse_inertia_tensor);
      Debug.Sanity(_inverse_inertia_tensor_world);
      Debug.Sanity(_orientation);
      Debug.Sanity(_transform_matrix);

    }


    public void integrate(float duration)
    {
        if (!_awake || !HasFiniteMass)
        {
            return;
        }

        // No, I'm not fucking kidding.
        Debug.Sanity(_last_acceleration);
        Debug.Sanity(_acceleration);
        Debug.Sanity(_force_accum);
        Debug.Sanity(_torque_accum);
        Debug.Sanity(_inverse_inertia_tensor_world);
        Debug.Sanity(_velocity);

        _last_acceleration = _acceleration;
        _acceleration = _force_accum * duration;

        Vector3 angularAccel = Vector3.Transform(_torque_accum, _inverse_inertia_tensor_world);


        _velocity += _acceleration * duration;
        _rotation += angularAccel * duration;
        _position += _velocity * duration;

        _velocity *= (float)Math.Pow(_linear_damping, duration);
        _rotation *= (float)Math.Pow(_angular_damping, duration);


        // Push the orientation quaternion ahead by the rotation (angular speed) vector
        Vector3 angularVelocity = _rotation * duration;

        Orientation = TrickyMath.AddVector(Orientation, angularVelocity);
        /*
        Quaternion spin = (new Quaternion(angularVelocity.X, angularVelocity.Y, angularVelocity.Z, 0f) * 0.5f);

        if (spin.Length() > 0)
        {
            Orientation += Quaternion.Concatenate(Orientation, spin);
        }
        
        Orientation.Normalize();
        */
        
        Debug.Sanity(_orientation);

        calculateDerivedData();
        clearAccumulators();

        trySleep(duration);
    }

    private void trySleep(float duration)
    {
        //return;
        if (CanSleep && Awake)
        {
            float currentMotion = TrickyMath.ScalarProduct(Velocity, Velocity) + TrickyMath.ScalarProduct(Rotation, Rotation);

            float bias = (float)Math.Pow(BaseOfMotionTrackingBias, (double)duration);
            float newMotion = bias * _motion + (1 - bias) * currentMotion;
            if (newMotion > 0 && newMotion < this.SmallestPositiveMotion)
            {
                this.SmallestPositiveMotion = newMotion;
            }

            if (Awake && newMotion < SleepEpsilon)
            {
                _awake = false;
                _motion = 0;
            }
            else if (newMotion > 10 * SleepEpsilon)
            {
                _motion = 10 * SleepEpsilon;
            }

            _motion = newMotion;


        }

    }

    public void addForce(Vector3 force)
    {

      _force_accum += force;
      _awake = true;
    }
  
    public void addForce(Vector3 force, Vector3 point)
    {
      Debug.Sanity(force);
      Debug.Sanity(point);
      Debug.Sanity(_transform_matrix);

      // Convert to coordinates relative to center of mass.
      Vector3 pt = point;
      pt -= _position;

        
      // you probably think i'm kidding...
      Debug.Sanity(_torque_accum);
      _torque_accum += Vector3.Cross(pt, force);
      Debug.Sanity(_torque_accum);

      this.addForce(force);
      
    }
    public void addTorque(Vector3 torque)
    {
      _torque_accum += torque;
      _awake = true;
    }


    public Vector3 CenterOfMass { get { return _position; } }
    public void Translate(Vector3 trans)
    {
      _position += trans;
    }

 
  }
}
