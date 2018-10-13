using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Raavanan
{
    public class PathFinding : MonoBehaviour
    {
        public static PathFinding _instace;

        private void Awake()
        {
            _instace = this;
        }

        #region A* PathFinding
        public List<Node> FindPath (Node pStartNode, Node pTargetNode)
        {
            List<Node> InOpenSet = new List<Node>();
            HashSet<Node> InClosedSet = new HashSet<Node>();
            InOpenSet.Add(pStartNode);
            while (InOpenSet.Count > 0)
            {
                Node InCurrentNode = InOpenSet[0];
                for (int i = 1; i < InOpenSet.Count; i++)
                {
                    if (InOpenSet[i]._fCost < InCurrentNode._fCost || InOpenSet[i]._fCost == InCurrentNode._fCost && InOpenSet[i]._hCost < InCurrentNode._hCost)
                    {
                        InCurrentNode = InOpenSet[i];
                    }
                }
                InOpenSet.Remove(InCurrentNode);
                InClosedSet.Add(InCurrentNode);
                if (InCurrentNode == pTargetNode)
                {
                    return RetracePath(pStartNode, pTargetNode);
                }

                foreach (Node neighbour in GameManager._Instance.GetNeighbourNods (InCurrentNode))
                {
                    if (InClosedSet.Contains(neighbour))
                        continue;

                    int InNewCost = InCurrentNode._gCost + GetDistance(InCurrentNode, neighbour);
                    if (InNewCost < neighbour._gCost || !InOpenSet.Contains (neighbour))
                    {
                        neighbour._gCost = InNewCost;
                        neighbour._hCost = GetDistance(neighbour, pTargetNode);
                        neighbour._Parent = InCurrentNode;

                        if (!InOpenSet.Contains (neighbour))
                        {
                            InOpenSet.Add(neighbour);
                        }
                    }
                }
            }
            return null;
        }

        private List<Node> RetracePath (Node pStartNode, Node pEndNode)
        {
            List<Node> InNodePath = new List<Node>();
            Node InCurrentNode = pEndNode;
            while (InCurrentNode != pStartNode)
            {
                InNodePath.Add(InCurrentNode);
                InCurrentNode = InCurrentNode._Parent;
            }
            InNodePath.Reverse();
            return InNodePath;
        }

        private int GetDistance (Node pA, Node pB)
        {
            int InDstX = Mathf.Abs(pA._x - pB._x);
            int InDstY = Mathf.Abs(pA._y - pB._y);

            if (InDstX > InDstY)
            {
                return 10 * InDstY + 10 * (InDstX - InDstY);
            }
            return 10 * InDstX + 10 * (InDstY - InDstX);
        }
        #endregion
    }
}