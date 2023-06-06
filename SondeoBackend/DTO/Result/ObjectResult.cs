namespace SondeoBackend.DTO.Result
{
    public class ObjectResult<T>
    {
        public bool Result { get; set; }
        public string Respose { get; set; }
        public T Object { get; set; }
        public List<T> ListObject { get; set; }
    }
}
