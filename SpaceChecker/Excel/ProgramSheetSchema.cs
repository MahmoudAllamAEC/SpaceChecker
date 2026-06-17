namespace SpaceChecker.Excel
{
    /// <summary>
    /// Single source of truth for the space-program sheet layout. Both the
    /// importer (which READS it) and the template/data exporters (which WRITE it)
    /// reference these constants, so they can never drift apart.
    ///
    /// Area-first layout, grouped into blocks: an area's Name, RequiredAreaTotal
    /// and Levels are on the block's FIRST row; the rooms (RoomName +
    /// RequiredRoomArea) follow, one per row. A filled AreaName cell starts a new
    /// block; blank below means "belongs to the area above" (fill-down).
    /// </summary>
    internal static class ProgramSheetSchema
    {
        public const string SheetName = "Space Program";

        public const int HeaderRow = 1;
        public const int FirstDataRow = 2;

        // 1-based column indices (area columns first, then room columns)
        public const int ApartmentCol = 1;        // AreaName
        public const int RequiredAptAreaCol = 2;  // RequiredAreaTotal
        public const int LevelCol = 3;            // Levels the area spans
        public const int RoomNameCol = 4;
        public const int RequiredRoomAreaCol = 5;

        // Header captions
        public const string ApartmentHeader = "AreaName";
        public const string RequiredAptAreaHeader = "RequiredAreaTotal";
        public const string LevelHeader = "Levels";
        public const string RoomNameHeader = "RoomName";
        public const string RequiredRoomAreaHeader = "RequiredRoomArea";
    }
}
