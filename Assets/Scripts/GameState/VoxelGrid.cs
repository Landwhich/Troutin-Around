using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class VoxelGrid : NetworkBehaviour
{
    public static VoxelGrid Instance;
    [SerializeField] private GameObject voxelPrefab;
    private Dictionary<Vector3Int, GameObject> placedVoxels = new();

    void Awake() => Instance = this;

    [Rpc(SendTo.Server)]
    public void PlaceVoxelServerRpc(Vector3Int gridPos, RpcParams rpcParams = default)
    {
        // Only the host may place voxels
        if (!NetworkManager.Singleton.IsHost &&
            rpcParams.Receive.SenderClientId != NetworkManager.ServerClientId) return;

        ulong senderId = rpcParams.Receive.SenderClientId;
        if (senderId != NetworkManager.ServerClientId) return;

        if (placedVoxels.ContainsKey(gridPos)) return;

        Vector3 worldPos = new Vector3(gridPos.x + 0.5f, gridPos.y + 0.5f, gridPos.z + 0.5f);
        var obj = Instantiate(voxelPrefab, worldPos, Quaternion.identity);
        obj.GetComponent<NetworkObject>().Spawn();
        placedVoxels[gridPos] = obj;
    }

    [Rpc(SendTo.Server)]
    public void DestroyVoxelServerRpc(Vector3Int gridPos, RpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;
        if (senderId != NetworkManager.ServerClientId) return; 

        if (!placedVoxels.TryGetValue(gridPos, out GameObject voxel)) return;
        if (voxel != null) voxel.GetComponent<NetworkObject>().Despawn(true);
        placedVoxels.Remove(gridPos);
    }
}