using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Completed {
	using System.Collections.Generic;		//Allows us to use Lists. 
	using UnityEngine.UI;					//Allows us to use UI.
	
	public class GameManager : MonoBehaviour
	{
		public float titleDelay = 2f;						//Time to wait before starting level, in seconds.
		public float turnDelay = 0.1f;							//Delay between each Player turn.
		//public static int playerFoodPoints = 100;						//Starting value for Player food points.
		public int playerHP;									//Stores player's HP value between levels.
		public static GameManager instance = null;				//Static instance of GameManager which allows it to be accessed by any other script.
		public static bool playersTurn = true;		//Boolean to check if it's players turn, hidden in inspector but public.
		private GameObject titlecard;							//Image to block out level as levels are being set up, background for titlecardText.
		private Text titlecardText;									//Text to display current level number.
		private GameObject menu;
		private GameObject maps;
		private GameObject dunGen;
		private GameObject activeMap;
		private GameObject settings;
		private List<GameObject> menuList;
		private List<GameObject> mapsList;

		private float centerX;
		private float centerY;
		private float currY;
		private BoardManager boardScript;						//Store a reference to our BoardManager which will set up the level.
		private int level = 1;									//Current level number, expressed in game as "Day 1".
		private List<Enemy> enemies;							//List of all Enemy units, used to issue them move commands.
		private bool enemiesMoving;								//Boolean to check if enemies are moving.
		private float scrollRate=10;
		public bool displayMap = false;

		//Awake is always called before any Start functions
		void Awake()
		{
            //Check if instance already exists
            if (instance == null)

                //if not, set instance to this
                instance = this;

            //If instance already exists and it's not this:
            else if (instance != this)

                //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
                Destroy(gameObject);	
			
			//Sets this to not be destroyed when reloading scene
			DontDestroyOnLoad(gameObject);
			
			//Assign enemies to a new List of Enemy objects.
			enemies = new List<Enemy>();
			
			//Get a component reference to the attached BoardManager script
			Init();
		}

        //this is called only once, and the paramter tell it to be called only after the scene was loaded
        //(otherwise, our Scene Load callback would be called the very first load, and we don't want that)
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static public void CallbackInitialization()
        {
            //register the callback to be called everytime the scene is loaded
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        //This is called each time a scene is loaded.
        static private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1) {
            instance.level++;
            instance.Init();
        }
		
		void Init() {
			titlecard = GameObject.Find("Titlecard");	
			titlecardText = GameObject.Find("TitlecardText").GetComponent<Text>();
			titlecardText.text = "homebrew";
			centerX = titlecardText.transform.position.x;
			centerY = titlecardText.transform.position.y;
			currY = centerY-500;
			titlecardText.transform.position = new Vector2 (centerX, currY);
			titlecard.SetActive(true);

			menu = GameObject.Find("Menu");	
			menuList = new List<GameObject> ();
			menuList.Add(GameObject.Find ("MenuBrews"));
			menuList.Add(GameObject.Find ("MenuMaps"));
			menuList.Add(GameObject.Find ("MenuMonsters"));
			menuList.Add(GameObject.Find ("MenuDice"));
			menuList.Add(GameObject.Find ("MenuCharacters"));
			menuList.Add(GameObject.Find ("MenuSettings"));
			float listY = centerY + 200;
			for (var i = 0; i < 6; i++) {
				menuList [i].transform.position = new Vector2 (centerX, listY -= 30);
			}
			menu.SetActive(false);

			maps = GameObject.Find("Maps");	
			mapsList = new List<GameObject> ();
			mapsList.Add(GameObject.Find ("MapsSaved"));
			mapsList.Add(GameObject.Find ("MapsTexturePacks"));
			mapsList.Add(GameObject.Find ("MapsGenerateNewMap"));
			mapsList.Add(GameObject.Find ("MapsFindNewTextures"));
			maps.SetActive(false);
			listY = centerY + 200;
			for (var i = 0; i < 4; i++) {
				mapsList [i].transform.position = new Vector2 (centerX, listY -= 30);
			}	

			settings = GameObject.Find("Settings");	
			settings.SetActive(false);

			dunGen = GameObject.Find("dunGen");	
			dunGen.SetActive (false);

			activeMap = GameObject.Find("ActiveMap");	
			activeMap.SetActive (false);
			enemies.Clear();
		}

		void HideTitleImage() {
			titlecard.SetActive(false);
		}
			
		void Update() {
			if (titlecard.activeSelf) {
				if (currY < centerY) {
					currY += scrollRate;
					if (scrollRate > 1) {
						scrollRate -= 0.1f;
					}
					titlecardText.transform.position = new Vector2 (centerX, currY);
				} else {
					menu.SetActive (true);
					Invoke ("HideTitleImage", titleDelay);
				}
			}
			if (!displayMap || playersTurn || enemiesMoving) {
				return;
			}
			StartCoroutine (MoveEnemies ());
		}

		public void DisplayMap(){
			displayMap = true;
		}

		public void AddEnemyToList(Enemy script){
			enemies.Add(script);
		}

		public void GameOver()
		{
			titlecardText.text = "After " + level + " days, you starved.";
			titlecard.SetActive(true);
			
			//Disable this GameManager.
			enabled = false;
		}
		
		//Coroutine to move enemies in sequence.
		IEnumerator MoveEnemies()
		{
			//While enemiesMoving is true player is unable to move.
			enemiesMoving = true;
			
			//Wait for turnDelay seconds, defaults to .1 (100 ms).
			yield return new WaitForSeconds(turnDelay);
			
			//If there are no enemies spawned (IE in first level):
			if (enemies.Count == 0) 
			{
				//Wait for turnDelay seconds between moves, replaces delay caused by enemies moving when there are none.
				yield return new WaitForSeconds(turnDelay);
			}

			for (int i = 0; i < enemies.Count; i++) {
				if(enemies[i].isActiveAndEnabled) {
					if (enemies [i].MoveEnemy ()) {
						yield return new WaitForSeconds (0);
					}
				}
			}
			playersTurn = true;
			enemiesMoving = false;
		}

	}
}

