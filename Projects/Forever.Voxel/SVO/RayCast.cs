﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

using Forever.Extensions;

namespace Forever.Voxel.SVO
{
    /* This is all based on blog code that I found and ported.  It came attached with the license so it is included as well.
     * 
     * http://codingshuttle.com/2014/03/implemnting-an-octree-raycasting-algorithm/
    */

    static internal class RayCast
    {
        // Static memory access is slower. 
		// the paper suggested passing the possible exist nodes as function arguments on the stack which is faster
		static uint[,] NextNodeTable ;

        static RayCast ()
        {
            NextNodeTable = new uint[,]{
			    {4, 2, 1},
			    {5, 3, 8},
			    {6, 8, 3},
			    {7, 8, 8},
			    {8, 6, 5},
			    {8, 7, 8},
			    {8, 8, 7},
			    {8, 8, 8}
            };
        }

        static uint GetFirstNode( float tx0, float ty0, float tz0, float txm, float tym, float tzm)
	    {		

		    uint entryPlane = 0;
		    //Get entry plane

		    if( tx0 > ty0 ){
			    if (tx0 > tz0 )
			    {
				    if(tym < tx0)entryPlane |= 2;
				    if( tzm < tx0 )entryPlane |= 1;
				    return entryPlane;
			    }
		    }
		    else if( ty0 > tz0){
			    if( txm < ty0)entryPlane |= 4;
			    if( tzm < ty0 )entryPlane |= 1;
			    return entryPlane;
		    }

		    if( txm < tz0 ) entryPlane |= 4; 
		    if( tym < tz0 ) entryPlane |= 2;
             
		    return entryPlane; //returns the first node
        }


	    static uint GetNextNode(uint currentNode, float tx1, float ty1, float tz1)
	    {

		    //Get exit plane
		    float min; 
		    int minIndex = 0;
		    if( tx1 < ty1 ){
			    min = tx1;
			    minIndex = 0;
		    }
		    else {
			    minIndex = 1;
			    min = ty1;
		    }
		    if ( tz1 < min ){
			    minIndex = 2;
		    }
            
		    int exitPlane = minIndex;

		    return RayCast.NextNodeTable[currentNode, exitPlane];
	    }

