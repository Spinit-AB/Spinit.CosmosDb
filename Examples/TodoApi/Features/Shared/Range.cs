namespace TodoApi.Features.Shared
{
    public class Range<T> where T : struct
    {
        public T? Min { get; set; }
        public T? Max { get; set; }

        public bool IsEmpty()
        {
            return Min == null && Max == null;
        }
    }
}
