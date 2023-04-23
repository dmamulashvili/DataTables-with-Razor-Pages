namespace WebApplicationNET6.DataTables
{
    public class DataTablesRequest
    {
        public int Draw { get; set; }

        public IEnumerable<Column> Columns { get; set; }

        public IEnumerable<Order> Order { get; set; }

        public int Start { get; set; }

        public int Length { get; set; }

        public Search Search { get; set; }
    }
}
