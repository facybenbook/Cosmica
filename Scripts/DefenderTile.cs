﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DefenderTile : MonoBehaviour {
    public Defender defenderPrefab;

    [SerializeField]
    int rowNumber = 1;
    public EnemySpawner targetSpawner = null;  // The enemy spawner on the same row as the tile

    [SerializeField] Defender defenderOnTile = null;  // Reference to the defender that's on this tile

    // ===== Popup =====
    [SerializeField]
    float popupLife = 1.5f;  // How long the popup displays for
    [SerializeField]
    Vector3 popupOffset = new Vector3(0, 0, 0);  // Spawn the popup a specific offset from the centre of the tile
    [SerializeField]
    GameObject insuffEnergyPopup = null;  // Text UI element that pops up when the player attempts to spend more energy than they have
    [SerializeField]
    GameObject noDefenderPopup = null;  // Text UI element that pops up when the player attempts to spend more energy than they have

    // ===== Highlighting Units =====
    [SerializeField] ParticleSystem selectionGlow = null;
    public static bool highlighted = false;   // Keeps track of whether there is currently a unit highlighted
    public bool isHighlighted = false;        // Keeps track of whether THIS INSTANCE is the one that's highlighted
    [SerializeField] Color32 validTileColour;
    [SerializeField] ParticleSystem validTileGlow = null;

    // ===== FX =====
    [SerializeField] GameObject spawnGlowPrefab = null;
    [SerializeField] GameObject overtimeGlowPrefab = null;

    // ===== Link =====
    [SerializeField] LevelStatus levelStatus = null;
    [SerializeField] Canvas gameCanvas = null;

    void Update() {
        if (defenderOnTile != null) {
            if (targetSpawner.EnemyExistsInRow() == true && defenderOnTile.defenderUnit.defenderIsShooting == false) {
                defenderOnTile.defenderUnit.defenderIsShooting = true;
                // Debug.Log(defenderOnTile.defenderUnit.defenderIsShooting);
                defenderOnTile.defenderUnit.StartShooting();
            } else if (targetSpawner.EnemyExistsInRow() == false && defenderOnTile.defenderUnit.defenderIsShooting == true) {
                defenderOnTile.defenderUnit.defenderIsShooting = false;
                defenderOnTile.defenderUnit.StopShooting();
            }
        }
    }

    // OnMouseDown is called whenever the mouse clicks on a region within the tile's collider region (the circle region)
    public void OnMouseDown() {
        if (!levelStatus.levelStarted) {  // This if block is executed during preparation phase
            if (highlighted) {
                if (DefenderIsPresent()) {
                    if (isHighlighted) {
                        Debug.Log("Removing highlight since same unit was clicked again");
                        RemoveHighlight();
                    } else {
                        // Deselect any highlighted tiles
                        DefenderTile[] tiles = FindObjectsOfType<DefenderTile>();
                        foreach (DefenderTile tile in tiles) {
                            if (tile.isHighlighted) {
                                tile.RemoveHighlight();
                                break;
                            }
                        }
                        ToggleHighlight();
                    }
                } else {
                    MoveUnitHere();
                }
            } else if (!highlighted) {
                if (!DefenderIsPresent()) {
                    PlayPulseAnimation();
                    if (defenderPrefab != null) {
                        if (!levelStatus.endingLevel) {
                            SpawnDefender();
                        }
                    } else {
                        Debug.Log("Not highlighted and no defender selected");
                        SpawnNotification(noDefenderPopup);
                    }
                } else if (DefenderIsPresent()) {
                    ToggleHighlight();
                }
            }
        } else {  // This block is executed outside of preparation phase
            if (!DefenderIsPresent()) {
                PlayPulseAnimation();
                if (defenderPrefab != null) {
                    if (!levelStatus.endingLevel) {
                        SpawnDefender();
                    }
                } else {
                    SpawnNotification(noDefenderPopup);
                }
            }
        }
    }

    private void PlayPulseAnimation() {
        if (!DefenderIsPresent()) {
            Animation pulse = GetComponent<Animation>();
            pulse.Play();
        }
    }

    private void SpawnDefender() {
        LevelStatus levelStatus = FindObjectOfType<LevelStatus>();
        // Only spawn a unit if we have enough energy available
        if (levelStatus.energy >= defenderPrefab.defenderUnit.costToSpawn) {
            Defender spawnedDefender = Instantiate(defenderPrefab, transform.position, Quaternion.identity) as Defender;
            int defenderCost = defenderPrefab.defenderUnit.costToSpawn;
            spawnedDefender.transform.SetParent(gameCanvas.transform, false);
            spawnedDefender.transform.SetParent(transform);
            spawnedDefender.transform.position = transform.position;
            spawnedDefender.transform.localScale = new Vector3(1, 1, 1);
            levelStatus.SpendEnergy(defenderCost);
            defenderOnTile = spawnedDefender;
            if (levelStatus.isOvertime) {
                SpawnFX(overtimeGlowPrefab);
            } else {
                SpawnFX(spawnGlowPrefab);
            }
        } else {
            SpawnNotification(insuffEnergyPopup);
        }
    }

    private void SpawnFX(GameObject glowPrefab) {
        GameObject glow = Instantiate(glowPrefab, transform.position, Quaternion.identity) as GameObject;
        glow.transform.SetParent(gameCanvas.transform, false);
        glow.transform.position = transform.position;
        Destroy(glow, 3);
    }

    private void SpawnNotification(GameObject popupPrefab) {
        foreach (Transform child in transform) {  // Destroy any popup currently displayed before instantiating a new one
            if (child.tag == "Popup") {  // Popup gameobjects have the "popup" tag
                Destroy(child.gameObject);
            }
        }
        GameObject popup = Instantiate(popupPrefab, Vector2.zero, Quaternion.identity) as GameObject;
        popup.transform.SetParent(transform);
        popup.transform.position = transform.position + popupOffset;
        Destroy(popup, popupLife);
    }

    public bool DefenderIsPresent() {
        foreach (Transform child in transform) {
            if (child.tag == "DefenderContainer") {
                return true;
            }
        }
        return false;
    }

    
    public int GetRowNumber() {
        return rowNumber;
    }

    public void TellDefenderToShoot() {
        if (defenderOnTile != null) {
            StartCoroutine(defenderOnTile.defenderUnit.Shoot());
        }
    }

    public void TellDefenderToStopShooting() {
        if (defenderOnTile != null) {
            StopCoroutine(defenderOnTile.defenderUnit.Shoot());
        }
    }

    public void ToggleHighlight() {
        if (highlighted) {
            if (isHighlighted) {  // If the current instance is selected, then deselect it on the second click
                isHighlighted = false;
                highlighted = false;
                Debug.Log("Destroying child");
                foreach (Transform child in transform) {
                    if (child.tag == "Tile Selection Glow") {
                        Destroy(child.gameObject);
                    }
                }
            }
        } else if (!highlighted && !isHighlighted) {
            SpawnGlow(selectionGlow);
            highlighted = true;
            isHighlighted = true;
            // Spawn a glow particle system for all valid tiles in the battlefield
            ToggleValidTileHighlight();
        }
    }

    public void RemoveHighlight() {
        isHighlighted = false;
        highlighted = false;
        foreach (Transform child in transform) {
            if (child.tag == "Tile Selection Glow") {
                Destroy(child.gameObject);
            }
        }
        DestroyAllValidTileGlow();
    }

    public void SpawnGlow(ParticleSystem glowPrefab) {
        ParticleSystem glow = Instantiate(glowPrefab, transform.position, Quaternion.identity) as ParticleSystem;
        glow.transform.SetParent(gameCanvas.transform, false);
        glow.transform.SetParent(transform);
        glow.transform.position = transform.position;
    }

    public void DestroyAllValidTileGlow() {
        DefenderTile[] tiles = FindObjectsOfType<DefenderTile>();
        foreach (DefenderTile tile in tiles) {
            foreach (Transform child in tile.transform) {
                if (child.tag == "Valid Tile Glow") {
                    Destroy(child.gameObject);
                }
            }
        }
    }

    private void ToggleValidTileHighlight() {
        if (highlighted) {
            // Spawn a glow particle system for all valid tiles in the battlefield
            DefenderTile[] tiles = FindObjectsOfType<DefenderTile>();
            foreach (DefenderTile tile in tiles) {
                if (!tile.DefenderIsPresent()) {
                    tile.SpawnGlow(validTileGlow);
                }
            }
        } else {
            DestroyAllValidTileGlow();
        }
    }

    public void UpdateValidTileHighlight() {
        if (highlighted) {
            SpawnGlow(validTileGlow);
        }
    }

    private void MoveUnitHere() {
        DefenderTile[] tiles = FindObjectsOfType<DefenderTile>();
        Defender unitToMove = null;
        // Search for the unitToMove
        foreach (DefenderTile tile in tiles) {
            if (tile.isHighlighted == true) {
                tile.RemoveHighlight();
                foreach (Transform child in tile.transform) {
                    if (child.tag == "DefenderContainer") {
                        unitToMove = child.GetComponent<Defender>();
                    }
                }
                // break;
            }
        }
        // Move the unit to this tile
        unitToMove.transform.SetParent(gameCanvas.transform, false);
        unitToMove.transform.SetParent(transform);
        unitToMove.transform.position = transform.position;
        unitToMove.transform.localScale = new Vector3(1, 1, 1);
        defenderOnTile = unitToMove;
    }
}
