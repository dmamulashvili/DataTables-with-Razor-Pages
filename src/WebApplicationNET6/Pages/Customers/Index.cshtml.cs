using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplicationNET6.Data;

namespace WebApplicationNET6.Pages.Customers
{
    public class IndexModel : PageModel
    {
        private readonly WebApplicationNET6.Data.ApplicationDbContext _context;

        public IndexModel(WebApplicationNET6.Data.ApplicationDbContext context)
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
}
