using UnityEngine;
using System.Collections;

namespace Completed
{
	//Enemy inherits from MovingObject, our base class for objects that can move, Player also inherits from this.
	public class Enemy : MovingObject
	{
		public int playerDamage; 							//The amount of food points to subtract from the player when attacking.
		public AudioClip attackSound1;						//First of two audio clips to play when attacking the player.
		public AudioClip attackSound2;						//Second of two audio clips to play when attacking the player.
		public int sightRange = 10;
		private Animator animator;							//Variable of type Animator to store a reference to the enemy's Animator component.
		private Transform target;							//Transform to attempt to move toward each turn.
		private bool skipMove;								//Boolean to determine whether or not enemy should skip a turn or move this turn.
		
		
		//Start overrides the virtual Start function of the base class.
		protected override void Start ()
		{
			//Register this enemy with our instance of GameManager by adding it to a list of Enemy objects. 
			//This allows the GameManager to issue movement commands.
			GameManager.instance.AddEnemyToList (this);
			
			//Get and store a reference to the attached Animator component.
			animator = GetComponent<Animator> ();
			
			//Find the Player GameObject using it's tag and store a reference to its transform component.
			target = GameObject.FindGameObjectWithTag ("Player").transform;
			
			//Call the start function of our base class MovingObject.
			base.Start ();
		}
		
		
		//Override the AttemptMove function of MovingObject to include functionality needed for Enemy to skip turns.
		//See comments in MovingObject for more on how base AttemptMove function works.
		protected override void AttemptMove <T> (int xDir, int yDir)
		{
			//Check if skipMove is true, if so set it to false and skip this turn.
			if(skipMove)
			{
				skipMove = false;
				return;
				
			}
			
			//Call the AttemptMove function from MovingObject.
			base.AttemptMove <T> (xDir, yDir);
			
			//Now that Enemy has moved, set skipMove to true to skip next move.
			skipMove = true;
		}

		public bool MoveEnemy () {
			//Declare variables for X and Y axis move directions, these range from -1 to 1.
			//These values allow us to choose between the cardinal directions: up, down, left and right.
			int xDir = 0;
			int yDir = 0;

			if (Mathf.Abs (transform.position.x - target.position.x) < sightRange && Mathf.Abs (transform.position.y - target.position.y) < sightRange) {
					
				//If the difference in x positions isnt approximately zero (Epsilon) do the following: 
				if (Mathf.Abs (target.position.x - transform.position.x) > float.Epsilon) {
					xDir = target.position.x > transform.position.x ? 1 : -1;
				} 
				if (Mathf.Abs (target.position.y - transform.position.y) > float.Epsilon) {
					yDir = target.position.y > transform.position.y ? 1 : -1;
				}
				AttemptMove <Player> (xDir, yDir);
				return true;
			}
			return false;
		}
		/*
		public bool vision() {
			int a = 45;
			int sight = 10;
			if (Vector3.Angle(target.position - transform.position, transform.forward) < a)
			{
				if (Vector3.Distance(target.position, transform.position) < sight)
				{
					return true;
				}
			}
			return false;
		}
		*/
		
		//OnCantMove is called if Enemy attempts to move into a space occupied by a Player, it overrides the OnCantMove function of MovingObject 
		//and takes a generic parameter T which we use to pass in the component we expect to encounter, in this case Player
		protected override void OnCantMove <T> (T component)
		{
			//Declare hitPlayer and set it to equal the encountered component.
			Player hitPlayer = component as Player;
			
			//Call the LoseFood function of hitPlayer passing it playerDamage, the amount of foodpoints to be subtracted.
			hitPlayer.LoseFood (playerDamage);
			
			//Set the attack trigger of animator to trigger Enemy attack animation.
			animator.SetTrigger ("enemyAttack");
			
			//Call the RandomizeSfx function of SoundManager passing in the two audio clips to choose randomly between.
			SoundManager.instance.RandomizeSfx (attackSound1, attackSound2);
		}
	}
}
