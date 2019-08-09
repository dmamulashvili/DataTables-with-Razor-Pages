using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication.Data;

namespace WebApplication.Pages.Customers
{
    public class IndexModel : PageModel
    {
        private readonly WebApplication.Data.ApplicationDbContext _context;

        public IndexModel(WebApplication.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Customer> Customer { get;set; }

        public async Task OnGetAsync()
        {
            //Customer = await _context.Customers.ToListAsync();
        }

        [BindProperty]
        public DataTables.DataTablesRequest BasicDataTablesRequest { get; set; }

        public async Task<JsonResult> OnPostAsync()
        {
            var recordsTotal = _context.Customers.Count();

            var customersQuery = _context.Customers.AsQueryable();

            var searchText = BasicDataTablesRequest.Search.Value;
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

            var sortColumnName = BasicDataTablesRequest.Columns[BasicDataTablesRequest.Order[0].Column].Name;
            var sortDirection = BasicDataTablesRequest.Order[0].Dir.ToLower();

            customersQuery = sortDirection == "desc" ?
                customersQuery.OrderByDescending(s => s.GetType().GetProperty(sortColumnName).GetValue(s))
                :
                customersQuery.OrderBy(s => s.GetType().GetProperty(sortColumnName).GetValue(s));

            var skip = BasicDataTablesRequest.Start;
            var take = BasicDataTablesRequest.Length;
            var data = await customersQuery
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return new JsonResult(new
            {
                Draw = BasicDataTablesRequest.Draw,
                RecordsTotal = recordsTotal,
                RecordsFiltered = recordsFiltered,
                Data = data
            });
        }
    }
}
