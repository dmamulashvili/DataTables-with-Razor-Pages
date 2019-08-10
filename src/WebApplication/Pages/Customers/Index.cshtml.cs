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

        public IList<Customer> Customers { get;set; }

        public async Task OnGetAsync()
        {
            //Customer = await _context.Customers.ToListAsync();
        }

        [BindProperty]
        public DataTables.DataTablesRequest DataTablesRequest { get; set; }

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

            var sortColumnName = DataTablesRequest.Columns.ElementAt(DataTablesRequest.Order.ElementAt(0).Column).Name;
            var sortDirection = DataTablesRequest.Order.ElementAt(0).Dir.ToLower();

            customersQuery = sortDirection == "desc" ?
                customersQuery.OrderByDescending(s => s.GetType().GetProperty(sortColumnName).GetValue(s))
                :
                customersQuery.OrderBy(s => s.GetType().GetProperty(sortColumnName).GetValue(s));

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
}
