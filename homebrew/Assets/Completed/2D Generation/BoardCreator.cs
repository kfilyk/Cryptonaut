using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEditor;
using System.Collections.Generic; 		//Allows us to use Lists.


namespace Completed {
	public class BoardCreator : MonoBehaviour
	{

		public enum TileType{
			Wall, Floor,
		}


		public int columns = 100;                                 // The number of columns on the board (how wide it will be).
		public int rows = 100;                                    // The number of rows on the board (how tall it will be).
		public static int mapType=0;
		public static int numRoomsMin=5;
		public static int numRoomsMax=20;
		public IntRange numRooms = new IntRange (numRoomsMin, numRoomsMax);         // The range of the number of rooms there can be.
		public static int roomWidthMin=3;
		public static int roomWidthMax = 10;
		public IntRange roomWidth = new IntRange (3, 10);         // The range of widths rooms can have.
		public static int roomHeightMin=3;
		public static int roomHeightMax = 10;
		public IntRange roomHeight = new IntRange (3, 10);        // The range of heights rooms can have.
		public static int corridorLengthMin=3;
		public static int corridorLengthMax = 10;
		public IntRange corridorLength = new IntRange (3, 10);    // The range of lengths corridors between rooms can have.
		public GameObject[] floorTiles;    
		public GameObject[] wallTiles;    
		public GameObject[] outerWallTiles;   
		public GameObject[] enemyTiles; // Array of enemy unit prefabs (script+sprite)
		public int enemyCount = 1;
		public GameObject player;

		private TileType[][] tiles;                               // A jagged array of tile types representing the board, like a grid.
		private Room[] rooms;                                     // All the rooms that are created for this board.
		private Corridor[] corridors;                             // All the corridors that connect the rooms.
		private GameObject boardHolder;                           // GameObject that acts as a container for all other tiles.

		private void Start ()
		{
			// Create the board holder.
			boardHolder = new GameObject("BoardHolder");

			SetupTilesArray ();

			CreateRoomsAndCorridors ();

			GenerateTiles ();
			SetTilesValuesForRooms ();
			SetTilesValuesForCorridors ();

			InstantiateTiles ();
			InstantiateOuterWalls ();

		}

		public void setNumRoomsMin() {
			if (numRoomsMin < 40) {
				numRoomsMin++;
			} else {
				numRoomsMin = 1;
			}

			if (numRoomsMin > numRoomsMax) {
				numRoomsMax = numRoomsMin;
			}
			GameObject DGMinR = GameObject.Find ("dunGenMinRooms");
			ButtonStuff bs=DGMinR.GetComponent<ButtonStuff> ();
			bs.buttonString = "<Min Rooms: " + numRoomsMin+">";

			GameObject DGMaxR = GameObject.Find ("dunGenMaxRooms");
			bs=DGMaxR.GetComponent<ButtonStuff> ();
			bs.buttonString = "Max Rooms: " + numRoomsMax;

			numRooms = new IntRange (numRoomsMin, numRoomsMax);
		}
		public void setNumRoomsMax() {
			if (numRoomsMax < 40) {
				numRoomsMax++;
			} else {
				numRoomsMax = 1;
			}
			if (numRoomsMax < numRoomsMin) {
				numRoomsMin = numRoomsMax;
			}
			GameObject DGMinR = GameObject.Find ("dunGenMinRooms");
			ButtonStuff bs=DGMinR.GetComponent<ButtonStuff> ();
			bs.buttonString = "Min Rooms: " + numRoomsMin;

			GameObject DGMaxR = GameObject.Find ("dunGenMaxRooms");
			bs=DGMaxR.GetComponent<ButtonStuff> ();
			bs.buttonString = "<Max Rooms: " + numRoomsMax+">";

			numRooms = new IntRange (numRoomsMin, numRoomsMax);
		}

		void SetupTilesArray ()
		{
			// Set the tiles jagged array to the correct width.
			tiles = new TileType[columns][];

			// Go through all the tile arrays...
			for (int i = 0; i < tiles.Length; i++)
			{
				// ... and set each tile array is the correct height.
				tiles[i] = new TileType[rows];
			}
		}


