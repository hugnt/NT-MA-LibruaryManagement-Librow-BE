using Librow.Application.Models.Requests;
using Librow.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Services;
public interface IDashboardService
{
    public Task<Result> GetSummaryInfo();
    public Task<Result> GetPopularBooks(int top = 5);
    public Task<Result> GetRequestAnalysis();
}
