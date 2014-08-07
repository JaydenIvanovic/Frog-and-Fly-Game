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

[RequireComponent(typeof(Collider2D))]
public class AStarTargeter : Targeter {
	
	private ArrayList path = null;
	private Vector2 targetPos;
	private Node goalNode;
	private Grid grid;
	private Targeter targeter;
	private float timeSinceUpdate = 0.0f;

	public static int SEARCH_LIMIT = 10000;

	public float updateFrequency;
	public bool drawDebug;
	public GameObject goalFlag;
	public Mode searchMode = Mode.AStarWithJPS;
	public int gridDivisionsPerSquare = 5;

	public override Vector2? GetTarget ()
	{
		if (path == null || path.Count == 0) {
			return null;
		} else {
			return (Vector2?)targetPos;
		}
	}

	public float DistanceFromGoal(Vector2 pos) {
		
		// Manhattan distance
		return (float)(Math.Abs(goalNode.GetPosition().x - pos.x) + Math.Abs(goalNode.GetPosition().y - pos.y));
		
		// Euclidean distance
		//return (goalNode.GetPosition() - pos).magnitude;
	}
	
	void Start () {

		// TO DO: Ensure that there are 2 targeters (self and the original targeter)
		Targeter[] targeters = GetComponents<Targeter>();
		foreach (Targeter t in targeters) {
			// Don't use self as a targeter!
			if (t.GetType() != typeof(AStarTargeter)) {
				targeter = t;
			}
		}

		// Initialise the grid of nodes used for A*
		// TO DO: Fix magic numbers!

		Collider2D collider = GetComponent<Collider2D>();
		float blockDetectionRadius;

		// Assume all scaling is the same (i.e. x scaling = y scaling)
		if (collider.GetType() == typeof(CircleCollider2D)) {
			blockDetectionRadius = ((CircleCollider2D)collider).radius * transform.localScale.x;
		} else if (collider.GetType() == typeof(BoxCollider2D)) {
			blockDetectionRadius = ((BoxCollider2D)collider).size.x * transform.localScale.x;
		} else {
			Debug.Log("ERROR: Unsupported collider type!");
			blockDetectionRadius = 0.0f;
		}
		 
		grid = new Grid(GameObject.Find("LeftBoundary").transform.position.x,
		                GameObject.Find("RightBoundary").transform.position.x,
		                GameObject.Find("BottomBoundary").transform.position.y,
		                GameObject.Find("TopBoundary").transform.position.y,
		                (float)gridDivisionsPerSquare,
		                blockDetectionRadius);
		
		// Set the goal at the player's position so that they won't start moving immediately
		goalNode = grid.GetClosestSquare(transform.position);
		if (goalNode == null) {
			Debug.Log("ERROR: Player placed outside of grid!");
		}

		// Hide the target flag
		if (goalFlag != null)
			goalFlag.GetComponent<SpriteRenderer>().enabled = false;
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

		timeSinceUpdate += Time.deltaTime;

		if (timeSinceUpdate > updateFrequency) {

			timeSinceUpdate = 0.0f;

			// Update path if target has updated
			Vector2? tempTarget = targeter.GetTarget();

			if (tempTarget != null) {

				// TO DO: If the closest square is blocked then return the nearest unblocked square
				Node tempGoal = grid.GetClosestSquare((Vector2)tempTarget);
				tempGoal = grid.GetClosestUnblockedNode(tempGoal);

				if ((tempGoal != null) && (!tempGoal.Equals (goalNode))) {
					goalNode = tempGoal;

					ArrayList tempPath = GetAStarPath ();

					// Ensure that A* managed to find a path
					if (tempPath != null) {
						path = tempPath;

						if (goalFlag != null)
							goalFlag.transform.position = goalNode.GetPosition ();

						// Target the first waypoint in the path
						targetPos = (Vector2)path [0];

						// Show the target flag
						if (goalFlag != null)
							goalFlag.GetComponent<SpriteRenderer> ().enabled = true;
					}
				}
			}
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
			if (goalFlag != null)
				goalFlag.GetComponent<SpriteRenderer>().enabled = false;
		}
	}
	
	private ArrayList GetAStarPath() {
		
		grid.Reset();
		
		Node startPoint = grid.GetClosestSquare((Vector2)transform.position);
		startPoint = grid.GetClosestUnblockedNode(startPoint);

		if (startPoint == null) {
			Debug.Log("WARNING: Starting point is outside the grid.");
			return null;
		}

		startPoint.SetFScore(DistanceFromGoal((Vector2)transform.position));
		startPoint.SetGScore(0.0f);
		
		// TO DO: A sorted list would be better for this because it would allow faster insertion time and no explicit sorting
		ArrayList openSet = new ArrayList();
		
		Hashtable closedSet = new Hashtable();
		
		if (grid.IsBlocked(goalNode))
			return null;
		
		openSet.Add(startPoint);

		if (!grid.IsConnected(startPoint, goalNode)) {
			return null;
		}
		
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
				int layerMask = 1 << LayerMask.NameToLayer(Grid.OBSTACLES_LAYER_NAME);
				
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
		
		// Couldn't find a path (this may be ok, e.g. predators can't reach you)
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
}