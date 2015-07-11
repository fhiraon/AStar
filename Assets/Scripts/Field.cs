using UnityEngine;	
using System;
using System.Collections;

public class Field : MonoBehaviour {

	public GameObject nodePrefab;
	public GameObject pointPrefab;
	public GameObject beamPrefab;

	public static int fieldSize = 20;
	public static bool[,] blocks = new bool[fieldSize, fieldSize];

	public Material startMat;
	public Material goalMat;
	public Material blockMat;
	public Material nomalMat;

	void Start () {
		GetComponent<AStar>().Initialize(fieldSize);
		SetNodes();
		SetPoint();
	}
	
	void Update () {
	}

	// from to をビームで繋ぐ
	public void ChainBeam(Vector3 from, Vector3 to) {
		GameObject beam;
		GameObject point;
		beam = Instantiate(beamPrefab, transform.position, transform.rotation) as GameObject;
		beam.transform.SetParent(transform);
		beam.GetComponent<LineRenderer>().SetPosition(0, from);
		beam.GetComponent<LineRenderer>().SetPosition(1, to);

		point = Instantiate(pointPrefab, transform.position, transform.rotation) as GameObject;
		point.transform.SetParent(transform);
		point.transform.localPosition = to;
	}

	// スタートとゴールに色を付け、さらに点を設置する
	public void SetPoint() {
		GameObject point, node;
		point = Instantiate(pointPrefab, transform.position, transform.rotation) as GameObject;
		point.transform.SetParent(transform);
		point.transform.localPosition = new Vector3(0f, 0.3f, 0f);
		point = Instantiate(pointPrefab, transform.position, transform.rotation) as GameObject;
		point.transform.SetParent(transform);
		point.transform.localPosition = new Vector3(fieldSize - 1, 0.3f, fieldSize - 1);

		node = GameObject.Find(String.Format("AStar/Node_{0, 2:00}_{1, 2:00}", 0, 0));
		node.GetComponent<MeshRenderer>().material = startMat;
		node = GameObject.Find(String.Format("AStar/Node_{0, 2:00}_{1, 2:00}", fieldSize - 1, fieldSize - 1));
		node.GetComponent<MeshRenderer>().material = goalMat;
	}

	// 地点(x, y)にブロックをセットする
	// 既にブロックである場合にはそれを解除する
	public void SetBlock(int x, int y) {
		if (x == 0 && y == 0) { return; }
		if (x == fieldSize - 1 && y == fieldSize - 1) { return; }

		GameObject node;
		node = GameObject.Find(String.Format("AStar/Node_{0, 2:00}_{1, 2:00}", x, y));

		if (GetComponent<AStar>().nodes[x, y].block) {
			node.GetComponent<MeshRenderer>().material = nomalMat;
			GetComponent<AStar>().nodes[x, y].block = false;
		}
		else {
			node.GetComponent<MeshRenderer>().material = blockMat;
			GetComponent<AStar>().nodes[x, y].block = true;
		}
	}

	// 何もないマスを敷き詰める
	void SetNodes () {
		GameObject node;
		Vector3 nodePosition;
		for (int x = 0; x < fieldSize; x++) {
			for (int y = 0; y < fieldSize; y++) {
				nodePosition = new Vector3(x, 0f, y);
				node = Instantiate(nodePrefab, transform.position, transform.rotation) as GameObject;
				node.transform.SetParent(transform);
				node.transform.localPosition = nodePosition;
				node.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
				node.name = String.Format("Node_{0, 2:00}_{1, 2:00}", x, y);
				GetComponent<AStar>().nodes[x, y] = new Node(new Vector2(x, y));
				GetComponent<AStar>().openNodes[x, y] = new Node(new Vector2(x, y));
				GetComponent<AStar>().closedNodes[x, y] = new Node(new Vector2(x, y));
			}
		}
	}
}
