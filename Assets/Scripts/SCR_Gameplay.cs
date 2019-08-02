// class: SCR_Gameplay
using GoogleMobileAds.Api;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SCR_Gameplay : MonoBehaviour
{
	public readonly int[] CHARACTER_UNLOCKED_LEVELS = new int[7]
	{
		0,
		5,
		10,
		20,
		30,
		40,
		50
	};

	public const float TIME_SHOW_ADS = 30f;

	public const float FINISH_POINT_OFFSET_X = 10f;

	public const float DISTANCE_ANCHORS = 10f;

	public const float LEVEL_1_WALL_OFFSET_X = -6f;

	public const float LEVEL_1_WALL_OFFSET_Y = -4f;

	public const float ANCHOR_POSITION_Y_MIN = 4f;

	public const float ANCHOR_POSITION_Y_MAX = 6f;

	public const float WALL_POSITION_Y_MIN = -12.5f;

	public const float WALL_POSITION_Y_MAX = -9.5f;

	public static float SCREEN_WIDTH;

	public static float SCREEN_HEIGHT;

	public static SCR_Gameplay instance;

	private static bool skipMenu = false;

	public int character;

	public GameObject PFB_ANCHOR;

	public GameObject PFB_WALL;

	public GameObject PFB_START_POINT;

	public GameObject PFB_FINISH_POINT;

	public RuntimeAnimatorController[] ACL_CHARACTERS;

	public RuntimeAnimatorController[] ACL_MENU_CHARACTERS;

	public GameObject player;

	public GameObject endLevel;

	public GameObject imgWellDone;

	public GameObject imgKeepGoing;

	public GameObject tmpLevelNumber;

	public GameObject btnReplay;

	public GameObject btnNext;

	public GameObject imgExclamation;

	public GameObject mainMenu;

	public GameObject btnPlay;

	public GameObject tmpUnlock;

	public SCR_ProgressBar scrProgressBar;

	public SCR_Background scrBackgroundFront;

	public SCR_Background scrBackgroundMiddle;

	public SCR_Background scrBackgroundBack;

	public GameObject imgCharacter;

	public SCR_Tutorial scrTutorial;

	public bool updateCamera;

	public int currentLevel;

	public GameState state;

	private List<GameObject> anchors = new List<GameObject>();

	private List<GameObject> walls = new List<GameObject>();

	// Điểm bắt đầu khởi tạo player
	private GameObject startPoint;

	// Điểm kết thúc level
	private GameObject finishPoint;

	private float maxProgress;


	private int currentAnchor;
    private int numberCurrent = 1;

	private int recommendedCharacter;

	private InterstitialAd interstitial;

	private static float timeShowAds = 30f;

	//--------------
	public Transform anchorLast;
    public Transform wallLast;
    //private Transform[] anchorS = new Transform[5];
    //private Transform[] wallS = new Transform[4];
    private List<Transform> wallS;
    private List<Transform> anchorS;
    //private Transform[] wallS;
    public Transform anchorPrefab;
    public Transform wallPrefab;
    public Transform enemyPrefab;
    public int number = 5;
    private int firstAnchor = 7;
    private int firstWall = 4;
    private float num2 = 7.5f;
    //---------------- Config
    public ConfigLevelRecord cfLevel;

    public void Awake()
	{
		SCREEN_HEIGHT = Camera.main.orthographicSize * 2f;
		SCREEN_WIDTH = SCREEN_HEIGHT * (float)Screen.width / (float)Screen.height;
		instance = this;

        anchorS = new List<Transform>();
        wallS   = new List<Transform>(); 

	}

    public void AddLastAnchor(Transform anchor)
    {
        anchorS.Add(anchor);
        anchorLast = anchor;
    }
    //public List<Transform> GetListAnchor()
    //{
    //    for (int i = 0; i < firstAnchor - 1; i++)
    //    {
    //        var item = anchorS[0];
    //        anchorS.RemoveAt(0);
    //        anchorS.Add(item);
    //    }
    //    return anchorS;
    //}


    //public void MoveListAnchor()
    //{
    //    var item1 = anchorS[0];
    //    item1.position = new Vector3(anchorLast.position.x + 10f, GetRandomY(), 0);
    //    anchorS.Add(item1);
    //    var item2 = anchorS[1];
    //    item2.position = new Vector3(anchorLast.position.x + 20f, GetRandomY(), 0);
    //    anchorS.Add(item2);

    //    Debug.Log(anchorS.Count);
    //    for (int i = 2; i < anchorS.Count; i++)
    //    {
    //        anchorS[i] = anchorS[i - 2];
    //        Debug.Log("OK");
    //    }
    //    anchorLast = anchorS[anchorS.Count - 1];
    //}

    public void MoveAnchor()
    {
        for (int i = 0; i < anchorS.Count; i++)
        {
            if (anchorS[i].position.x < (player.transform.position.x -10))
            {
                anchorS[i].position = new Vector3(anchorLast.position.x + 10, GetRandomY(), 0);
                AddLastAnchor(anchorS[i]);
            }
        } 
    }
    public void Start()
	{
		Application.targetFrameRate = 60;
		currentLevel = PlayerPrefs.GetInt("currentLevel", 0);
		character = PlayerPrefs.GetInt("character", 0);
		recommendedCharacter = PlayerPrefs.GetInt("recommendedCharacter", 0);
		if (!skipMenu && recommendedCharacter != 0)
		{
			character = recommendedCharacter;
			PlayerPrefs.SetInt("character", character);
			recommendedCharacter = 0;
			PlayerPrefs.SetInt("recommendedCharacter", recommendedCharacter);
		}
		UpdateCharacter();
		player.SetActive(value: false);
		scrProgressBar.gameObject.SetActive(value: false);
		endLevel.SetActive(value: false);
		mainMenu.SetActive(value: true);
		maxProgress = 0f;
		scrProgressBar.SetProgress(0f);
		Time.timeScale = 1f;
		Time.fixedDeltaTime = Time.timeScale * 0.02f;
		updateCamera = true;
		scrTutorial.Hide();
		state = GameState.MENU;
		MobileAds.Initialize("ca-app-pub-0081066185741622~8874259147");
		RequestInterstitial();
        // Button Replay
		if (skipMenu)
		{
            if (SCR_Gameplay.instance.state == GameState.READYENDLESS)
			    SwitchState(GameState.READYENDLESS);
            else
                SwitchState(GameState.READYLEVEL);
        }
	}
	private void RequestInterstitial()
	{
		string adUnitId = "ca-app-pub-0081066185741622/9034041242";
		interstitial = new InterstitialAd(adUnitId);
		AdRequest request = new AdRequest.Builder().AddTestDevice("36e6813a9776d338128e27da33a0467f").AddTestDevice("287cea1fe1bd25fe56e59ea1e0566114").Build();
		interstitial.LoadAd(request);
	}

	private void ShowAds()
	{
		interstitial.Show();
		interstitial.Destroy();
		RequestInterstitial();
		timeShowAds = 0f;
	}
	public void Update()
	{
		timeShowAds += Time.unscaledDeltaTime;
		if (player != null && startPoint != null && finishPoint != null)
		{
			float num = player.transform.position.x - startPoint.transform.position.x;
			float num2 = finishPoint.transform.position.x - startPoint.transform.position.x;
			float num3 = num / num2;
			if (maxProgress < num3)
			{
				maxProgress = num3;
				scrProgressBar.SetProgress(maxProgress);
			}
		}
	}

	public void LateUpdate()
	{
		if (player != null && updateCamera)
		{
			float num = 0.3f * SCREEN_WIDTH;
			Transform transform = Camera.main.transform;
			float x = transform.position.x;
			float num2 = player.transform.position.x + num;
			transform.position = new Vector3(num2, transform.position.y, transform.position.z);
			scrBackgroundFront.Move(num2 - x);
			scrBackgroundMiddle.Move(num2 - x);
			scrBackgroundBack.Move(num2 - x);
		}
	}
    private void ReadyLevel()
    {
        // Hide mainMenu
        mainMenu.SetActive(value: false);
        // if new player show tutorial
        // else show ProgressBar
        if (currentLevel == 0)
        {
            scrTutorial.Show();
            scrProgressBar.gameObject.SetActive(value: false);
        }
        else
        {
            scrProgressBar.SetLevel(currentLevel + 1);
            scrProgressBar.gameObject.SetActive(value: false);
        }
        // Get Anim Player
        // Show Player
        player.GetComponent<Animator>().runtimeAnimatorController = ACL_CHARACTERS[character];
        player.SetActive(value: true);
        // Use Data Config
        ConfiglevelKey key = new ConfiglevelKey();
        key.level = cfLevel.level;
        cfLevel = ConfigManager.instance.configlevel.GetRecordBykeySearch(key);
        
        //-------------Create Anchor
        for (int i = 0; i < cfLevel.anchor; i++)
        {
            //Debug.Log(num);
            Transform anchor = CreateAnchor();
            anchor.name = "anchor number " + i;
            anchorS.Add(anchor);
            anchor.position = new Vector3(num2 + (float)i * 10f, GetRandomY(), 0);
            AnchorControl anchorControl = anchor.GetComponent<AnchorControl>();
            anchorControl.Setup(this);
            //anchorS[];
        }
        anchorLast = anchorS[anchorS.Count - 1];
        //-------------Create Wall
        for (int j = 0; j < cfLevel.wall; j++)
        {
            //Debug.Log(wallS.Length + "    " + j);
            //Debug.Log(num);
            Transform wall = CreateWall();
            wallS[j] = wall;
            wall.position = new Vector3(num2 + (float)j * 10f, GetRandomY2(), 0);
            WallControl wallControl = wall.GetComponent<WallControl>();
            wallControl.Setup(this);

            if (j == wallS.Count - 1)
                wallLast = wall;
        }
        //-------------Create StartPoint
        startPoint = UnityEngine.Object.Instantiate(PFB_START_POINT);
    }
    private void ReadyEndless()
	{
		mainMenu.SetActive(value: false);
		if (currentLevel == 0)
		{
			scrTutorial.Show();
			scrProgressBar.gameObject.SetActive(value: false);
		}
		else
		{
			scrProgressBar.SetLevel(currentLevel + 1);
			scrProgressBar.gameObject.SetActive(value: true);
		}
		player.GetComponent<Animator>().runtimeAnimatorController = ACL_CHARACTERS[character];
		player.SetActive(value: true);

        // Create Point 
        // Xét vị trí x và vị trí y
        // Xét vị trí cho node được tạo
        // thêm vào danh sách
        //int num = 0;
        //num = ((currentLevel >= SCR_Config.NUMBER_ANCHORS.Length) ? (currentLevel + 1) : SCR_Config.NUMBER_ANCHORS[currentLevel]);
        /* for (int i = 0; i < num; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(PFB_ANCHOR);
			float x = num2 + (float)i * 10f;
			float y = UnityEngine.Random.Range(4f, 6f);
			gameObject.transform.position = new Vector3(x, y, gameObject.transform.position.z);
			anchors.Add(gameObject);
		}*/
        //----------------------------------
        //int num3 = 0;
        //num3 = ((currentLevel >= SCR_Config.NUMBER_WALLS.Length) ? currentLevel : SCR_Config.NUMBER_WALLS[currentLevel]);
        // Create Wall
        //for (int j = 0; j < num3; j++)
        //{
        //	GameObject gameObject2 = UnityEngine.Object.Instantiate(PFB_WALL);
        //	float x2 = anchorS[j].transform.position.x + 5f;
        //	float y2 = UnityEngine.Random.Range(-12.5f, -9.5f);
        //	if (currentLevel == 1)
        //	{
        //		y2 = gameObject2.transform.position.y;
        //	}
        //	gameObject2.transform.position = new Vector3(x2, y2, gameObject2.transform.position.z);
        //	walls.Add(gameObject2);
        //}

        //-----------------------------------Create Finish Point

        //finishPoint = UnityEngine.Object.Instantiate(PFB_FINISH_POINT);
        //finishPoint.transform.position = new Vector3(anchors[anchors.Count - 1].transform.position.x + 10f, finishPoint.transform.position.y, finishPoint.transform.position.z);
        //if (currentLevel == 1)
        //{
        //    walls[0].transform.position = new Vector3(finishPoint.transform.position.x + -6f, finishPoint.transform.position.y + -4f, walls[0].transform.position.z);
        //}
        //----------------------------------Create Anchor
        for (int i = 0; i < firstAnchor; i++)
        {
            //Debug.Log(num);
            Transform anchor = CreateAnchor();
            anchor.name = "anchor number " + i;
            anchorS.Add(anchor);
            anchor.position = new Vector3(num2 + (float)i * 10f, GetRandomY(), 0);
            AnchorControl anchorControl = anchor.GetComponent<AnchorControl>();
            anchorControl.Setup(this);
		}
        anchorLast = anchorS[anchorS.Count - 1];
        //----------------------------------Create Wall
        for (int j = 0; j < firstWall; j++)
        {
            Transform wall = CreateWall();
            wallS.Add(wall);
            wall.position = new Vector3(num2 + (float)j * 10f, GetRandomY2(), 0);
            WallControl wallControl = wall.GetComponent<WallControl>();
            wallControl.Setup(this);
        }
        wallLast = wallS[wallS.Count - 1];
        //----------------------------------Create StartPoint
		startPoint = UnityEngine.Object.Instantiate(PFB_START_POINT);
        // At initialization currentAnchor = -1(not grab any time yet)
        currentAnchor = -1;
    }

	public float GetRandomY()
    {
       return UnityEngine.Random.Range(4f, 6f);
    }
    public float GetRandomY2()
    {
        return UnityEngine.Random.Range(-10f, -13f);
    }
    private Transform CreateAnchor()
    {
        Transform column = Instantiate(anchorPrefab);
        return column;
    }
    private Transform CreateWall()
    {
        Transform wall = Instantiate(wallPrefab);
        return wall;
    }
    private Transform CreateEnemy()
    {
        Transform enemy = Instantiate(enemyPrefab);
        return enemy;
    }
    public void OnCreateEnemy()
    {
        int numberWall = UnityEngine.Random.Range(1, 4);
        //int numberWall = 2;
        if (numberCurrent > numberWall)
        {
            Transform enemy = CreateEnemy();
            //enemy.position = new Vector3(wallLast.position.x,0, 0);
            enemy.position = new Vector3(anchorS[currentAnchor].position.x + 10f, 0, 0);
            numberCurrent = 0;
        }
    }

	public void SwitchState(GameState s)
	{
		if (state == GameState.GAME_OVER || state == GameState.LEVEL_CLEARED)
		{
			return;
		}
		state = s;
		if (state == GameState.READYENDLESS)
		{
			ReadyEndless();
		}
        if (state == GameState.READYLEVEL)
        {
            ReadyLevel();
        }
		else if (state == GameState.PLAY)
		{
			startPoint.GetComponent<Collider2D>().enabled = false;
		}
		else if (state == GameState.GAME_OVER)
		{
			scrProgressBar.gameObject.SetActive(value: false);
			tmpLevelNumber.GetComponent<TextMeshProUGUI>().text = (currentLevel + 1).ToString("D2");
			imgWellDone.SetActive(value: false);
			imgKeepGoing.SetActive(value: true);
			btnNext.SetActive(value: false);
			btnReplay.SetActive(value: true);
			endLevel.SetActive(value: true);
			imgExclamation.SetActive(value: false);
			scrTutorial.Hide();
		}
		else
		{
			if (state != GameState.LEVEL_CLEARED)
			{
				return;
			}
			scrProgressBar.gameObject.SetActive(value: false);
			tmpLevelNumber.GetComponent<TextMeshProUGUI>().text = (currentLevel + 1).ToString("D2");
			imgWellDone.SetActive(value: true);
			imgKeepGoing.SetActive(value: false);
			btnNext.SetActive(value: true);
			btnReplay.SetActive(value: false);
			endLevel.SetActive(value: true);
			scrTutorial.Hide();
			for (int i = 0; i < CHARACTER_UNLOCKED_LEVELS.Length; i++)
			{
				if (currentLevel + 1 == CHARACTER_UNLOCKED_LEVELS[i])
				{
					recommendedCharacter = i;
					PlayerPrefs.SetInt("recommendedCharacter", recommendedCharacter);
					break;
				}
			}
			if (recommendedCharacter != 0)
			{
				imgExclamation.SetActive(value: true);
			}
			else
			{
				imgExclamation.SetActive(value: false);
			}
			currentLevel++;
			PlayerPrefs.SetInt("currentLevel", currentLevel);
		}
	}

	public GameObject GetNextAnchor()
	{
		do
		{
			currentAnchor++;
		}
		while (currentAnchor < anchors.Count && anchors[currentAnchor].transform.position.x < player.transform.position.x);
		if (currentAnchor < anchors.Count)
		{
			return anchors[currentAnchor];
		}
		return null;
	}
    public Transform GetNextAnchor2()
    {
        do
        {
            currentAnchor++;
            numberCurrent++;
            //Debug.Log(numberCurrent);
        }
        while (currentAnchor < anchorS.Count && anchorS[currentAnchor].position.x < player.transform.position.x);
        if (currentAnchor < anchorS.Count)
        {
            OnCreateEnemy();
            return anchorS[currentAnchor];
           

        }
        return null;
    }

    public bool IsLastAnchor(Rigidbody2D rb)
	{
		if (rb.gameObject == anchors[anchors.Count - 1])
		{
			return true;
		}
		return false;
	}
    public bool IsLastAnchor2(Rigidbody2D rb)
    {
        if (rb.gameObject == anchorS[anchorS.Count - 1])
        {
            return true;
        }
        return false;
    }
    public void OnReplay()
	{
		if (timeShowAds >= 30f && interstitial.IsLoaded())
		{
			ShowAds();
		}
		skipMenu = true;
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	public void OnNext()
	{
		if (timeShowAds >= 30f && interstitial.IsLoaded())
		{
			ShowAds();
		}
		skipMenu = true;
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	public void OnHome()
	{
		if (timeShowAds >= 30f && interstitial.IsLoaded())
		{
			ShowAds();
		}
		skipMenu = false;
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	public void OnArrowLeft()
	{
		character--;
		if (character < 0)
		{
			character = ACL_MENU_CHARACTERS.Length - 1;
		}
		UpdateCharacter();
		PlayerPrefs.SetInt("character", character);
	}

	public void OnArrowRight()
	{
		character++;
		if (character >= ACL_MENU_CHARACTERS.Length)
		{
			character = 0;
		}
		UpdateCharacter();
		PlayerPrefs.SetInt("character", character);
	}

	private void UpdateCharacter()
	{
		imgCharacter.GetComponent<Animator>().runtimeAnimatorController = ACL_MENU_CHARACTERS[character];
		if (currentLevel + 1 >= CHARACTER_UNLOCKED_LEVELS[character])
		{
			btnPlay.SetActive(value: true);
			tmpUnlock.transform.parent.gameObject.SetActive(value: false);
		}
		else
		{
			tmpUnlock.GetComponent<TextMeshProUGUI>().text = "UNLOCKED AT LEVEL " + CHARACTER_UNLOCKED_LEVELS[character];
			btnPlay.SetActive(value: false);
			tmpUnlock.transform.parent.gameObject.SetActive(value: true);
		}
	}

	public void OnPlay()
	{
		SwitchState(GameState.READYENDLESS);
	}
    public void OnPlayLevel()
    {
        SwitchState(GameState.READYLEVEL);
    }
}
