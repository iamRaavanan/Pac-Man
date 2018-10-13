using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Raavanan
{
    public class GameManager : MonoBehaviour
    {
        #region Variables declaration
        [Tooltip ("Node related properties like size of the contents can be updated here.")]
        [Header("Node Properties")]
        [SerializeField]
        private int                     mMaxHeight;
        [SerializeField]
        private int                     mMaxWidth;
        [SerializeField]
        private Color                   mBackgroundColor1;
        [SerializeField]
        private Color                   mBackgroundColor2;
        [SerializeField]
        private Color                   mBlockColor;
        [SerializeField]
        private Transform               mCameraHolder;
        [SerializeField]
        private Sprite                  mCoinSprite;

        [Tooltip("Player Sprite and move speed can be updated here.")]
        [Header("Player Properties")]
        [SerializeField]
        private Sprite                  mPlayerSprite;
        [SerializeField]
        private float                   mMoveRate = 0.5f;
        [SerializeField]
        private Text                    mScoreTxt;

        [Tooltip("Different type of enemy sprites and speed can be updated here")]
        [Header("Enemy Prorperties")]
        [SerializeField]
        private EnemyDetails[]          mEnemyDetails;

        [Tooltip("Number of blocks placed can be updated here. Range from minimum to maximum can be freezed here.")]
        [Header("Blocke Range")]
        [SerializeField]
        private int                     mMinRange;
        [SerializeField]
        private int                     mMaxRange;
        [SerializeField]
        private Text                    mGameOverTxt;

        private int                     mCoinsCaptured;
        private int                     mTotalCoins;

        private GameObject              mMapObject;
        private GameObject              mPlayerObject;
        private GameObject              mStartObject;
        private GameObject              mExitObject;
        private List<GameObject>        mEnemyObjectList = new List<GameObject> ();
        private List<GameObject>        mBlockObjectList = new List<GameObject> ();
        private List<GameObject>        mCoinObjectList = new List<GameObject> ();

        private SpriteRenderer          mMapRenderer;
        private SpriteRenderer          mPlayerRenderer;
        private SpriteRenderer          mBlockRenderer;

        private Node                    mPlayerNode;
        private Node                    mStartNode;
        private Node                    mExitNode;
        private Node[,]                 mGrid;

        [HideInInspector]
        public List<Node>               mAvailableNodes = new List<Node>();
        private List<Node>              mBlockNodes = new List<Node>();
        [HideInInspector]
        public bool                     _IsGameOver;
        [HideInInspector]
        public bool                     _IsFirstInput;

        public UnityEvent               _OnStart;
        public UnityEvent               _OnGameover;

        public static GameManager _Instance;
        #endregion

        #region Init
        private void Start()
        {
            _Instance = this;
            _OnStart.Invoke();
        }

        public void StartNewGame()
        {
            ClearReferences();
            CreateMap();
            PlacePlayer();
            PlaceBlocker();
            mTotalCoins = mAvailableNodes.Count;
            PlaceCoins();
            PlaceEnemies();            
            PlaceCamera();            
        }

        private void ClearReferences()
        {
            if (mMapObject != null)
            {
                Destroy(mMapObject);
            }
            if (mPlayerObject != null)
            {
                Destroy(mPlayerObject);
            }
            if (mStartObject != null)
            {
                Destroy(mStartObject);
            }
            if (mExitObject != null)
            {
                Destroy(mExitObject);
            }
            foreach (GameObject GO in mBlockObjectList)
            {
                Destroy(GO);
            }
            foreach (GameObject GO in mEnemyObjectList)
            {
                Destroy(GO);
            }
            foreach (GameObject GO in mCoinObjectList)
            {
                Destroy(GO);
            }
            mAvailableNodes.Clear();
            mEnemyObjectList.Clear();
            mBlockObjectList.Clear();
            mBlockNodes.Clear();
            mGrid = null;
            GC.Collect();
            mCoinsCaptured = 0;
            _IsGameOver = _IsFirstInput = false;
        }

        private void CreateMap()
        {
            mMapObject = new GameObject("Map");
            mMapRenderer = mMapObject.AddComponent<SpriteRenderer>();
            mGrid = new Node[mMaxWidth, mMaxHeight];
            Texture2D InTexture = new Texture2D(mMaxWidth, mMaxHeight);
            Vector3 InTexturePos = Vector3.zero;
            Node InNode = null;
            #region Visual Representation
            for (int i = 0;  i < mMaxWidth; i++)
            {
                for (int j = 0; j < mMaxHeight; j++)
                {
                    InTexturePos.x = i;
                    InTexturePos.y = j;
                    InNode = new Node(i, j, InTexturePos);
                    //InNode._gCost = InNode._hCost = 1;
                    mAvailableNodes.Add(InNode);
                    mGrid[i, j] = InNode;
                    if (i % 2 == 0)
                    {
                        if (j % 2 == 0)
                        {
                            InTexture.SetPixel(i, j, mBackgroundColor1);
                        }
                        else
                        {
                            InTexture.SetPixel(i, j, mBackgroundColor2);
                        }
                    }
                    else
                    {
                        if (j % 2 == 0)
                        {
                            InTexture.SetPixel(i, j, mBackgroundColor2);
                        }
                        else
                        {
                            InTexture.SetPixel(i, j, mBackgroundColor1);
                        }
                    }
                }
            }
            InTexture.filterMode = FilterMode.Point;
            InTexture.Apply();
            #endregion
            Rect InRect = new Rect(0, 0, mMaxWidth, mMaxHeight);
            Sprite InSprite = Sprite.Create(InTexture, InRect, Vector2.zero, 1, 0, SpriteMeshType.FullRect);
            mMapRenderer.sprite = InSprite;
        }

        private void PlacePlayer()
        {
            mPlayerObject = new GameObject("Player");
            mPlayerRenderer = mPlayerObject.AddComponent<SpriteRenderer>();
            mPlayerRenderer.sprite = mPlayerSprite;
            mPlayerRenderer.sortingOrder = 2;
            BoxCollider2D InB2D = mPlayerObject.AddComponent<BoxCollider2D>();
            InB2D.isTrigger = true;
            Rigidbody2D InRB = mPlayerObject.AddComponent<Rigidbody2D>();
            InRB.bodyType = RigidbodyType2D.Kinematic;
            PlayerController InPlayerController = mPlayerObject.AddComponent<PlayerController>();
            InPlayerController.UpdatePlayerDetails(mMoveRate);
            int InRandomValue = UnityEngine.Random.Range(0, mAvailableNodes.Count);
            mPlayerNode = mAvailableNodes[InRandomValue];
            mAvailableNodes.RemoveAt(InRandomValue);
            PlaceObject(mPlayerObject, mPlayerNode._WorldPos);
            // Exit Node => 4 possibilities of placing exit node at every edge (00,06,60,66)
            PlaceEntryExitNode();
        }

        private void PlaceEntryExitNode()
        {
            mStartObject = new GameObject("Start");
            SpriteRenderer InStartRenderer = mStartObject.AddComponent<SpriteRenderer>();
            InStartRenderer.sprite = CreateSprite(Color.green);
            InStartRenderer.sortingOrder = 1;
            PlaceObject(mStartObject, mPlayerNode._WorldPos);
            mExitObject = new GameObject("Exit");
            SpriteRenderer InExitRenderer = mExitObject.AddComponent<SpriteRenderer>();
            InExitRenderer.sprite = CreateSprite(Color.magenta);
            InExitRenderer.sortingOrder = 1;
            int InRandomValue = UnityEngine.Random.Range(0, mAvailableNodes.Count);
            mExitNode = mAvailableNodes[InRandomValue];
            mAvailableNodes.RemoveAt(InRandomValue);
            PlaceObject(mExitObject, mExitNode._WorldPos);
        }

        private void PlaceBlocker()
        {
            int InBlockLength = UnityEngine.Random.Range(mMinRange, mMaxRange);
            GameObject InBlockerParentGO = new GameObject("Blocker");
            mBlockObjectList.Add(InBlockerParentGO);
            GameObject InBlockerGO;
            int InRandomValue = 0;
            Node InTargetNode = null;
            for (int i = 0; i < InBlockLength; i++)
            {
                InBlockerGO = new GameObject();
                mBlockObjectList.Add(InBlockerGO);
                InBlockerGO.transform.parent = InBlockerParentGO.transform;
                mBlockRenderer = InBlockerGO.AddComponent<SpriteRenderer>();
                mBlockRenderer.sprite = CreateSprite(mBlockColor);
                mBlockRenderer.sortingOrder = 1;
                //int InFrom = (i * mMaxWidth);
                //int InTo = InFrom + mMaxWidth;
                InRandomValue = UnityEngine.Random.Range(0, mAvailableNodes.Count);
                InTargetNode = mAvailableNodes[InRandomValue];
                mAvailableNodes.RemoveAt(InRandomValue);
                //Debug.Log("(" + InFrom + ", " + InTo + ") => "+ InRandomValue + " = ("+ InTargetNode._x + ", " + InTargetNode._y + ")");
                PlaceObject(InBlockerGO, InTargetNode._WorldPos);
                mBlockNodes.Add(InTargetNode);
            }
        }

        private void PlaceCoins ()
        {
            GameObject InCoinsParentGO = new GameObject("Coins");
            mCoinObjectList.Add(InCoinsParentGO);
            GameObject InCoinsGO = null;
            SpriteRenderer InCoinRenderer = null;
            Node InCoinNode = null;
            for (int i = 0; i < mTotalCoins; i++)
            {
                InCoinsGO = new GameObject();
                mCoinObjectList.Add(InCoinsGO);
                InCoinsGO.transform.parent = InCoinsParentGO.transform;
                InCoinRenderer = InCoinsGO.AddComponent<SpriteRenderer>();
                BoxCollider2D InB2D = InCoinsGO.AddComponent<BoxCollider2D>();
                InB2D.isTrigger = true;
                InB2D.size = Vector2.one * 0.5f;
                InCoinRenderer.sprite = mCoinSprite;
                InCoinRenderer.sortingOrder = 2;
                InCoinNode = mAvailableNodes[i];
                PlaceObject(InCoinsGO, InCoinNode._WorldPos);
            }
        }

        private void PlaceEnemies ()
        {
            GameObject InCharacterGO = null;
            SpriteRenderer InCharacterSR = null;
            EnemyHandler InEnemyHandler = null;
            Node InTargetNode = null;
            BoxCollider2D InB2D = null;
            for (int i = 0; i < mEnemyDetails.Length; i++)
            {
                InCharacterGO = new GameObject(mEnemyDetails[i]._CharcterSprite.name);
                InCharacterGO.tag = "Enemy";
                InB2D = InCharacterGO.AddComponent<BoxCollider2D>();
                InB2D.size = Vector3.one * 0.5f;
                InB2D.isTrigger = true;
                mEnemyObjectList.Add(InCharacterGO);
                InCharacterSR = InCharacterGO.AddComponent<SpriteRenderer>();
                InCharacterSR.sprite = mEnemyDetails[i]._CharcterSprite;
                InCharacterSR.sortingOrder = 3;
                InEnemyHandler = InCharacterGO.AddComponent<EnemyHandler>();
                int InRandomValue = UnityEngine.Random.Range(0, mAvailableNodes.Count);
                InTargetNode = mAvailableNodes[InRandomValue];
                InEnemyHandler.UpdateEnemy(mEnemyDetails[i]._Type, InTargetNode, mEnemyDetails[i]._Speed);
                PlaceObject (InCharacterGO, InTargetNode._WorldPos);
            }
        }

        private void PlaceCamera()
        {
            Node InNode = GetNode(mMaxWidth / 2, mMaxHeight / 2);
            Vector3 InPos = InNode._WorldPos;
            InPos += Vector3.one * 0.5f;
            mCameraHolder.transform.position = InPos;
        }
        #endregion

        #region Utilities
        private Sprite CreateSprite(Color pTargetColor)
        {
            Texture2D InTexture = new Texture2D(mMaxWidth, mMaxHeight);
            InTexture.SetPixel(0, 0, pTargetColor);
            InTexture.Apply();
            InTexture.filterMode = FilterMode.Point;
            Rect InRect = new Rect(0, 0, 1, 1);
            return Sprite.Create(InTexture, InRect, Vector3.one * 0.5f, 1.0f, 0, SpriteMeshType.FullRect);
        }

        public void GameOver()
        {
            _IsGameOver = true;
            _IsFirstInput = false;
            _OnGameover.Invoke();
        }
        
        public bool IsBlockNode(Node pNode)
        {
            int InCount = mBlockNodes.Count;
            for (int i = 0; i < InCount; i++)
            {
                if (mBlockNodes[i] == pNode)
                {
                    return true;
                }
            }
            return false;
        }
        
        public void UpdateCoins ()
        {
            mCoinsCaptured++;
            mScoreTxt.text = string.Format("{0:0}", mCoinsCaptured);
        }

        public void UpdateGameOverText (string pText)
        {
            mGameOverTxt.text = pText;
        }

        public bool IsExitNode(Node pNode)
        {
            if (mExitNode == pNode && mTotalCoins == mCoinsCaptured)
            {
                return true;
            }
            return false;
        }

        public void PlaceObject(GameObject pObject, Vector3 pPosition)
        {
            pPosition += Vector3.one * 0.5f;
            pObject.transform.position = pPosition;
        }

        public Node GetPlayerNode ()
        {
            return mPlayerNode;
        }

        public void SetPlayerNode (Node pNode)
        {
            mPlayerNode = pNode;
        }

        public Node GetNode(int pX, int pY)
        {
            if (pX < 0 || pX > mMaxWidth - 1 || pY < 0 || pY > mMaxHeight - 1)
            {
                return null;
            }
            return mGrid[pX, pY];
        }

        public List<Node> GetNeighbourNods (Node pNode)
        {
            List<Node> InNeighbours = new List<Node>();
            Node InNode = null;
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == y || x == -y)
                        continue;
                    int InX = pNode._x + x;
                    int InY = pNode._y + y;

                    if (InX >= 0 && InX < mMaxWidth && InY >= 0 && InY < mMaxHeight)
                    {
                        InNode = GetNode(InX, InY);
                        if (!IsBlockNode (InNode))
                        {
                            InNeighbours.Add(InNode);
                        }                        
                    }
                }
            }
            return InNeighbours;
        }
        #endregion
    }

    [System.Serializable]
    public struct EnemyDetails
    {
        public Sprite      _CharcterSprite;
        public float _Speed;
        public EnemyType _Type;
    }
}
