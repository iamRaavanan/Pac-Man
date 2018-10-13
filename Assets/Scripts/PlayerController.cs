using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Raavanan
{
    public class PlayerController : MonoBehaviour
    {
        #region Variables
        private float                   mTimer;
        private float                   mMoveRate;
        private bool                    mUp;
        private bool                    mDown;
        private bool                    mLeft;
        private bool                    mRight;
        private Node                    mPlayerNode
        {
            get
            {
                return GameManager._Instance.GetPlayerNode();
            }
        }
        private Direction               mCurrentDirection;
        public enum Direction
        {
            E_Up,
            E_Down,
            E_Left,
            E_Right
        }
        #endregion

        #region Update Player movement
        public void UpdatePlayerDetails (float pSpeed)
        {
            mMoveRate = pSpeed;
        }

        private void Update()
        {
            if (GameManager._Instance._IsGameOver)
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    GameManager._Instance._OnStart.Invoke();
                }
                return;
            }

            GetInput();
            SetPlayerDirection();
            if (GameManager._Instance._IsFirstInput)
            {
                mTimer += Time.deltaTime;
                if (mTimer > mMoveRate)
                {
                    mTimer = 0;
                    MovePlayer();
                }
            }
            else
            {
                GameManager._Instance._IsFirstInput = (mUp || mDown || mLeft || mRight);
            }
        }

        private void GetInput()
        {
            mUp = Input.GetButtonDown("Up");
            mDown = Input.GetButtonDown("Down");
            mRight = Input.GetButtonDown("Right");
            mLeft = Input.GetButtonDown("Left");
        }

        private void SetPlayerDirection()
        {
            if (mUp)
            {
                mCurrentDirection = Direction.E_Up;
                transform.localEulerAngles = Vector3.forward * 90;
            }
            else if (mDown)
            {
                mCurrentDirection = Direction.E_Down;
                transform.localEulerAngles = Vector3.forward * 270;
            }
            else if (mRight)
            {
                mCurrentDirection = Direction.E_Right;
                transform.localEulerAngles = Vector3.zero;
            }
            else if (mLeft)
            {
                mCurrentDirection = Direction.E_Left;
                transform.localEulerAngles = Vector3.forward * 180;
            }
        }

        private void MovePlayer()
        {
            int InX = 0, InY = 0;
            InX = ((mCurrentDirection == Direction.E_Right) ? 1 : ((mCurrentDirection == Direction.E_Left) ? -1 : 0));
            InY = ((mCurrentDirection == Direction.E_Up) ? 1 : ((mCurrentDirection == Direction.E_Down) ? -1 : 0));
            Node InTargetNode = GameManager._Instance.GetNode(mPlayerNode._x + InX, mPlayerNode._y + InY);
            if (InTargetNode != null)
            {
                if (!GameManager._Instance.IsBlockNode(InTargetNode))
                {
                    GameManager._Instance.PlaceObject(this.gameObject, InTargetNode._WorldPos);
                    GameManager._Instance.SetPlayerNode(InTargetNode);

                    if (GameManager._Instance.IsExitNode(InTargetNode))
                    {
                        // Level Completed
                        GameManager._Instance.UpdateGameOverText("LEVEL COMPLETE");
                        GameManager._Instance.GameOver();
                    }
                }
            }
        }
        #endregion

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.tag == "Enemy")
            {
                GameManager._Instance.UpdateGameOverText("GAME OVER");
                GameManager._Instance.GameOver();
            }
            else
            {
                SpriteRenderer InSR = collision.GetComponent<SpriteRenderer>();
                if (InSR.enabled)
                {
                    InSR.enabled = false;
                    collision.GetComponent<BoxCollider2D>().enabled = false;
                    GameManager._Instance.UpdateCoins();
                }
            }            
        }
    }
}
