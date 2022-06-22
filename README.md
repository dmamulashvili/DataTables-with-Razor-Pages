# DataTables-with-Razor-Pages
jQuery DataTables Simple Server-side processing with ASP.NET Core Razor Pages, Entity Framework Core & SQLite

## Data Models
1. `/Data/Customer.cs`
```cs
public class Customer
  {
      public int Id { get; set; }

      public string Name { get; set; }

      public string PhoneNumber { get; set; }

      public string Address { get; set; }

      public string PostalCode { get; set; }
  }
```
2. `/Data/ApplicationDbContext.cs`
```cs
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public virtual DbSet<Customer> Customers { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}
```
## DataTables Implementation
1. Style Sheets & Scripts `/Pages/Shared/_Layout.cshtml`
```html
<link rel="stylesheet" href="https://cdn.datatables.net/1.10.19/css/jquery.dataTables.min.css" />
<script src="https://cdn.datatables.net/1.10.19/js/jquery.dataTables.min.js"></script>
```
2. DataTables Request Models
```cs
public class DataTablesRequest
{
    public int Draw { get; set; }

    public List<Column> Columns { get; set; }

    public List<Order> Order { get; set; }

    public int Start { get; set; }

    public int Length { get; set; }

    public Search Search { get; set; }
}

public class Column
{
    public string Data { get; set; }

    public string Name { get; set; }

    public bool Searchable { get; set; }

    public bool Orderable { get; set; }

    public Search Search { get; set; }
}

public class Order
{
    public int Column { get; set; }

    public string Dir { get; set; }
}

public class Search
{
    public string Value { get; set; }

    public bool IsRegex { get; set; }
}
```
3. Server-side processing `/Pages/Customers/Index.cshtml.cs`
```cs
public class IndexModel : PageModel
{
    private readonly WebApplication.Data.ApplicationDbContext _context;

    public IndexModel(WebApplication.Data.ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<Customer> Customers { get;set; }

    public async Task OnGetAsync()
    {
        //Customers = await _context.Customers.ToListAsync();
    }

    [BindProperty]
    public DataTables.DataTablesRequest DataTablesRequest { get; set; }

    public async Task<JsonResult> OnPostAsync()
    {
        var recordsTotal = _context.Customers.Count();

        var customersQuery = _context.Customers.AsQueryable();

        var searchText = DataTablesRequest.Search.Value?.ToUpper();
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            customersQuery = customersQuery.Where(s =>
                s.Name.ToUpper().Contains(searchText) ||
                s.PhoneNumber.ToUpper().Contains(searchText) ||
                s.Address.ToUpper().Contains(searchText) ||
                s.PostalCode.ToUpper().Contains(searchText)
            );
        }

        var recordsFiltered = customersQuery.Count();

        var sortColumnName = DataTablesRequest.Columns.ElementAt(DataTablesRequest.Order.ElementAt(0).Column).Name;
        var sortDirection = DataTablesRequest.Order.ElementAt(0).Dir.ToLower();
        
        // using System.Linq.Dynamic.Core
        customersQuery = customersQuery.OrderBy($"{sortColumnName} {sortDirection}");

        var skip = DataTablesRequest.Start;
        var take = DataTablesRequest.Length;
        var data = await customersQuery
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        return new JsonResult(new
        {
            Draw = DataTablesRequest.Draw,
            RecordsTotal = recordsTotal,
            RecordsFiltered = recordsFiltered,
            Data = data
        });
    }
}
```
4. Client-side `/Pages/Customers/Index.cshtml`
```razor
<p>
    <a asp-page="Create">Create New</a>
</p>
<table id="myTable" class="table">
    <thead>
        <tr>
            <th>
                @Html.DisplayNameFor(model => model.Customers[0].Id)
            </th>
            <th>
                @Html.DisplayNameFor(model => model.Customers[0].Name)
            </th>
            <th>
                @Html.DisplayNameFor(model => model.Customers[0].PhoneNumber)
            </th>
            <th>
                @Html.DisplayNameFor(model => model.Customers[0].Address)
            </th>
            <th>
                @Html.DisplayNameFor(model => model.Customers[0].PostalCode)
            </th>
            <th></th>
        </tr>
    </thead>
    <tbody>
    </tbody>
</table>

@section Scripts {
    <script>
    $(document).ready(function () {
        $('#myTable').DataTable({
            "proccessing": true,
            "serverSide": true,
            "ajax": {
                url: "/customers",
                type: 'POST',
                headers: { 'RequestVerificationToken': $('@Html.AntiForgeryToken()').val() }
            },
            "columnDefs": [
                {
                    "targets": -1,
                    "data": null,
                    "render": function (data, type, row, meta) {
                        return '<a href="/customers/edit?id=' + row.id + '">Edit</a> | <a href="/customers/details?id=' + row.id + '">Details</a> | <a href="/customers/delete?id=' + row.id + '">Delete</a>';
                    },
                    "sortable": false
                },
                { "name": "Id", "data": "id", "targets": 0, "visible": false },
                { "name": "Name", "data": "name", "targets": 1 },
                { "name": "PhoneNumber", "data": "phoneNumber", "targets": 2 },
                { "name": "Address", "data": "address", "targets": 3 },
                { "name": "PostalCode", "data": "postalCode", "targets": 4 }
            ],
            "order": [[0, "desc"]]
        });
    });
    </script>
}
```
