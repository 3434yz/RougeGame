public interface ISpaceItemInt<T> where T : ISpaceItemInt<T>
{
    XYi Position { get; }
    int IndexAtItems { get; set; }
    int IndexAtCells { get; set; }
    T? Prev { get; set; }
    T? Next { get; set; }
}

public class SpaceIndexInt<T> where T : class, ISpaceItemInt<T>
{
}