        static IEnumerable<OctTreeNode<T>> ProcessSubNode<T>(
            OctTreeNode<T> node, Vector3 origin, Vector3 dir, 
            float tx0, float ty0, float tz0, 
            float tx1, float ty1, float tz1, 
            int level, uint a)
        {
            // if any of the maximum extents are hit behind us, then no branch intersection
            if(tx1 < 0 || ty1 < 0 || tz1 < 0) yield break;
                      
            if (node.IsLeaf)
            {
                yield return node;
            }
            else
            {
                float txM = 0.5f * (tx0 + tx1);
                    
                if (tx0 == float.NegativeInfinity && tx1 == float.PositiveInfinity)
                {
                    if (origin.X < (node.Box.Min.X + node.Box.Max.X) * 0.5f)
                    {
                        txM = float.PositiveInfinity;
                    }
                    else
                    {
                        txM = float.NegativeInfinity;
                    }
                }
                     
                float tyM = 0.5f * (ty0 + ty1);
              
                if (ty0 == float.NegativeInfinity && ty1 == float.PositiveInfinity)
                {
                    if (origin.Y < (node.Box.Min.Y + node.Box.Max.Y) * 0.5f)
                    {
                        tyM = float.PositiveInfinity;
                    }
                    else
                    {
                        tyM = float.NegativeInfinity;
                    }
                }
                    
                float tzM = 0.5f * (tz0 + tz1);
                   
                if (tz0 == float.NegativeInfinity && tz1 == float.PositiveInfinity)
                {
                    if (origin.Z < (node.Box.Min.Z + node.Box.Max.Z) * 0.5f)
                    {
                        tzM = float.PositiveInfinity;
                    }
                    else
                    {
                        tzM = float.NegativeInfinity;
                    }
                }
                    
                uint currentNode = GetFirstNode(tx0, ty0, tz0, txM, tyM, tzM);

                do
                {
                    switch (currentNode)
                    {
                        case 0:
                            foreach (var n in ProcessSubNode(node.Children[a], origin, dir, tx0, ty0, tz0, txM, tyM, tzM, level + 1, a))
                            {
                                yield return n;
                            }
                            currentNode = GetNextNode(currentNode, txM, tyM, tzM);
                            break;

                        case 1:
                            foreach (var n in ProcessSubNode(node.Children[a ^ 1], origin, dir, tx0, ty0, tzM, txM, tyM, tz1, level + 1, a))
                            {
                                yield return n;
                            }
                            currentNode = GetNextNode(currentNode, txM, tyM, tz1);
                            break;

                        case 2:
                            foreach (var n in ProcessSubNode(node.Children[a ^ 2], origin, dir, tx0, tyM, tz0, txM, ty1, tzM, level + 1, a))
                            {
                                yield return n;
                            }
                            currentNode = GetNextNode(currentNode, txM, ty1, tzM);
                            break;

                        case 3:
                            foreach (var n in ProcessSubNode(node.Children[a ^ 3], origin, dir, tx0, tyM, tzM, txM, ty1, tz1, level + 1, a))
                            {
                                yield return n;
                            }
                            currentNode = GetNextNode(currentNode, txM, ty1, tz1);
                            break;

                        case 4:
                            foreach (var n in ProcessSubNode(node.Children[a ^ 4], origin, dir, txM, ty0, tz0, tx1, tyM, tzM, level + 1, a))
                            {
                                yield return n;
                            }
                            currentNode = GetNextNode(currentNode, tx1, tyM, tzM);

                            break;

                        case 5:
                            foreach (var n in ProcessSubNode(node.Children[a ^ 5], origin, dir, txM, ty0, tzM, tx1, tyM, tz1, level + 1, a))
                            {
                                yield return n;
                            }
                            currentNode = GetNextNode(currentNode, tx1, tyM, tz1);

                            break;

                        case 6:
                            foreach (var n in ProcessSubNode(node.Children[a ^ 6], origin, dir, txM, tyM, tz0, tx1, ty1, tzM, level + 1, a))
                            {
                                yield return n;
                            }
                            currentNode = GetNextNode(currentNode, tx1, ty1, tzM);

                            break;

                        case 7:
                            foreach (var n in ProcessSubNode(node.Children[a ^ 7], origin, dir, txM, tyM, tzM, tx1, ty1, tz1, level + 1, a))
                            {
                                yield return n;
                            }
                            currentNode = 8;		//if we reach the far top right node then there are no nodes we can reach from here, given that our ray is always travelling in a positive direction.
                            break;
                    }

                } while (currentNode < 8);
            }
        }

                
	    public static IEnumerable<OctTreeNode<T>> RayTraverse<T>(OctTreeNode<T> root, Ray ray)
	    {
		    // Calculate the initiale ray parameters.
		    uint a=0; // flag for negative ray components.
            var halfsize = root.Box.GetHalfSize();
            var origin = ray.Position - root.Box.Min;
            var bounds = new BoundingBox(Vector3.Zero, halfsize * 2);
            var boxSize = bounds.Max;
            var dir = ray.Direction;

            if (dir.X < 0.0f)
            {
                origin.X = boxSize.X - origin.X;
                dir.X *= -1.0f;
            }
            else
            {
                // don't ask me
                a |= 4;
            }

            if (dir.Y < 0.0f)
            {
                origin.Y = boxSize.Y - origin.Y;
                a |= 2;
                dir.Y *= -1.0f;
            }

            if (dir.Z < 0.0f)
            {
                origin.Z = boxSize.Z - origin.Z;
                a |= 1;
                dir.Z *= -1.0f;
            }

		    float invDirx = 1.0f / dir.X;
		    float invDiry = 1.0f / dir.Y;
		    float invDirz = 1.0f / dir.Z;

		    float tx0 = ( bounds.Min.X - origin.X) * invDirx;
		    float tx1 = ( bounds.Max.X - origin.X) * invDirx;
		
		    float ty0 = ( bounds.Min.Y - origin.Y) * invDiry;
		    float ty1 = ( bounds.Max.Y - origin.Y) * invDiry;
		
		    float tz0 = ( bounds.Min.Z - origin.Z) * invDirz;
		    float tz1 = ( bounds.Max.Z - origin.Z) * invDirz;

		    float maxOf0 = Max( Max(tx0, ty0), tz0);
		    float minOf1 = Min( Min(tx1, ty1), tz1);

		    // Visiting current node, perform actual work here
            if (maxOf0 < minOf1)
            {
                foreach(var n in ProcessSubNode(root, ray.Position, dir, tx0, ty0, tz0, tx1, ty1, tz1, 0, a))
                {
                    yield return n;
                }
            }
	    }
        public static float Max(float f1, float f2)
        {
            return (float)Math.Max((float)f1, (float)f2);
        }
        public static float Min(float f1, float f2)
        {
            return (float)Math.Min((float)f1, (float)f2);
        }
    }
}


