using UnityEngine;
using System.Collections;
using System;

enum NodeDirections
{
	First = 0,
	Left = 0,
	Right = 1,
	Bottom = 2,
	Top = 3,
	BottomLeft = 4,
	TopLeft = 5,
	BottomRight = 6,
	TopRight = 7,
	Last = 7
};

public enum Mode
{
	AStarVanilla,
	AStarWithJPS
}

// We need a rigidbody to apply forces
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class FindPath : MonoBehaviour {
	
	private ArrayList path = null;
	private Vector2 targetPos;
	private Vector3 clickPos;
	private Node goalNode;
	private Rigidbody2D rb;
	private Grid grid;
	private Animator animator;
	
	public static string OBSTACLES_LAYER_NAME = "Obstacles";
	public static int SEARCH_LIMIT = 10000;
	public static float blockDetectionRadius = 0.15f;
	
	public bool drawDebug;
	public float acceleration = 2.0f;
	public float angularAccel = 5.0f;
	public float angularPosition = 0.0f;
	public float maxVel = 2.0f;
	public GameObject goalFlag;
	public Mode searchMode;
	public int gridDivisionsPerSquare;
	
	public float DistanceFromGoal(Vector2 pos) {
		
		// Manhattan distance
		return (float)(Math.Abs(goalNode.GetPosition().x - pos.x) + Math.Abs(goalNode.GetPosition().y - pos.y));
		
		// Euclidean distance
		//return (goalNode.GetPosition() - pos).magnitude;
	}
	
	void Start () {
		
		rb = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
		
		angularPosition = rb.transform.localEulerAngles.z;
		
		// Initialise the grid of nodes used for A*
		// TO DO: Fix magic numbers!
		grid = new Grid(-20.0f, 20.0f, -20.0f, 20.0f, (float)gridDivisionsPerSquare);
		
		// Set the goal at the player's position so that they won't start moving immediately
		goalNode = grid.GetClosestSquare(transform.position);
		if (goalNode == null) {
			Debug.Log("ERROR: Player placed outside of grid!");
		}
		
		clickPos = new Vector3(transform.position.x, transform.position.y, 0.0f);
		
		// Hide the target flag
		goalFlag.GetComponent<SpriteRenderer>().enabled = false;
	}
	
	private void UpdateTarget() {
		
		// Set the goal at the closest A* node to the click location
		Node closestNode = grid.GetClosestSquare((Vector2)clickPos);
		
		// Ensure that the target is within the map's bounds
		if (closestNode != null) {
			
			goalNode = closestNode;
			
			ArrayList tempPath = GetAStarPath();
			
			// Ensure that A* managed to find a path
			if (tempPath != null) {
				path = tempPath;
				goalFlag.transform.position = goalNode.GetPosition();
				
				// Target the first waypoint in the path
				targetPos = (Vector2)path[0];
				
				// Show the target flag
				goalFlag.GetComponent<SpriteRenderer>().enabled = true;
			}
		}
	}
	
	// Draw the current path (only shows in the "scene" window)
	void DebugDrawPath() {
		if ((path != null) && (path.Count > 0)) {
			Debug.DrawLine((Vector2)transform.position, (Vector2)path[0], Color.green);
			for (int i = 0; i < path.Count - 1; i++) {
				Debug.DrawLine((Vector2)path[i], (Vector2)path[i + 1], Color.green);
			}
		}
	}
	
	void Update() {
		
		if (drawDebug) {
			DebugDrawPath();
		}
		
		if (drawDebug) {
			grid.DebugDrawBlocked();
		}
		
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
		
		Node startPoint = grid.GetClosestSquare((Vector2)transform.position);
		startPoint.SetFScore(DistanceFromGoal((Vector2)transform.position));
		startPoint.SetGScore(0.0f);
		
		// TO DO: A sorted list would be better for this because it would allow faster insertion time and no explicit sorting
		ArrayList openSet = new ArrayList();
		
		Hashtable closedSet = new Hashtable();
		
		if (grid.IsBlocked(goalNode))
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
			
			if (drawDebug) {
				currentNode.DebugDraw(grid.GetDivisionSize(), 2.0f);
			}
			
			// Wiki: if current = goal return reconstruct_path(came_from, goal)
			if (currentNode.Equals(goalNode)) {
				
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
				closedSet.Add(currentNode, currentNode);
			}
			
			if (searchMode == Mode.AStarWithJPS) {
				
				ArrayList jpsSuccessors = identifySuccessors(grid, currentNode, goalNode);
				
				foreach (object o in jpsSuccessors) {
					
					Node n = (Node)o;
					
					if (n != null) {
						
						float tentG = currentNode.GetGScore() + (n.GetPosition() - currentNode.GetPosition()).magnitude;
						float fsc = tentG + DistanceFromGoal(n.GetPosition());
						
						if (!grid.IsBlocked(n) && !closedSet.Contains(n)) {
							if (!openSet.Contains(n)) {
								n.SetFScore(fsc);
								n.SetGScore(tentG);
								n.SetParent(currentNode);
								openSet.Add(n);
							} else {
								if (tentG < n.GetGScore()) {
									n.SetGScore(tentG);
									n.SetParent(currentNode);
								}
							}
						}
					}
				}
			} else if (searchMode == Mode.AStarVanilla) {
				
				float[] tentativeGScores = new float[(int)NodeDirections.Last + 1];
				
				// Directly adjacent g scores
				for (int i = 0; i < ((int)NodeDirections.Last + 1) / 2; i++) {
					tentativeGScores[i] = currentNode.GetGScore() + grid.GetDivisionSize();
				}
				
				// Diagonally adjacent g scores
				for (int i = ((int)NodeDirections.Last + 1) / 2; i < ((int)NodeDirections.Last + 1); i++) {
					tentativeGScores[i] = currentNode.GetGScore() + (float)(Math.Sqrt(2.0)) * grid.GetDivisionSize();
				}
				
				Node[] neighbours = new Node[(int)NodeDirections.Last + 1];
				float[] fScores = new float[(int)NodeDirections.Last + 1];
				
				
				for (int i = (int)NodeDirections.First; i <= (int)NodeDirections.Last; i++) {
					neighbours[i] = currentNode.GetNeighbours()[i];
					if (neighbours[i] != null) {
						fScores[i] = tentativeGScores[i] + DistanceFromGoal(neighbours[i].GetPosition());
					}
				}
				
				for (int i = 0; i < neighbours.Length; i++) {
					
					Node n = neighbours[i];
					
					if (n != null) {
						
						if (!grid.IsBlocked(n) && !closedSet.Contains(n)) {
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
			}
			
			searchTime++;
		}
		
		Debug.Log("ERROR: Couldn't find a path!");
		return null;
	}
	
	private Node jump(Grid g, Node currentArg, int directionXArg, int directionYArg, Node goal) {
		
		Stack nodeStack = new Stack();
		Stack dirXStack = new Stack();
		Stack dirYStack = new Stack();
		Stack consumeStack = new Stack();
		
		Stack result = new Stack();
		
		nodeStack.Push(currentArg);
		dirXStack.Push(directionXArg);
		dirYStack.Push(directionYArg);
		consumeStack.Push(false);
		
		// TO DO: Might be nice to fix all the gotos
		// Add a recursive version of this function as well
	MainLoopStart:
		while (nodeStack.Count > 0) {
			
			Node current = (Node)nodeStack.Pop();
			int directionX = (int)dirXStack.Pop();
			int directionY = (int)dirYStack.Pop();
			bool consume = (bool)consumeStack.Pop();
			
			Node nextNode = current.getNextNode(directionX, directionY);
			
			if (g.IsBlocked(nextNode)) {
				result.Push(null);
				goto MainLoopStart;
			}
			
			if (nextNode.Equals(goal)) {
				result.Push(nextNode);
				goto MainLoopStart;
			}
			
			Node offsetNode = nextNode;
			
			Node n1, n2, n3, n4;
			
			// Diagonal movement
			if (directionX != 0 && directionY != 0) {
				
				while (true) {
					
					// Diagonal Forced Neighbor Check
					n1 = offsetNode.getNextNode(-directionX, directionY);
					n2 = offsetNode.getNextNode(-directionX, 0);
					n3 = offsetNode.getNextNode(directionX, - directionY);
					n4 = offsetNode.getNextNode(0, - directionY);
					
					if (((!g.IsBlocked(n1)) && g.IsBlocked(n2))
					    || ((!g.IsBlocked(n3)) && g.IsBlocked(n4))) {
						
						result.Push(offsetNode);
						goto MainLoopStart;
					}
					
					// Check if we've diagonally moved to a point where we now need to go non-diagonally (if that makes sense...)
					if (consume) {
						
						// Don't consume again while we're in this loop!
						consume = false;
						
						Node result1 = (Node)result.Pop();
						Node result2 = (Node)result.Pop();
						
						if ((result1 != null) || (result2 != null)) {
							result.Push(offsetNode);
							goto MainLoopStart;
						}
						
					} else {
						// Ensure that we get back here once the recursive calls are done
						nodeStack.Push(current);
						dirXStack.Push(directionX);
						dirYStack.Push(directionY);
						consumeStack.Push(true);
						
						nodeStack.Push(offsetNode);
						dirXStack.Push(directionX);
						dirYStack.Push(0);
						consumeStack.Push(false);
						
						nodeStack.Push(offsetNode);
						dirXStack.Push(0);
						dirYStack.Push(directionY);
						consumeStack.Push(false);
						
						goto MainLoopStart;
					}
					
					/*
					// Alternative recursive call
					if ((jump(g, offsetNode, directionX, 0, goal) != null)
					    || (jump(g, offsetNode, 0, directionY, goal) != null)) {

						result.Push(offsetNode);
						goto MainLoopStart;
					}*/
					
					current = offsetNode;
					offsetNode = offsetNode.getNextNode(directionX, directionY);
					
					if (g.IsBlocked(offsetNode)) {
						result.Push(null);
						goto MainLoopStart;
					}
					
					if (offsetNode.Equals(goal)) {
						result.Push(offsetNode);
						goto MainLoopStart;
					}
				}
			} else {
				
				// Horizontal movement
				if (directionX != 0) {
					while (true) {
						
						// Diagonally up
						n1 = offsetNode.getNextNode(directionX, 1);
						
						// Up
						n2 = offsetNode.getNextNode(0, 1);
						
						// Diagonally down
						n3 = offsetNode.getNextNode(directionX, -1);
						
						// Down
						n4 = offsetNode.getNextNode(0, -1);
						
						if (((!g.IsBlocked(n1)) && g.IsBlocked(n2))
						    || ((!g.IsBlocked(n3)) && g.IsBlocked(n4))) {
							
							result.Push(offsetNode);
							goto MainLoopStart;
						}
						
						offsetNode = offsetNode.getNextNode(directionX, directionY);
						
						if (g.IsBlocked(offsetNode)) {
							result.Push(null);
							goto MainLoopStart;
						}
						
						if (offsetNode.Equals(goal)) {
							result.Push(offsetNode);
							goto MainLoopStart;
						}
					}
				}
				else {
					
					// Vertical movement
					while (true) {
						
						// Diagonally right
						n1 = offsetNode.getNextNode(1, directionY);
						
						// Right
						n2 = offsetNode.getNextNode(1, 0);
						
						// Diagonally left
						n3 = offsetNode.getNextNode(-1, directionY);
						
						// Left
						n4 = offsetNode.getNextNode(-1, 0);
						
						if (((!g.IsBlocked(n1)) && g.IsBlocked(n2))
						    || ((!g.IsBlocked(n3)) && g.IsBlocked(n4))) {
							
							result.Push(offsetNode);
							goto MainLoopStart;
						}
						
						offsetNode = offsetNode.getNextNode(directionX, directionY);
						
						if (g.IsBlocked(offsetNode)) {
							result.Push(null);
							goto MainLoopStart;
						}
						
						if (offsetNode.Equals(goal)) {
							result.Push(offsetNode);
							goto MainLoopStart;
						}
					}
				}
			}
		}
		
		return (Node)result.Pop();
	}
	
	private ArrayList identifySuccessors(Grid g, Node current, Node goal) {
		
		ArrayList successors = new ArrayList();
		
		ArrayList neighbours = current.GetJPSNeighbours(g);
		
		int directionX, directionY;
		
		foreach (object o in neighbours) {
			
			Node n = (Node)o;
			
			directionX = (int)Math.Round((n.GetPosition().x - current.GetPosition().x) / g.GetDivisionSize());
			directionY = (int)Math.Round((n.GetPosition().y - current.GetPosition().y) / g.GetDivisionSize());
			
			Node jumpPoint = jump(g, current, directionX, directionY, goal);
			
			if (jumpPoint != null) {
				successors.Add(jumpPoint);
			}
		}
		
		return successors;
	}
	
	class FScoreComparer : IComparer  {
		
		int IComparer.Compare(object x, object y)  {
			return(((Node)x).GetFScore ().CompareTo (((Node)y).GetFScore ()));
		}
	}
	
	class Node {
		
		private int gridRow;
		private int gridColumn;
		private Vector2 position;
		private float fScore = float.MaxValue;
		private float gScore = float.MaxValue;
		private Node parent = null;
		
		private Node[] neighbours = new Node[(int)NodeDirections.Last + 1];
		
		public int GetGridRow() {
			return gridRow;
		}
		
		public int GetGridColumn() {
			return gridColumn;
		}
		
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
			if (other == null)
				return false;
			else
				return ((gridRow == ((Node)other).GetGridRow()) && (gridColumn == ((Node)other).GetGridColumn()));
		}
		
		// Several examples on the net calculate the hash this way
		public override int GetHashCode() {
			
			unchecked // We don't care if the hash overflows
			{
				int hash = 17;
				hash = hash * 23 + gridRow;
				hash = hash * 23 + gridColumn;
				return hash;
			}
		}
		
		public Node(int gridRow, int gridColumn, Vector2 position) {
			this.gridRow = gridRow;
			this.gridColumn = gridColumn;
			this.position = position;
		}
		
		public void DebugDraw(float gridDivisionSize) {
			Debug.DrawLine(position + new Vector2(-0.4f, 0.4f) * gridDivisionSize, position + new Vector2(0.4f, 0.4f) * gridDivisionSize, Color.red);
			Debug.DrawLine(position + new Vector2(0.4f, 0.4f) * gridDivisionSize, position + new Vector2(0.4f, -0.4f) * gridDivisionSize, Color.red);
			Debug.DrawLine(position + new Vector2(0.4f, -0.4f) * gridDivisionSize, position + new Vector2(-0.4f, -0.4f) * gridDivisionSize, Color.red);
			Debug.DrawLine(position + new Vector2(-0.4f, -0.4f) * gridDivisionSize, position + new Vector2(-0.4f, 0.4f) * gridDivisionSize, Color.red);
		}
		
		public void DebugDraw(float gridDivisionSize, float drawTime) {
			Debug.DrawLine(position + new Vector2(-0.4f, 0.4f) * gridDivisionSize, position + new Vector2(0.4f, 0.4f) * gridDivisionSize, Color.blue, drawTime);
			Debug.DrawLine(position + new Vector2(0.4f, 0.4f) * gridDivisionSize, position + new Vector2(0.4f, -0.4f) * gridDivisionSize, Color.blue, drawTime);
			Debug.DrawLine(position + new Vector2(0.4f, -0.4f) * gridDivisionSize, position + new Vector2(-0.4f, -0.4f) * gridDivisionSize, Color.blue, drawTime);
			Debug.DrawLine(position + new Vector2(-0.4f, -0.4f) * gridDivisionSize, position + new Vector2(-0.4f, 0.4f) * gridDivisionSize, Color.blue, drawTime);
		}
		
		public Node getNextNode(int directionX, int directionY) {
			
			Node nextNode = null;
			
			if (directionX == -1) {
				if (directionY == -1) {
					nextNode = neighbours[(int)NodeDirections.BottomLeft];
				} else if (directionY == 0) {
					nextNode = neighbours[(int)NodeDirections.Left];
				} else if (directionY == 1) {
					nextNode = neighbours[(int)NodeDirections.TopLeft];
				}
			} else if (directionX == 0) {
				if (directionY == -1) {
					nextNode = neighbours[(int)NodeDirections.Bottom];
				} else if (directionY == 0) {
					Debug.Log("WARNING: Node passed itself to getNextNode!");
					return this;
				} else if (directionY == 1) {
					nextNode = neighbours[(int)NodeDirections.Top];
				}
			} else if (directionX == 1) {
				if (directionY == -1) {
					nextNode = neighbours[(int)NodeDirections.BottomRight];
				} else if (directionY == 0) {
					nextNode = neighbours[(int)NodeDirections.Right];
				} else if (directionY == 1) {
					nextNode = neighbours[(int)NodeDirections.TopRight];
				}
			}
			
			return nextNode;
		}
		
		public ArrayList GetJPSNeighbours(Grid g) {
			
			ArrayList result = new ArrayList();
			
			// If we're at the start node then just return all neighbours
			if (parent == null) {
				foreach (Node n in neighbours) {
					if (!g.IsBlocked(n)) {
						result.Add(n);
					}
				}
				return result;
			}
			
			int directionX = (int)Math.Round((position.x - parent.GetPosition().x) / g.GetDivisionSize());
			int directionY = (int)Math.Round((position.y - parent.GetPosition().y) / g.GetDivisionSize());
			
			Node directionXNode = null, directionYNode = null, reverseXNode = null, reverseYNode = null, directionYRightNode = null, directionYLeftNode = null, directionXTopNode = null, directionXBottomNode = null;
			
			if (directionX > 0) {
				directionXNode = neighbours[(int)NodeDirections.Right];
				reverseXNode = neighbours[(int)NodeDirections.Left];
				directionXTopNode = neighbours[(int)NodeDirections.TopRight];
				directionXBottomNode = neighbours[(int)NodeDirections.BottomRight];
			} else if (directionX < 0) {
				directionXNode = neighbours[(int)NodeDirections.Left];
				reverseXNode = neighbours[(int)NodeDirections.Right];
				directionXTopNode = neighbours[(int)NodeDirections.TopLeft];
				directionXBottomNode = neighbours[(int)NodeDirections.BottomLeft];
			}
			
			if (directionY > 0) {
				directionYNode = neighbours[(int)NodeDirections.Top];
				reverseYNode = neighbours[(int)NodeDirections.Bottom];
				directionYRightNode = neighbours[(int)NodeDirections.TopRight];
				directionYLeftNode = neighbours[(int)NodeDirections.TopLeft];
			} else if (directionY < 0) {
				directionYNode = neighbours[(int)NodeDirections.Bottom];
				reverseYNode = neighbours[(int)NodeDirections.Top];
				directionYRightNode = neighbours[(int)NodeDirections.BottomRight];
				directionYLeftNode = neighbours[(int)NodeDirections.BottomLeft];
			}
			
			// Diagonal movement
			if (directionX != 0 && directionY != 0) {
				
				Node directionDiagNode = null, reverseXForwardYNode = null, reverseYForwardXNode = null;
				
				if ((directionX > 0) && (directionY > 0)) {
					directionDiagNode = neighbours[(int)NodeDirections.TopRight];
					reverseXForwardYNode = neighbours[(int)NodeDirections.TopLeft];
					reverseYForwardXNode = neighbours[(int)NodeDirections.BottomRight];
				} else if ((directionX > 0) && (directionY < 0)) {
					directionDiagNode = neighbours[(int)NodeDirections.BottomRight];
					reverseXForwardYNode = neighbours[(int)NodeDirections.BottomLeft];
					reverseYForwardXNode = neighbours[(int)NodeDirections.TopRight];
				} else if ((directionX < 0) && (directionY > 0)) {
					directionDiagNode = neighbours[(int)NodeDirections.TopLeft];
					reverseXForwardYNode = neighbours[(int)NodeDirections.TopRight];
					reverseYForwardXNode = neighbours[(int)NodeDirections.BottomLeft];
				} else if ((directionX < 0) && (directionY < 0)) {
					directionDiagNode = neighbours[(int)NodeDirections.BottomLeft];
					reverseXForwardYNode = neighbours[(int)NodeDirections.BottomRight];
					reverseYForwardXNode = neighbours[(int)NodeDirections.TopLeft];
				}
				
				// Left/right
				if (!g.IsBlocked(directionXNode)) {
					result.Add(directionXNode);
				}
				
				// Up/down
				if (!g.IsBlocked(directionYNode)) {
					result.Add(directionYNode);
				}
				
				// Diagonal
				if ((!g.IsBlocked(directionXNode)) || (!g.IsBlocked(directionYNode))) {
					if (!g.IsBlocked(directionDiagNode)) {
						result.Add(directionDiagNode);
					}
				}
				
				// Forced neighbours
				if ((g.IsBlocked(reverseXNode)) && (!g.IsBlocked(directionYNode))) {
					if (!g.IsBlocked(reverseXForwardYNode)) {
						result.Add(reverseXForwardYNode);
					}
				}
				if ((g.IsBlocked(reverseYNode)) && (!g.IsBlocked(directionXNode))) {
					if (!g.IsBlocked(reverseYForwardXNode)) {
						result.Add(reverseYForwardXNode);
					}
				}
			}
			else
			{
				// Vertical movement
				if (directionX == 0) {
					
					if (!g.IsBlocked(directionYNode)) {
						
						result.Add(directionYNode);
						
						if (g.IsBlocked(neighbours[(int)NodeDirections.Right])) {
							if (!g.IsBlocked(directionYRightNode)) {
								result.Add(directionYRightNode);
							}
						}
						
						if (g.IsBlocked(neighbours[(int)NodeDirections.Left])) {
							if (!g.IsBlocked(directionYLeftNode)) {
								result.Add(directionYLeftNode);
							}
						}
					}
				}
				// Horizontal movement
				else {
					if (!g.IsBlocked(directionXNode)) {
						
						result.Add(directionXNode);
						
						if (g.IsBlocked(neighbours[(int)NodeDirections.Top])) {
							if (!g.IsBlocked(directionXTopNode)) {
								result.Add(directionXTopNode);
							}
						}
						
						if (g.IsBlocked(neighbours[(int)NodeDirections.Bottom])) {
							if (!g.IsBlocked(directionXBottomNode)) {
								result.Add(directionXBottomNode);
							}
						}
					}
				}
			}
			
			return result;
		}
	}
	
	class Grid {
		
		private float gridLeft;
		private float gridRight;
		private float gridBottom;
		private float gridTop;
		private float gridDivisionsPerUnit;
		private float divisionSize;
		private Hashtable blockedSet;
		
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
		
		public bool IsBlocked(Node n) {
			if (n == null)
				return true;
			else
				return blockedSet.Contains(n);
		}
		
		public void DebugDrawBlocked() {
			foreach (object o in blockedSet.Keys) {
				((Node)o).DebugDraw(divisionSize);
			}
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
			
			blockedSet = new Hashtable();
			
			int layerMask = 1 << LayerMask.NameToLayer(OBSTACLES_LAYER_NAME);
			
			for (int i = 0; i < gridWidth; i++) {
				squares[i] = new Node[gridHeight];
			}
			
			for (int i = 0; i < gridWidth; i++) {
				for (int j = 0; j < gridHeight; j++) {
					Vector2 pos = new Vector2(gridLeft + (float)i / gridDivisionsPerUnit, gridBottom + (float)j / gridDivisionsPerUnit);
					squares[i][j] = new Node(i, j, pos);
				}
			}
			
			for (int i = 0; i < gridWidth; i++) {
				
				for (int j = 0; j < gridHeight; j++) {
					
					Node[] neighbours = new Node[(int)NodeDirections.Last + 1];
					
					if (i > 0)
						neighbours[(int)NodeDirections.Left] = squares[i - 1][j];
					
					if (i < (gridWidth - 1))
						neighbours[(int)NodeDirections.Right] = squares[i + 1][j];
					
					if (j > 0)
						neighbours[(int)NodeDirections.Bottom] = squares[i][j - 1];
					
					if (j < (gridHeight - 1))
						neighbours[(int)NodeDirections.Top] = squares[i][j + 1];
					
					if ((i > 0) && (j > 0))
						neighbours[(int)NodeDirections.BottomLeft] = squares[i - 1][j - 1];
					
					if ((i > 0) && (j < (gridHeight - 1)))
						neighbours[(int)NodeDirections.TopLeft] = squares[i - 1][j + 1];
					
					if ((i < (gridWidth - 1)) && (j > 0))
						neighbours[(int)NodeDirections.BottomRight] = squares[i + 1][j - 1];
					
					if ((i < (gridWidth - 1)) && (j < (gridHeight - 1)))
						neighbours[(int)NodeDirections.TopRight] = squares[i + 1][j + 1];
					
					squares[i][j].SetNeighbours(neighbours);
					
					Vector2[] rayDirs = new Vector2[] {new Vector2(1.0f, 0.0f), new Vector2(-1.0f, 0.0f), new Vector2(0.0f, 1.0f), new Vector2(0.0f, -1.0f),
						new Vector2(1.0f, 1.0f), new Vector2(-1.0f, 1.0f), new Vector2(1.0f, -1.0f), new Vector2(-1.0f, -1.0f)};
					
					foreach (Vector2 ray in rayDirs) {
						if (Physics2D.Raycast(squares[i][j].GetPosition(), ray, blockDetectionRadius, layerMask)) { // A bit hacky...
							blockedSet.Add(squares[i][j], squares[i][j]);
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
					
					Node leftNode = squares[i][j].GetNeighbours()[(int)NodeDirections.Left];
					Node rightNode = squares[i][j].GetNeighbours()[(int)NodeDirections.Left];
					Node bottomNode = squares[i][j].GetNeighbours()[(int)NodeDirections.Left];
					Node topNode = squares[i][j].GetNeighbours()[(int)NodeDirections.Left];
					
					if (!blockedSet.Contains(squares[i][j])
					    && (leftNode != null) && blockedSet.Contains(leftNode)
					    && (rightNode != null) && blockedSet.Contains(rightNode)
					    && (bottomNode != null) && blockedSet.Contains(bottomNode)
					    && (topNode != null) && blockedSet.Contains(topNode)) {
						
						cornerBlocked.Add(squares[i][j]);
					}
				}
			}
			foreach (object o in cornerBlocked) {
				Node n = (Node)o;
				blockedSet.Add(n, n);
			}
		}
	}
}