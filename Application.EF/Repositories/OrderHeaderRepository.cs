using Application.EF.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TBL.Core.Contracts;
using TBL.Core.Enums;
using TBL.Core.Models;

namespace TBL.EF.Repositories
{
    public class OrderHeaderRepository : BaseRepository<OrderHeader>,IOrderHeaderRepository
    {
        public OrderHeaderRepository(AppDbContext context):base(context)
        {
            
        }

		public async Task UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
		{
			var orderHeader = await _context.OrderHeader.FirstOrDefaultAsync(x => x.Id == id);
			if (orderHeader != null)
			{
				orderHeader.OrderStatus = orderStatus;
				if (!string.IsNullOrEmpty(paymentStatus))
				{
					orderHeader.PaymentStatus= paymentStatus;
				}
			}
  		}

		public async Task UpdateStrpePaymentId(int id, string sessionId, string PaymentInytentId)
		{
			var orderHeader = await _context.OrderHeader.FirstOrDefaultAsync(x => x.Id == id);
			if (!string.IsNullOrEmpty(sessionId))
			{
				orderHeader.SessionId = sessionId;
			}
		 	if (!string.IsNullOrEmpty(PaymentInytentId))
		 	{
		 		orderHeader.PaymentIntentId = PaymentInytentId;
				orderHeader.PaymentDate= DateTime.Now;
		 	}
		}
	}
}
