using UnityEngine;
using System;
using System.Collections;

// 各マスが保持する構造体
public struct Node {
	public Vector2 to;
	public Vector2 from;

	public bool block;
	public bool empty;

	public double moveCost;
	public double heuristicCost;

	public Node(Vector2 position) :this() {
		to = position;
		block = false;
		empty = true; // openNodes closedNodes 用
		heuristicCost = Math.Sqrt(
			Math.Pow(AStar.goalPosition.x - to.x, 2) +
			Math.Pow(AStar.goalPosition.y - to.y, 2)
		);
	}

	public void Show() {
		Debug.Log(String.Format("({0}, {1})", to.x, to.y));
	}

	public double GetScore() {
		return moveCost + heuristicCost;
	}
}

public class AStar : MonoBehaviour {

	public int fieldSize;
	public static Vector2 goalPosition;

	public Material openMat;
	public Material closedMat;
	public Material activeMat;
	public Material nomalMat;

	public Node[,] nodes;
	public Node[,] openNodes;
	public Node[,] closedNodes;

	// Fieldクラスで SetNodes() する前に呼び出されたい
	public void Initialize(int size) {
		fieldSize = size;
		nodes = new Node[fieldSize, fieldSize];
		openNodes = new Node[fieldSize, fieldSize];
		closedNodes = new Node[fieldSize, fieldSize];
		goalPosition = new Vector2(fieldSize - 1, fieldSize - 1);
	}

	void Start() {
	}

	void Update() {
	}

	public void Reset() {
		GameObject node;

		// リストを初期化
		for (int x = 0; x < fieldSize; x++) {
			for (int y = 0; y < fieldSize; y++) {
				node = GameObject.Find(String.Format("AStar/Node_{0, 2:00}_{1, 2:00}", x, y));
				node.GetComponent<MeshRenderer>().material = nomalMat;
				nodes[x, y] = new Node(new Vector2(x, y));
				openNodes[x, y] = new Node(new Vector2(x, y));
				closedNodes[x, y] = new Node(new Vector2(x, y));
			}
		}

		// 経路の表示を削除
		foreach (Transform child in transform) {
			if (child.name == "Point(Clone)" || child.name == "Beam(Clone)") {
				Destroy(child.gameObject);
			}
		}

		// スタートとゴールの設置
		GetComponent<Field>().SetPoint();
	}

	// A*探索アルゴリズムを開始する
	public IEnumerator Search() {
		// スタート地点の初期化
		openNodes[0, 0] = new Node(new Vector2(0, 0));
		openNodes[0, 0].from = new Vector2(0, 0);
		openNodes[0, 0].moveCost = 0;
		openNodes[0, 0].empty = false;

		yield return new WaitForSeconds(0.8f);
		while(true) {
			Vector2 targetPosition;
			targetPosition = MinScoreNodePosition();
			yield return StartCoroutine(OpenNode((int)targetPosition.x, (int)targetPosition.y));
			if (targetPosition == goalPosition) {
				Debug.Log(closedNodes[(int)goalPosition.x, (int)goalPosition.y].moveCost);
			 break; }
		}
		yield return new WaitForSeconds(0.5f);
		yield return StartCoroutine(DrawLine());
	}

	// ノードを展開する
	IEnumerator OpenNode(int x, int y) {
		double moveCost = 0d;
		GameObject node;
		node = GameObject.Find(String.Format("AStar/Node_{0, 2:00}_{1, 2:00}", x, y));
		node.GetComponent<MeshRenderer>().material = activeMat;

		for(int dx = -1; dx < 2; dx++) {
			for (int dy = -1; dy < 2; dy++) {
				if (dx == 0 && dy == 0) { continue; }
				if (x + dx == -1 || x + dx == fieldSize) { continue; }
				if (y + dy == -1 || y + dy == fieldSize) { continue; }
				if (nodes[x + dx, y + dy].block) { continue; }

				moveCost = x * y == 0 ? 1 : Math.Sqrt(2);
				nodes[x + dx, y + dy].moveCost = openNodes[x, y].moveCost + moveCost;
				nodes[x + dx, y + dy].from = new Vector2(x, y);
				CheckUnique(x + dx, y + dy);
			}
		}
		yield return new WaitForSeconds(0.01f);

		// 展開が終わったノードは closed に
		closedNodes[x, y] = openNodes[x, y];
		closedNodes[x, y].empty = false; // closed に追加
		openNodes[x, y].empty = true; // open から除外
		node.GetComponent<MeshRenderer>().material = closedMat;
	}

	// 新規ノードであるか調べる
	void CheckUnique(int x, int y) {
		GameObject node;

		// openNodes に含まれる場合
		// より優秀なスコアであるなら moveCost と from を更新する
		if (!openNodes[x, y].empty) {
			if (openNodes[x, y].GetScore() > nodes[x, y].GetScore()) {
				openNodes[x, y].moveCost = nodes[x, y].moveCost;
				openNodes[x, y].from = nodes[x, y].from;
				node = GameObject.Find(String.Format("AStar/Node_{0, 2:00}_{1, 2:00}", x, y));
				node.GetComponent<MeshRenderer>().material = openMat;
				return;
			}
			else { return; }
		}

		// closedNodes に含まれる場合
		// より優秀なスコアであるなら closedNodes から除外し openNodes に追加する
		if (!closedNodes[x, y].empty) {
			if (closedNodes[x, y].GetScore() > nodes[x, y].GetScore()) {
				closedNodes[x, y].empty = true; // closed から除外
				openNodes[x, y].empty = false; // open に追加
				openNodes[x, y].moveCost = nodes[x, y].moveCost;
				openNodes[x, y].from = nodes[x, y].from;
				node = GameObject.Find(String.Format("AStar/Node_{0, 2:00}_{1, 2:00}", x, y));
				node.GetComponent<MeshRenderer>().material = openMat;
				return;
			}
			else { return; }
		}

		// openNodes にも closedNodes にも存在しない場合
		// 新しく openNodes に追加する
		openNodes[x, y] = new Node(new Vector2(x, y));
		openNodes[x, y].from = nodes[x, y].from;
		openNodes[x, y].moveCost = nodes[x, y].moveCost;
		openNodes[x, y].empty = false;
		node = GameObject.Find(String.Format("AStar/Node_{0, 2:00}_{1, 2:00}", x, y));
		node.GetComponent<MeshRenderer>().material = openMat;
	}

	// 最終的な経路を表示する
	IEnumerator DrawLine() {
		int x, y;
		Node node = closedNodes[(int)goalPosition.x, (int)goalPosition.y];
		Vector3 from, to;

		while (true) {
			x = (int)node.from.x;
			y = (int)node.from.y;
			from = new Vector3((int)node.to.x, 0.3f, (int)node.to.y);
			to = new Vector3(x, 0.3f, y);
			GetComponent<Field>().ChainBeam(from, to);
			if (x == 0 && y == 0) { break; }
			node = closedNodes[x, y];
		}
		yield return new WaitForSeconds(0.02f);
	}

	// openNodes から最小スコアのノードがある地点を選んで返す
	Vector2 MinScoreNodePosition() {
		Vector2 ansPosition = new Vector2(0, 0);
		double min = double.MaxValue;
		for (int x = 0; x < fieldSize; x++) {
			for (int y = 0; y < fieldSize; y++) {
				if (openNodes[x, y].empty) { continue; }
				if (min > openNodes[x, y].GetScore()) {
					min = openNodes[x, y].GetScore();
					ansPosition = openNodes[x, y].to;
				}
			}
		}
		return ansPosition;
	}
}
