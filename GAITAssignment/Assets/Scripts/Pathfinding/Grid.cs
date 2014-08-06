using UnityEngine;
using System.Collections;

public class Grid {

	// TO DO: This is duplicated in FindPath... Fix
	public static string OBSTACLES_LAYER_NAME = "Obstacles";
	public static float blockDetectionRadius = 0.15f;
	
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