//The MIT License (MIT)
//
//Copyright (c) 2014 Mohammad Ghabboun
//
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in
//all copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//THE SOFTWARE.


/*
unsigned char a; //flags for the negative direction, used to transform the octree nodes using an XOR operation.

	unsigned int GetFirstNode( float tx0, float ty0, float tz0, float txm, float tym, float tzm, unsigned char rayFlags )
	{		

		unsigned int entryPlane = 0;
		//Get entry plane

		if( tx0 > ty0 ){
			if (tx0 > tz0 )
			{
				if(tym < tx0)entryPlane |= 2;
				if( tzm < tx0 )entryPlane |= 1;
				return entryPlane;
			}
		}
		else if( ty0 > tz0){
			if( txm < ty0)entryPlane |= 4;
			if( tzm < ty0 )entryPlane |= 1;
			return entryPlane;
		}


		if( txm < tz0 ) entryPlane |= 4; 
		if( tym < tz0 ) entryPlane |= 2;

	
		return entryPlane; //returns the first node
	}

	unsigned int GetNextNode( unsigned char currentNode, float tx1, float ty1, float tz1)
	{
		enum ExitPlane : unsigned char	{
			YZ = 0,
			XZ = 1,
			XY = 2,			
		};

		//Get exit plane
		float min; 
		int minIndex = 0;
		if( tx1 < ty1 ){
			min = tx1;
			minIndex = 0;
		}
		else {
			minIndex = 1;
			min = ty1;
		}
		if ( tz1 < min ){
			minIndex = 2;
		}

		unsigned char exitPlane = minIndex;

		// Static memory access is slower. 
		// the paper suggested passing the possible exist nodes as function arguments on the stack which is faster
		static unsigned char nextNodeTable[8][3] = 
		{
			{4, 2, 1},
			{5, 3, 8},
			{6, 8, 3},
			{7, 8, 8},
			{8, 6, 5},
			{8, 7, 8},
			{8, 8, 7},
			{8, 8, 8}
		};

		//look up next node 
		unsigned char next =  nextNodeTable[currentNode][exitPlane];
		return next;
	}

	void RayTraverse(OctreeNode* root, glm::vec3 origin, glm::vec3 dir, std::vector<OctreeNode*>&  nodes )
	{
		if( root == NULL )
			return; 

		// Calculate the initiale ray parameters.
		a=0; // flag for negative ray components.
		AxisAlignedBox bounds = root->m_bounds;
		glm::vec3 minimum = bounds.GetMinimum();
		glm::vec3 maximum = bounds.GetMaximum();
		glm::vec3 halfsize = bounds.GetHalfSize();
		origin = origin - bounds.GetCenter() + halfsize;
		bounds.SetMinimum( minimum-minimum);
		bounds.SetMaximum( maximum - minimum);
		glm::vec3 boxSize = root->m_bounds.GetSize();

		
		if (dir.x < 0.0f){
			origin.x = boxSize.x - origin.x;
			dir.x *= -1.0f;
			a |= 4;
        }

        if (dir.y < 0.0f){
			origin.y = boxSize.y - origin.y;
		    dir.y *= -1.0f;
			a |= 2;
        }

        if (dir.z < 0.0f){
			origin.z = boxSize.z - origin.z;

		    dir.z *= -1.0f;
			a |= 1;
        }

		float invDirx = 1.0f / dir.x;
		float invDiry = 1.0f / dir.y;
		float invDirz = 1.0f / dir.z;


		float tx0 = ( bounds.GetMinimum().x - origin.x) * invDirx;
		float tx1 = ( bounds.GetMaximum().x - origin.x) * invDirx;
		
		float ty0 = ( bounds.GetMinimum().y - origin.y) * invDiry;
		float ty1 = ( bounds.GetMaximum().y - origin.y) * invDiry;
		
		float tz0 = ( bounds.GetMinimum().z - origin.z) * invDirz;
		float tz1 = ( bounds.GetMaximum().z - origin.z) * invDirz;

		float maxOf0 = glm::max( glm::max(tx0, ty0), tz0 );
		float minOf1 = glm::min( glm::min(tx1, ty1), tz1);

		// Visiting current node, perform actual work here
		if ( maxOf0  < minOf1 ) 
			ProcessSubNode(root, tx0, ty0, tz0, tx1, ty1, tz1, nodes, 0 );
	}

	void ProcessSubNode( OctreeNode* node, float tx0, float ty0, float tz0, float tx1, float ty1, float tz1 , std::vector<OctreeNode*>&  nodes, int level )
	{
		if( node == NULL )
			return;

		if ( tx1 < 0 || ty1 < 0 || tz1 < 0 ) 
			return;

		if ( node->IsLeaf() )
		{	
			nodes.push_back( node );		
			return;
		}

		float txM = 0.5 * (tx0 + tx1); 
		float tyM = 0.5 * (ty0 + ty1); 
		float tzM = 0.5 * (tz0 + tz1);

		unsigned char currentNode = GetFirstNode( tx0, ty0, tz0, txM, tyM, tzM , a);

		do 
		{
			switch( currentNode )
			{
			case 0: ProcessSubNode( node->m_children[a],   tx0, ty0, tz0, txM, tyM, tzM, nodes, level+1 ); 
					currentNode = GetNextNode(currentNode, txM, tyM, tzM); 
				break; 

			case 1:	ProcessSubNode( node->m_children[a^1], tx0, ty0, tzM, txM, tyM, tz1, nodes, level+1 );
					currentNode = GetNextNode(currentNode, txM, tyM, tz1); 
				break;

			case 2:	ProcessSubNode(node->m_children[a^2], tx0, tyM, tz0, txM, ty1, tzM, nodes, level+1 ); 
					currentNode = GetNextNode(currentNode, txM, ty1, tzM ); 
				break;

			case 3:	ProcessSubNode( node->m_children[a^3], tx0, tyM, tzM, txM, ty1, tz1, nodes, level+1); 
					currentNode = GetNextNode(currentNode, txM, ty1, tz1 ); 
				break;

			case 4:	ProcessSubNode(node->m_children[a^4], txM, ty0, tz0, tx1, tyM, tzM, nodes, level+1 );
					currentNode = GetNextNode(currentNode, tx1, tyM, tzM ); 		

				break;

			case 5: ProcessSubNode(node->m_children[a^5],  txM, ty0, tzM, tx1, tyM, tz1, nodes, level+1 );
					currentNode = GetNextNode(currentNode, tx1, tyM, tz1 ); 				

				break;

			case 6:	ProcessSubNode(node->m_children[a^6], txM, tyM, tz0, tx1, ty1, tzM, nodes, level+1);
					currentNode = GetNextNode(currentNode, tx1, ty1, tzM ); 

				break;

			case 7: ProcessSubNode( node->m_children[a^7], txM, tyM, tzM, tx1, ty1, tz1, nodes, level+1);
					currentNode = 8;		//if we reach the far top right node then there are no nodes we can reach from here, given that our ray is always travelling in a positive direction.
				break;			
			}

		} while ( currentNode < 8 );
	}

*/