using UnityEngine;
using System.Collections;
using System;

enum NodeDirections
{
	FIRST = 0,
	LEFT_NODE = 0,
	RIGHT_NODE = 1,
	BOTTOM_NODE = 2,
	TOP_NODE = 3,
	BOTTOM_LEFT = 4,
	TOP_LEFT = 5,
	BOTTOM_RIGHT = 6,
	TOP_RIGHT = 7,
	LAST = 7
};

// We need a rigidbody to apply forces
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class FindPath : MonoBehaviour {

	private ArrayList path = null;
	private Vector2 targetPos;
	private Vector3 clickPos;
	private Vector2 goalPosition;
	private Rigidbody2D rb;
	private Grid grid;
	private Animator animator;

	public static string OBSTACLES_LAYER_NAME = "Obstacles";
	public static int SEARCH_LIMIT = 10000;
	public static float blockDetectionRadius = 0.15f;

	public float acceleration = 2.0f;
	public float angularAccel = 5.0f;
	public float angularPosition = 0.0f;
	public float maxVel = 2.0f;
	public GameObject goalFlag;

	public float DistanceFromGoal(Vector2 pos) {

		// Manhattan distance
		//return (float)(Math.Abs(goalPosition.x - pos.x) + Math.Abs(goalPosition.y - pos.y));

		// Euclidean distance
		return (goalPosition - pos).magnitude;
	}
	
	void Start () {

		// Set the goal at the player's position so that they won't start moving immediately
		goalPosition = transform.position;
		clickPos = new Vector3(goalPosition.x, goalPosition.y, 0.0f);

		// Hide the target flag
		goalFlag.GetComponent<SpriteRenderer>().enabled = false;

		rb = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();

		angularPosition = rb.transform.localEulerAngles.z;

		// Initialise the grid of nodes used for A*
		grid = new Grid(-30.0f, 30.0f, -30.0f, 30.0f, 4.0f);
	}

	private void UpdateTarget() {

		// Set the goal at the closest A* node to the click location
		Node closestNode = grid.GetClosestSquare((Vector2)clickPos);

		// Ensure that the target is within the map's bounds
		if (closestNode != null) {

			goalPosition = closestNode.GetPosition();

			ArrayList tempPath = GetAStarPath();

			// Ensure that A* managed to find a path
			if (tempPath != null) {
				path = tempPath;
				goalFlag.transform.position = goalPosition;

				// Target the first waypoint in the path
				targetPos = (Vector2)path[0];

				// Show the target flag
				goalFlag.GetComponent<SpriteRenderer>().enabled = true;
			}
		}
	}

	// Draw the current path (only shows in the "scene" window)
	void DebugDrawPath() {
		if (path != null && path.Count > 0) {
			Debug.DrawLine((Vector2)transform.position, (Vector2)path[0], Color.green);
			for (int i = 0; i < path.Count - 1; i++) {
				Debug.DrawLine((Vector2)path[i], (Vector2)path[i + 1], Color.green);
			}
		}
	}

	void DebugDrawBlocked() {
		foreach (object o in grid.GetBlocked()) {
			Vector2 center = ((Node)o).GetPosition();
			Debug.DrawLine(center + new Vector2(-0.4f, 0.4f) * grid.GetDivisionSize(), center + new Vector2(0.4f, 0.4f) * grid.GetDivisionSize(), Color.red);
			Debug.DrawLine(center + new Vector2(0.4f, 0.4f) * grid.GetDivisionSize(), center + new Vector2(0.4f, -0.4f) * grid.GetDivisionSize(), Color.red);
			Debug.DrawLine(center + new Vector2(0.4f, -0.4f) * grid.GetDivisionSize(), center + new Vector2(-0.4f, -0.4f) * grid.GetDivisionSize(), Color.red);
			Debug.DrawLine(center + new Vector2(-0.4f, -0.4f) * grid.GetDivisionSize(), center + new Vector2(-0.4f, 0.4f) * grid.GetDivisionSize(), Color.red);
		}
	}

	void Update() {

		DebugDrawPath();
		DebugDrawBlocked();

		// Use right-click to move
		if(Input.GetMouseButtonDown(1)) {
			clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			UpdateTarget();
		}

		if (path != null && path.Count > 0) {
			// If we're about to overshoot the mark then move to the next waypoint
			// TO DO: Replace this 0.2f crap with a better way of detecting when we've reached the target
			if ((targetPos - (Vector2)transform.position).magnitude < 0.2f) {

				// Move to the next point
				path.RemoveAt(0);

				if (path.Count > 0) {
					// There's another waypoint left, so go to it
					targetPos = (Vector2)path[0];
				} else {
					// We've arrived at the destination
					path = null;
				}
			}
		} else {
			// Hide the goal flag when there's no current path
			goalFlag.GetComponent<SpriteRenderer>().enabled = false;
		}

		Vector2 desiredVel;
		float targetAngularVel;

		if (path != null && path.Count > 0) {

			animator.SetBool("Sitting", false);

			desiredVel = (targetPos - (Vector2)transform.position).normalized * maxVel;

			// Angular acceleration
			targetAngularVel = (Mathf.Rad2Deg * Mathf.Atan2(desiredVel.y, desiredVel.x) - 90.0f) - angularPosition;

			while (targetAngularVel > 180.0f)
				targetAngularVel -= 360.0f;

			while (targetAngularVel < -180.0f)
				targetAngularVel += 360.0f;

			angularPosition = angularPosition + targetAngularVel * angularAccel * Time.deltaTime;
			rb.transform.localEulerAngles = new Vector3(0.0f, 0.0f, angularPosition);

		} else {
			animator.SetBool("Sitting", true);

			if (rb.velocity.magnitude > 0.05f) { // TO DO: Fix magic number!
				desiredVel = Vector2.zero;
				Vector2 stoppingForce = (desiredVel - rb.velocity).normalized * acceleration;
				rb.AddForce(stoppingForce);
			} else { // TO DO: Fix magic number!
				rb.velocity = Vector2.zero;
			}
			return;
		}
		Vector2 velChange = (desiredVel - rb.velocity).normalized * acceleration;

		rb.AddForce(velChange);

		rb.angularVelocity = 0.0f; // We're doing this manually
	}

	ArrayList GetAStarPath() {

		grid.Reset();

		Node goal = new Node(goalPosition);

		Node startPoint = grid.GetClosestSquare((Vector2)transform.position);
		startPoint.SetFScore(DistanceFromGoal((Vector2)transform.position));
		startPoint.SetGScore(0.0f);

		ArrayList blocks = new ArrayList();
		ArrayList openSet = new ArrayList();
		ArrayList closedSet = new ArrayList();

		blocks = grid.GetBlocked();

		if (blocks.Contains(goal))
			return null;

		openSet.Add(startPoint);

		int searchTime = 0;

		Node currentNode = startPoint;

		while (openSet.Count != 0) {

			if (searchTime >= SEARCH_LIMIT) {
				Debug.Log("Exceeded search limit!");
				return null;
			}

			openSet.Sort(new FScoreComparer());

			// Wiki: current := the node in openset having the lowest f_score[] value
			currentNode = (Node)openSet[0];

			// Wiki: if current = goal return reconstruct_path(came_from, goal)
			if (currentNode.Equals(goal)) {

				ArrayList path = new ArrayList();

				while (currentNode != null) {
					path.Add(currentNode.GetPosition());
					currentNode = currentNode.GetParent();
				}

				path.Reverse();

				// Remove redundant waypoints
				int layerMask = 1 << LayerMask.NameToLayer(OBSTACLES_LAYER_NAME);

				for (int i = 0; i < path.Count - 2; i++) {

					// TO DO: Fix magic numbers!
					Vector2 rayDir = (Vector2)path[i + 2] - (Vector2)path[i];
					if (!Physics2D.Raycast((Vector2)path[i], rayDir, rayDir.magnitude, layerMask)
					    && !Physics2D.Raycast((Vector2)path[i] + new Vector2(0.0f, 0.35f), rayDir, rayDir.magnitude, layerMask)
					    && !Physics2D.Raycast((Vector2)path[i] + new Vector2(0.0f, -0.35f), rayDir, rayDir.magnitude, layerMask)
					    && !Physics2D.Raycast((Vector2)path[i] + new Vector2(0.35f, 0.0f), rayDir, rayDir.magnitude, layerMask)
					    && !Physics2D.Raycast((Vector2)path[i] + new Vector2(-0.35f, 0.0f), rayDir, rayDir.magnitude, layerMask)) {
						path.RemoveAt(i + 1);
						i--;
					}
				}

				// We don't want the current position in the path
				if (path.Count > 0)
					path.RemoveAt(0); 

				if (path.Count == 0) {
					return null;
				} else {
					return path;
				}
			}

			// Wiki: remove current from openset
			openSet.RemoveAt(0);

			// Wiki: add current to closedset
			if (!closedSet.Contains(currentNode)) {
				closedSet.Add(currentNode);
			}

			float[] tentativeGScores = new float[(int)NodeDirections.LAST + 1];

			// Directly adjacent g scores
			for (int i = 0; i < ((int)NodeDirections.LAST + 1) / 2; i++) {
				tentativeGScores[i] = currentNode.GetGScore() + grid.GetDivisionSize();
			}

			// Diagonally adjacent g scores
			for (int i = ((int)NodeDirections.LAST + 1) / 2; i < ((int)NodeDirections.LAST + 1); i++) {
				tentativeGScores[i] = currentNode.GetGScore() + (float)(Math.Sqrt(2.0)) * grid.GetDivisionSize();
			}

			Node[] neighbours = new Node[(int)NodeDirections.LAST + 1];
			float[] fScores = new float[(int)NodeDirections.LAST + 1];


			for (int i = (int)NodeDirections.FIRST; i <= (int)NodeDirections.LAST; i++) {
				neighbours[i] = currentNode.GetNeighbours()[i];
				if (neighbours[i] != null) {
					fScores[i] = tentativeGScores[i] + DistanceFromGoal(neighbours[i].GetPosition());
				}
			}

			for (int i = 0; i < neighbours.Length; i++) {

				Node n = neighbours[i];

				if (n != null) {
					if (!blocks.Contains(n) && !closedSet.Contains(n)) {
						if (!openSet.Contains(n)) {
							n.SetFScore(fScores[i]);
							n.SetGScore(tentativeGScores[i]);
							n.SetParent(currentNode);
							openSet.Add(n);
							//Debug.Log("Added a new node at " + n.GetPosition().x + ", " + n.GetPosition().y);
						} else {
							//int oldNodeIndex = openSet.IndexOf(n);
							//Node oldNode = (Node)openSet[oldNodeIndex];
							if (tentativeGScores[i] < n.GetGScore()) {
								n.SetGScore(tentativeGScores[i]);
								n.SetParent(currentNode);
								//Debug.Log("Updated a node at " + n.GetPosition().x + ", " + n.GetPosition().y);
								//openSet.RemoveAt(oldNodeIndex);
								//openSet.Add (n);
							}
						}
					}
				}
			}

			searchTime++;
		}

		Debug.Log("Couldn't find a path!");
		return null;
	}

	class FScoreComparer : IComparer  {
		
		int IComparer.Compare(object x, object y)  {
			return(((Node)x).GetFScore ().CompareTo (((Node)y).GetFScore ()));
		}
	}
	
	class Node {
		
		private float EQUALITY_THRESHOLD = 0.0001f;
		
		private bool V2Equal(Vector2 a, Vector2 b){
			return Vector2.SqrMagnitude (a - b) < EQUALITY_THRESHOLD;
		}
		
		private Vector2 position;
		private float fScore = float.MaxValue;
		private float gScore = float.MaxValue;
		private Node parent = null;
		
		private Node[] neighbours = new Node[(int)NodeDirections.LAST + 1];
		
		public Vector2 GetPosition() {
			return position;
		}
		
		public float GetFScore() {
			return fScore;
		}
		
		public float GetGScore() {
			return gScore;
		}
		
		public Node GetParent() {
			return parent;
		}
		
		public Node[] GetNeighbours() {
			return neighbours;
		}
		
		public void SetFScore(float fScore) {
			this.fScore = fScore;
		}
		
		public void SetGScore(float gScore) {
			this.gScore = gScore;
		}
		
		public void SetParent(Node parent) {
			this.parent = parent;
		}
		
		public void SetNeighbours(Node[] neighbours) {
			this.neighbours = neighbours;
		}
		
		public override bool Equals(object other)
		{ 
			return V2Equal(position, ((Node)other).GetPosition ());
		}
		
		public Node(Vector2 position) {
			this.position = position;
		}
	}
	
	class Grid {
		
		private float gridLeft;
		private float gridRight;
		private float gridBottom;
		private float gridTop;
		private float gridDivisionsPerUnit;
		private float divisionSize;
		
		private ArrayList blocked;
		
		// These are in terms of divisions, not world distance
		private int gridWidth;
		private int gridHeight;
		
		private Node[][] squares;
		
		public Node[][] GetSquares() {
			return squares;
		}
		
		public float GetDivisionSize() {
			return divisionSize;
		}
		
		public void Reset() {
			for (int i = 0; i < gridWidth; i++) {
				for (int j = 0; j < gridHeight; j++) {
					squares[i][j].SetFScore(float.MaxValue);
					squares[i][j].SetGScore(float.MaxValue);
					squares[i][j].SetParent(null);
				}
			}
		}
		
		public Node GetClosestSquare(Vector2 pos) {
			
			if ((pos.x < gridLeft) || (pos.x > gridRight) || (pos.y < gridBottom) || (pos.y > gridTop)) {
				return null;
			}
			
			int x = (int)Mathf.Round((pos.x - gridLeft) * gridDivisionsPerUnit);
			int y = (int)Mathf.Round((pos.y - gridBottom) * gridDivisionsPerUnit);
			
			return squares[x][y];
		}
		
		public ArrayList GetBlocked() {
			return blocked;
		}
		
		public Grid(float gridLeft, float gridRight, float gridBottom, float gridTop, float gridDivisionsPerUnit) {
			
			this.gridLeft = gridLeft;
			this.gridRight = gridRight;
			this.gridBottom = gridBottom;
			this.gridTop = gridTop;
			this.gridDivisionsPerUnit = gridDivisionsPerUnit;
			this.divisionSize = 1.0f / gridDivisionsPerUnit;
			
			this.gridWidth = (int)((gridRight - gridLeft) * gridDivisionsPerUnit);
			this.gridHeight = (int)((gridTop - gridBottom) * gridDivisionsPerUnit);
			
			squares = new Node[gridWidth][];
			
			blocked = new ArrayList();
			
			int layerMask = 1 << LayerMask.NameToLayer(OBSTACLES_LAYER_NAME);
			
			for (int i = 0; i < gridWidth; i++) {
				squares[i] = new Node[gridHeight];
			}
			
			for (int i = 0; i < gridWidth; i++) {
				for (int j = 0; j < gridHeight; j++) {
					Vector2 pos = new Vector2(gridLeft + (float)i / gridDivisionsPerUnit, gridBottom + (float)j / gridDivisionsPerUnit);
					squares[i][j] = new Node(pos);
				}
			}
			
			for (int i = 0; i < gridWidth; i++) {
				
				for (int j = 0; j < gridHeight; j++) {
					
					Node[] neighbours = new Node[(int)NodeDirections.LAST + 1];
					
					if (i > 0)
						neighbours[(int)NodeDirections.LEFT_NODE] = squares[i - 1][j];
					
					if (i < (gridWidth - 1))
						neighbours[(int)NodeDirections.RIGHT_NODE] = squares[i + 1][j];
					
					if (j > 0)
						neighbours[(int)NodeDirections.BOTTOM_NODE] = squares[i][j - 1];
					
					if (j < (gridHeight - 1))
						neighbours[(int)NodeDirections.TOP_NODE] = squares[i][j + 1];

					if ((i > 0) && (j > 0))
						neighbours[(int)NodeDirections.BOTTOM_LEFT] = squares[i - 1][j - 1];

					if ((i > 0) && (j < (gridHeight - 1)))
						neighbours[(int)NodeDirections.TOP_LEFT] = squares[i - 1][j + 1];

					if ((i < (gridWidth - 1)) && (j > 0))
						neighbours[(int)NodeDirections.BOTTOM_RIGHT] = squares[i + 1][j - 1];

					if ((i < (gridWidth - 1)) && (j < (gridHeight - 1)))
						neighbours[(int)NodeDirections.TOP_RIGHT] = squares[i + 1][j + 1];

					squares[i][j].SetNeighbours(neighbours);

					Vector2[] rayDirs = new Vector2[] {new Vector2(1.0f, 0.0f), new Vector2(-1.0f, 0.0f), new Vector2(0.0f, 1.0f), new Vector2(0.0f, -1.0f),
						                               new Vector2(1.0f, 1.0f), new Vector2(-1.0f, 1.0f), new Vector2(1.0f, -1.0f), new Vector2(-1.0f, -1.0f)};

					foreach (Vector2 ray in rayDirs) {
						if (Physics2D.Raycast(squares[i][j].GetPosition(), ray, blockDetectionRadius, layerMask)) { // A bit hacky...
							blocked.Add(squares[i][j]);
							break;
						}
					}
				}
			}

			// TO DO: Update this logic so it just fills unreachable areas
			// Fill holes
			ArrayList cornerBlocked = new ArrayList();
			for (int i = 0; i < gridWidth; i++) {
				for (int j = 0; j < gridHeight; j++) {

					Node leftNode = squares[i][j].GetNeighbours()[(int)NodeDirections.LEFT_NODE];
					Node rightNode = squares[i][j].GetNeighbours()[(int)NodeDirections.LEFT_NODE];
					Node bottomNode = squares[i][j].GetNeighbours()[(int)NodeDirections.LEFT_NODE];
					Node topNode = squares[i][j].GetNeighbours()[(int)NodeDirections.LEFT_NODE];

					if (!blocked.Contains(squares[i][j])
					    && (leftNode != null) && blocked.Contains(leftNode)
					    && (rightNode != null) && blocked.Contains(rightNode)
					    && (bottomNode != null) && blocked.Contains(bottomNode)
					    && (topNode != null) && blocked.Contains(topNode)) {

						cornerBlocked.Add(squares[i][j]);
					}
				}
			}
			blocked.AddRange(cornerBlocked);
		}
	}
}