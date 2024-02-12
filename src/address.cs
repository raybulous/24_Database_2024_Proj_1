public class Address
    {
        private int blkId;
        private int offset;

        public Address(int blkId, int offset)
        {
            this.blkId = blkId;
            this.offset = offset;
        }

        public int getBlkId()
        {
            return blkId;
        }

        public void setBlkId(int blockId)
        {
            this.blkId = blockId;
        }

        public int getOffset()
        {
            return offset;
        }

        public void setOffset(int offset)
        {
            this.offset = offset;
        }

        public override string toString()
        {
            return string.Format("@{0}-{1}", blkId, offset);
        }
    }