using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Raavanan
{
    public class EnemyHandler : MonoBehaviour
    {
        #region Variables
        private EnemyType mCurrentEnemyType;
        private Node mCurrentNode;
        private Node mPreviousNode;
        private Node mTargetNode;
        private List<Node> mWayPointList;

        private int mCheckCounter;
        private bool mIsIncrement;
        private bool mIsFindingPath;
        private float mMoveRate;
        private float mTimer;
        private Vector3 mPlayerPosition
        {
            get { return GameManager._Instance.GetPlayerNode()._WorldPos; }
        }
        private Vector3 mTargetPosition;
        #endregion

        #region Update Enemy movement
        private void Update()
        {
            if (!GameManager._Instance._IsFirstInput)
            {
                return;
            }
            if (mCurrentEnemyType == EnemyType.E_Lazy)
            {
                bool IsPlayerInRange = CheckForPlayerPosition();
                if (IsPlayerInRange)
                {
                    mTimer += Time.deltaTime;
                    if (mTimer > mMoveRate)
                    {
                        mTimer = 0;
                        FollowPlayer();
                    }
                }
                else
                {
                    mWayPointList = null;
                }
            }
            else if (mCurrentEnemyType == EnemyType.E_Active)
            {
                mTimer += Time.deltaTime;
                if (mTimer > mMoveRate)
                {
                    mTimer = 0;
                    RoamAroundMap();
                }
            }
            else
            {
                mTimer += Time.deltaTime;
                if (mTimer > mMoveRate)
                {
                    mTimer = 0;
                    PingPong();
                }
            }
        }

        public void UpdateEnemy (EnemyType pType, Node pNode, float pSpeed)
        {
            mCurrentEnemyType = pType;
            mCurrentNode = pNode;
            mMoveRate = pSpeed; 
            if (pType != EnemyType.E_Lazy)
            {
                UpdateWayPoints();
            }
        }

        private void UpdateWayPoints()
        {
            mIsFindingPath = true;
            Node InRandomNode = null;
            if (mCurrentEnemyType == EnemyType.E_Lazy)
            {
                InRandomNode = GameManager._Instance.GetPlayerNode();
            }
            else
            {
                int InCount = GameManager._Instance.mAvailableNodes.Count;
                InRandomNode = GameManager._Instance.mAvailableNodes[UnityEngine.Random.Range(0, InCount)];
                while (InRandomNode == mCurrentNode)
                {
                    InRandomNode = GameManager._Instance.mAvailableNodes[UnityEngine.Random.Range(0, InCount)];
                }
            }
            //mCheckCounter = 1;
            mIsIncrement = true;
            mWayPointList = PathFinding._instace.FindPath(mCurrentNode, InRandomNode);
            //for (int i = 0; i < mWayPointList.Count; i++)
            //{
            //    Debug.Log("mWayPointList:" + mWayPointList[i]._x + ", " + mWayPointList[i]._y + "=>" + mWayPointList[i]._WorldPos);
            //}
            mIsFindingPath = false;
            mWayPointList.Insert(0, mCurrentNode);
        }

        private bool CheckForPlayerPosition ()
        {
            if (Vector3.Distance (mCurrentNode._WorldPos, mPlayerPosition) < 3)
            {
                return true;
            }
            return false;
        }
        
        private void FollowPlayer ()
        {
            if (mWayPointList != null && mWayPointList.Count > 0)
            {
                try
                {
                    mTargetNode = mWayPointList[mCheckCounter];
                    mCurrentNode = mTargetNode;
                    GameManager._Instance.PlaceObject(this.gameObject, mTargetNode._WorldPos);
                    mCheckCounter += 1;
                    if (mCheckCounter == mWayPointList.Count)
                    {
                        mCheckCounter = 0;
                        mWayPointList.Clear();
                        mWayPointList = null;
                        UpdateWayPoints();
                    }
                }
                catch (System.Exception e)
                {
                    if (!mIsFindingPath)
                    {
                        UpdateWayPoints();
                    }
                }
            }
            else
            {
                if (!mIsFindingPath)
                {
                    UpdateWayPoints();
                }
            }
        }

        private void RoamAroundMap ()
        {
            if (mWayPointList != null && mWayPointList.Count > 0)
            {
                mTargetNode = mWayPointList[mCheckCounter];
                mCurrentNode = mTargetNode;
                GameManager._Instance.PlaceObject(this.gameObject, mTargetNode._WorldPos);                
                mCheckCounter += 1;
                if (mCheckCounter == mWayPointList.Count - 1)
                {
                    mCheckCounter = 0;
                    mWayPointList.Clear();
                    mWayPointList = null;
                    UpdateWayPoints();
                }
            }
        }

        private void PingPong ()
        {
            if (mWayPointList != null)
            {
                mTargetNode = mWayPointList[mCheckCounter];
                mCurrentNode = mTargetNode;
                GameManager._Instance.PlaceObject(this.gameObject, mTargetNode._WorldPos);                
                mCheckCounter += ((mIsIncrement) ? 1 : -1);
                if (mCheckCounter == 0 && !mIsIncrement)
                {
                    mIsIncrement = true;
                }
                if (mCheckCounter == mWayPointList.Count-1 && mIsIncrement)
                {
                    mIsIncrement = false;
                }
            }
        }
        #endregion

    }

    public enum EnemyType
    {
        E_Lazy,
        E_Active,
        E_Patroller
    }
}
