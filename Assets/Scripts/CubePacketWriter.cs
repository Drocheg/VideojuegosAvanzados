
using UnityEngine;

public class CubePacketWriter : PacketWriter
{

    public Transform cube;

    private void Start()
    {
        GameObject temp = GameObject.Find("Cube");
        cube = temp.GetComponent<Transform>();
    }

    public void writeSnapshot()
    {
        // Packet newPacket = new CubeSnapshotPacket(cube.position.x, cube.position.y, cube.position.z);
        // writeNonReliable(newPacket);
    }
    
}
