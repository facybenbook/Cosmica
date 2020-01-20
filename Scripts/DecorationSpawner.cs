﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorationSpawner : MonoBehaviour {
    [SerializeField] float minSpawnInterval = 1;
    [SerializeField] float maxSpawnInterval = 5;
    [SerializeField] float maxVerticalOffset = 2;    // In world units
    // ===== Decoration Objects =====
    [SerializeField] Decoration[] decorations = null;

    void Start() {
        StartCoroutine(SpawnDecoration());
    }

    private IEnumerator SpawnDecoration() {
        while (true) {
            yield return new WaitForSeconds(Random.Range(minSpawnInterval, maxSpawnInterval));
            Debug.Log("Spawning!");
            int randomIndex = Random.Range(0, decorations.Length);
            Vector3 randomOffset = RandomSpawnPosition();
            Decoration decoration = Instantiate(decorations[randomIndex], Vector3.zero, Quaternion.identity) as Decoration;
            decoration.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, false);
            decoration.transform.SetParent(transform);
            decoration.transform.position = transform.position + randomOffset;
        }
    }

    // In world units
    Vector3 RandomSpawnPosition() {
        return new Vector3(0, Random.Range(-maxVerticalOffset, maxVerticalOffset), 0);
    }
}