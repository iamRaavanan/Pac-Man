using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Raavanan
{
    public class Node
    {
        #region Variables
        public Node                 _Parent;
        public int                  _x;
        public int                  _y;
        public Vector3              _WorldPos;

        public int                  _gCost;
        public int                  _hCost;
        public int                  _fCost
        {
            get
            {
                return _gCost + _hCost;
            }
        }
        #endregion

        public Node(int pX, int pY, Vector3 pWorldPos)
        {
            _x = pX;
            _y = pY;
            _WorldPos = pWorldPos;
        }
    }
}