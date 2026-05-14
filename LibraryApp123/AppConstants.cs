namespace libraryapp
{
    /// <summary>
    /// Константы идентификаторов ролей пользователей.
    /// </summary>
    public static class RoleIds
    {
        public const int Reader = 1;
        public const int Author = 2;
        public const int Admin = 3;
    }
    /// <summary>
    /// Типы книжных полок (статусы чтения).
    /// </summary>
    public static class ShelfTypes
    {
        public const byte Abandoned = 0;
        public const byte Planned = 1;
        public const byte Reading = 2;
        public const byte Read = 3;
    }
    /// <summary>
    /// Типы объектов, на которые можно пожаловаться.
    /// </summary>
    public static class ComplaintKinds
    {
        public const byte Book = 0;
        public const byte Author = 1;
        public const byte Review = 2;
    }
    /// <summary>
    /// Типы объектов, по которым может быть открыт спор.
    /// </summary>
    public static class DisputeKinds
    {
        public const byte Book = 0;
        public const byte Account = 1;
        public const byte Review = 2;
    }
    /// <summary>
    /// Статусы обработки запросов (жалоб/споров).
    /// </summary>
    public static class RequestStatus
    {
        public const byte Pending = 0;
        public const byte Accepted = 1;
        public const byte Rejected = 2;
    }
}