		void CreateRoomsAndCorridors ()
		{
			// Create the rooms array with a random size.
			rooms = new Room[numRooms.Random];

			// There should be one less corridor than there is rooms.
			corridors = new Corridor[rooms.Length - 1];

			// Create the first room and corridor.
			rooms[0] = new Room ();
			corridors[0] = new Corridor ();

			// Setup the first room, there is no previous corridor so we do not use one.
			rooms[0].SetupRoom(roomWidth, roomHeight, columns, rows);

			// Setup the first corridor using the first room.
			corridors[0].SetupCorridor(rooms[0], corridorLength, roomWidth, roomHeight, columns, rows, true);
			for (int i = 1; i < rooms.Length; i++) {
				// Create a room.
				rooms[i] = new Room ();

				// Setup the room based on the previous corridor.
				rooms[i].SetupRoom (roomWidth, roomHeight, columns, rows, corridors[i - 1]);

				// If we haven't reached the end of the corridors array...
				if (i < corridors.Length)
				{
					// ... create a corridor.
					corridors[i] = new Corridor ();

					// Setup the corridor based on the room that was just created.
					corridors[i].SetupCorridor(rooms[i], corridorLength, roomWidth, roomHeight, columns, rows, false);
				}

				if (i == rooms.Length *.5f) { 
					Vector3 playerPos = new Vector3 (rooms[i].xPos, rooms[i].yPos, 0);
					Instantiate(player, playerPos, Quaternion.identity);
				}
				if (i != rooms.Length *.5f) { 
					int eX = Random.Range (rooms [i].xPos, rooms [i].xPos + rooms [i].roomWidth);
					int eY = Random.Range (rooms [i].yPos, rooms [i].yPos + rooms [i].roomHeight);
					while (player.transform.position.x == eX && player.transform.position.y == eY) {
						eX = Random.Range (rooms [i].xPos, rooms [i].xPos + rooms [i].roomWidth);
						eY = Random.Range (rooms [i].yPos, rooms [i].yPos + rooms [i].roomHeight);					
					}
					Vector3 enemyPos = new Vector3(eX, eY, 0);
					GameObject enemyChoice = enemyTiles[Random.Range (0, enemyTiles.Length)];
					Instantiate(enemyChoice, enemyPos, Quaternion.identity);				
				}

			}

		}
		void GenerateTiles() {
			if (mapType == 0) {  //Dungeon type map. Now wall tile prefabs must be modified such that sprites are changed. 
				Texture2D tex = new Texture2D(256,256);	// 1. Create spritesheet
				int dim=16;
				for (int i = 0; i < 8; i++) { //gen wall tiles
					Color col = new Color();
					Color col2 = new Color();
					Color col3 = new Color();
					Color dcol2 = new Color();
					Color dcol3 = new Color();
					float wallRCol = Random.Range(0.3f, 0.3f);
					float wallGCol = Random.Range(0.3f, 0.3f);
					float wallBCol = Random.Range(0.3f, 0.3f);
					float wallRCol2 = wallRCol - 0.02f;
					float wallGCol2 = wallGCol - 0.02f;
					float wallBCol2 = wallBCol - 0.02f;
					float wallRCol3 = wallRCol - 0.04f;
					float wallGCol3 = wallGCol - 0.04f;
					float wallBCol3 = wallBCol - 0.04f;
					float destWallRCol2 = wallRCol - 0.04f;
					float destWallGCol2 = wallGCol - 0.04f;
					float destWallBCol2 = wallBCol - 0.04f;
					float destWallRCol3 = wallRCol - 0.08f;
					float destWallGCol3 = wallGCol - 0.08f;
					float destWallBCol3 = wallBCol - 0.08f;
					col.r = wallRCol;
					col.g = wallGCol;
					col.b = wallBCol;
					col.a = 1.0f;
					col2.r = wallRCol2;
					col2.g = wallGCol2;
					col2.b = wallBCol2;
					col2.a = 1.0f;
					col3.r = wallRCol3;
					col3.g = wallGCol3;
					col3.b = wallBCol3;
					col3.a = 1.0f;
					dcol2.r = destWallRCol2;
					dcol2.g = destWallGCol2;
					dcol2.b = destWallBCol2;
					dcol2.a = 1.0f;
					dcol3.r = destWallRCol3;
					dcol3.g = destWallGCol3;
					dcol3.b = destWallBCol3;
					dcol3.a = 1.0f;
					for (int k = (i*dim); k<(dim+(i*dim)); k++) {
						for(int j=0;j<16; j++) { //Create 8 16x16 pixel sprites of wall type
						
							float random = Random.Range (0, 3);
							if (j + 2 < dim) {
								if (random == 1) {
									tex.SetPixel (j, k, col); //first layer
									tex.SetPixel (j+16, k, col);  //second layer
									tex.SetPixel (++j, k, col); //first layer
									tex.SetPixel (j+16, k, col); //second layer
								} else if (random == 2) {
									tex.SetPixel (j, k, col2);
									tex.SetPixel (j+16, k, dcol2); 
									tex.SetPixel (++j, k, col2);
									tex.SetPixel (j+16, k, dcol2);
								} else {
									tex.SetPixel (j, k, col3);
									tex.SetPixel (j + 16, k, dcol3);
									tex.SetPixel (++j, k, col3);
									tex.SetPixel (j+16, k, dcol3);
								}
							} else {
								if (random == 1) {
									tex.SetPixel (j, k, col);
									tex.SetPixel (j+16, k, col); 
								} else if (random == 2) {
									tex.SetPixel (j, k, col2);
									tex.SetPixel (j+16, k, dcol2);
								} else {
									tex.SetPixel (j, k, col3);
									tex.SetPixel (j+16, k, dcol3); 
								}
							}
						}
					}
				}
				for (int i = 0; i < 8; i++) { // gen floor tiles
					Color col = new Color();
					Color col2 = new Color();
					Color col3 = new Color();
					float wallRCol = Random.Range(0.5f, 0.5f);
					float wallGCol = Random.Range(0.5f, 0.5f);
					float wallBCol = Random.Range(0.5f, 0.5f);
					float wallRCol2 = wallRCol - 0.02f;
					float wallGCol2 = wallGCol - 0.02f;
					float wallBCol2 = wallBCol - 0.02f;
					float wallRCol3 = wallRCol - 0.04f;
					float wallGCol3 = wallGCol - 0.04f;
					float wallBCol3 = wallBCol - 0.04f;
					col.r = wallRCol;
					col.g = wallGCol;
					col.b = wallBCol;
					col.a = 1.0f;
					col2.r = wallRCol2;
					col2.g = wallGCol2;
					col2.b = wallBCol2;
					col2.a = 1.0f;
					col3.r = wallRCol3;
					col3.g = wallGCol3;
					col3.b = wallBCol3;
					col3.a = 1.0f;

					for (int k = (i*dim); k<(dim+(i*dim)); k++) {
						for(int j=32;j<48; j++) { //Create 8 16x16 pixel sprites of floor type
							float random = Random.Range (0, 3);
							float random2 = Random.Range (0, 2);
							if (j + 2 < 48 && random2 == 0) {
								if (random == 1) {
									tex.SetPixel (j, k, col);
									tex.SetPixel (++j, k, col);
								} else if (random == 2) {
									tex.SetPixel (j, k, col2);
									tex.SetPixel (++j, k, col2);
								} else {
									tex.SetPixel (j, k, col3);
									tex.SetPixel (++j, k, col3);
								}
							} else {
								if (random == 1) {
									tex.SetPixel (j, k, col);
								} else if (random == 2) {
									tex.SetPixel (j, k, col2);
								} else {
									tex.SetPixel (j, k, col3);
								}
							}
						}
					}
				}
				for (int i = 0; i < 8; i++) { //gen outer wall tiles
					Color col = new Color();
					Color col2 = new Color();
					Color col3 = new Color();
					float wallRCol = Random.Range(0.25f, 0.25f);
					float wallGCol = Random.Range(0.25f, 0.25f);
					float wallBCol = Random.Range(0.25f, 0.25f);
					float wallRCol2 = wallRCol - 0.04f;
					float wallGCol2 = wallGCol - 0.04f;
					float wallBCol2 = wallBCol - 0.04f;
					float wallRCol3 = wallRCol - 0.08f;
					float wallGCol3 = wallGCol - 0.08f;
					float wallBCol3 = wallBCol - 0.08f;
					col.r = wallRCol;
					col.g = wallGCol;
					col.b = wallBCol;
					col.a = 1.0f;
					col2.r = wallRCol2;
					col2.g = wallGCol2;
					col2.b = wallBCol2;
					col2.a = 1.0f;
					col3.r = wallRCol3;
					col3.g = wallGCol3;
					col3.b = wallBCol3;
					col3.a = 1.0f;
					for (int k = (i*dim); k<(dim+(i*dim)); k++) {
						for(int j=48;j<64; j++) { //Create 8 16x16 pixel sprites of floor type

							float random = Random.Range (0, 3);
							float random2 = Random.Range (0, 2);
							if (j + 2 < 64 && random2 == 0) {
								if (random == 1) {
									tex.SetPixel (j, k, col);
									tex.SetPixel (++j, k, col);
								} else if (random == 2) {
									tex.SetPixel (j, k, col2);
									tex.SetPixel (++j, k, col2);
								} else {
									tex.SetPixel (j, k, col3);
									tex.SetPixel (++j, k, col3);
								}
							} else {
								if (random == 1) {
									tex.SetPixel (j, k, col);
								} else if (random == 2) {
									tex.SetPixel (j, k, col2);
								} else {
									tex.SetPixel (j, k, col3);
								}
							}
						}
					}
				}
				if (!Directory.Exists (Application.dataPath + "/Resources/")) {
					Directory.CreateDirectory (Application.dataPath + "/Resources/");
				}
				byte[] bytes = tex.EncodeToPNG();
				DestroyImmediate(tex);
				File.WriteAllBytes (Application.dataPath + "/Resources/DungeonSprites.png", bytes);

				AssetDatabase.ImportAsset("Assets/Resources/DungeonSprites.png", ImportAssetOptions.ForceUpdate);
				AssetDatabase.Refresh();
				Texture2D myTexture = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Resources/DungeonSprites.png", typeof(Texture2D));
				string path = AssetDatabase.GetAssetPath(myTexture);
				TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
				ti.textureType = TextureImporterType.Sprite;
				ti.spritePixelsPerUnit=16;
				ti.spriteImportMode = SpriteImportMode.Multiple;
				ti.mipmapEnabled = false;
				ti.filterMode = FilterMode.Point;
				ti.isReadable = true;
				List<SpriteMetaData> newData = new List<SpriteMetaData>();

				int SliceWidth = 16;
				int SliceHeight = 16; 
				for (int i = 0; i < myTexture.width; i += SliceWidth) {
					for(int j = myTexture.height; j > 0;  j -= SliceHeight) {
						SpriteMetaData smd = new SpriteMetaData();
						smd.pivot = new Vector2(0.5f, 0.5f);
						smd.alignment = 0;
						smd.name = (myTexture.height - j)/SliceHeight + ", " + i/SliceWidth;
						smd.rect = new Rect(i, j-SliceHeight, SliceWidth, SliceHeight);
						newData.Add(smd);
					}
				}
				ti = AssetImporter.GetAtPath(path) as TextureImporter;
				ti.spritesheet = newData.ToArray();
				AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
				Object[] s = Resources.LoadAll("DungeonSprites");

				for (int i = 0; i < 8; i++) {
					GameObject w = wallTiles [i];
					SpriteRenderer sr = w.GetComponent<SpriteRenderer> ();
					int rwsprite = Random.Range (0, 7);
					sr.sprite = (Sprite)s [129+16*rwsprite];
					Wall dest = w.GetComponent<Wall> ();
					dest.dmgSprite = (Sprite)s [130+16*rwsprite];
					GameObject f = floorTiles [i];
					sr = f.GetComponent<SpriteRenderer> ();
					sr.sprite = (Sprite)s[131+16*Random.Range(0,7)];
					GameObject ow = outerWallTiles [i];
					sr = ow.GetComponent<SpriteRenderer> ();
					sr.sprite = (Sprite)s[132+16*Random.Range(0,7)];	


				}
			}
		}

