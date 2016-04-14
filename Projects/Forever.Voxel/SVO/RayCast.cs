using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Forever.Voxel.SVO
{
    /* This is all based on blog code that I found and ported.  It came attached with the license so it is included as well.
     * 
     * http://codingshuttle.com/2014/03/implemnting-an-octree-raycasting-algorithm/
    */

    public static class RayCast
    {
        //public static uint GetFirstNode(Ray ray, OctTree<T> tree)
        //{
        //    return 0;
           // return GetFirstNode(ray.
        //}
        /*
        static uint GetFirstNode(float tx0, float ty0, float tz0, float txm, float tym, float tzm)
        {
            return 0;
        }
         * */

        static OctTreeNode<T> GetFirstNode<T>(OctTree<T> tree, Ray ray)
        {
            return null;
        }

        static OctTreeNode<T> GetNextNode<T>(OctTreeNode<T> node, Ray ray)
        {
            return null;
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