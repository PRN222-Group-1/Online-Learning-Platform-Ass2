using OnlineLearningPlatformAss2.Service.DTOs.VnPay;
using Microsoft.AspNetCore.Http;

namespace OnlineLearningPlatformAss2.Service.Services.Interfaces;

public interface IVnPayService
{
    string CreatePaymentUrl(HttpContext context, VnPayRequestModel model);
    VnPayResponseModel PaymentExecute(IQueryCollection collections);
}
