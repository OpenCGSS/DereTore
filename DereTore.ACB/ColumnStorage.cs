namespace DereTore.ACB {
    public enum ColumnStorage : byte {

        Zero = 0x10,
        Constant = 0x30,
        PerRow = 0x50,
        Constant2 = 0x70,
        Mask = 0xf0

    }
}