		void SetTilesValuesForRooms ()
		{
			// Go through all the rooms...
			for (int i = 0; i < rooms.Length; i++) {
				Room currentRoom = rooms[i];

				// ... and for each room go through it's width.
				for (int j = 0; j < currentRoom.roomWidth; j++)
				{
					int xCoord = currentRoom.xPos + j;

					// For each horizontal tile, go up vertically through the room's height.
					for (int k = 0; k < currentRoom.roomHeight; k++){
						int yCoord = currentRoom.yPos + k;

						// The coordinates in the jagged array are based on the room's position and it's width and height.
						tiles[xCoord][yCoord] = TileType.Floor;
					}
				}
			}
		}


		void SetTilesValuesForCorridors ()
		{
			// Go through every corridor...
			for (int i = 0; i < corridors.Length; i++)
			{
				Corridor currentCorridor = corridors[i];

				// and go through it's length.
				for (int j = 0; j < currentCorridor.corridorLength; j++)
				{
					// Start the coordinates at the start of the corridor.
					int xCoord = currentCorridor.startXPos;
					int yCoord = currentCorridor.startYPos;

					// Depending on the direction, add or subtract from the appropriate
					// coordinate based on how far through the length the loop is.
					switch (currentCorridor.direction)
					{
					case Direction.North:
						yCoord += j;
						break;
					case Direction.East:
						xCoord += j;
						break;
					case Direction.South:
						yCoord -= j;
						break;
					case Direction.West:
						xCoord -= j;
						break;
					}

					// Set the tile at these coordinates to Floor.
					tiles[xCoord][yCoord] = TileType.Floor;
				}
			}
		}


