using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

namespace Forever.Physics.Collide
{
  public class CollisionData
  {
      public CollisionData()
      {
          contacts = new List<Contact>();
          contactsLeft = 1;
          restitution = 1f;
          friction = 0f;
      }

      public List<Contact> contacts { get; set; }
      public int contactsLeft { get; set; }
      public float restitution { get; set; }
      public float friction { get; set; }
      public float tolerance { get; set; }

  }

  public class CollisionDetector
  {



      public bool Intersecting(Primitive primOne, Primitive primTwo) 
      {
          return FindContacts(primOne, primTwo) > 0;
      }


      public int FindContacts(Primitive primOne, Primitive primTwo)
      {
          CollisionData data = new CollisionData();
          data.contactsLeft = 1;
          int numContacts = FindContacts(primOne, primTwo, data);
          return numContacts;
      }

      public int FindContacts(Primitive primOne, Primitive primTwo, CollisionData data)
      {
          return IntersectionTests.primAndPrim(primOne, primTwo, data);
      }


  }

}
