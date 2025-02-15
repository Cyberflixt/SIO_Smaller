using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DestructibleWall : MonoBehaviour
{
    #region Settings
    // Public
    public DestructibleWallData wallType;
    public Vector2Int wallSize = new Vector2Int(5,3);

    // Hidden
    [HideInInspector]
    public bool[] cellsAlive;
    [HideInInspector]
    public bool startFullAlive = true;

    // Private
    private Transform[] cellsMesh;
    private Vector3 worldCellSize;


    private const float noiseScale = .3f;
    private const float noiseStrength = 2;
    private const float attachThreshold = 1.05f;

    // Binary lookup tables
    private readonly int[] neighborsMeshLookup = {
        0,2,2,4,2,3,4,5,2,4,3,5,4,5,5,6,
    };
    private readonly int[] neighborsRotationLookup = {
        0,2,3,3,0,0,0,0,1,2,1,3,1,2,1,0,
    };

    private readonly (int, int)[] neighborsOffset = {
        (-1,0),
        (0,-1),
        (1,0),
        (0,1),
    };
    private readonly (int, int)[] attachmentsOffset = {
        (0,1),
        (-1,1),
        (-1,0),
        (-1,-1),
        (0,-1),
        (1,-1),
        (1,0),
        (1,1),
    };

    #endregion

    // !=======[ METHODS ]=======!



    #region Utilities
    private int CellPosToIndex(int x, int y){
        // Cell's index in the grid
        return x + y*wallSize.x;
    }
    private Vector3 GetCellWorldPosition(int x, int y){
        // Returns the world position of a cell
        return transform.position + transform.right * (x+.5f) * wallType.cellSize.x + transform.up * (y+.5f) * wallType.cellSize.y;
    }
    private bool PosIsInGrid(int x, int y){
        return x>=0 && x<wallSize.x && y>=0 && y<wallSize.y;
    }
    private void RefreshWorldCellSize(){
        worldCellSize = Utils.VectorAbs(transform.localToWorldMatrix * wallType.cellSize);
    }

    private bool[] ShrinkSelection(bool[] selection){
        // Shrink cells selection by 1 cells on its borders
        // (Imagine a selection on an image and shrinking from 1 pixel)
        bool[] newSelection = new bool[selection.Length];

        int i = 0;
        for (int y=0; y<wallSize.y; y++){
            for (int x=0; x<wallSize.x; x++){
                if (selection[i]){
                    bool nextState = true;
                    foreach ((int dx, int dy) in neighborsOffset){
                        if (PosIsInGrid(x+dx, y+dy)){
                            int neigh_int = CellPosToIndex(x+dx, y+dy);
                            if (!selection[neigh_int]){
                                nextState = false;
                                break;
                            }
                        }
                    }
                    newSelection[i] = nextState;
                }
                i++;
            }
        }
        return newSelection;
    }

    private void ReduceGrid(){
        // Make grid smaller by removing edges filled with 0
        // For optimisation

        detachedCellsAlive = new bool[0];
        
        // Top side
        int removeSideTop = -1;
        for (int y=0; y<wallSize.y; y++){
            bool empty = true;
            for (int x=0; x<wallSize.x; x++){
                if (cellsAlive[x+y*wallSize.x]){
                    empty = false;
                    break;
                }
            }
            if (empty) removeSideTop++; else break;
        }

        // Left side
        int removeSideLeft = -1;
        for (int x=0; x<wallSize.x; x++){
            bool empty = true;
            for (int y=0; y<wallSize.y; y++){
                if (cellsAlive[x+y*wallSize.x]){
                    empty = false;
                    break;
                }
            }
            if (empty) removeSideLeft++; else break;
        }

        // Bottom side
        int removeSideBottom = -1;
        for (int y=wallSize.y-1; y>=0; y--){
            bool empty = true;
            for (int x=0; x<wallSize.x; x++){
                if (cellsAlive[x+y*wallSize.x]){
                    empty = false;
                    break;
                }
            }
            if (empty) removeSideBottom++; else break;
        }

        // Right side
        int removeSideRight = -1;
        for (int x=wallSize.x-1; x>=0; x--){
            bool empty = true;
            for (int y=0; y<wallSize.y; y++){
                if (cellsAlive[x+y*wallSize.x]){
                    empty = false;
                    break;
                }
            }
            if (empty) removeSideRight++; else break;
        }

        // Crop 2D array
        if (removeSideTop>0 || removeSideLeft>0 || removeSideRight>0 || removeSideBottom>0){
            // Remove negative values
            removeSideTop = Math.Max(0, removeSideTop);
            removeSideLeft = Math.Max(0, removeSideLeft);
            removeSideRight = Math.Max(0, removeSideRight);
            removeSideBottom = Math.Max(0, removeSideBottom);
            
            // Reduce size
            Vector2Int wallSizeOld = wallSize;
            wallSize.x -= removeSideRight + removeSideLeft;
            wallSize.y -= removeSideBottom + removeSideTop;

            // Replace cellsAlive
            bool[] newCellsAlive = new bool[wallSize.x*wallSize.y];
            int i = 0;
            for (int y=0; y<wallSize.y; y++){
                for (int x=0; x<wallSize.x; x++){
                    // Crop cells
                    bool v = cellsAlive[(y+removeSideTop)*wallSizeOld.x + x + removeSideLeft];
                    newCellsAlive[i] = v;
                    i++;
                }
            }
            cellsAlive = newCellsAlive;
            
            // Offset object
            transform.localPosition += new Vector3(removeSideLeft*wallType.cellSize.x, removeSideTop*wallType.cellSize.y, 0);
        }
    }

    #endregion

    #region Meshes
    [Button("Rebuild")]
    public void ResetMeshes(){
        // Remove previous meshes
        if (Application.isEditor && !Application.isPlaying){
            cellsAlive = TableUtils.ArrayFilled(wallSize.x * wallSize.y, true);
            transform.ClearAllChildrenEditMode();
        } else {
            transform.ClearAllChildren();
        }

        cellsMesh = new Transform[wallSize.x * wallSize.y];

        // For each cells:
        int i = 0;
        for (int y=0; y<wallSize.y; y++){
            for (int x=0; x<wallSize.x; x++){
                if (cellsAlive[i]){
                    // Alive
                    int rotation = 1;
                    if (wallType.mesh1RandomRotation){
                        rotation = UnityEngine.Random.Range(0,3);
                    }
                    CreateCellMesh(x,y, 1, rotation);
                } else {
                    // Dead
                    OnCellDestroyed(x,y);
                }
                i++;
            }
        }
    }
    
    private bool initialized;
    private void Start(){
        // Already started?
        if (initialized) return;
        initialized = true;

        // Initialize variables
        cellsMesh = new Transform[wallSize.x * wallSize.y];

        // Start full alive?
        if (startFullAlive){
            cellsAlive = TableUtils.ArrayFilled(wallSize.x * wallSize.y, true);
        }

        // Spawn meshes
        ResetMeshes();
        RefreshRigidBody();

        // Weld to nearby surfaces
        if (startFullAlive){
            CheckAnyCellsWeld();
        } else {
            //CheckAnyCellsUnweld();
        }
    }

    private void RefreshRigidBody(){
        // Reset rigidbody
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null){
            rb = gameObject.AddComponent<Rigidbody>();
        }

        //rb.isKinematic = true;
    }

    private Transform CreateCellMesh(int x, int y, int meshInt, int rotation){
        // Get the cell world position
        Vector3 pos = GetCellWorldPosition(x, y);

        // Create mesh using given int
        Transform mesh = wallType.InstantiateRandomMesh(meshInt);
        if (mesh){
            mesh.parent = transform;
            mesh.position = pos;

            // Rotate around z axis
            mesh.rotation = transform.rotation;
            mesh.localRotation *= Quaternion.AngleAxis((rotation + wallType.addedRotation)*90, Vector3.forward);

            // Save the mesh
            int i = CellPosToIndex(x, y);
            cellsMesh[i] = mesh;
        }
        return mesh;
    }

    #endregion
    #region Welding
    // Welding
    List<FixedJoint> weldsList = new List<FixedJoint>();
    List<Rigidbody> weldsTo = new List<Rigidbody>();
    private void DestroyWeld(FixedJoint weld){
        // Destroys a weld and its information
        int i = weldsList.IndexOf(weld);
        weldsList.RemoveAt(i);
        weldsTo.RemoveAt(i);
        Destroy(weld);
    }
    public void AddWeld(Rigidbody target){
        // Create weld and add to list
        FixedJoint weld = gameObject.AddComponent<FixedJoint>();
        weld.connectedBody = target;
        weldsList.Add(weld);
        weldsTo.Add(target);

        //DebugPlus.DrawArrowTo(transform.position, target.position);
    }
    private void ClearWelds(GameObject target){
        // Weld the wall to the target
        foreach (FixedJoint weld in weldsList){
            Destroy(weld);
        }
        weldsList = new List<FixedJoint>();
        weldsTo = new List<Rigidbody>();
    }


    private List<Rigidbody> GetCellTouchingRigidBodies(int x, int y){
        List<Rigidbody> found = new List<Rigidbody>();

        // Check objects inside box collision
        Vector3 cellPos = GetCellWorldPosition(x, y);
        Collider[] colliders = Physics.OverlapBox(cellPos, worldCellSize*attachThreshold);
        foreach (Collider collider in colliders){
            // Isn't self?
            if (!collider.transform.IsChildOf(transform)){
                // Is a rigidbody?
                Rigidbody rigidbody = collider.transform.GetComponentInParent<Rigidbody>();
                //name.Log(collider,rigidbody);
                if (rigidbody != null){
                    found.AddUnique(rigidbody);
                }
            }
        }

        return found;
    }

    /// <summary>
    /// Adds weld to any rigidbodies touching a cell at a given position (x,y)
    /// </summary>
    /// <param name="x">cell's x position</param>
    /// <param name="y">cell's y position</param>
    private void CheckCellWeld(int x, int y){
        foreach (Rigidbody rb in GetCellTouchingRigidBodies(x,y)){
            if (!weldsTo.Contains(rb))
                AddWeld(rb);
        }
    }

    [Button("Weld")]
    public void CheckAnyCellsWeld(){
        RefreshWorldCellSize();

        int i = 0;
        for (int y=0; y<wallSize.y; y++){
            for (int x=0; x<wallSize.x; x++){
                if (cellsAlive[i]){
                    CheckCellWeld(x,y);
                }
                i++;
            }
        }
    }


    // Un-welding
    [Button("Unwelding")]
    public void CheckAnyCellsUnweld(){
        List<Rigidbody> unweldsAccu = new List<Rigidbody>();
        RefreshWorldCellSize();

        int i = 0;
        for (int y=0; y<wallSize.y; y++){
            for (int x=0; x<wallSize.x; x++){
                if (cellsAlive[i]){
                    foreach (Rigidbody rb in GetCellTouchingRigidBodies(x,y)){
                        unweldsAccu.AddUnique(rb);
                    }
                }
                i++;
            }
        }

        // Destroy joints that are not connected anymore
        for (i=0; i<weldsList.Count; i++){
            FixedJoint joint = weldsList[i];
            if (!unweldsAccu.Contains(joint.connectedBody)){
                DestroyWeld(joint);
                i--;
            }
        }
    }

    #endregion
    #region Debris

    private DestructibleWall CreateSubGrid(bool[] newCellsAlive){
        // Create new object
        GameObject obj = new GameObject("Wall Split");
        obj.transform.parent = transform.parent;
        obj.transform.position = transform.position;
        obj.transform.rotation = transform.rotation;

        // Add component
        DestructibleWall comp = obj.AddComponent<DestructibleWall>();
        comp.wallType = wallType;
        comp.wallSize = wallSize;
        comp.cellsAlive = newCellsAlive;
        comp.startFullAlive = false;
        comp.ReduceGrid();

        // Add sfx component
        SoundCollider sfx = obj.AddComponent<SoundCollider>();
        if (sfx)
            sfx.material = ObjectMaterial.Concrete;
        
        return comp;
    }

    public void Explosion(Vector3 pos, float radius){
        List<(int, int)> toDestroy = new List<(int, int)>();
        List<int> aliveNeighbors = new List<int>();
        bool[] cellsDestroyed = new bool[wallSize.x*wallSize.y];

        // For each cells, first get the ones to destroy
        int i = 0;
        int deleted = 0;
        for (int y=0; y<wallSize.y; y++){
            for (int x=0; x<wallSize.x; x++){
                // Cell alive?
                if (cellsAlive[i]){
                    // Get cell position and noise value
                    Vector3 cellPos = GetCellWorldPosition(x, y);
                    float noise = Mathf.PerlinNoise(cellPos.x * noiseScale, (cellPos.y+cellPos.z)*noiseScale)*2-1;

                    // Check if cell is in radius + a little bit of noise
                    if ((cellPos-pos).magnitude + noise*noiseStrength < radius){
                        // Cell is now dead :/
                        cellsAlive[i] = false;
                        cellsDestroyed[i] = true;
                        toDestroy.Add((x,y));
                        deleted++;
                    }
                }
                
                i++;
            }
        }

        // Also add destroyed neighbors
        int length = toDestroy.Count;
        for (i=0; i<length; i++){
            (int x,int y) = toDestroy[i];

            // For each neighbor
            foreach ((int dx, int dy) in neighborsOffset){

                // Check if they are in the grid AND dead
                int neigh_int = CellPosToIndex(x+dx, y+dy);
                if (PosIsInGrid(x+dx, y+dy)){
                    if (cellsAlive[neigh_int]){
                        // Add alive neighbors for later checks
                        aliveNeighbors.AddUnique(neigh_int);
                    } else {
                        // Refresh destroyed neighbors
                        toDestroy.AddUnique((x+dx, y+dy));
                    }
                }
            }
        }
                    

        // Now call OnCellDestroyed on them
        foreach ((int x,int y) in toDestroy){
            OnCellDestroyed(x, y);
        }

        // Check if wall pieces have been detached
        bool wasSplit = false;
        List<DestructibleWall> newPieces = new List<DestructibleWall>();
        for (int y=0; y<wallSize.y; y++){
            for (int x=0; x<wallSize.x; x++){
                DestructibleWall newPiece = CheckCellDetached(x,y);
                if (newPiece){
                    wasSplit = true;
                    newPieces.Add(newPiece);
                }
            }
        }

        if (wasSplit){
            //ReduceGrid();
        }
        //CheckAnyCellsUnweld();
        foreach (DestructibleWall newPiece in newPieces){
            //newPiece.CheckAnyCellsUnweld();
        }


        // Create flying pieces
        cellsDestroyed = ShrinkSelection(cellsDestroyed);
        CreateDestroyedPieces(cellsDestroyed);

        // Concrete audio
        Sounds.PlayAudioRandomRange(deleted/30+1, "debrisConcrete", transform.position);
    }



    

    private void CreateDestroyedRec(bool[] newCellsAlive, bool[] newPieceAlive, int x, int y, int i, int depth){
        newCellsAlive[i] = false;
        newPieceAlive[i] = true;

        if (depth>0){
            // For each neighbor
            foreach ((int dx, int dy) in neighborsOffset){
                // In grid and alive?
                if (PosIsInGrid(x+dx, y+dy)){
                    int neigh_int = CellPosToIndex(x+dx,y+dy);
                    if (newCellsAlive[neigh_int] && !newPieceAlive[neigh_int]){
                        CreateDestroyedRec(newCellsAlive, newPieceAlive, x+dx,y+dy, neigh_int, depth-1);
                    }
                }
            }
        }
    }

    private void CreateDestroyedPieces(bool[] newCellsAlive){
        int i = 0;
        for (int y=0; y<wallSize.y; y++){
            for (int x=0; x<wallSize.x; x++){
                if (newCellsAlive[i]){
                    bool[] newPieceAlive = new bool[wallSize.x*wallSize.y];
                    CreateDestroyedRec(newCellsAlive, newPieceAlive, x,y,i, 3);

                    // Create flying piece
                    DestructibleWall piece = CreateSubGrid(newPieceAlive);
                    piece.Start();
                }
                i++;
            }
        }
    }

    private void CheckCellDetachedRec(bool[] newCellsAlive, int x, int y, int index){
        newCellsAlive[index] = true;

        // For each 8 neighbors (diagonals included)
        foreach ((int dx, int dy) in attachmentsOffset){
            // Is in grid?
            if (PosIsInGrid(x+dx, y+dy)){
                // Is alive?
                int neigh_int = CellPosToIndex(x+dx, y+dy);
                if (cellsAlive[neigh_int]){
                    // Isn't already accounted?
                    if (!newCellsAlive[neigh_int]){
                        CheckCellDetachedRec(newCellsAlive, x+dx, y+dy, neigh_int);
                    }
                }
                
            }
        }
    }

    bool[] detachedCellsAlive = new bool[0];
    private DestructibleWall CheckCellDetached(int x, int y){
        // Is cell in grid and alive?
        int index = CellPosToIndex(x,y);
        if (PosIsInGrid(x,y) && cellsAlive[index]){
            // Return false if it was part of the previous check
            if (index<detachedCellsAlive.Length && detachedCellsAlive[index]) return null;

            // Initialize
            detachedCellsAlive = new bool[wallSize.x * wallSize.y];

            CheckCellDetachedRec(detachedCellsAlive, x,y, index);
            if (!detachedCellsAlive.ValuesEqual(cellsAlive)){
                // Piece was detached!

                // Turn off cells of new piece:
                int i = 0;
                for (int y0=0; y0<wallSize.y; y0++){
                    for (int x0=0; x0<wallSize.x; x0++){
                        // Belongs to the new piece?
                        if (detachedCellsAlive[i]){
                            // Turn off
                            cellsAlive[i] = false;

                            // Destroy mesh
                            Transform mesh = cellsMesh[i];
                            if (mesh)
                                Destroy(mesh.gameObject);
                            
                            // Destroy neighboring meshes
                            foreach ((int dx, int dy) in neighborsOffset){
                                if (PosIsInGrid(x0+dx, y0+dy)){
                                    int neigh_int = CellPosToIndex(x0+dx, y0+dy);
                                    mesh = cellsMesh[neigh_int];
                                    if (mesh)
                                        Destroy(mesh.gameObject);
                                }
                            }
                        }
                        i++;
                    }
                }

                // Create new object
                DestructibleWall comp = CreateSubGrid(detachedCellsAlive);

                // Copy welds
                foreach (Rigidbody rigidbody in weldsTo){
                    comp.AddWeld(rigidbody);
                }

                // Start new component (before Start)
                comp.Start();

                return comp;
            }
        }
        return null;
    }
    
    private void OnCellDestroyed(int x, int y){
        int cellInt = CellPosToIndex(x, y);

        // Remove old mesh
        Transform oldMesh = cellsMesh[cellInt];
        if (oldMesh){
            Destroy(oldMesh.gameObject);
        }

        // Get neighbors binary value
        int neighbors = 0;
        for (int n=0; n<4; n++){
            // Get neighbor position
            (int dx, int dy) = neighborsOffset[n];
            int neighbor_x = x + dx;
            int neighbor_y = y + dy;

            // Is the neighbor in the grid?
            if (PosIsInGrid(neighbor_x, neighbor_y)){

                // Is neighbor alive
                int neighborIndex = CellPosToIndex(neighbor_x, neighbor_y);
                if (cellsAlive[neighborIndex]){
                    // Add to binary value
                    neighbors += 1<<n;
                }
            }
        }

        // Calculate desired mesh
        int meshInt = 0;
        int rotation = 0;
        if (neighbors > 0){
            // Binary lookup table
            meshInt = neighborsMeshLookup[neighbors];
            rotation = neighborsRotationLookup[neighbors];
        }

        // Create new mesh
        CreateCellMesh(x, y, meshInt, rotation);
    }
    #endregion
}