		void InstantiateTiles ()
		{
			// Go through all the tiles in the jagged array...
			for (int i = 0; i < tiles.Length; i++)
			{
				for (int j = 0; j < tiles[i].Length; j++)
				{
					// ... and instantiate a floor tile for it.
					InstantiateFromArray (floorTiles, i, j);

					// If the tile type is Wall...
					if (tiles[i][j] == TileType.Wall)
					{
						// ... instantiate a wall over the top.
						InstantiateFromArray (wallTiles, i, j);
					}
				}
			}
		}


		void InstantiateOuterWalls ()
		{
			// The outer walls are one unit left, right, up and down from the board.
			float leftEdgeX = -1f;
			float rightEdgeX = columns + 0f;
			float bottomEdgeY = -1f;
			float topEdgeY = rows + 0f;

			// Instantiate both vertical walls (one on each side).
			InstantiateVerticalOuterWall (leftEdgeX, bottomEdgeY, topEdgeY);
			InstantiateVerticalOuterWall(rightEdgeX, bottomEdgeY, topEdgeY);

			// Instantiate both horizontal walls, these are one in left and right from the outer walls.
			InstantiateHorizontalOuterWall(leftEdgeX + 1f, rightEdgeX - 1f, bottomEdgeY);
			InstantiateHorizontalOuterWall(leftEdgeX + 1f, rightEdgeX - 1f, topEdgeY);
		}


