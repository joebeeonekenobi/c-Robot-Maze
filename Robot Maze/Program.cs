using System;
using System.Collections;

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

			//Generate the maze walls
			generateMaze();

			printMaze();
		}

		public Coord robotCoordinate { 
		
			get {
				return robotPosition;
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
				//Console.WriteLine("Warning : The robot has elected to stay put! - Confused robot not optimal.");
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

		private void generateMaze() {

			//dfs using backtracking ::  https://en.wikipedia.org/wiki/Maze_generation_algorithm

			bool[][] visited = new bool[width][];
			for (var i = 0; i < width; i++) {
				visited[i] = new bool[height];
			}

			Stack s = new Stack();
			Random r = new Random();

			//Make the initial cell the current cell and mark it as visited
			Coord current = new Coord(0, 0);
			Coord deepest = current;
			visited[0][0] = true; 
			bool initial = true;

			//The inital cell is open.
			maze[current.x][current.y] = true;

			//While there are still avenues to explore
			while (initial || s.Count != 0) {

				initial = false;

				Coord[] possibleOptions = generatePossibleOptionsFor(current, visited);

				//If the current cell has any neighbours which have not been visited
				if (possibleOptions.Length > 0){

					//Choose randomly one of the unvisited neighbours
					Coord chosen = possibleOptions[r.Next(0, possibleOptions.Length)];

					//Push the current cell to the stack
					s.Push(current);

					//Remove the wall between the current cell and the chosen cell
					maze[chosen.x][chosen.y] = true;

					//Make the chosen cell the current cell and mark it as visited
					current = chosen;
					visited[current.x][current.y] = true;

					if (current.x + current.y > deepest.x + deepest.y) {
						deepest = current;
					}
				}
				//Else if stack is not empty
				else if(s.Count != 0){

					current = (Coord)s.Pop();
				}
			}

			//Set the goal to be the deepest location visited and open
			goal = deepest;
		}

		private void printMaze() {

			char x = 'x';
			char o = 'o';

			for (var i = 0; i < maze.Length; i++) {
				for (var j = 0; j < maze[i].Length; j++) {
					if (maze[j][i]) {
						Console.Write(o);
					}
					else { 
						Console.Write(x);
					}
				}
				Console.WriteLine();
			}
		}

		private Coord[] generateNeighbours(Coord c) {

			Stack toReturn = new Stack();

			Coord above = new Coord(c.x, c.y - 1);
			Coord below = new Coord(c.x, c.y + 1);
			Coord left = new Coord(c.x - 1, c.y);
			Coord right = new Coord(c.x + 1, c.y);

			if (exists(above)) {
				toReturn.Push(above);
			}
			if (exists(below)) {
				toReturn.Push(below);
			}
			if (exists(left)) {
				toReturn.Push(left);
			}
			if (exists(right)) {
				toReturn.Push(right);
			}

			return Array.ConvertAll(toReturn.ToArray(), item => (Coord)item);
		}

		private Coord[] generateUnvisitedNeighboursFor(Coord c, bool[][] visited) {

			Stack stack = new Stack();

			Coord[] nei = generateNeighbours(c);
			//At this point we have the neighbours that exist

			//Remove the visited neighbours
			for (var i = 0; i < nei.Length; i++) {

				if (!visited[nei[i].x][nei[i].y]) {

					stack.Push(nei[i]);
				}
			}

			//We now have the non visited existing neighbours
			return Array.ConvertAll(stack.ToArray(), item => (Coord)item);
		}

		private Coord[] generatePossibleOptionsFor(Coord c, bool[][] visited) {

			Coord[] nei = generateUnvisitedNeighboursFor(c, visited);
			Stack stack = new Stack();

			//of the non visited neighbours
			for (var i = 0; i < nei.Length; i++) {

				//Generate THEIR neighbours
				Coord[] itsNei = generateNeighbours(nei[i]);

				//maintain a flag for if any of their neighbours are open (not walls)
				bool hasNeiOpen = false;

				//of the neighbours, if any of ITS neighbours are open, we cannot go there. (exclude the current)
				for (var j = 0; j < itsNei.Length; j++) {

					if (itsNei[j] == c) {
						continue;
					}

					if (maze[itsNei[j].x][itsNei[j].y]) {
						hasNeiOpen = true;
						break;
					}
				}

				//if all of its neighbours are closed, add the original neighbour as a visitable option
				if (!hasNeiOpen) {
					stack.Push(nei[i]);
				}
			}

			return Array.ConvertAll(stack.ToArray(), item => (Coord)item);

		}
	}

	class Robot{

		public delegate Direction PollPointer(Robot robot);
		public PollPointer pollMethod;
		public Maze maze;
		public enum Direction {

			North,
			East,
			South,
			West,
			None
		}

		public Coord coordinate { 
		
			get {
				return maze.robotCoordinate;
			}
		}

		public Robot(Maze maze, PollPointer poll) {

			this.maze = maze;
			pollMethod = poll;
		}

		public bool isOpenDirection(Direction direction){

			return maze.checkDirection(direction);
		}

		public bool isAtGoal(){

			return maze.isAtGoal();
		}

	}

	class Poll { 
	
		public static Robot.Direction poll(Robot robot) {

			/*
				Method to populate with logic 
			*/

			Random r = new Random();

			while (!robot.isAtGoal()) { 
			
				Robot.Direction d = (Robot.Direction) r.Next(0, 4);

				if (robot.isOpenDirection(d)) {

					return d;

				}
				else {
					continue;
				}
			}


			return Robot.Direction.None;
		}	
	}
	
	class Program {

		static void Main(string[] args){

			Maze maze = new Maze(10, 10);
			Robot.PollPointer pollPointer = new Robot.PollPointer(Poll.poll);
			Robot robot = new Robot(maze, pollPointer);

			while (!maze.isAtGoal()) {

				maze.moveDirection(robot.pollMethod(robot));
				maze.readout();
			}
 

			Console.WriteLine("Goal!");
			Console.ReadLine();
		}
	}
}

