﻿// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE AS LONG AS SOFTWARE FUNDS ARE DONATED TO THE POOR
// ---------------------------------------------------------------

using System.Threading.Tasks;
using OtripleS.Web.Api.Models.Fees;

namespace OtripleS.Web.Api.Services.Fees
{
    public interface IFeeService
    {
        ValueTask<Fee> AddFeeAsync(Fee fee);
    }
}
