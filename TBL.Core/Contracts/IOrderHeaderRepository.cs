using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBL.Core.Models;

namespace TBL.Core.Contracts
{
    public interface IOrderHeaderRepository : IBaseRepository<OrderHeader>
    {
		 Task UpdateStatus(int id, string orderStatus, string? paymentStatus = null);
        Task UpdateStrpePaymentId(int id, string sessionId, string PaymentInytentId);
       
    }
}