		void InstantiateVerticalOuterWall (float xCoord, float startingY, float endingY)
		{
			// Start the loop at the starting value for Y.
			float currentY = startingY;

			// While the value for Y is less than the end value...
			while (currentY <= endingY)
			{
				// ... instantiate an outer wall tile at the x coordinate and the current y coordinate.
				InstantiateFromArray(outerWallTiles, xCoord, currentY);

				currentY++;
			}
		}


		void InstantiateHorizontalOuterWall (float startingX, float endingX, float yCoord)
		{
			// Start the loop at the starting value for X.
			float currentX = startingX;

			// While the value for X is less than the end value...
			while (currentX <= endingX)
			{
				// ... instantiate an outer wall tile at the y coordinate and the current x coordinate.
				InstantiateFromArray (outerWallTiles, currentX, yCoord);

				currentX++;
			}
		}


		void InstantiateFromArray (GameObject[] prefabs, float xCoord, float yCoord)
		{
			// Create a random index for the array.
			int randomIndex = Random.Range(0, prefabs.Length);

			// The position to be instantiated at is based on the coordinates.
			Vector3 position = new Vector3(xCoord, yCoord, 0f);

			// Create an instance of the prefab from the random index of the array.
			GameObject tileInstance = Instantiate(prefabs[randomIndex], position, Quaternion.identity) as GameObject;

			// Set the tile's parent to the board holder.
			tileInstance.transform.parent = boardHolder.transform;
		}
	}
}