using UnityEngine;
using System;
using System.Collections;

public class Configuration : MonoBehaviour {

	private bool[,] toggles;
	private int fieldSize;

	Ray ray;
	RaycastHit hit;

	void OnGUI() {
		GUI.Box(new Rect(Screen.width - 160, 50f, 100f, 110f), "");

		// 探索開始
		if (GUI.Button(new Rect(Screen.width - 150, 60f, 80f, 40f), "Start")) {
			StartCoroutine(GetComponent<AStar>().Search());
		}

		// 全てのマスを初期化する
		if (GUI.Button(new Rect(Screen.width - 150, 110f, 80f, 40f), "Reset")) {
			GetComponent<AStar>().Reset();
		}
	}

	void Start () {
		fieldSize = Field.fieldSize;
		toggles = new bool[fieldSize, fieldSize];
	}

	// マウスでクリックしたマスがブロック（通過不可マス）になる
	void Update () {
		GameObject hitObject;
		if (Input.GetMouseButtonDown(0)) {
			ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit, 100)) {
				hitObject = hit.collider.gameObject;
				GetComponent<Field>().SetBlock(
					(int)hitObject.transform.localPosition.x,
					(int)hitObject.transform.localPosition.z
				);
			}
		}
	}
}
