namespace DefaultNamespace
{
    public class CubeSnapshotPacket : Packet
    {
        private float x;
        private float y;
        private float z;

        public CubeSnapshotPacket(float x, float y, float z) : base(0)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
}