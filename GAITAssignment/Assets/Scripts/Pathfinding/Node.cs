using UnityEngine;
using System.Collections;
using System;

public class Node {
	
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
