# DataTables-with-Razor-Pages
jQuery DataTables Server-side processing with ASP.NET Core Razor Pages, Entity Framework Core & SQLite

## Used NuGet Packages
1. AspNetCore implementation for DataTables.AspNet.
```
Install-Package DataTables.AspNet.AspNetCore -Version 2.0.2
```
2. SQLite database provider for Entity Framework Core.
```
Install-Package Microsoft.EntityFrameworkCore.Sqlite -Version 2.2.6
```

## Sample Project Configurations
1. Updated `Startup.cs`'s `ConfigureServices` method:
```
public void ConfigureServices(IServiceCollection services)
{
    services.Configure<CookiePolicyOptions>(options =>
    {
        // This lambda determines whether user consent for non-essential cookies is needed for a given request.
        options.CheckConsentNeeded = context => true;
        options.MinimumSameSitePolicy = SameSiteMode.None;
    });
    
    // DataTables.AspNet.AspNetCore
    services.RegisterDataTables();

    services.AddDbContext<ApplicationDbContext>(options =>
            // Microsoft.EntityFrameworkCore.Sqlite
            options.UseSqlite(Configuration.GetConnectionString("ApplicationDbContextConnection")));

    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
}
```
2. Added connection string `appsettings.json`.
```
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "ApplicationDbContextConnection": "DataSource=WebApplication.db"
  }
}
```

## Data Models
1. `Customer.cs`
```
public class Customer
  {
      public int Id { get; set; }

      public string Name { get; set; }

      public string PhoneNumber { get; set; }

      public string Address { get; set; }

      public string PostalCode { get; set; }
  }
```
2. `ApplicationDbContext.cs`
```
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
1. Server-side processing `/Pages/Customers/Index.cshtml.cs`
```
public class IndexModel : PageModel
{
    private readonly WebApplication.Data.ApplicationDbContext _context;

    public IndexModel(WebApplication.Data.ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<Customer> Customer { get;set; }

    [BindProperty]
    public IDataTablesRequest DataTablesRequest { get; set; }

    public async Task OnGetAsync()
    {
        //Customer = await _context.Customers.ToListAsync();
    }

    public async Task<JsonResult> OnPostAsync()
    {
        var recordsTotal = _context.Customers.Count();

        var customersQuery = _context.Customers.AsQueryable();

        var searchText = DataTablesRequest.Search.Value;
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            customersQuery = customersQuery.Where(s =>
                s.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                s.PhoneNumber.Contains(searchText) ||
                s.Address.Contains(searchText, StringComparison.OrdinalIgnoreCase) || 
                s.PostalCode.Contains(searchText)
            );
        }

        var recordsFiltered = customersQuery.Count();

        var sortColumn = DataTablesRequest.Columns.FirstOrDefault(s => s.Sort != null);

        customersQuery = sortColumn.Sort.Direction == SortDirection.Descending ?
            customersQuery.OrderByDescending(s => s.GetType().GetProperty(sortColumn.Name).GetValue(s))
            :
            customersQuery.OrderBy(s => s.GetType().GetProperty(sortColumn.Name).GetValue(s));

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
2. Client-side `/Pages/Customers/Index.cshtml`
```
<p>
    <a asp-page="Create">Create New</a>
</p>
<table id="myTable" class="table">
    <thead>
        <tr>
            <th>
                @Html.DisplayNameFor(model => model.Customer[0].Id)
            </th>
            <th>
                @Html.DisplayNameFor(model => model.Customer[0].Name)
            </th>
            <th>
                @Html.DisplayNameFor(model => model.Customer[0].PhoneNumber)
            </th>
            <th>
                @Html.DisplayNameFor(model => model.Customer[0].Address)
            </th>
            <th>
                @Html.DisplayNameFor(model => model.Customer[0].PostalCode)
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
                        return '<a href="/customers/edit/' + data.id + '">Edit</a> | <a href="/customers/details/' + data.id + '">Details</a> | <a href="/customers/delete/' + data.id + '">Delete</a>';
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
