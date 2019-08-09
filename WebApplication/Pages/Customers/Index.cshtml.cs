using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataTables.AspNet.Core;
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
}
