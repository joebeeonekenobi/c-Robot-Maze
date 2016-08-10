using System;
using System.Collections.Generic;
using System.Text;

namespace Robot_Maze {

	/*
		Assumptions:

			The robot always starts at 0, 0.
			The maze goal is always width-1, height-1.
	 
	*/

	public struct Coord {

		public int x, y;

		public Coord(int x, int y) {

			this.x = x;
			this.y = y;
		}

		public override string ToString() {

			return "(" + x + ", " + y + ")";
		}

		public override bool Equals(object obj) {

			//If the types dont match, they are not equal.
			if (!(obj is Coord)){
				return false;
			}

			var cast = (Coord)obj;

			return (this.x == cast.x && this.y == cast.y);
		}

		public static bool operator ==(Coord coord1, Coord coord2){

			return coord1.Equals(coord2);
		}

		public static bool operator !=(Coord coord1, Coord coord2)
		{
			return !coord1.Equals(coord2);
		}

		public override int GetHashCode(){ 
		
			unchecked{
				int hash = 0;
				int cf = 397;
				hash = (hash * cf) ^ x;
				hash = (hash * cf) ^ y;
				return hash;
			}
		}
	}

	class Maze {

		//The structure of the maze will essentially be a square 2d matrix of booleans; true if able to passthrough, false otherwise.
		private bool[][] maze;
		private Coord robotPosition;
		private Coord goal;
		private int width;
		private int height;

		//Instantiates the maze.
		public Maze(int x, int y) {

			if (x <= 0){
				throw new ArgumentException("Parameter cannot be <=0", "x");
			}
			else if (y <= 0){
				throw new ArgumentException("Parameter cannot be <=0", "y");
			}

			width = x;
			height = y;

			//Initial setup of robot current coordinates
			robotPosition = new Coord(0, 0);

			//Initial setup of robot goal coordinates
			goal = new Coord(x - 1, y - 1);

			//Instantiate the maze
			maze = new bool[x][];
			for (var i = 0; i < x; i++) {

				maze[i] = new bool[y];
			}

			for (var i = 0; i < x; i++) {
				for (var j = 0; j < y; j++) {
					maze[i][j] = true;
				} 
			}
		}

		//Moves the robot to a new location, given a direction.
		public void moveDirection(Robot.Direction direction) {

			Coord coord = getCoordinateForDirection(direction);

			if (!canPassThrough(coord)) {

				throw new InvalidOperationException("The robot cannot move to a coordinate that it cannot pass through. (" + coord.x + ", " + coord.y + ")");
			}

			teleportRobot(coord);
		}

		//Translates the given direction and stored position of the robot into a target direction coordinate.
		public Coord getCoordinateForDirection(Robot.Direction direction) {

			Coord position;

			if (direction == Robot.Direction.North){
				position = new Coord(robotPosition.x, robotPosition.y - 1);
			}
			else if (direction == Robot.Direction.South){
				position = new Coord(robotPosition.x, robotPosition.y + 1);
			}
			else if (direction == Robot.Direction.East){
				position = new Coord(robotPosition.x + 1, robotPosition.y);
			}
			else if (direction == Robot.Direction.West){
				position = new Coord(robotPosition.x - 1, robotPosition.y);
			}
			else if (direction == Robot.Direction.None)
			{
				Console.WriteLine("Warning : The robot has elected to stay put! - Confused robot not optimal.");
				position = new Coord(robotPosition.x, robotPosition.y);
			}
			else
			{
				throw new ArgumentException("Direction is not defined");
			}

			return position;
		}

		//Returns whether or not the robot can move in a given direction.
		public bool checkDirection(Robot.Direction direction) {

			Coord coord = getCoordinateForDirection(direction);

			return canPassThrough(coord);
		}

		//Returns whether or not the robot has reached the goal.
		public bool isAtGoal() {

			if (robotPosition == goal)
			{

				return true;
			}

			return false;
		}

		public void readout(){

			Console.WriteLine("The robot is currently at "+robotPosition+".");
		}


		/*
			Private Methods
		*/

		private void teleportRobot(Coord coord) {

			if (!canPassThrough(coord)){

				throw new InvalidOperationException("The robot cannot be teleported to a coordinate that it cannot pass through. ("+ coord.x +", "+ coord.y +")");
			}

			robotPosition = coord;
		}

		//Returns whether or not the coordinate of the maze exists
		private bool exists(Coord coord) {

			if ((coord.x < 0) || (coord.x >= width)){

				return false;
			}
			else if ((coord.y < 0) || (coord.y >= this.height)){

				return false;
			}

			return true;
		}

		//Returns whether or not the 
		private bool canPassThrough(Coord coord) {

			if (!exists(coord)) {

				return false;
			}

			return maze[coord.x][coord.y];
		}
	}

	class Robot{

		public delegate Direction PollPointer(Robot robot, Maze maze);
		public PollPointer pollMethod;
		public Maze maze;
		public enum Direction {

			North,
			East,
			South,
			West,
			None
		}


		public Robot(Maze maze, PollPointer poll) {

			this.maze = maze;
			pollMethod = poll;
		}

		public bool isOpenDirection(Direction direction){

			return maze.checkDirection(direction);
		}

		public void moveRobot(Direction direction) {

			maze.moveDirection(direction);
		}

		public bool isAtGoal(){

			return maze.isAtGoal();
		}

	}

	class Poll { 
	
		public static Robot.Direction poll(Robot robot, Maze maze) {

			/*
				Method to populate with logic 
			*/

			return Robot.Direction.South;
		}	
	}
	
	class Program {

		static void Main(string[] args){

			Maze maze = new Maze(10, 10);
			Robot.PollPointer pollPointer = new Robot.PollPointer(Poll.poll);
			Robot robot = new Robot(maze, pollPointer);

			while (!maze.isAtGoal()) {

				robot.moveRobot(robot.pollMethod(robot, maze));
				maze.readout();
			}

			Console.WriteLine("Goal!");
			Console.ReadLine();
		}
	}
}

